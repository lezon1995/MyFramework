using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MoreMountains;

/// <summary>Repeatable prefab migration; leaves physics, animation and SpriteRenderer references intact.</summary>
public static class GameplayInstancingSetup
{
    const string ShaderFolder = "Assets/GameResources/_Gameplay/Shader/Instancing/";
    const string SsuName = "Sprite Shaders Ultimate/2D Lit URP SSU";
    static readonly string[] Folders = { "Assets/GameResources/_Gameplay/Bricks", "Assets/GameResources/_Gameplay/Balls" };

    [MenuItem("Tools/Gameplay/GPU Instancing/Generate SSU Shader")]
    public static void GenerateShader()
    {
        string text = File.ReadAllText("Assets/Sprite Shaders Ultimate/Shaders/2D Lit URP SSU.shader");
        int footer = text.IndexOf("/*ASEBEGIN", StringComparison.Ordinal);
        if (footer >= 0) text = text.Substring(0, footer);
        text = text.Replace("Shader \"" + SsuName + "\"", "Shader \"Gameplay/SSU Instanced\"");
        text = text.Replace("\tProperties\n\t{", "\tProperties\n\t{\n        _GameplayColor(\"Instance Color\", Color) = (1,1,1,1)\n        _GameplayFlip(\"Instance Flip\", Vector) = (1,1,1,1)");
        text = text.Replace("#pragma target 2.0", "#pragma target 3.5");
        text = text.Replace("#pragma vertex vert", "#pragma multi_compile_instancing\n            #pragma vertex vert");
        var properties = new Dictionary<string, string>
        {
            { "_StrongTintFade", "float" }, { "_FrozenFade", "float" }, { "_PoisonFade", "float" },
            { "_PixelOutlineFade", "float" }, { "_PixelOutlineColor", "float4" },
            { "_GameplayColor", "float4" }, { "_GameplayFlip", "float4" }
        };
        var buffer = new StringBuilder("\n            UNITY_INSTANCING_BUFFER_START(GameplaySprite)\n");
        foreach (var pair in properties)
        {
            text = Regex.Replace(text, @"^[ \t]*" + pair.Value + " " + pair.Key + @";\s*$", "", RegexOptions.Multiline);
            buffer.AppendLine($"                UNITY_DEFINE_INSTANCED_PROP({pair.Value}, {pair.Key})");
        }
        buffer.AppendLine("            UNITY_INSTANCING_BUFFER_END(GameplaySprite)");
        foreach (var pair in properties)
            buffer.AppendLine($"            #define {pair.Key} UNITY_ACCESS_INSTANCED_PROP(GameplaySprite, {pair.Key})");
        text = text.Replace("CBUFFER_END", "CBUFFER_END\n" + buffer);
        text = text.Replace("v.positionOS = UnityFlipSprite( v.positionOS, unity_SpriteProps.xy );",
            "v.positionOS = UnityFlipSprite( v.positionOS, _GameplayFlip.xy );");
        int pass = 0;
        text = Regex.Replace(text, @"UNITY_SETUP_INSTANCE_ID\(v\);", match =>
            match.Value + "\n                v." + (++pass <= 3 ? "color" : "ase_color") + " *= _GameplayColor;");
        text = Regex.Replace(text, @"(half4 frag\(\s*VertexOutput IN\s*\).*?\n\s*\{)(?!\s*UNITY_SETUP_INSTANCE_ID)",
            "$1\n                UNITY_SETUP_INSTANCE_ID(IN);");
        if (pass != 5) throw new InvalidOperationException("SSU shader layout changed; inspect generator before migrating.");
        Directory.CreateDirectory(ShaderFolder);
        File.WriteAllText(ShaderFolder + "GameplaySSUInstanced.shader",
            "// Generated from SSU 2D Lit URP. Regenerate with GameplayInstancingSetup.GenerateShader.\n" + text);
        AssetDatabase.ImportAsset(ShaderFolder + "GameplaySSUInstanced.shader", ImportAssetOptions.ForceSynchronousImport);
    }

