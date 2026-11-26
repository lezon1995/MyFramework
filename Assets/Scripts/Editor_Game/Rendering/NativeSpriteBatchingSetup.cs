using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace MoreMountains;

/// <summary>Audits native sprite material/atlas coverage, including sprite animation frames.</summary>
public static class NativeSpriteBatchingSetup
{
    internal static readonly string[] Folders = { "Assets/GameResources/_Gameplay/Bricks", "Assets/GameResources/_Gameplay/Balls" };

    [MenuItem("Tools/Gameplay/Sprite Batching/Audit And Complete Atlas Coverage")]
    public static void Apply()
    {
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/GameResources/_Gameplay/Sprites/Sprite_Play.spriteatlas");
        if (!atlas) throw new InvalidOperationException("Gameplay atlas is missing");
        var atlases = new List<SpriteAtlas>();
        foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
            atlases.Add(AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid)));
        var sprites = new HashSet<Sprite>();
        var localUvSprites = new HashSet<Sprite>();
        var report = new StringBuilder("Native SpriteRenderer material/atlas audit\n");
        int renderers = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", Folders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Array.IndexOf(Folders, Path.GetDirectoryName(path)?.Replace('\\', '/')) < 0) continue;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            report.AppendLine(path);
            foreach (var renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderers++;
                if (renderer.sprite)
                {
                    sprites.Add(renderer.sprite);
                    // The procedural bar interprets UV as 0..1 progress/chamfer coordinates.
                    // Keep its white sprite standalone rather than silently changing those coordinates.
                    if (renderer.sharedMaterial && renderer.sharedMaterial.shader.name == "Game/DamageChunkHealthBar")
                        localUvSprites.Add(renderer.sprite);
                }
                report.AppendLine($"  {renderer.name}: material={AssetDatabase.GetAssetPath(renderer.sharedMaterial)}; shader={renderer.sharedMaterial?.shader?.name}; drawMode={renderer.drawMode}");
            }
            foreach (var dep in AssetDatabase.GetDependencies(path))
            {
                if (!dep.EndsWith(".anim", StringComparison.OrdinalIgnoreCase)) continue;
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(dep);
                foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                        foreach (var frame in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                            if (frame.value is Sprite sprite) sprites.Add(sprite);
            }
        }
        var additions = new List<UnityEngine.Object>();
        foreach (var sprite in sprites)
        {
            var path = AssetDatabase.GetAssetPath(sprite);
            if (localUvSprites.Contains(sprite))
            {
                atlas.Remove(new UnityEngine.Object[] { sprite });
                EditorUtility.SetDirty(atlas);
                report.AppendLine($"Sprite: {path}: kept standalone for procedural health UV");
                continue;
            }
            if (!path.StartsWith("Assets/", StringComparison.Ordinal)) continue; // Unity's built-in white sprite.
            bool covered = false;
            foreach (var candidate in atlases)
            {
                foreach (var packable in candidate.GetPackables())
                {
                    string owner = AssetDatabase.GetAssetPath(packable);
                    if (owner == path || (AssetDatabase.IsValidFolder(owner) && path.StartsWith(owner + "/", StringComparison.Ordinal)))
                    { covered = true; break; }
                }
                if (covered) break;
            }
            report.AppendLine($"Sprite: {path} / {sprite.name}: {(covered ? "existing atlas" : "added to gameplay atlas")}");
            if (!covered) additions.Add(sprite);
        }
        if (additions.Count > 0)
        {
            atlas.Add(additions.ToArray());
            EditorUtility.SetDirty(atlas);
        }
        AssetDatabase.SaveAssetIfDirty(atlas);
        SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
        var packed = new Sprite[atlas.spriteCount];
        atlas.GetSprites(packed);
        var pages = new HashSet<Texture2D>();
        foreach (var sprite in packed)
        {
            pages.Add(sprite.texture);
            UnityEngine.Object.DestroyImmediate(sprite);
        }
        report.AppendLine($"Packed gameplay atlas pages: {pages.Count}");
        Directory.CreateDirectory("Library/GameplaySpriteBatching");
        File.WriteAllText("Library/GameplaySpriteBatching/prefab-audit.txt", report.ToString());
        Debug.Log($"[SpriteBatching] Audited {renderers} native sprite renderers, {sprites.Count} distinct sprites including animation frames; atlas additions={additions.Count}.");
    }
}
