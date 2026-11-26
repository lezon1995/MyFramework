using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MoreMountains;

public static class GameplayInstancingValidation
{
    [MenuItem("Tools/Gameplay/GPU Instancing/Validate")]
    public static void Run()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/_Gameplay/Balls/Ball_Normal.prefab");
        var config = prefab.GetComponentInChildren<InstancedSpriteProxy>(true);
        Require(config && config.instancedMaterial && config.instancedMaterial.enableInstancing, "Missing build-time instancing template");
        var originalSprite = config.GetComponent<SpriteRenderer>();
        var objects = new List<GameObject>();
        try
        {
            Mesh sharedMesh = null;
            Material sharedMaterial = null;
            for (int i = 0; i < 64; i++)
            {
                var go = new GameObject("Instancing validation " + i) { hideFlags = HideFlags.HideAndDontSave };
                objects.Add(go);
                var sprite = go.AddComponent<SpriteRenderer>();
                sprite.sprite = originalSprite.sprite;
                sprite.sharedMaterial = originalSprite.sharedMaterial;
                var proxy = go.AddComponent<InstancedSpriteProxy>();
                proxy.instancedShader = config.instancedShader;
                proxy.instancedMaterial = config.instancedMaterial;
                Invoke(proxy, "OnEnable");
                var mesh = go.GetComponentInChildren<MeshFilter>();
                var renderer = go.GetComponentInChildren<MeshRenderer>();
                Require(mesh && renderer && renderer.enabled && sprite.forceRenderingOff, "Proxy failed to replace original drawing");
                if (i == 0) { sharedMesh = mesh.sharedMesh; sharedMaterial = renderer.sharedMaterial; }
                Require(sharedMesh == mesh.sharedMesh && sharedMaterial == renderer.sharedMaterial, "Instances do not share mesh/material");
            }

            var first = objects[0].GetComponent<SpriteRenderer>();
            var firstProxy = objects[0].GetComponent<InstancedSpriteProxy>();
            var firstTarget = objects[0].GetComponentInChildren<MeshRenderer>();
            var effects = new RendererPropertyOverrides(first);
            int fade = Shader.PropertyToID("_PixelOutlineFade");
            effects.SetFloat(fade, 0.37f);
            first.color = Color.red;
            first.flipX = true;
            first.sortingOrder = 123;
            Invoke(firstProxy, "LateUpdate");
            var block = new MaterialPropertyBlock();
            firstTarget.GetPropertyBlock(block);
            Require(Mathf.Approximately(block.GetFloat(fade), 0.37f), "Per-instance effect lost");
            Require(block.GetColor("_GameplayColor") == Color.red && block.GetVector("_GameplayFlip").x == -1, "Color or flip not synchronized");
            Require(firstTarget.sortingOrder == 123, "Sorting order not synchronized");
            objects[1].GetComponent<SpriteRenderer>().GetPropertyBlock(block);
            Require(!block.HasFloat(fade), "Effect leaked into another instance");
            first.enabled = false;
            Invoke(firstProxy, "LateUpdate");
            Require(!firstTarget.enabled, "Disabled sprite still draws");
            first.enabled = true;
            first.drawMode = SpriteDrawMode.Tiled;
            Invoke(firstProxy, "LateUpdate");
            Require(!firstTarget.enabled && !first.forceRenderingOff, "Unsupported mode did not fall back");
            first.drawMode = SpriteDrawMode.Simple;
            Invoke(firstProxy, "LateUpdate");
            Require(firstTarget.enabled && first.forceRenderingOff, "Fallback did not recover");
            var brick = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/_Gameplay/Bricks/Brick_1x1.prefab");
            SpriteRenderer slicedSource = null;
            foreach (var renderer in brick.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.name == "Shield") slicedSource = renderer;
            Require(slicedSource, "Missing nine-slice validation sprite");
            first.sprite = slicedSource.sprite;
            first.drawMode = SpriteDrawMode.Sliced;
            first.size = new Vector2(2, 3);
            Invoke(firstProxy, "LateUpdate");
            var slicedMesh = objects[0].GetComponentInChildren<MeshFilter>().sharedMesh;
            Require(firstTarget.enabled && slicedMesh.vertexCount == 16 && slicedMesh.triangles.Length == 54,
                "Nine-slice mesh incorrect");
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach (var vertex in slicedMesh.vertices)
            {
                minX = Mathf.Min(minX, vertex.x); maxX = Mathf.Max(maxX, vertex.x);
                minY = Mathf.Min(minY, vertex.y); maxY = Mathf.Max(maxY, vertex.y);
            }
            Require(Mathf.Approximately(maxX - minX, 2) && Mathf.Approximately(maxY - minY, 3), "Nine-slice size incorrect");

            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/GameResources/_Gameplay/Shader/Instancing" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                for (int pass = 0; pass < material.passCount; pass++) ShaderUtil.CompilePass(material, pass, true);
                foreach (var message in ShaderUtil.GetShaderMessages(material.shader))
                    Require(message.severity.ToString() != "Error", material.shader.name + ": " + message.message);
            }
            ValidateGpuDraw(sharedMesh);
            Directory.CreateDirectory("Library/GameplayInstancing");
            File.WriteAllText("Library/GameplayInstancing/validation.txt",
                "PASS: 64 instances share one mesh and material; effects are isolated; color/flip/sorting sync; disable/fallback/recovery; nine-slice geometry and size; shader compilation; two-instance GPU draw with independent tint.\n");
            Debug.Log("[GameplayInstancing] Validation passed. Library/GameplayInstancing/validation.txt");
        }
        finally
        {
            foreach (var go in objects)
            {
                Invoke(go.GetComponent<InstancedSpriteProxy>(), "OnDestroy");
                Object.DestroyImmediate(go);
            }
        }
    }

    static void ValidateGpuDraw(Mesh mesh)
    {
        var shader = Shader.Find("Gameplay/Sprite Instanced");
        var material = new Material(shader) { enableInstancing = true };
        var rt = RenderTexture.GetTemporary(64, 32, 0, RenderTextureFormat.ARGB32);
        var pixels = new Texture2D(64, 32, TextureFormat.RGBA32, false);
        var previous = RenderTexture.active;
        var commands = new CommandBuffer { name = "Gameplay GPU instancing validation" };
        try
        {
            material.SetTexture("_MainTex", Texture2D.whiteTexture);
            var block = new MaterialPropertyBlock();
            block.SetVectorArray("_GameplayColor", new[] { new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1) });
            block.SetVectorArray("_GameplayFlip", new[] { Vector4.one, Vector4.one });
            float scale = 0.5f / Mathf.Max(mesh.bounds.extents.x, mesh.bounds.extents.y);
            commands.SetRenderTarget(rt);
            commands.ClearRenderTarget(false, true, Color.black);
            commands.SetViewProjectionMatrices(Matrix4x4.identity, GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-2, 2, -1, 1, -10, 10), true));
            commands.DrawMeshInstanced(mesh, 0, material, 0, new[]
            {
                Matrix4x4.TRS(new Vector3(-1, 0, 0), Quaternion.identity, Vector3.one * scale),
                Matrix4x4.TRS(new Vector3(1, 0, 0), Quaternion.identity, Vector3.one * scale)
            }, 2, block);
            Graphics.ExecuteCommandBuffer(commands);
            RenderTexture.active = rt;
            pixels.ReadPixels(new Rect(0, 0, 64, 32), 0, 0);
            pixels.Apply();
            var left = pixels.GetPixel(16, 16);
            var right = pixels.GetPixel(48, 16);
            Require(left.r > 0.9f && left.g < 0.1f && right.g > 0.9f && right.r < 0.1f, "GPU instanced colors incorrect");
            File.WriteAllBytes("Library/GameplayInstancing/gpu-validation.png", pixels.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            commands.Dispose();
            RenderTexture.ReleaseTemporary(rt);
            Object.DestroyImmediate(pixels);
            Object.DestroyImmediate(material);
        }
    }

    static void Invoke(InstancedSpriteProxy proxy, string method) =>
        typeof(InstancedSpriteProxy).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(proxy, null);

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
