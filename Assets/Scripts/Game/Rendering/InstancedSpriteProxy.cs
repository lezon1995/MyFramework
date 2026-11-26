using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Sprites;

namespace MoreMountains
{
    /// <summary>
    /// Keeps SpriteRenderer as the animation/gameplay interface, submits shared sprite meshes
    /// through MeshRenderer so Unity can instance compatible draws in its normal transparent queue.
    /// SortingGroup and 2D lighting remain under the render pipeline's control.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    [DefaultExecutionOrder(10000)]
    public sealed class InstancedSpriteProxy : MonoBehaviour
    {
        public Shader instancedShader;
        [Tooltip("Referenced template preserves instancing and effect shader variants in player builds.")]
        public Material instancedMaterial;
        SpriteRenderer source;
        MeshRenderer target;
        MeshFilter filter;
        GameObject proxy;
        Sprite sprite;
        Vector2 geometrySize;
        Material sourceMaterial;
        Texture texture;
        Material cachedMaterial;
        Mesh cachedMesh;
        bool originalForceOff;
        MaterialPropertyBlock block;
        static readonly int ColorId = Shader.PropertyToID("_GameplayColor");
        static readonly int FlipId = Shader.PropertyToID("_GameplayFlip");
        static readonly Dictionary<(Sprite, Vector2), MeshEntry> Meshes = new();
        static readonly Dictionary<(Material, Texture, Shader), MaterialEntry> Materials = new();
        sealed class MeshEntry { public Mesh mesh; public int references; }
        sealed class MaterialEntry { public Material material; public int references; }

        void OnEnable()
        {
            source = GetComponent<SpriteRenderer>();
            block ??= new MaterialPropertyBlock();
            originalForceOff = source.forceRenderingOff;
            if (!SystemInfo.supportsInstancing || !instancedShader || !instancedShader.isSupported) return;
            if (proxy) { Sync(); return; }
            proxy = new GameObject("GPU Instanced Sprite") { hideFlags = HideFlags.DontSave, layer = gameObject.layer };
            proxy.transform.SetParent(transform, false);
            filter = proxy.AddComponent<MeshFilter>();
            target = proxy.AddComponent<MeshRenderer>();
            target.shadowCastingMode = ShadowCastingMode.Off;
            target.receiveShadows = false;
            target.lightProbeUsage = LightProbeUsage.Off;
            target.reflectionProbeUsage = ReflectionProbeUsage.Off;
            Sync();
        }

        void LateUpdate() => Sync();

        void Sync()
        {
            if (!target) return;
            // Tiled sprites, masks and secondary atlas textures need SpriteRenderer semantics.
            // Keep a conservative fallback if an animator or gameplay changes these at runtime.
            var currentSprite = source.sprite;
            bool compatible = source.enabled && currentSprite && source.sharedMaterial &&
                source.drawMode != SpriteDrawMode.Tiled && source.maskInteraction == SpriteMaskInteraction.None &&
                (source.drawMode != SpriteDrawMode.Sliced || !currentSprite.packed || currentSprite.packingRotation == SpritePackingRotation.None) &&
                !currentSprite.associatedAlphaSplitTexture && currentSprite.GetSecondaryTextureCount() == 0;
            if (!compatible || originalForceOff)
            {
                target.enabled = false;
                source.forceRenderingOff = originalForceOff;
                return;
            }

            var currentSize = source.drawMode == SpriteDrawMode.Sliced ? source.size : Vector2.zero;
            if (sprite != currentSprite || geometrySize != currentSize || sourceMaterial != source.sharedMaterial || texture != currentSprite.texture)
            {
                Release();
                sprite = currentSprite;
                geometrySize = currentSize;
                sourceMaterial = source.sharedMaterial;
                texture = currentSprite.texture;
                if (!Meshes.TryGetValue((sprite, geometrySize), out var meshEntry))
                {
                    var vertices = sprite.vertices;
                    var uv = sprite.uv;
                    var indices = Array.ConvertAll(sprite.triangles, index => (int)index);
                    if (geometrySize != Vector2.zero) BuildSliced(sprite, geometrySize, out vertices, out uv, out indices);
                    var positions = new Vector3[vertices.Length];
                    var colors = new Color32[vertices.Length];
                    var normals = new Vector3[vertices.Length];
                    var tangents = new Vector4[vertices.Length];
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        positions[i] = vertices[i];
                        colors[i] = new Color32(255, 255, 255, 255);
                        normals[i] = Vector3.back;
                        tangents[i] = new Vector4(1, 0, 0, -1);
                    }
                    var mesh = new Mesh { name = sprite.name + " [Instanced]", hideFlags = HideFlags.DontSave };
                    mesh.vertices = positions;
                    mesh.uv = uv;
                    mesh.colors32 = colors;
                    mesh.normals = normals;
                    mesh.tangents = tangents;
                    mesh.triangles = indices;
                    // Flip flags must not move geometry outside its culling bounds.
                    var bounds = mesh.bounds;
                    bounds.extents += new Vector3(Mathf.Abs(bounds.center.x), Mathf.Abs(bounds.center.y), 0.01f);
                    bounds.center = Vector3.zero;
                    mesh.bounds = bounds;
                    meshEntry = new MeshEntry { mesh = mesh };
                    Meshes.Add((sprite, geometrySize), meshEntry);
                }
                meshEntry.references++;
                filter.sharedMesh = cachedMesh = meshEntry.mesh;

                var key = (sourceMaterial, texture, instancedShader);
                if (!Materials.TryGetValue(key, out var materialEntry))
                {
                    var material = new Material(instancedMaterial ? instancedMaterial : sourceMaterial)
                    {
                        name = sourceMaterial.name + " [Instanced " + texture.name + "]",
                        hideFlags = HideFlags.DontSave,
                        shader = instancedShader,
                        enableInstancing = true
                    };
                    material.SetTexture("_MainTex", texture);
                    materialEntry = new MaterialEntry { material = material };
                    Materials.Add(key, materialEntry);
                }
                materialEntry.references++;
                target.sharedMaterial = cachedMaterial = materialEntry.material;
            }

