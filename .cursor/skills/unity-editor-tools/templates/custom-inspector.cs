using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    /// <summary>
    /// [对象名] 自定义 Inspector。
    /// </summary>
    [CustomEditor(typeof(TargetType), true)]
    [CanEditMultipleObjects]
    public class TargetTypeEditor : Editor
    {
        private SerializedProperty _fieldA;
        private SerializedProperty _fieldB;
        private SerializedProperty _listField;

        private void OnEnable()
        {
            _fieldA = serializedObject.FindProperty("fieldA");
            _fieldB = serializedObject.FindProperty("fieldB");
            _listField = serializedObject.FindProperty("listField");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("分组标题", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_fieldA);
            EditorGUILayout.SliderProperty(_fieldB, 0f, 100f);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("列表", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_listField, true);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("运行时操作"))
                {
                    // 运行时逻辑
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override bool RequiresConstantRepaint()
        {
            // 有动画/实时值时才返回 true
            return false;
        }
    }
}
