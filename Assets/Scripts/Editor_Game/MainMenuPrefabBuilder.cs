using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains;

/// <summary>生成与项目世界空间 UI、ScaleAnchor 一致的可编辑主菜单白模。</summary>
public static partial class MainMenuPrefabBuilder
{
    const string Folder = "Assets/GameResources/UI/UIPrefab/";
    static TMP_FontAsset font;

    [MenuItem("Tools/Game UI/Rebuild Main Menu Pages")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before rebuilding UI prefabs.");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("2daaf1932b00a2344b2cd83354e78002"));
        foreach (var name in new[] { "EncyclopediaScreen", "StatisticsScreen", "PatchNotesScreen", "MainMenuConfirmScreen", "DesktopSettingsScreen" })
        {
            var root = Node(name, null, Vector2.zero, new Vector2(1920, 1080));
            try
            {
                var canvas = root.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.overrideSorting = true;
                root.gameObject.AddComponent<CanvasGroup>();
                root.gameObject.AddComponent<GraphicRaycaster>();
                // 根节点跟随全屏；页面脚本按根尺寸等比缩放整个面板及其控件。
                var anchor = root.gameObject.AddComponent<ScaleAnchor>();
                anchor.mKeepAspect = false;
                Image(root, new Color(0, 0, 0, .88f));
                var panel = Node("Panel", root, Vector2.zero, new Vector2(1560, 930));
                Image(panel, new Color(.14f, .14f, .14f, 1));
                Text("Title", panel, new Vector2(0, 387), new Vector2(1400, 65), name, 42);
                Text("Status", panel, new Vector2(0, -328), new Vector2(1400, 75), "", 24);
                Button("Back", panel, new Vector2(-530, -405), new Vector2(250, 60), "返回 / Back");
                if (name == "DesktopSettingsScreen") Settings(panel);
                else
                {
                    bool library = name == "EncyclopediaScreen";
                    Scroll(panel, library ? new Vector2(0, -20) : new Vector2(0, 35), library ? 470 : 560);
                    if (library)
                    {
                        for (int i = 0; i < 3; i++)
                            Button("Category" + (i + 1), panel, new Vector2((i - 1) * 370, 290), new Vector2(330, 60), new[] { "角色", "球", "遗物" }[i]);
                        Button("Previous", panel, new Vector2(-220, -270), new Vector2(330, 55), "上一项");
                        Button("Next", panel, new Vector2(220, -270), new Vector2(330, 55), "下一项");
                    }
                    if (name == "MainMenuConfirmScreen")
                        Button("Confirm", panel, new Vector2(530, -405), new Vector2(250, 60), "确认 / Confirm");
                }
                PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + name + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
        }
        AssetDatabase.SaveAssets();
        Validate();
        Debug.Log("[MainMenu] Generated and validated five menu page prefabs.");
    }

    static void Settings(RectTransform panel)
    {
        for (int i = 0; i < 4; i++)
            Button("Tab" + i, panel, new Vector2(-540 + i * 360, 290), new Vector2(330, 65), new[] { "显示", "声音", "游戏", "操作" }[i]);
        var rows = Node("Rows", panel, Vector2.zero, new Vector2(1400, 480));
        rows.gameObject.AddComponent<CanvasGroup>();
        for (int i = 0; i < 7; i++)
        {
            var row = Node("Row" + i, rows, new Vector2(0, 215 - i * 72), new Vector2(1380, 65));
            Text("Label", row, new Vector2(-400, 0), new Vector2(550, 60), "Setting", 28);
            Button("Value", row, new Vector2(385, 0), new Vector2(560, 60), "Value");
            var sliderRect = Node("Slider", row, new Vector2(-10, 0), new Vector2(220, 45));
            var slider = sliderRect.gameObject.AddComponent<Slider>();
            var track = Node("Track", sliderRect, Vector2.zero, new Vector2(220, 10));
            Image(track, new Color(.35f, .35f, .35f));
            var handleArea = Node("HandleArea", sliderRect, Vector2.zero, new Vector2(190, 45));
            var handle = Node("Handle", handleArea, Vector2.zero, new Vector2(24, 40));
            slider.targetGraphic = Image(handle, Color.white);
            slider.handleRect = handle;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = .5f;
            sliderRect.gameObject.SetActive(false);
        }
        Button("Defaults", panel, new Vector2(-180, -405), new Vector2(280, 60), "恢复默认");
        Button("Apply", panel, new Vector2(180, -405), new Vector2(250, 60), "应用");
        Button("Keep", panel, new Vector2(530, -405), new Vector2(250, 60), "保留设置").gameObject.SetActive(false);
        Button("Revert", panel, new Vector2(530, -270), new Vector2(250, 55), "恢复显示").gameObject.SetActive(false);
    }

    static void Scroll(RectTransform panel, Vector2 position, float height)
    {
        var scroll = Node("Scroll", panel, position, new Vector2(1380, height));
        var rect = scroll.gameObject.AddComponent<ScrollRect>();
        Image(scroll, new Color(.1f, .1f, .1f));
        var viewport = Node("Viewport", scroll, Vector2.zero, new Vector2(1340, height - 24));
        viewport.gameObject.AddComponent<RectMask2D>();
        var body = Text("Body", viewport, Vector2.zero, new Vector2(1300, height - 24), "Content", 30);
        var transform = body.rectTransform;
        transform.anchorMin = new Vector2(0, 1);
        transform.anchorMax = Vector2.one;
        transform.pivot = new Vector2(.5f, 1);
        transform.sizeDelta = new Vector2(-40, 0);
        body.alignment = TextAlignmentOptions.TopLeft;
        body.textWrappingMode = TextWrappingModes.Normal;
        var fitter = body.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rect.viewport = viewport;
        rect.content = transform;
        rect.horizontal = false;
        rect.vertical = true;
        rect.movementType = ScrollRect.MovementType.Clamped;
        rect.scrollSensitivity = 45;
    }

    static RectTransform Node(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var node = new GameObject(name, typeof(RectTransform));
        node.layer = 5;
        var rect = (RectTransform)node.transform;
        rect.SetParent(parent, false);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static Image Image(RectTransform rect, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    static TMP_Text Text(string name, Transform parent, Vector2 position, Vector2 size, string value, int fontSize)
    {
        var text = Node(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.text = value;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    static Button Button(string name, Transform parent, Vector2 position, Vector2 size, string value)
    {
        var rect = Node(name, parent, position, size);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = Image(rect, new Color(.29f, .29f, .29f));
        var colors = button.colors;
        colors.highlightedColor = new Color(1.5f, 1.5f, 1.5f);
        colors.selectedColor = new Color(1.7f, 1.7f, 1.7f);
        button.colors = colors;
        Text("Text", rect, Vector2.zero, size - new Vector2(16, 6), value, 28);
        return button;
    }

    [MenuItem("Tools/Game UI/Validate Main Menu Pages")]
    public static void Validate()
    {
        foreach (var name in new[] { "EncyclopediaScreen", "StatisticsScreen", "PatchNotesScreen", "MainMenuConfirmScreen", "DesktopSettingsScreen" })
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab");
            if (root == null || root.GetComponent<Canvas>() == null || root.transform.Find("Panel/Back") == null)
                throw new InvalidDataException("Invalid menu prefab: " + name);
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                    throw new InvalidDataException("Missing script: " + name + "/" + transform.name);
            if (name == "DesktopSettingsScreen")
                for (int i = 0; i < 7; i++)
                    if (root.transform.Find($"Panel/Rows/Row{i}/Slider") == null)
                        throw new InvalidDataException("Missing settings row " + i);
        }
        Debug.Log("[MainMenu] PASS: five prefabs, framework canvas/anchors, required controls, no missing scripts.");
        ValidateBindings();
    }

    static void ValidateBindings()
    {
        var runtime = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "HotFix");
        var transformField = typeof(LayoutScript).GetField("mTransform", BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var name in new[] { "EncyclopediaScreen", "StatisticsScreen", "PatchNotesScreen", "MainMenuConfirmScreen", "DesktopSettingsScreen" })
        {
            var instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab"));
            try
            {
                var type = runtime.GetType("MoreMountains." + name, true);
                var script = (LayoutScript)Activator.CreateInstance(type);
                transformField.SetValue(script, (RectTransform)instance.transform);
                script.assignWindow();
                script.init();
                if (name == "DesktopSettingsScreen")
                {
                    var prefs = runtime.GetType("MoreMountains.MenuPreferences", true);
                    var draft = prefs.GetMethod("Defaults").Invoke(null, null);
                    type.GetField("draft", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(script, draft);
                    type.GetMethod("Render", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(script, null);
                    for (int tab = 0; tab < 4; tab++)
                        instance.transform.Find("Panel/Tab" + tab).GetComponent<Button>().onClick.Invoke();
                    instance.transform.Find("Panel/Tab1").GetComponent<Button>().onClick.Invoke();
                    var slider = instance.transform.Find("Panel/Rows/Row0/Slider").GetComponent<Slider>();
                    slider.value = .73f;
                    var actual = (float)draft.GetType().GetField("master").GetValue(draft);
                    if (Mathf.Abs(actual - .73f) > .001f) throw new InvalidOperationException("Master volume binding failed.");
                    instance.transform.Find("Panel/Defaults").GetComponent<Button>().onClick.Invoke();
                    instance.transform.Find("Panel/Tab0").GetComponent<Button>().onClick.Invoke();
                    Preview(instance);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
        Debug.Log("[MainMenu] PASS: all page assignWindow/init bindings; four settings tabs; volume slider; defaults. No saves or player preferences modified.");
    }

    public static void PreviewResult(GameObject root, string name) => Preview(root, name);

    static void Preview(GameObject root, string previewName = "main-menu-settings")
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            Debug.Log("[MainMenu] Visual preview skipped: Unity was started with -nographics.");
            return;
        }
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        var cameraObject = new GameObject("MenuPreviewCamera", typeof(Camera));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var target = new RenderTexture(1920, 1080, 24);
        var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        var defaultPipeline = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        var qualityPipeline = QualitySettings.renderPipeline;
        try
        {
            // UI 白模预览使用内置渲染，避免批处理首次初始化 URP RenderGraph 的时序依赖。
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = null;
            QualitySettings.renderPipeline = null;
            root.transform.position = new Vector3(10000, 10000, 0);
            var camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene;
            camera.enabled = false;
            camera.transform.position = root.transform.position + new Vector3(0, 0, -1000);
            camera.orthographic = true;
            camera.orthographicSize = 540;
            camera.farClipPlane = 2000;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.targetTexture = target;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            target.Create();
            root.GetComponent<Canvas>().worldCamera = camera;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            image.Apply();
            var path = Path.Combine(Path.GetTempPath(), previewName + "-preview.png");
            File.WriteAllBytes(path, image.EncodeToPNG());
            Debug.Log("[MainMenu] Preview: " + path);
        }
        finally
        {
            cameraObject.GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = previous;
            QualitySettings.renderPipeline = qualityPipeline;
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = defaultPipeline;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