            source.GetPropertyBlock(block);
            block.SetColor(ColorId, source.color);
            block.SetVector(FlipId, new Vector4(source.flipX ? -1 : 1, source.flipY ? -1 : 1, 1, 1));
            target.SetPropertyBlock(block);
            target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder;
            target.renderingLayerMask = source.renderingLayerMask;
            proxy.layer = gameObject.layer;
            target.enabled = true;
            source.forceRenderingOff = true;
        }

        // A shared 4x4 vertex mesh preserves SpriteRenderer nine-slice borders and pivot.
        static void BuildSliced(Sprite sprite, Vector2 size, out Vector2[] vertices, out Vector2[] uv, out int[] indices)
        {
            var border = sprite.border / sprite.pixelsPerUnit;
            var pivot = sprite.pivot / sprite.rect.size;
            var outer = DataUtility.GetOuterUV(sprite);
            var inner = DataUtility.GetInnerUV(sprite);
            float scaleX = border.x + border.z > 0 ? Mathf.Min(1, size.x / (border.x + border.z)) : 1;
            float scaleY = border.y + border.w > 0 ? Mathf.Min(1, size.y / (border.y + border.w)) : 1;
            var xs = new[] { 0f, border.x * scaleX, size.x - border.z * scaleX, size.x };
            var ys = new[] { 0f, border.y * scaleY, size.y - border.w * scaleY, size.y };
            var us = new[] { outer.x, inner.x, inner.z, outer.z };
            var vs = new[] { outer.y, inner.y, inner.w, outer.w };
            vertices = new Vector2[16];
            uv = new Vector2[16];
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    vertices[y * 4 + x] = new Vector2(xs[x] - size.x * pivot.x, ys[y] - size.y * pivot.y);
                    uv[y * 4 + x] = new Vector2(us[x], vs[y]);
                }
            indices = new int[54];
            int index = 0;
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                {
                    int start = y * 4 + x;
                    indices[index++] = start; indices[index++] = start + 4; indices[index++] = start + 1;
                    indices[index++] = start + 1; indices[index++] = start + 4; indices[index++] = start + 5;
                }
        }

        void Release()
        {
            if (cachedMesh && Meshes.TryGetValue((sprite, geometrySize), out var meshEntry) && --meshEntry.references == 0)
            {
                DisposeObject(meshEntry.mesh);
                Meshes.Remove((sprite, geometrySize));
            }
            if (cachedMaterial && Materials.TryGetValue((sourceMaterial, texture, instancedShader), out var materialEntry) && --materialEntry.references == 0)
            {
                DisposeObject(materialEntry.material);
                Materials.Remove((sourceMaterial, texture, instancedShader));
            }
            sprite = null;
            sourceMaterial = null;
            texture = null;
            cachedMaterial = null;
            cachedMesh = null;
        }

        void OnDisable()
        {
            if (source) source.forceRenderingOff = originalForceOff;
            if (target) target.enabled = false;
        }

        void OnDestroy()
        {
            OnDisable();
            Release();
            if (proxy) DisposeObject(proxy);
            proxy = null;
            target = null;
            filter = null;
        }

        static void DisposeObject(UnityEngine.Object value)
        {
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}
