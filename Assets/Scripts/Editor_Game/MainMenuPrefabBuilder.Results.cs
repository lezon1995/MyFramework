using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains;

public static partial class MainMenuPrefabBuilder
{
    [MenuItem("Tools/Game UI/Rebuild Run Result Pages")]
    public static void BuildResults()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before rebuilding UI.");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("2daaf1932b00a2344b2cd83354e78002"));
        foreach (var name in new[] { "DefeatScreen", "VictoryScreen" })
        {
            var root = Node(name, null, Vector2.zero, new Vector2(1920, 1080));
            try
            {
                var canvas = root.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.overrideSorting = true;
                root.gameObject.AddComponent<CanvasGroup>();
                root.gameObject.AddComponent<GraphicRaycaster>();
                root.gameObject.AddComponent<ScaleAnchor>().mKeepAspect = false;
                Image(root, new Color(0, 0, 0, .92f));
                var panel = Node("Panel", root, Vector2.zero, new Vector2(1640, 990));
                Image(panel, new Color(.14f, .14f, .14f));
                Text("Title", panel, new Vector2(0, 425), new Vector2(1480, 75), name == "VictoryScreen" ? "游戏胜利 / VICTORY" : "游戏失败 / DEFEAT", 46);
                var emblem = Text("Celebration", panel, new Vector2(0, 345), new Vector2(1450, 65), "-  胜 利 凯 旋  -", 34);
                emblem.color = new Color(1, .84f, .4f);
                emblem.gameObject.AddComponent<CanvasGroup>();
                emblem.gameObject.SetActive(name == "VictoryScreen");
                Text("Overview", panel, new Vector2(0, 260), new Vector2(1480, 100), "角色    关卡\n击杀 0    波次 1 / 20    难度：进阶 0    时长 00:00:00", 28);
                ResultScroll(panel, "Equipment", -390);
                ResultScroll(panel, "Damage", 390);
                Text("Status", panel, new Vector2(0, -350), new Vector2(1480, 60), "", 22);
                Button("Restart", panel, new Vector2(-340, -430), new Vector2(480, 68), "重来 / Restart").gameObject.SetActive(name == "DefeatScreen");
                Button("Home", panel, new Vector2(name == "VictoryScreen" ? 0 : 340, -430), new Vector2(480, 68), name == "VictoryScreen" ? "胜利凯旋 / Return in triumph" : "退回大厅 / Return to lobby");
                PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + name + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
        }
        AssetDatabase.SaveAssets();
        ValidateResults();
    }

    static void ResultScroll(RectTransform panel, string name, float x)
    {
        Scroll(panel, new Vector2(x, -65), 510);
        var scroll = (RectTransform)panel.Find("Scroll");
        scroll.name = name;
        scroll.sizeDelta = new Vector2(730, 510);
        ((RectTransform)scroll.Find("Viewport")).sizeDelta = new Vector2(700, 486);
        var body = scroll.Find("Viewport/Body").GetComponent<TMP_Text>();
        body.fontSize = 28;
        body.text = name == "Equipment" ? "<b>已装备的球</b>\n\n<b>已装备的遗物</b>" : "<b>累计伤害 / 伤害占比</b>";
    }

    [MenuItem("Tools/Game UI/Validate Run Result Pages")]
    public static void ValidateResults()
    {
        foreach (var name in new[] { "DefeatScreen", "VictoryScreen" })
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab");
            if (root == null || root.GetComponent<Canvas>() == null || root.GetComponent<GraphicRaycaster>() == null)
                throw new InvalidOperationException("Invalid result canvas: " + name);
            foreach (var path in new[] { "Title", "Overview", "Status", "Celebration", "Restart/Text", "Home/Text", "Equipment/Viewport/Body", "Damage/Viewport/Body" })
                if (root.transform.Find("Panel/" + path) == null) throw new InvalidOperationException(name + " missing " + path);
        }
        Debug.Log("[Settlement] Both result prefabs validated.");
    }
}
