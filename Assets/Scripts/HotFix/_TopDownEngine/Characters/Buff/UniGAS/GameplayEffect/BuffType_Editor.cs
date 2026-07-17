#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MoreMountains
{
    public partial class BuffType : SerializedScriptableObject
    {
        [ContextMenu("Create Buff Prefab")]
        public void CreatePrefab()
        {
            // 获取当前选中的对象
            BuffType scriptableObject = Selection.activeObject as BuffType;
            if (scriptableObject)
            {
                // 使用AssetDatabase获取路径
                string path = AssetDatabase.GetAssetPath(scriptableObject);

                var go = new GameObject(name);
                var buff = go.AddComponent<Buff>();
                buff.BuffType = this;

                // 定义保存预制体的路径
                string localPath = path.Replace(".asset", ".prefab");

                // 检查路径是否已经有预制体存在，如果有则覆盖
                if (AssetDatabase.LoadAssetAtPath<GameObject>(localPath))
                {
                    if (EditorUtility.DisplayDialog("Prefab Already Exists", "Do you want to overwrite the existing prefab?", "Yes", "No"))
                    {
                        SavePrefab(go, localPath);
                    }
                }
                else
                {
                    SavePrefab(go, localPath);
                }

                // 删除创建的临时对象
                DestroyImmediate(go);
            }
            else
            {
                Debug.LogError("No ScriptableObject selected!");
            }
        }

        static void SavePrefab(GameObject obj, string localPath)
        {
            // 保存为预制体文件
            PrefabUtility.SaveAsPrefabAsset(obj, localPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Prefab created at " + localPath);
        }
    }
}
#endif