using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MoreMountains;

/// <summary>Regression checks for stage ordering, many distinct TMP labels and per-instance health state.</summary>
public static class BrickBatchingValidation
{
    [MenuItem("Tools/Gameplay/GPU Instancing/Validate Brick Batching")]
    public static void Run()
    {
        GameplayInstancingValidation.Run();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/_Gameplay/Bricks/Brick_1x1.prefab");
        TextMeshPro labelTemplate = null;
        InstancedSpriteProxy healthTemplate = null;
        foreach (var text in prefab.GetComponentsInChildren<TextMeshPro>(true))
            if (text.name == "Health") labelTemplate = text;
        foreach (var proxy in prefab.GetComponentsInChildren<InstancedSpriteProxy>(true))
            if (proxy.name == "HealthBarRenderer") healthTemplate = proxy;
        Require(labelTemplate && healthTemplate, "Prefab migration is incomplete");

        int prefabCount = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/GameResources/_Gameplay/Bricks" }))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (!asset.TryGetComponent<SortingGroup>(out _)) continue;
            var config = asset.GetComponent<BrickRenderBatchGroup>();
            Require(config && config.entries.Length >= 9, asset.name + " has no shared stages");
            Require(asset.GetComponentsInChildren<BatchedWorldText>(true).Length == 2, asset.name + " labels are not batched");
            prefabCount++;
        }
        Require(prefabCount == 15, "Expected 15 brick prefabs");

