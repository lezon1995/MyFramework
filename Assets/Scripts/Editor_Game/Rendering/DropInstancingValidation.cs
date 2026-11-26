using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreMountains;

/// <summary>Checks actual GPU output as well as shared renderer counts, pooling and fallback behavior.</summary>
public static class DropInstancingValidation
{
    const string Output = "Library/DropInstancing";
    const int TestLayer = 31;
    static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [MenuItem("Tools/Gameplay/Drop GPU Instancing/Validate")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run validation outside Play mode.");
        Directory.CreateDirectory(Output);
        Require(SystemInfo.supportsInstancing, "This device cannot validate GPU instancing");
        var shader = Shader.Find("Gameplay/Drop Mesh Particle Instanced");
        Require(shader && shader.isSupported && !ShaderUtil.ShaderHasError(shader), "Instanced shader is invalid");
        var objects = new List<GameObject>();
        var drops = new List<DropInstancedSprite>();
        var materials = new List<Material>();
        var report = new StringBuilder();
        DropInstanceRenderer manager = null;
        Camera camera = null;
        RenderTexture target = null;
        try
        {
            var camObject = new GameObject("Drop validation camera") { hideFlags = HideFlags.HideAndDontSave };
            objects.Add(camObject);
            camera = camObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 3;
            camera.aspect = 1;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 30;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 1 << TestLayer;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            target = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;
            target.Create();

            for (int kind = 0; kind < 2; kind++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(kind == 0 ? DropInstancingSetup.CoinPath : DropInstancingSetup.ExpPath);
                var config = prefab.GetComponentInChildren<DropInstancedSprite>(true);
                var template = prefab.GetComponentInChildren<SpriteRenderer>(true);
                Require(config && config.sourceMaterial == template.sharedMaterial && config.instancedMaterial.enableInstancing,
                    "Prefab has not been configured");
                Require(prefab.GetComponentsInChildren<MeshRenderer>(true).Length == 0, "Per-drop mesh proxy found");
                var native = new Material(config.sourceMaterial) { hideFlags = HideFlags.HideAndDontSave };
                var instanced = new Material(config.instancedMaterial) { hideFlags = HideFlags.HideAndDontSave, enableInstancing = true };
                materials.Add(native); materials.Add(instanced);
                // Freeze the flipbook so two camera renders cannot straddle an animation frame.
                if (native.HasProperty("_AnimationSpeed")) native.SetFloat("_AnimationSpeed", 0);
                instanced.SetFloat("_AnimationSpeed", 0);
                for (int i = 0; i < 32; i++)
                {
                    var go = new GameObject("Drop regression " + kind + "/" + i) { hideFlags = HideFlags.HideAndDontSave, layer = TestLayer };
                    objects.Add(go);
                    go.transform.position = new Vector3(-2.6f + (i % 8) * 0.74f, -2.5f + (kind * 4 + i / 8) * 0.7f, 0);
                    go.transform.localScale = new Vector3(0.18f + (i % 3) * 0.01f, 0.16f + (i % 4) * 0.02f, 1);
                    go.transform.rotation = Quaternion.Euler(0, 0, i * 11);
                    var sprite = go.AddComponent<SpriteRenderer>();
                    sprite.sprite = template.sprite;
                    sprite.sharedMaterial = native;
                    sprite.sortingLayerID = template.sortingLayerID;
                    sprite.sortingOrder = template.sortingOrder;
                    // White first, then verify per-instance tint and alpha using a second capture.
                    sprite.color = Color.white;
                    var drop = go.AddComponent<DropInstancedSprite>();
                    drop.sourceMaterial = native;
                    drop.instancedMaterial = instanced;
                    drops.Add(drop);
                }
            }
            var before = Capture(camera);
            foreach (var drop in drops) Invoke(drop, "OnEnable");
            manager = Resources.FindObjectsOfTypeAll<DropInstanceRenderer>()[0];
            Require(manager, "No centralized drop renderer");
            manager.Synchronize();
            Require(manager.VisibleInstanceCount == 64 && manager.ActiveBatchCount == 2,
                $"Expected 64 instances / 2 batches; got {manager.VisibleInstanceCount} / {manager.ActiveBatchCount}");
            foreach (var renderer in manager.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                Require(renderer.enableGPUInstancing && renderer.supportsMeshInstancing, "Particle renderer cannot instance");
                Require(renderer.sortingLayerName == "Level" && renderer.sortingOrder == 0, "Sorting layer changed");
            }
            var after = Capture(camera);
            Compare(before, after, "native-vs-instanced", report);
            Object.DestroyImmediate(before); Object.DestroyImmediate(after);

            // Changing alpha, tint, transform and flip must still match native rendering.
            for (int i = 32; i < drops.Count; i++)
            {
                var sprite = drops[i].GetComponent<SpriteRenderer>();
                sprite.color = new Color(0.3f + (i % 3) * 0.3f, 0.7f, 0.5f, 0.25f + (i % 4) * 0.2f);
                sprite.flipX = i % 2 == 0;
                sprite.flipY = i % 3 == 0;
                sprite.transform.localScale *= 0.8f;
                sprite.transform.Rotate(0, 0, 17);
            }
            // Toggle original renderers and batch renderers only for the reference capture.
            SetNativeVisible(drops, manager, true);
            before = Capture(camera);
            SetNativeVisible(drops, manager, false);
            manager.Synchronize();
            after = Capture(camera);
            Compare(before, after, "animated-tint-alpha-flip", report);
            Object.DestroyImmediate(before); Object.DestroyImmediate(after);

            var source = drops[32].GetComponent<SpriteRenderer>();
            var block = new MaterialPropertyBlock(); block.SetColor("_Color", Color.red);
            source.SetPropertyBlock(block);
            manager.Synchronize();
            Require(manager.VisibleInstanceCount == 63 && !source.forceRenderingOff, "Private overrides failed to fall back");
            source.SetPropertyBlock(null);
            manager.Synchronize();
            Require(manager.VisibleInstanceCount == 64 && source.forceRenderingOff, $"Fallback failed to recover: instances={manager.VisibleInstanceCount}, hidden={source.forceRenderingOff}, overrides={source.HasPropertyBlock()}");
            source.enabled = false;
            manager.Synchronize();
            Require(manager.VisibleInstanceCount == 63, "Disabled renderer left a ghost instance");
            source.enabled = true;
            manager.Synchronize();
            Invoke(drops[0], "OnDisable");
            Require(manager.VisibleInstanceCount == 63, "Pooling did not remove the instance immediately");
            Require(!drops[0].GetComponent<SpriteRenderer>().forceRenderingOff, "Pooling did not restore renderer state");
            Invoke(drops[0], "OnEnable");
            manager.Synchronize();
            Require(manager.VisibleInstanceCount == 64, "Reacquired instance missing");

            // Go beyond DrawMeshInstanced's usual 1023 limit: particle instancing owns its own buffer.
            for (int i = 0; i < 1100; i++)
            {
                var go = new GameObject("Drop capacity regression") { hideFlags = HideFlags.HideAndDontSave, layer = TestLayer };
                objects.Add(go);
                go.transform.position = new Vector3(100 + i, 100, 0);
                var sprite = go.AddComponent<SpriteRenderer>();
                var template = drops[0].GetComponent<SpriteRenderer>();
                sprite.sprite = template.sprite; sprite.sharedMaterial = template.sharedMaterial;
                sprite.sortingLayerID = template.sortingLayerID;
                var drop = go.AddComponent<DropInstancedSprite>();
                drop.sourceMaterial = drops[0].sourceMaterial;
                drop.instancedMaterial = drops[0].instancedMaterial;
                drops.Add(drop);
                Invoke(drop, "OnEnable");
            }
            manager.Synchronize();
            Require(manager.VisibleInstanceCount == 1164, "More than 1023 instances were truncated");
            int particles = 0;
            foreach (var system in manager.GetComponentsInChildren<ParticleSystem>()) particles += system.particleCount;
            Require(particles == 1164, "GPU particle buffer count mismatch");
            report.AppendLine("PASS: 64 drops => 2 mesh-particle GPU batches; 1164-instance capacity; transform/tint/alpha/flip; fallback/recovery; pooling.");
            foreach (var drop in drops) Invoke(drop, "OnDisable");
            Require(manager.VisibleInstanceCount == 0 && manager.GetComponentsInChildren<ParticleSystemRenderer>().Length == 0,
                "All-pooled batch resources were not released");
            File.WriteAllText(Output + "/validation.txt", report.ToString());
            Debug.Log("[DropInstancing] " + report);
        }
        finally
        {
            foreach (var drop in drops) if (drop) Invoke(drop, "OnDisable");
            foreach (var obj in objects) if (obj) Object.DestroyImmediate(obj);
            foreach (var material in materials) Object.DestroyImmediate(material);
            if (manager) Object.DestroyImmediate(manager.gameObject);
            if (target) { target.Release(); Object.DestroyImmediate(target); }
        }
    }

