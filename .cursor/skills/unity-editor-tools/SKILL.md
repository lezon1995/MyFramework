---
name: unity-editor-tools
description: Unity Editor 扩展与自动化工具开发指南，覆盖自定义 Inspector、EditorWindow、AssetPostprocessor 导入回调、菜单工具、Prefab/资源批处理、构建脚本与 CI 自动化。Use when writing editor scripts, creating custom inspectors or editor windows, adding menu items, automating asset or prefab processing, hooking into import pipeline, building bundles or players from CLI, or adding validation tools to the project.
---

# Unity Editor 工具开发

## 何时使用

- 编写自定义 Inspector / EditorWindow / 属性抽屉
- 做资源批量处理（Prefab、Material、Texture、Addressable）
- 处理资源导入（AssetPostprocessor）、自动改名/改标签
- 自动化构建、打包、CI 流程
- 给策划做一键工具

## 1. 目录与程序集约定

**Editor 代码必须放在 Editor 目录或 Editor 程序集内**，否则会打进运行时包。

本仓库程序集划分：

| 程序集 | 路径 | 说明 |
|--------|------|------|
| `Editor_Game` | `Assets/Scripts/Editor_Game` | 本项目编辑器工具放这里 |
| `Frame_Frame`（Editor 变体） | `Assets/Scripts/Editor_*` | 框架级编辑器工具 |

推荐做法：新建 `Assets/Scripts/Editor_Game/<Module>/`，在其下建 `.asmdef` 且设置：

```json
{
    "name": "Editor_Game",
    "includePlatforms": ["Editor"],
    "references": ["Game", "HotFix", "Frame_Game"],
    "autoReferenced": true
}
```

`includePlatforms: ["Editor"]` 是关键——它保证代码不会进入运行时构建。

**命名约定**：
- 自定义 Inspector：`XxxEditor.cs`，类名 `XxxEditor`，继承 `Editor`，配 `[CustomEditor(typeof(Xxx))]`
- EditorWindow：`XxxWindow.cs`，继承 `EditorWindow`，用 `EditorWindow.GetWindow<T>("标题")` 打开
- 工具类：`XxxTools.cs`，静态方法 + `[MenuItem("Tools/Xxx/...")]`

## 2. 自定义 Inspector

```csharp
using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    [CustomEditor(typeof(PlayerConfig))]
    public class PlayerConfigEditor : Editor
    {
        private SerializedProperty _health;
        private SerializedProperty _speed;

        private void OnEnable()
        {
            _health = serializedObject.FindProperty("health");
            _speed = serializedObject.FindProperty("speed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("基础数值", EditorStyles.boldLabel);
            _health.floatValue = EditorGUILayout.Slider("生命值", _health.floatValue, 0f, 1000f);
            _speed.floatValue = EditorGUILayout.FloatField("速度", _speed.floatValue);

            EditorGUILayout.Space();
            if (GUILayout.Button("重置为默认值"))
            {
                _health.floatValue = 100f;
                _speed.floatValue = 5f;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
```

要点：
- 用 `SerializedProperty` + `serializedObject.Update()/ApplyModifiedProperties()`，保证 Undo/Redo 与多对象编辑生效。
- 不要直接改字段值（会丢失 Undo）。
- 多对象编辑：加 `[CanEditMultipleObjects]`。
- `OnInspectorGUI` 每帧调用，**不要在里面做重计算或 `AssetDatabase` 查询**（用 `RequiresConstantRepaint`）。

### 常用绘制 API
`EditorGUILayout.PropertyField` / `Slider` / `IntSlider` / `Toggle` / `EnumPopup` / `TextField` / `ObjectField` / `Foldout` / `Space` / `HelpBox` / `BeginHorizontal` / `BeginVertical` / `DisabledScope` / `IndentLevel`。

## 3. 属性绘制（PropertyDrawer）

适用于自定义可序列化字段类型：