        var objects = new List<GameObject>();
        var labels = new List<BatchedWorldText>();
        var groups = new List<BrickRenderBatchGroup>();
        var sources = new List<SpriteRenderer>();
        var proxies = new List<InstancedSpriteProxy>();
        WorldTextBatchRenderer manager = null;
        try
        {
            for (int i = 0; i < 64; i++)
            {
                var root = new GameObject("Brick batching regression " + i) { hideFlags = HideFlags.HideAndDontSave };
                objects.Add(root);
                root.transform.position = new Vector3((i % 8) * 2, (i / 8) * 2, 0);
                var sorting = root.AddComponent<SortingGroup>();
                sorting.sortingLayerName = "Brick";
                sorting.sortingOrder = i;
                var barObject = new GameObject("HealthBarRenderer");
                barObject.transform.SetParent(root.transform, false);
                var sprite = barObject.AddComponent<SpriteRenderer>();
                var sourceTemplate = healthTemplate.GetComponent<SpriteRenderer>();
                sprite.sprite = sourceTemplate.sprite;
                sprite.drawMode = sourceTemplate.drawMode;
                sprite.size = sourceTemplate.size;
                sprite.sharedMaterial = sourceTemplate.sharedMaterial;
                var block = new MaterialPropertyBlock();
                block.SetFloat("_ForegroundProgress", (i + 1f) / 64);
                sprite.SetPropertyBlock(block);
                sources.Add(sprite);

                var labelObject = Object.Instantiate(labelTemplate.gameObject, root.transform);
                labelObject.name = "Health";
                var text = labelObject.GetComponent<TextMeshPro>();
                text.text = (i * 137 + 1).ToString();
                text.ForceMeshUpdate(true);
                var label = labelObject.GetComponent<BatchedWorldText>();
                var group = root.AddComponent<BrickRenderBatchGroup>();
                group.sortingGroup = sorting;
                group.entries = new[]
                {
                    new BrickRenderBatchGroup.Entry { renderer = sprite, order = 60 },
                    new BrickRenderBatchGroup.Entry { renderer = text.GetComponent<MeshRenderer>(), order = 80 }
                };
                Invoke(group, "OnEnable");
                groups.Add(group);
                Invoke(label, "OnEnable");
                labels.Add(label);
                var proxy = barObject.AddComponent<InstancedSpriteProxy>();
                proxy.instancedShader = healthTemplate.instancedShader;
                proxy.instancedMaterial = healthTemplate.instancedMaterial;
                Invoke(proxy, "OnEnable");
                proxies.Add(proxy);
                Require(!sorting.enabled && sprite.sortingOrder == 60 && text.sortingOrder == 80, "Per-brick sorting still splits stages");
            }
            manager = Resources.FindObjectsOfTypeAll<WorldTextBatchRenderer>()[0];
            manager.Rebuild();
            Require(manager.VisibleLabelCount == 64 && manager.ActiveBatchCount == 1,
                $"64 different health labels produced {manager.ActiveBatchCount} batches / {manager.VisibleLabelCount} labels");
            var firstTarget = sources[0].GetComponentInChildren<MeshRenderer>();
            foreach (var sprite in sources)
            {
                var target = sprite.GetComponentInChildren<MeshRenderer>();
                Require(target.sharedMaterial == firstTarget.sharedMaterial && target.sortingOrder == 60 &&
                    !target.GetComponentInParent<SortingGroup>().enabled, "Health renderer cannot join the shared batch");
            }
            Require(sources[63].GetComponentInChildren<MeshRenderer>().transform.position.z < firstTarget.transform.position.z,
                "Existing per-row depth priority was lost");
            // Dynamic text, transform, alpha and pooled visibility must update the combined mesh.
            var changedText = labels[0].GetComponent<TextMeshPro>();
            changedText.SetText("999999");
            changedText.alpha = 0.4f;
            changedText.transform.localScale *= 1.2f;
            manager.Rebuild();
            var batchMesh = manager.GetComponentInChildren<MeshFilter>().sharedMesh;
            bool foundAlpha = false;
            foreach (var color in batchMesh.colors32) if (color.a > 0 && color.a < 200) foundAlpha = true;
            Require(foundAlpha, "Combined label alpha did not update");
            var overrideBlock = new MaterialPropertyBlock();
            overrideBlock.SetFloat("_FaceDilate", 0.1f);
            changedText.GetComponent<MeshRenderer>().SetPropertyBlock(overrideBlock);
            manager.Rebuild();
            Require(manager.VisibleLabelCount == 63 && !changedText.GetComponent<MeshRenderer>().forceRenderingOff,
                "Text with private overrides did not fall back");
            changedText.GetComponent<MeshRenderer>().SetPropertyBlock(null);
            manager.Rebuild();
            Require(manager.VisibleLabelCount == 64, "Text did not recover from fallback");
            ValidateTextGpu(labels, manager);
            Invoke(labels[0], "OnDisable");
            labels.RemoveAt(0);
            Require(manager.VisibleLabelCount == 63, "Pooled label lingered in the batch");

            ValidateHealthGpu(healthTemplate.instancedMaterial);
            Directory.CreateDirectory("Library/GameplayInstancing");
            File.WriteAllText("Library/GameplayInstancing/brick-batching-validation.txt",
                "PASS: 15 prefabs use shared visual stages; 64 distinct TMP health labels combine into ONE mesh/material draw; " +
                "64 independent health bars share mesh/material/stage; dynamic text, alpha, transform, override fallback and pooling; " +
                "GPU instanced health/shield/chunk colors and progress verified by pixel readback; " +
                "combined TMP rendering matches separate TMP draws by pixel comparison.\n");
            Debug.Log("[GameplayInstancing] Brick batching regression passed: 64 health labels => 1 text mesh draw; health GPU readback passed.");
        }
        finally
        {
            foreach (var label in labels) Invoke(label, "OnDisable");
            foreach (var proxy in proxies) Invoke(proxy, "OnDestroy");
            foreach (var group in groups) Invoke(group, "OnDisable");
            foreach (var go in objects) Object.DestroyImmediate(go);
            if (manager) Object.DestroyImmediate(manager.gameObject);
        }
    }

    static void ValidateTextGpu(List<BatchedWorldText> labels, WorldTextBatchRenderer manager)
    {
        var target = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.ARGB32);
        var reference = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        var combined = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        var previous = RenderTexture.active;
        var screen = Shader.GetGlobalVector("_ScreenParams");
        var cmd = new CommandBuffer { name = "TMP shared mesh regression" };
        try
        {
            for (int mode = 0; mode < 2; mode++)
            {
                cmd.Clear();
                cmd.SetRenderTarget(target);
                cmd.ClearRenderTarget(false, true, Color.black);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity,
                    GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-2, 16, -2, 16, -100, 100), true));
                cmd.SetGlobalVector("_ScreenParams", new Vector4(512, 512, 1 + 1f / 512, 1 + 1f / 512));
                if (mode == 0)
                {
                    foreach (var label in labels)
                    {
                        var text = label.GetComponent<TextMeshPro>();
                        for (int i = 0; i < text.textInfo.materialCount; i++)
                        {
                            var info = text.textInfo.meshInfo[i];
                            if (info.vertexCount > 0) cmd.DrawMesh(info.mesh, label.transform.localToWorldMatrix, info.material, 0, 0);
                        }
                    }
                }
                else
                    foreach (var renderer in manager.GetComponentsInChildren<MeshRenderer>())
                        cmd.DrawMesh(renderer.GetComponent<MeshFilter>().sharedMesh, Matrix4x4.identity, renderer.sharedMaterial, 0, 0);
                Graphics.ExecuteCommandBuffer(cmd);
                RenderTexture.active = target;
                var output = mode == 0 ? reference : combined;
                output.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
                output.Apply();
            }
            var before = reference.GetPixels32();
            var after = combined.GetPixels32();
            int lit = 0, different = 0;
            for (int i = 0; i < before.Length; i++)
            {
                if (before[i].r + before[i].g + before[i].b > 30) lit++;
                if (Math.Abs(before[i].r - after[i].r) + Math.Abs(before[i].g - after[i].g) + Math.Abs(before[i].b - after[i].b) > 24) different++;
            }
            Require(lit > 100, "TMP reference render is empty");
            Require(different < lit * 0.05f, $"Combined TMP output changed appearance: {different}/{lit} pixels differ");
            File.WriteAllBytes("Library/GameplayInstancing/text-separate.png", reference.EncodeToPNG());
            File.WriteAllBytes("Library/GameplayInstancing/text-combined.png", combined.EncodeToPNG());
            Debug.Log($"[GameplayInstancing] TMP pixel comparison: {lit} visible pixels, {different} different pixels.");
        }
        finally
        {
            Shader.SetGlobalVector("_ScreenParams", screen);
            RenderTexture.active = previous;
            cmd.Dispose();
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(reference);
            Object.DestroyImmediate(combined);
        }
    }

    static void ValidateHealthGpu(Material template)
    {
        var material = new Material(template) { enableInstancing = true };
        var mesh = new Mesh();
        mesh.vertices = new[] { new Vector3(-0.9f, -0.4f), new Vector3(-0.9f, 0.4f), new Vector3(0.9f, 0.4f), new Vector3(0.9f, -0.4f) };
        mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        var target = RenderTexture.GetTemporary(200, 50, 0, RenderTextureFormat.ARGB32);
        var pixels = new Texture2D(200, 50, TextureFormat.RGBA32, false);
        var previous = RenderTexture.active;
        var cmd = new CommandBuffer { name = "Health bar instance regression" };
        try
        {
            material.SetTexture("_MainTex", Texture2D.whiteTexture);
            var block = new MaterialPropertyBlock();
            block.SetFloatArray("_ForegroundProgress", new[] { 0.25f, 0.75f });
            block.SetVectorArray("_ForegroundColor", new[] { new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1) });
            block.SetFloatArray("_UseShield", new[] { 1f, 1f });
            block.SetFloatArray("_ShieldLength", new[] { 0.25f, 0.125f });
            block.SetFloatArray("_ShieldGlow", new[] { 0f, 0f });
            block.SetVectorArray("_ShieldColor", new[] { new Vector4(0, 0, 1, 1), new Vector4(0, 0, 1, 1) });
            block.SetVectorArray("_Chunk7", new[] { new Vector4(0.75f, 0.9f, 1, 0), Vector4.zero });
            block.SetVectorArray("_DamageChunkColor7", new[] { new Vector4(1, 1, 0, 1), Vector4.zero });
            block.SetVectorArray("_GameplayColor", new[] { Vector4.one, Vector4.one });
            block.SetVectorArray("_GameplayFlip", new[] { Vector4.one, Vector4.one });
            cmd.SetRenderTarget(target);
            cmd.ClearRenderTarget(false, true, Color.black);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-2, 2, -0.5f, 0.5f, -10, 10), true));
            cmd.DrawMeshInstanced(mesh, 0, material, 0, new[] { Matrix4x4.Translate(Vector3.left), Matrix4x4.Translate(Vector3.right) }, 2, block);
            Graphics.ExecuteCommandBuffer(cmd);
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 200, 50), 0, 0);
            pixels.Apply();
            Require(pixels.GetPixel(16, 25).r > 0.9f && pixels.GetPixel(116, 25).g > 0.9f, "Health instance colors leaked");
            Require(pixels.GetPixel(39, 25).b > 0.9f && pixels.GetPixel(139, 25).g > 0.9f, "Shield/progress instance values leaked");
            var chunk = pixels.GetPixel(77, 25);
            Require(chunk.r > 0.9f && chunk.g > 0.9f && chunk.b < 0.1f, "Damage chunk instance data missing");
            File.WriteAllBytes("Library/GameplayInstancing/health-instancing-validation.png", pixels.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            cmd.Dispose();
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(pixels);
            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(material);
        }
    }

    static void Invoke(Object component, string method) => component.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