    static void SetNativeVisible(List<DropInstancedSprite> drops, DropInstanceRenderer manager, bool native)
    {
        foreach (var drop in drops) drop.GetComponent<SpriteRenderer>().forceRenderingOff = !native;
        foreach (var renderer in manager.GetComponentsInChildren<ParticleSystemRenderer>()) renderer.enabled = !native;
    }

    static Texture2D Capture(Camera camera)
    {
        camera.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;
        var pixels = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        pixels.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        pixels.Apply();
        RenderTexture.active = previous;
        return pixels;
    }

    static void Compare(Texture2D before, Texture2D after, string name, StringBuilder report)
    {
        File.WriteAllBytes(Output + "/" + name + "-native.png", before.EncodeToPNG());
        File.WriteAllBytes(Output + "/" + name + "-instanced.png", after.EncodeToPNG());
        var a = before.GetPixels32(); var b = after.GetPixels32();
        int visible = 0, different = 0;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].r + a[i].g + a[i].b > 30) visible++;
            if (Math.Abs(a[i].r - b[i].r) + Math.Abs(a[i].g - b[i].g) + Math.Abs(a[i].b - b[i].b) > 30) different++;
        }
        report.AppendLine($"{name}: {visible} visible pixels, {different} different pixels.");
        Debug.Log($"[DropInstancing] {name}: {visible} visible pixels, {different} different pixels.");
        Require(visible > 100, "Reference capture is empty");
        Require(different < visible * 0.05f, $"{name}: native and GPU rendering disagree ({different}/{visible})");
    }

    static void Invoke(Object obj, string method) => obj.GetType().GetMethod(method, PrivateInstance).Invoke(obj, null);
    static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
