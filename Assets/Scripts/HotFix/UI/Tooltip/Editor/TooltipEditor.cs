using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains
{
    /// <summary>
    /// TooltipTrigger的自定义编辑器
    /// 提供更好的Inspector展示和快捷操作
    /// </summary>
    [CustomEditor(typeof(TooltipTrigger))]
    public class TooltipTriggerEditor : Editor
    {
        TooltipTrigger _trigger;
        SerializedProperty _titleProp;
        SerializedProperty _descriptionProp;
        SerializedProperty _iconProp;
        SerializedProperty _showDelayProp;
        SerializedProperty _displayDurationProp;
        SerializedProperty _positionModeProp;
        SerializedProperty _anchorDirectionProp;
        SerializedProperty _mouseOffsetProp;
        SerializedProperty _anchorOffsetProp;
        SerializedProperty _enableMetaTooltipProp;
        SerializedProperty _customContentGeneratorProp;
        SerializedProperty _useGlobalSettingsProp;

        bool _showAdvancedSettings;
        bool _showPositionSettings;
        bool _showMetaSettings;

        static readonly string[] _excludedProperties = {
            "Script",
            "TooltipTitle",
            "TooltipDescription",
            "Icon",
            "ShowDelay",
            "DisplayDuration",
            "PositionMode",
            "AnchorDirection",
            "MouseOffset",
            "AnchorOffset",
            "EnableMetaTooltip",
            "CustomContentGenerator",
            "UseGlobalSettings"
        };

        void OnEnable()
        {
            _trigger = (TooltipTrigger)target;

            _titleProp = serializedObject.FindProperty("TooltipTitle");
            _descriptionProp = serializedObject.FindProperty("TooltipDescription");
            _iconProp = serializedObject.FindProperty("Icon");
            _showDelayProp = serializedObject.FindProperty("ShowDelay");
            _displayDurationProp = serializedObject.FindProperty("DisplayDuration");
            _positionModeProp = serializedObject.FindProperty("PositionMode");
            _anchorDirectionProp = serializedObject.FindProperty("AnchorDirection");
            _mouseOffsetProp = serializedObject.FindProperty("MouseOffset");
            _anchorOffsetProp = serializedObject.FindProperty("AnchorOffset");
            _enableMetaTooltipProp = serializedObject.FindProperty("EnableMetaTooltip");
            _customContentGeneratorProp = serializedObject.FindProperty("CustomContentGenerator");
            _useGlobalSettingsProp = serializedObject.FindProperty("UseGlobalSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            DrawHeader();
            EditorGUILayout.Space(5);

            DrawContentSection();
            EditorGUILayout.Space(5);

            DrawTimingSection();
            EditorGUILayout.Space(5);

            DrawPositionSection();
            EditorGUILayout.Space(5);

            DrawMetaSection();
            EditorGUILayout.Space(5);

            DrawAdvancedSection();

            EditorGUILayout.Space(10);
            DrawActionButtons();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Tooltip Trigger", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_titleProp);
            EditorGUILayout.PropertyField(_descriptionProp);
            EditorGUILayout.PropertyField(_iconProp);

            EditorGUILayout.EndVertical();
        }

        void DrawContentSection()
        {
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced Settings", true);

            if (_showAdvancedSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_customContentGeneratorProp);
                EditorGUILayout.PropertyField(_useGlobalSettingsProp);

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        void DrawTimingSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Timing Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_showDelayProp);
            EditorGUILayout.PropertyField(_displayDurationProp);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset to Global", GUILayout.Width(120)))
            {
                _showDelayProp.floatValue = 0.5f;
                _displayDurationProp.floatValue = 5f;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void DrawPositionSection()
        {
            _showPositionSettings = EditorGUILayout.Foldout(_showPositionSettings, "Position Settings", true);

            if (_showPositionSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_positionModeProp);

                TooltipPositionMode positionMode = (TooltipPositionMode)_positionModeProp.enumValueIndex;

                if (positionMode == TooltipPositionMode.PivotAnchored)
                {
                    EditorGUILayout.PropertyField(_anchorDirectionProp);
                    EditorGUILayout.PropertyField(_anchorOffsetProp);
                }
                else if (positionMode == TooltipPositionMode.MousePosition)
                {
                    EditorGUILayout.PropertyField(_mouseOffsetProp);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        void DrawMetaSection()
        {
            _showMetaSettings = EditorGUILayout.Foldout(_showMetaSettings, "Meta Tooltip Settings", true);

            if (_showMetaSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_enableMetaTooltipProp);

                if (_enableMetaTooltipProp.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "Meta Tooltip会根据内容中的关键字（如[灼烧]）自动显示附加信息。\n" +
                        "你可以在TooltipManager的Settings中配置关键字预设。",
                        MessageType.Info);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        void DrawAdvancedSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Component Info", EditorStyles.boldLabel);
            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                ShowHelpPopup();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("GameObject", _trigger.gameObject, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Show"))
            {
                _trigger.ShowTooltip();
            }

            if (GUILayout.Button("Test Hide"))
            {
                _trigger.HideTooltip();
            }

            if (GUILayout.Button("Refresh"))
            {
                _trigger.RefreshTooltip();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Copy Tooltip Text to Clipboard"))
            {
                string text = string.IsNullOrEmpty(_trigger.TooltipTitle)
                    ? _trigger.TooltipDescription
                    : $"<b>{_trigger.TooltipTitle}</b>\n{_trigger.TooltipDescription}";
                EditorGUIUtility.systemCopyBuffer = text;
            }
        }

        void ShowHelpPopup()
        {
            string message = "TooltipTrigger 使用说明:\n\n" +
                             "1. 在UI元素上添加此组件\n" +
                             "2. 设置Tooltip的标题和描述\n" +
                             "3. 配置显示时机、位置等参数\n" +
                             "4. 确保场景中有TooltipManager组件\n\n" +
                             "支持的触发方式:\n" +
                             "- 鼠标悬停 (Pointer Enter)\n" +
                             "- UI选择 (Select)\n" +
                             "- 手动调用 ShowTooltip()";

            EditorUtility.DisplayDialog("Tooltip Trigger Help", message, "OK");
        }
    }

    /// <summary>
    /// TooltipManager的自定义编辑器
    /// </summary>
    [CustomEditor(typeof(TooltipManager))]
    public class TooltipManagerEditor : Editor
    {
        TooltipManager _manager;
        SerializedProperty _settingsProp;
        SerializedProperty _tooltipBoxPrefabProp;
        SerializedProperty _metaTooltipBoxPrefabProp;

        bool _showSettings = true;
        bool _showPresets = false;
        bool _showPrefabs = false;

        void OnEnable()
        {
            _manager = (TooltipManager)target;
            _settingsProp = serializedObject.FindProperty("_settings");
            _tooltipBoxPrefabProp = serializedObject.FindProperty("_tooltipBoxPrefab");
            _metaTooltipBoxPrefabProp = serializedObject.FindProperty("_metaTooltipBoxPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            DrawHeader();
            EditorGUILayout.Space(5);

            DrawSettingsSection();
            EditorGUILayout.Space(5);

            DrawPrefabsSection();
            EditorGUILayout.Space(5);

            DrawActionsSection();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Tooltip Manager", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Instance Active", _manager.gameObject.activeInHierarchy);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        void DrawSettingsSection()
        {
            _showSettings = EditorGUILayout.Foldout(_showSettings, "Global Settings", true);

            if (_showSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                var iterator = serializedObject.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if (iterator.propertyPath == "_settings")
                    {
                        DrawSettingsProperties(iterator);
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        void DrawSettingsProperties(SerializedProperty settingsProperty)
        {
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;

            SerializedProperty enableProp = settingsProperty.FindPropertyRelative("enableTooltip");
            EditorGUILayout.PropertyField(enableProp);

            if (enableProp.boolValue)
            {
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("defaultShowDelay"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("defaultDisplayDuration"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("defaultPositionMode"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("defaultAnchorDirection"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("fixedPosition"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("mouseOffset"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("anchorOffset"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("screenEdgePadding"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("metaTooltipSpacing"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("autoAdjustPosition"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("fadeInDuration"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("fadeOutDuration"));
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("keywordPattern"));

                EditorGUILayout.BeginHorizontal();
                _showPresets = EditorGUILayout.Foldout(_showPresets, $"Keyword Presets ({settingsProperty.FindPropertyRelative("keywordPresets").arraySize})", true);
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    settingsProperty.FindPropertyRelative("keywordPresets").InsertArrayElementAtIndex(
                        settingsProperty.FindPropertyRelative("keywordPresets").arraySize);
                }

                EditorGUILayout.EndHorizontal();

                if (_showPresets)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty presetsArray = settingsProperty.FindPropertyRelative("keywordPresets");
                    for (int i = 0; i < presetsArray.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        SerializedProperty preset = presetsArray.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(preset, new GUIContent($"Preset {i + 1}"));

                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            presetsArray.DeleteArrayElementAtIndex(i);
                            break;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        void DrawPrefabsSection()
        {
            _showPrefabs = EditorGUILayout.Foldout(_showPrefabs, "Prefab References", true);

            if (_showPrefabs)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_tooltipBoxPrefabProp);
                EditorGUILayout.PropertyField(_metaTooltipBoxPrefabProp);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create Default Prefabs", GUILayout.Width(150)))
                {
                    CreateDefaultPrefabs();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        void DrawActionsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Show Tooltip"))
            {
                TooltipRequest request = new TooltipRequest
                {
                    content = new TooltipContent("Test Title", "这是测试内容，包含[灼烧]和[冰冻]关键字。"),
                    positionMode = _manager.settings.defaultPositionMode,
                    anchorDirection = _manager.settings.defaultAnchorDirection,
                    durationMode = TooltipDurationMode.Timed,
                    displayDuration = 5f
                };
                _manager.ShowTooltip(request);
            }

            if (GUILayout.Button("Hide All"))
            {
                _manager.HideTooltipImmediate();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void CreateDefaultPrefabs()
        {
            string path = "Assets/Scripts/HotFix/UI/Tooltip/";

            GameObject tooltipBoxPrefab = CreateTooltipBoxPrefab(path + "TooltipBox_Prefab.prefab");
            _manager._tooltipBoxPrefab = tooltipBoxPrefab;

            GameObject metaTooltipBoxPrefab = CreateMetaTooltipBoxPrefab(path + "MetaTooltipBox_Prefab.prefab");
            _manager._metaTooltipBoxPrefab = metaTooltipBoxPrefab;

            EditorUtility.SetDirty(_manager);
        }

        GameObject CreateTooltipBoxPrefab(string path)
        {
            GameObject go = new GameObject("TooltipBox_Prefab");
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>();
            go.AddComponent<Outline>();
            TooltipBox box = go.AddComponent<TooltipBox>();
            box.CreateDefaultStructure();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);

            return prefab;
        }

        GameObject CreateMetaTooltipBoxPrefab(string path)
        {
            GameObject go = new GameObject("MetaTooltipBox_Prefab");
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>();
            go.AddComponent<Outline>();
            MetaTooltipBox box = go.AddComponent<MetaTooltipBox>();
            box.CreateDefaultStructure();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);

            return prefab;
        }
    }
}