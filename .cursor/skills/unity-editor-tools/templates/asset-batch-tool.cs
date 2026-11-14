using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    /// <summary>
    /// 资源批量处理工具。
    /// </summary>
    public static class AssetBatchTool
    {
        private const string SearchRoot = "Assets/GameResources/_Gameplay";

        /// <summary>
        /// 批量替换预制体中的旧材质为新材质。
        /// </summary>
        [MenuItem("Tools/MyGame/批量替换材质")]
        private static void ReplaceMaterials()
        {
            var oldMaterial = Selection.activeObject as Material;
            if (oldMaterial == null)
            {
                EditorUtility.DisplayDialog("批量替换材质", "请先在 Project 窗口选中一个目标材质。", "确定");
                return;
            }

            var newMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GetAssetPath(oldMaterial));
            if (newMaterial == null)
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { SearchRoot });
            int changedCount = 0;

            try
            {
                EditorUtility.DisplayProgressBar("批量替换材质", "处理中...", 0f);

                for (int i = 0; i < guids.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("批量替换材质", guids[i], (float)i / guids.Length);

                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                    {
                        continue;
                    }

                    var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                    bool changed = false;

                    for (int r = 0; r < renderers.Length; r++)
                    {
                        var mats = renderers[r].sharedMaterials;
                        bool rendererChanged = false;

                        for (int m = 0; m < mats.Length; m++)
                        {
                            if (mats[m] == oldMaterial)
                            {
                                mats[m] = newMaterial;
                                rendererChanged = true;
                            }
                        }

                        if (rendererChanged)
                        {
                            renderers[r].sharedMaterials = mats;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        EditorUtility.SetDirty(prefab);
                        changedCount++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[AssetBatchTool] 完成，修改 {changedCount} 个预制体。");
        }

        /// <summary>
        /// 批量设置预制体的 Layer。
        /// </summary>
        [MenuItem("Tools/MyGame/批量设置 Layer")]
        private static void SetLayer()
        {
            int layer = Selection.activeGameObject != null
                ? LayerMask.NameToLayer(Selection.activeGameObject.layer)
                : 0;

            if (layer < 0)
            {
                layer = 0;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { SearchRoot });
            int count = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                    {
                        continue;
                    }

                    prefab.layer = layer;
                    EditorUtility.SetDirty(prefab);
                    count++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"[AssetBatchTool] 已设置 {count} 个预制体的 Layer 为 {LayerMask.LayerToName(layer)}。");
        }
    }
}