    [MenuItem("Tools/Gameplay/GPU Instancing/Apply To Brick And Ball Prefabs")]
    public static void Apply()
    {
        var ssu = AssetDatabase.LoadAssetAtPath<Shader>(ShaderFolder + "GameplaySSUInstanced.shader");
        var simple = AssetDatabase.LoadAssetAtPath<Shader>(ShaderFolder + "GameplaySpriteInstanced.shader");
        if (!ssu || !simple) throw new InvalidOperationException("Instancing shaders have not imported.");
        var report = new StringBuilder("GPU Instancing prefab audit\n");
        var templates = new Dictionary<Material, Material>();
        int added = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", Folders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Weapon/AI prefabs are separate effects with their own blend modes and lifecycle.
            if (Path.GetDirectoryName(path)?.Replace('\\', '/') != Folders[0] &&
                Path.GetDirectoryName(path)?.Replace('\\', '/') != Folders[1]) continue;
            string originalText = File.ReadAllText(path);
            var root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            try
            {
                report.AppendLine(path);
                foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    var material = renderer.sharedMaterial;
                    Shader shader = null;
                    if (material && material.shader && material.shader.name == SsuName) shader = ssu;
                    else if (renderer.name == "SpriteShadow" && material && material.shader.name == "Sprites/Default") shader = simple;
                    else if (material && material.shader && material.shader.name == "Universal Render Pipeline/2D/Sprite-Unlit-Default") shader = simple;
                    bool eligible = shader && renderer.drawMode != SpriteDrawMode.Tiled &&
                        renderer.maskInteraction == SpriteMaskInteraction.None;
                    report.AppendLine($"  {AnimationUtility.CalculateTransformPath(renderer.transform, root.transform)}: " +
                        (eligible ? "instancing proxy" : "original renderer") + $"; shader={material?.shader?.name}; mode={renderer.drawMode}; order={renderer.sortingOrder}");
                    if (!eligible) continue;
                    if (!templates.TryGetValue(material, out var template))
                    {
                        string templatePath = ShaderFolder + material.name + "_Instanced.mat";
                        template = AssetDatabase.LoadAssetAtPath<Material>(templatePath);
                        if (!template)
                        {
                            template = new Material(material) { shader = shader, enableInstancing = true };
                            AssetDatabase.CreateAsset(template, templatePath);
                        }
                        templates.Add(material, template);
                    }
                    var proxy = renderer.GetComponent<InstancedSpriteProxy>();
                    if (!proxy) { proxy = renderer.gameObject.AddComponent<InstancedSpriteProxy>(); added++; changed = true; }
                    if (proxy.instancedShader != shader) { proxy.instancedShader = shader; changed = true; }
                    if (proxy.instancedMaterial != template) { proxy.instancedMaterial = template; changed = true; }
                }
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    // Avoid unrelated Unity-version/OnValidate serialization changes in gameplay data.
                    string savedText = File.ReadAllText(path);
                    File.WriteAllText(path, PreserveOriginalSerialization(originalText, savedText));
                    AssetDatabase.ImportAsset(path);
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        Directory.CreateDirectory("Library/GameplayInstancing");
        File.WriteAllText("Library/GameplayInstancing/prefab-audit.txt", report.ToString());
        Debug.Log($"[GameplayInstancing] Added {added} proxies. Audit: Library/GameplayInstancing/prefab-audit.txt");
    }

    static string PreserveOriginalSerialization(string original, string saved)
    {
        string scriptGuid = AssetDatabase.AssetPathToGUID("Assets/Scripts/Game/Rendering/InstancedSpriteProxy.cs");
        foreach (Match match in Regex.Matches(saved, @"--- !u!114 &(-?\d+)\n.*?(?=--- !u!|\z)", RegexOptions.Singleline))
        {
            if (!match.Value.Contains("guid: " + scriptGuid)) continue;
            string proxyText = Regex.Replace(match.Value, @"[ \t]+$", "", RegexOptions.Multiline);
            string id = match.Groups[1].Value;
            string go = Regex.Match(match.Value, @"m_GameObject: \{fileID: (-?\d+)\}").Groups[1].Value;
            string existingPattern = @"--- !u!114 &" + id + @"\n.*?(?=--- !u!|\z)";
            if (Regex.IsMatch(original, existingPattern, RegexOptions.Singleline))
                original = Regex.Replace(original, existingPattern, _ => proxyText, RegexOptions.Singleline);
            else
            {
                original = Regex.Replace(original, @"--- !u!1 &" + go + @"\n.*?(?=--- !u!|\z)", obj =>
                    Regex.Replace(obj.Value, @"(  m_Component:\n(?:  - component: [^\n]+\n)+)",
                        list => list.Value + "  - component: {fileID: " + id + "}\n"), RegexOptions.Singleline);
                original += proxyText;
            }
        }
        return original;
    }
}
