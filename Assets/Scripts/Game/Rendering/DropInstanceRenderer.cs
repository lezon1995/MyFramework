using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoreMountains
{
    /// <summary>
    /// Uses Unity's mesh-particle procedural instancing to draw many drops with one renderer per
    /// sprite/material/sorting state. The particle simulation is paused; gameplay owns every transform.
    /// No per-drop MeshRenderer, mesh, material, or particle simulation is created.
    /// </summary>
    [DefaultExecutionOrder(12000)]
    public sealed class DropInstanceRenderer : MonoBehaviour
    {
        static DropInstanceRenderer instance;
        readonly List<DropInstancedSprite> sources = new();
        readonly Dictionary<Key, Batch> batches = new();
        public int ActiveBatchCount { get; private set; }
        public int VisibleInstanceCount { get; private set; }

        readonly struct Key : IEquatable<Key>
        {
            public readonly Sprite sprite;
            public readonly Texture texture;
            public readonly Material material;
            public readonly int layer, order, gameLayer;
            public readonly uint renderingLayers;
            public readonly bool flipX, flipY;
            public Key(DropInstancedSprite item)
            {
                var r = item.source;
                sprite = r.sprite;
                texture = sprite.texture;
                material = item.instancedMaterial;
                layer = r.sortingLayerID;
                order = r.sortingOrder;
                gameLayer = r.gameObject.layer;
                renderingLayers = r.renderingLayerMask;
                flipX = r.flipX; flipY = r.flipY;
            }
            public bool Equals(Key other) => sprite == other.sprite && texture == other.texture && material == other.material &&
                layer == other.layer && order == other.order && gameLayer == other.gameLayer && renderingLayers == other.renderingLayers &&
                flipX == other.flipX && flipY == other.flipY;
            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(sprite, texture, material, layer, order, gameLayer,
                renderingLayers, (flipX ? 1 : 0) | (flipY ? 2 : 0));
        }

        internal sealed class Batch
        {
            public ParticleSystem system;
            public ParticleSystemRenderer renderer;
            public Mesh mesh;
            public Material material;
            public readonly List<DropInstancedSprite> members = new();
            public ParticleSystem.Particle[] particles = new ParticleSystem.Particle[64];
            public bool dirty;

            public void Add(DropInstancedSprite item)
            {
                item.batch = this;
                item.slot = members.Count;
                members.Add(item);
                if (members.Count > particles.Length) Array.Resize(ref particles, Mathf.NextPowerOfTwo(members.Count));
                dirty = true;
            }

            public void Remove(DropInstancedSprite item)
            {
                int last = members.Count - 1, index = item.slot;
                var swapped = members[last];
                members[index] = swapped;
                particles[index] = particles[last];
                swapped.slot = index;
                members.RemoveAt(last);
                item.batch = null;
                dirty = true;
            }

            public void Upload()
            {
                if (!dirty) return;
                var main = system.main;
                if (main.maxParticles < particles.Length) main.maxParticles = particles.Length;
                system.SetParticles(particles, members.Count);
                renderer.enabled = members.Count > 0;
                dirty = false;
            }

            public void Dispose()
            {
                renderer.enabled = false;
                DisposeObject(system.gameObject);
                DisposeObject(mesh);
                DisposeObject(material);
            }
        }

        internal static void Register(DropInstancedSprite item)
        {
            if (!instance)
            {
                var go = new GameObject("Drop GPU Instances") { hideFlags = HideFlags.DontSave };
                instance = go.AddComponent<DropInstanceRenderer>();
                if (Application.isPlaying) DontDestroyOnLoad(go);
            }
            if (!instance.sources.Contains(item)) instance.sources.Add(item);
        }

        internal static void Unregister(DropInstancedSprite item)
        {
            if (item.batch != null)
            {
                var batch = item.batch;
                batch.Remove(item);
                batch.Upload(); // Remove the picked-up instance immediately, even after LateUpdate.
            }
            if (!instance) return;
            instance.sources.Remove(item);
            instance.UpdateCounts();
            // Release textures/materials when the pool has no active drops, including scene teardown.
            if (instance.sources.Count == 0) instance.ClearBatches();
        }

        void LateUpdate() => Synchronize();

        public void Synchronize()
        {
            foreach (var item in sources)
            {
                if (!item) continue;
                if (!item.TryGetParticle(out var particle))
                {
                    item.batch?.Remove(item);
                    item.Restore();
                    continue;
                }
                var key = new Key(item);
                if (!batches.TryGetValue(key, out var batch))
                {
                    batch = CreateBatch(key);
                    batches.Add(key, batch);
                }
                if (!batch.renderer.supportsMeshInstancing)
                {
                    item.batch?.Remove(item);
                    item.Restore();
                    continue;
                }
                if (item.batch != batch)
                {
                    item.batch?.Remove(item);
                    batch.Add(item);
                }
                batch.particles[item.slot] = particle;
                batch.dirty = true;
                item.source.forceRenderingOff = true;
            }
            foreach (var batch in batches.Values) batch.Upload();
            UpdateCounts();
        }

        Batch CreateBatch(Key key)
        {
            var go = new GameObject("Instanced drops: " + key.sprite.name) { hideFlags = HideFlags.DontSave, layer = key.gameLayer };
            go.transform.SetParent(transform, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Local;
            main.startSpeed = 0;
            main.startSize3D = true;
            main.startRotation3D = true;
            main.maxParticles = 64;
            var emission = system.emission; emission.enabled = false;
            var shape = system.shape; shape.enabled = false;
            system.Play(false);
            system.Pause(false);
            var mesh = new Mesh { name = "Drop mesh: " + key.sprite.name, hideFlags = HideFlags.DontSave };
            var positions = key.sprite.vertices;
            var vertices = new Vector3[positions.Length];
            var colors = new Color32[positions.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(positions[i].x * (key.flipX ? -1 : 1), positions[i].y * (key.flipY ? -1 : 1), 0);
                colors[i] = new Color32(255, 255, 255, 255);
            }
            mesh.vertices = vertices;
            mesh.uv = key.sprite.uv;
            mesh.colors32 = colors;
            mesh.triangles = Array.ConvertAll(key.sprite.triangles, index => (int)index);
            mesh.RecalculateBounds();
            var material = new Material(key.material) { name = key.material.name + " (Shared Drop Batch)", hideFlags = HideFlags.DontSave, enableInstancing = true };
            material.SetTexture("_MainTex", key.texture);
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = mesh;
            renderer.alignment = ParticleSystemRenderSpace.World;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.enableGPUInstancing = true;
            renderer.sharedMaterial = material;
            renderer.sortingLayerID = key.layer;
            renderer.sortingOrder = key.order;
            renderer.renderingLayerMask = key.renderingLayers;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            // AnimFrame is required to match URP DefaultParticleInstanceData stride, even though
            // the coin flipbook uses shader time rather than the texture-sheet animation module.
            renderer.SetActiveVertexStreams(new List<ParticleSystemVertexStream>
                { ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV, ParticleSystemVertexStream.AnimFrame });
            return new Batch { system = system, renderer = renderer, mesh = mesh, material = material };
        }

        void UpdateCounts()
        {
            ActiveBatchCount = VisibleInstanceCount = 0;
            foreach (var batch in batches.Values)
                if (batch.members.Count > 0) { ActiveBatchCount++; VisibleInstanceCount += batch.members.Count; }
        }

        void OnDisable()
        {
            foreach (var source in sources) if (source) source.Restore();
            ClearBatches();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            foreach (var source in sources) if (source) source.Restore();
            ClearBatches();
        }

        void ClearBatches()
        {
            foreach (var source in sources) if (source) source.batch = null;
            foreach (var batch in batches.Values) batch.Dispose();
            batches.Clear();
            UpdateCounts();
        }

        static void DisposeObject(UnityEngine.Object obj)
        {
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }
    }
}
