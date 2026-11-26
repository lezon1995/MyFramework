using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoreMountains;

public static class DropInstancingSetup
{
    public const string CoinPath = "Assets/GameResources/_Gameplay/Coin/Coin_0.prefab";
    public const string ExpPath = "Assets/GameResources/_Gameplay/Exp/Experence.prefab";
    public const string CoinMaterialPath = "Assets/GameResources/_Gameplay/Coin/TX_Coin_Instanced.mat";
    public const string ExpMaterialPath = "Assets/GameResources/_Gameplay/Exp/Exp_Instanced.mat";

    [MenuItem("Tools/Gameplay/Drop GPU Instancing/Apply")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Apply drop configuration outside Play mode.");
        var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/GameResources/_Gameplay/Shader/DropInstanced.shader");
        if (!shader || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Drop instancing shader has not compiled.");
        ApplyPrefab(CoinPath, CoinMaterialPath, shader, true);
        ApplyPrefab(ExpPath, ExpMaterialPath, shader, false);
        Debug.Log("[DropInstancing] Configured coin and experience prefabs; gameplay components and serialized references preserved.");
    }

    static void ApplyPrefab(string path, string materialPath, Shader shader, bool coin)
    {
        string original = File.ReadAllText(path);
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var sprite = root.GetComponentInChildren<SpriteRenderer>(true);
            var originalMaterial = sprite.sharedMaterial;
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = shader;
            material.enableInstancing = true;
            material.SetTexture("_MainTex", sprite.sprite.texture);
            material.SetFloat("_ColumnsX", coin ? originalMaterial.GetFloat("_ColumnsX") : 1);
            material.SetFloat("_RowsY", coin ? originalMaterial.GetFloat("_RowsY") : 1);
            material.SetFloat("_AnimationSpeed", coin ? originalMaterial.GetFloat("_AnimationSpeed") : 0);
            material.SetFloat("_Cutoff", coin ? originalMaterial.GetFloat("_Cutoff") : -1);
            // The original coin cutout shader ignores sprite tint/alpha. Preserve its appearance.
            material.SetFloat("_UseVertexColor", coin ? 0 : 1);
            material.SetFloat("_SrcBlend", (float)BlendMode.One);
            material.SetFloat("_DstBlend", (float)(coin ? BlendMode.Zero : BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_ZWrite", coin ? 1 : 0);
            material.renderQueue = originalMaterial.renderQueue;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            var component = sprite.GetComponent<DropInstancedSprite>();
            if (!component) component = sprite.gameObject.AddComponent<DropInstancedSprite>();
            component.sourceMaterial = originalMaterial;
            component.instancedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(root, path);
            string saved = File.ReadAllText(path);
            string guid = AssetDatabase.AssetPathToGUID("Assets/Scripts/Game/Rendering/DropInstancedSprite.cs");
            foreach (Match match in Regex.Matches(saved, @"--- !u!114 &(-?\d+)\n.*?(?=--- !u!|\z)", RegexOptions.Singleline))
            {
                if (!match.Value.Contains("guid: " + guid)) continue;
                string componentText = Regex.Replace(match.Value, @"[ \t]+$", "", RegexOptions.Multiline);
                string id = match.Groups[1].Value;
                string go = Regex.Match(match.Value, @"m_GameObject: \{fileID: (-?\d+)\}").Groups[1].Value;
                string pattern = @"--- !u!114 &" + id + @"\n.*?(?=--- !u!|\z)";
                if (Regex.IsMatch(original, pattern, RegexOptions.Singleline))
                    original = Regex.Replace(original, pattern, _ => componentText, RegexOptions.Singleline);
                else
                {
                    original = Regex.Replace(original, @"--- !u!1 &" + go + @"\n.*?(?=--- !u!|\z)", obj =>
                        Regex.Replace(obj.Value, @"(  m_Component:\n(?:  - component: [^\n]+\n)+)",
                            list => list.Value + "  - component: {fileID: " + id + "}\n"), RegexOptions.Singleline);
                    original += componentText;
                }
            }
            File.WriteAllText(path, original);
            AssetDatabase.ImportAsset(path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