```csharp
[CustomPropertyDrawer(typeof(RangeInt))]
public class RangeIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int start = property.FindPropertyRelative("start");
        int end = property.FindPropertyRelative("end");

        EditorGUI.BeginProperty(position, label, property);

        int newStart = EditorGUI.IntField(position, label.text, start.intValue);
        int newEnd = EditorGUI.IntField(position, label.text, end.intValue);

        start.intValue = Mathf.Min(newStart, newEnd);
        end.intValue = Mathf.Max(newStart, newEnd);

        EditorGUI.EndProperty();
    }

    // 属性高度
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
```

需要 `CustomPropertyDrawer` 的字段类型必须可序列化（`[Serializable]`）。

## 4. EditorWindow

```csharp
using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    public class PrefabBatchToolWindow : EditorWindow
    {
        private GameObject _targetPrefab;
        private Vector3 _offset = Vector3.one;

        [MenuItem("Tools/Gameplay/Prefab Batch Tool")]
        public static void Open()
        {
            var window = GetWindow<PrefabBatchToolWindow>("Prefab Batch");
            window.minSize = new Vector2(360f, 220f);
        }

        private void OnGUI()
        {
            _targetPrefab = (GameObject)EditorGUILayout.ObjectField("目标预制体", _targetPrefab, typeof(GameObject), false);
            _offset = EditorGUILayout.Vector3Field("位置偏移", _offset);

            using (new EditorGUI.DisabledScope(_targetPrefab == null))
            {
                if (GUILayout.Button("应用到所有子物体"))
                {
                    ApplyToChildren(_targetPrefab.transform, _offset);
                }
            }
        }

        private static void ApplyToChildren(Transform root, Vector3 offset)
        {
            Undo.RegisterCompleteObjectUndo(root, "Apply Offset");
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                child.localPosition += offset;
            }
            EditorUtility.SetDirty(root);
        }
    }
}
```

要点：
- 序列化窗口状态：在窗口类上加 `[SerializeField]` 字段（Unity 会保存窗口布局中的值）。
- `EditorGUILayout` vs `GUILayout`：`EditorGUILayout` 会对齐 Editor 样式。
- 长耗时操作用 `EditorUtility.DisplayProgressBar` / `ClearProgressBar`，并在 `finally` 里清理。

## 5. AssetPostprocessor（导入回调）

```csharp
using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    public class TexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            // 可在此做像素处理，注意不要在此长时间阻塞
        }
    }
}
```

可用回调：`OnPreprocessAsset` / `OnPostprocessAsset` / `OnPreprocessTexture` / `OnPostprocessTexture` / `OnPreprocessModel` / `OnPostprocessModel` / `OnPreprocessAudio` / `OnPostprocessAllAssets`（静态）。

**注意**：`OnPostprocessAllAssets` 在大项目中非常慢，避免在其中做重活。

## 6. 资源批处理

```csharp
using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    public static class AssetTools
    {
        [MenuItem("Tools/Gameplay/批量替换材质")]
        private static void ReplaceMaterial()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/GameResources/_Gameplay" });
            int count = 0;

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

                    var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                    bool changed = false;

                    for (int r = 0; r < renderers.Length; r++)
                    {
                        var mats = renderers[r].sharedMaterials;
                        for (int m = 0; m < mats.Length; m++)
                        {
                            if (mats[m] != null && mats[m].name.StartsWith("Old_"))
                            {
                                mats[m] = newMaterial;
                                changed = true;
                            }
                        }

                        if (changed)
                        {
                            renderers[r].sharedMaterials = mats;
                        }
                    }

                    if (changed)
                    {
                        EditorUtility.SetDirty(prefab);
                        count++;
                    }
                }
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[AssetTools] 完成，修改 {count} 个预制体。");
        }
    }
}
```

关键 API：
- `AssetDatabase.FindAssets("t:Prefab", new[] { "搜索路径" })` → GUID 列表
- `AssetDatabase.GUIDToAssetPath` → 路径
- `AssetDatabase.LoadAssetAtPath<T>` → 资源
- `AssetDatabase.SaveAssets()` / `Refresh()`
- `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` → 批量导入时避免频繁刷新
- `EditorUtility.SetDirty(obj)` → 标记修改
- `PrefabUtility.SaveAsPrefabAsset` / `ApplyPrefabInstance`
- `AssetDatabase.CreateAsset` / `AddObjectToAsset` / `DeleteAsset`
- `Undo.RegisterCompleteObjectUndo` → 支持撤销

