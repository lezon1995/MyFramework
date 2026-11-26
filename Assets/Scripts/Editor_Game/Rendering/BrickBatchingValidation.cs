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

/// <summary>Regression checks for stage ordering, many distinct TMP labels and native sprites and shared SSU effect states.</summary>
public static class BrickBatchingValidation
{
    [MenuItem("Tools/Gameplay/Sprite Batching/Validate Brick Batching")]
    public static void Run()
    {
        ValidateSharedEffects();
        ValidateNativePrefabs();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/_Gameplay/Bricks/Brick_1x1.prefab");
        TextMeshPro labelTemplate = null;
        SpriteRenderer healthTemplate = null;
        foreach (var text in prefab.GetComponentsInChildren<TextMeshPro>(true))
            if (text.name == "Health") labelTemplate = text;
        foreach (var sprite in prefab.GetComponentsInChildren<SpriteRenderer>(true))
            if (sprite.name == "HealthBarRenderer") healthTemplate = sprite;
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
        WorldTextBatchRenderer manager = null;
        try
        {
            for (int i = 0; i < 64; i++)
            {
                var root = new GameObject("Brick batching regression " + i) { hideFlags = HideFlags.HideAndDontSave };
                objects.Add(root);
                root.transform.position = new Vector3((i % 8) * 2, (i / 8) * 2, 0);
                var visualRoot = new GameObject("Root").transform;
                visualRoot.SetParent(root.transform, false);
                var sorting = root.AddComponent<SortingGroup>();
                sorting.sortingLayerName = "Brick";
                sorting.sortingOrder = i;
                var barObject = new GameObject("HealthBarRenderer");
                barObject.transform.SetParent(visualRoot, false);
                var sprite = barObject.AddComponent<SpriteRenderer>();
                var sourceTemplate = healthTemplate;
                sprite.sprite = sourceTemplate.sprite;
                sprite.drawMode = sourceTemplate.drawMode;
                sprite.size = sourceTemplate.size;
                sprite.sharedMaterial = sourceTemplate.sharedMaterial;
                var block = new MaterialPropertyBlock();
                block.SetFloat("_ForegroundProgress", (i + 1f) / 64);
                sprite.SetPropertyBlock(block);
                sources.Add(sprite);

                var labelObject = Object.Instantiate(labelTemplate.gameObject, visualRoot);
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
                Require(Mathf.Approximately(visualRoot.position.z, -i * 0.0001f), "Row depth priority was lost");
                Invoke(label, "OnEnable");
                labels.Add(label);
                Require(!sorting.enabled && sprite.sortingOrder == 60 && text.sortingOrder == 80, "Per-brick sorting still splits stages");
            }
            manager = Resources.FindObjectsOfTypeAll<WorldTextBatchRenderer>()[0];
            manager.Rebuild();
            Require(manager.VisibleLabelCount == 64 && manager.ActiveBatchCount == 1,
                $"64 different health labels produced {manager.ActiveBatchCount} batches / {manager.VisibleLabelCount} labels");
            foreach (var sprite in sources)
                Require(!sprite.forceRenderingOff && sprite.GetComponentsInChildren<MeshRenderer>().Length == 0 &&
                    sprite.sharedMaterial == healthTemplate.sharedMaterial && sprite.sortingOrder == 60,
                    "Health bar no longer uses native SpriteRenderer");
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


            Directory.CreateDirectory("Library/GameplaySpriteBatching");
            File.WriteAllText("Library/GameplaySpriteBatching/brick-batching-validation.txt",
                "PASS: 15 prefabs use shared visual stages; 64 distinct TMP health labels combine into ONE mesh/material draw; " +
                "64 native health SpriteRenderers share material/stage; dynamic text, alpha, transform, override fallback and pooling; " +
                "equal SSU effect states share materials, independent states remain isolated and disposal restores materials; " +
                "combined TMP rendering matches separate TMP draws by pixel comparison.\n");
            Debug.Log("[SpriteBatching] Brick batching regression passed: 64 health labels => 1 text mesh draw; native sprites and shared SSU states passed.");
        }
        finally
        {
            foreach (var label in labels) Invoke(label, "OnDisable");
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
            File.WriteAllBytes("Library/GameplaySpriteBatching/text-separate.png", reference.EncodeToPNG());
            File.WriteAllBytes("Library/GameplaySpriteBatching/text-combined.png", combined.EncodeToPNG());
            Debug.Log($"[SpriteBatching] TMP pixel comparison: {lit} visible pixels, {different} different pixels.");
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

    static void ValidateNativePrefabs()
    {
        int count = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", NativeSpriteBatchingSetup.Folders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Array.IndexOf(NativeSpriteBatchingSetup.Folders, Path.GetDirectoryName(path)?.Replace('\\', '/')) < 0) continue;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var sprite in prefab.GetComponentsInChildren<SpriteRenderer>(true))
            {
                Require(!sprite.forceRenderingOff && sprite.GetComponents<MeshRenderer>().Length == 0, "Sprite proxy still present: " + path);
                Require(sprite.sharedMaterial && !sprite.sharedMaterial.shader.name.StartsWith("Gameplay/"), "Proxy shader remains");
                count++;
            }
            foreach (var script in prefab.GetComponentsInChildren<MonoBehaviour>(true)) Require(script, "Missing script: " + path);
        }
        Require(count == 145, "Expected 145 native sprite renderers, got " + count);
    }

    static void ValidateSharedEffects()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/_Gameplay/Bricks/Brick_1x1.prefab");
        Material original = null;
        foreach (var sprite in prefab.GetComponentsInChildren<SpriteRenderer>(true))
            if (sprite.name == "SpriteBlock") original = sprite.sharedMaterial;
        Require(original, "Missing SSU template");
        var a = new GameObject("Shared material A") { hideFlags = HideFlags.HideAndDontSave };
        var b = new GameObject("Shared material B") { hideFlags = HideFlags.HideAndDontSave };
        var left = a.AddComponent<SpriteRenderer>();
        var right = b.AddComponent<SpriteRenderer>();
        left.sharedMaterial = right.sharedMaterial = original;
        var first = new RendererPropertyOverrides(left);
        var second = new RendererPropertyOverrides(right);
        int frozen = Shader.PropertyToID("_FrozenFade"), poison = Shader.PropertyToID("_PoisonFade");
        try
        {
            first.SetFloat(frozen, 1);
            second.SetFloat(frozen, 1);
            Require(left.sharedMaterial == right.sharedMaterial && left.sharedMaterial != original, "Equal effects failed to share");
            second.SetFloat(poison, 1);
            Require(left.sharedMaterial != right.sharedMaterial && left.sharedMaterial.GetFloat(poison) == original.GetFloat(poison), "Effects leaked");
            second.SetFloat(poison, original.GetFloat(poison));
            Require(left.sharedMaterial == right.sharedMaterial, "Effects failed to rejoin");
            first.SetFloat(frozen, original.GetFloat(frozen));
            Require(left.sharedMaterial == original && right.sharedMaterial.GetFloat(frozen) == 1, "Baseline or isolation failed");
            var block = new MaterialPropertyBlock();
            right.GetPropertyBlock(block);
            Require(block.isEmpty, "Custom effect property block still present");
            second.Dispose();
            Require(right.sharedMaterial == original, "Material disposal failed");
            Require(original.GetFloat(frozen) != 1, "Source material was mutated");
        }
        finally
        {
            first.Dispose();
            second.Dispose();
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }
    }

    static void Invoke(Object component, string method) => component.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
