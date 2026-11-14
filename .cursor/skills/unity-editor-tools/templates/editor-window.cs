using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    /// <summary>
    /// [工具名] 编辑器窗口。
    /// </summary>
    public class MyToolWindow : EditorWindow
    {
        // 用 [SerializeField] 保存窗口状态，Unity 会在布局变化时保留
        [SerializeField] private GameObject _targetPrefab;
        [SerializeField] private Vector3 _offset = Vector3.one;
        [SerializeField] private bool _includeInactive = true;

        private Vector2 _scrollPos;

        [MenuItem("Tools/MyGame/MyTool")]
        public static void Open()
        {
            var window = GetWindow<MyToolWindow>("My Tool");
            window.minSize = new Vector2(380f, 260f);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("目标资源", EditorStyles.boldLabel);
            _targetPrefab = (GameObject)EditorGUILayout.ObjectField("预制体", _targetPrefab, typeof(GameObject), false);
            _offset = EditorGUILayout.Vector3Field("偏移", _offset);
            _includeInactive = EditorGUILayout.Toggle("包含未激活", _includeInactive);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_targetPrefab == null))
            {
                if (GUILayout.Button("执行"))
                {
                    Execute(_targetPrefab, _offset);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void Execute(GameObject prefab, Vector3 offset)
        {
            try
            {
                EditorUtility.DisplayProgressBar("处理中", "正在处理...", 0.5f);

                Undo.RegisterCompleteObjectUndo(prefab, "Execute MyTool");

                var transforms = prefab.GetComponentsInChildren<Transform>(_includeInactive);
                for (int i = 0; i < transforms.Length; i++)
                {
                    transforms[i].localPosition += offset;
                }

                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