## 7. 菜单与快捷键

```csharp
[MenuItem("Tools/Gameplay/生成网格")]
private static void Generate() { }

[MenuItem("Tools/Gameplay/生成网格", true)]   // 校验函数，控制菜单是否可用
private static bool ValidateGenerate() => Selection.activeGameObject != null;
```

快捷键：`[Shortcut("Tools/MyTool", KeyCode.F5)]`（需 `UnityEditor.ShortcutManagement`）。

## 8. 构建与 CI

```csharp
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MyGame.EditorTools
{
    public static class BuildPipelineTool
    {
        [MenuItem("Tools/Build/Android")]
        public static void BuildAndroid()
        {
            var options = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = "Build/Android/MyGame.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] 成功，用时 {summary.totalTime.TotalSeconds:F1}s，大小 {summary.totalSize / (1024 * 1024)} MB");
            }
            else
            {
                Debug.LogError($"[Build] 失败：{summary.result}, {summary.totalErrors} 个错误");
            }
        }

        private static string[] GetScenePaths()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/GameResources/_Gameplay/Levels" }))
            {
                scenes.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            return scenes.ToArray();
        }
    }
}
```

CI 调用（命令行）：
```bash
Unity -batchmode -quit -projectPath . -executeMethod MyGame.EditorTools.BuildPipelineTool.BuildAndroid -buildTarget Android -logFile build.log
```

命令行参数约定：
- `-executeMethod <完整类名.方法名>`
- `-batchmode -quit`
- `-logFile <路径>`（`-` 表示输出到 stdout）
- `-buildTarget <Target>`
- 退出码：`0` 成功，`1` 失败（在方法末尾 `EditorApplication.Exit(0/1)`）

## 9. 校验工具（Validation）

```csharp
using UnityEditor;
using UnityEngine;

namespace MyGame.EditorTools
{
    [CustomEditor(typeof(PlayerConfig))]
    public class PlayerConfigValidator : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var config = (PlayerConfig)target;
            if (config.Health <= 0f)
            {
                EditorGUILayout.HelpBox("生命值必须大于 0", MessageType.Error);
            }
        }
    }
}
```

更推荐用 `IPreprocessBuildWithReport` 做打包前校验：

```csharp
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MyGame.EditorTools
{
    public class BuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var guids = AssetDatabase.FindAssets("t:PlayerConfig");
            for (int i = 0; i < guids.Length; i++)
            {
                var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (config != null && config.Health <= 0f)
                {
                    throw new BuildFailedException($"PlayerConfig 配置非法: {config.name}");
                }
            }
        }
    }
}
```

## 10. 最佳实践

- 所有耗时操作显示进度条，并在 `finally` 中 `EditorUtility.ClearProgressBar()`。
- 批量操作前 `Undo.RegisterCompleteObjectUndo`，或先 `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()`。
- 编辑器代码加 `#if UNITY_EDITOR` 保护（当文件不在 Editor 目录时）。
- 用 `AssetDatabase.IsValidFolder` 检查目录存在再创建。
- 工具菜单路径统一前缀：`Tools/<项目>/<模块>/<操作>`。
- 不在 `OnInspectorGUI` / `OnGUI` 里做 `AssetDatabase` 查询或文件 IO（每帧调用）。
- `Gizmos`/`Handles` 绘制用 `Handles.color` 保存与恢复。

## 11. 模板

- 自定义 Inspector：[templates/custom-inspector.cs](templates/custom-inspector.cs)
- EditorWindow 工具：[templates/editor-window.cs](templates/editor-window.cs)
- 资源批处理：[templates/asset-batch-tool.cs](templates/asset-batch-tool.cs)
- 构建脚本：[templates/build-pipeline.cs](templates/build-pipeline.cs)
- 编辑器工具清单：[templates/editor-tools-checklist.md](templates/editor-tools-checklist.md)
