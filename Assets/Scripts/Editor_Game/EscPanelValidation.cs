using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains;

/// <summary>隔离检查暂停面板的预制体、交互屏蔽及焦点恢复，不启动游戏或修改存档。</summary>
public static class EscPanelValidation
{
    [MenuItem("Tools/Game UI/Validate Pause Menu")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run pause menu validation outside Play mode.");
        var runtime = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "HotFix");
        var type = runtime.GetType("MoreMountains.EscPanel", true);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/UI/UIPrefab/EscPanel.prefab");
        if (prefab == null) throw new InvalidOperationException("Missing EscPanel prefab.");
        var instance = UnityEngine.Object.Instantiate(prefab);
        try
        {
            foreach (var name in new[] { "BtnContinue", "BtnRestart", "BtnSettings", "BtnReturnMainMenu" })
                if (instance.transform.Find("MenuOptionView/V/" + name)?.GetComponent<Button>() == null)
                    throw new InvalidOperationException("Missing pause menu button: " + name);
            foreach (var transform in instance.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                    throw new InvalidOperationException("Missing script: " + transform.name);
            var panel = (LayoutScript)Activator.CreateInstance(type);
            var root = new myUGUIObject();
            root.setObject(instance);
            panel.setRoot(root);
            typeof(LayoutScript).GetField("mTransform", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(panel, (RectTransform)instance.transform);
            // 验证已有/缺少 CanvasGroup 两种路径，以及关闭后可再次打开。
            var group = instance.GetComponent<CanvasGroup>();
            if (group != null) UnityEngine.Object.DestroyImmediate(group);
            type.GetMethod("OnPauseOpened").Invoke(panel, null);
            group = instance.GetComponent<CanvasGroup>();
            Require(group != null && group.interactable && group.blocksRaycasts, "Pause opening must enable interaction.");
            var interaction = type.GetMethod("SetInteraction", BindingFlags.Instance | BindingFlags.NonPublic);
            interaction.Invoke(panel, new object[] { false });
            Require(!group.interactable && !group.blocksRaycasts, "A modal must block underlying pause controls.");
            type.GetMethod("OnPauseClosed").Invoke(panel, null);
            Require(group.interactable && group.blocksRaycasts, "Closing must reset interaction.");
            type.GetMethod("OnPauseOpened").Invoke(panel, null);
            Require(!(bool)type.GetProperty("HasChildPage").GetValue(panel), "No stale child page after reopening.");

            // 模拟确认窗口的文本配置，检查共享 prefab 和局内确认使用的节点匹配。
            var confirmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/UI/UIPrefab/MainMenuConfirmScreen.prefab");
            Require(confirmPrefab != null, "Missing confirmation prefab.");
            Require(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/UI/UIPrefab/DesktopSettingsScreen.prefab") != null,
                "Missing settings prefab.");
            var game = runtime.GetType("MoreMountains.Game", true);
            Require(!(bool)game.GetProperty("RunExitPending").GetValue(null), "Unexpected pending exit outside Play mode.");
            game.GetMethod("RequestRunExit").Invoke(null, new object[] { false });
            Require(!(bool)game.GetProperty("RunExitPending").GetValue(null), "Exit requests outside gameplay must be rejected.");
            ValidateExitQueue(game);
            ValidateClockRestoration(runtime, game);
            Debug.Log("[EscPanel] PASS: four prefab buttons, no missing scripts, CanvasGroup, modal interaction, close/reopen, shared pages, deferred transition and duplicate guard, pause clock restoration. No save files changed.");
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static void ValidateExitQueue(Type game)
    {
        var mode = game.GetField("mode");
        var originalMode = mode.GetValue(null);
        var pending = game.GetField("<RunExitPending>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        var restart = game.GetField("restartRequested", BindingFlags.Static | BindingFlags.NonPublic);
        try
        {
            mode.SetValue(null, Enum.Parse(mode.FieldType, "GAMEPLAY"));
            game.GetMethod("RequestRunExit").Invoke(null, new object[] { false });
            Require((bool)game.GetProperty("RunExitPending").GetValue(null), "Gameplay exit must be queued.");
            game.GetMethod("RequestRunExit").Invoke(null, new object[] { true });
            Require(!(bool)restart.GetValue(null), "A second click must not replace a pending return with restart.");
            Require(mode.GetValue(null).Equals(Enum.Parse(mode.FieldType, "GAMEPLAY")), "Queueing must defer the actual transition.");
        }
        finally
        {
            pending.SetValue(null, false);
            restart.SetValue(null, false);
            mode.SetValue(null, originalMode);
        }
    }

    static void ValidateClockRestoration(Assembly runtime, Type game)
    {
        var serviceType = runtime.GetType("MoreMountains.EscPanelService", true);
        var stopClock = game.GetField("stopClock");
        var original = stopClock.GetValue(null);
        try
        {
            foreach (bool previouslyStopped in new[] { false, true })
            {
                var service = Activator.CreateInstance(serviceType);
                serviceType.GetField("_opened", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(service, true);
                serviceType.GetField("previousStopClock", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(service, previouslyStopped);
                stopClock.SetValue(null, true);
                serviceType.GetMethod("Close").Invoke(service, null);
                Require((bool)stopClock.GetValue(null) == previouslyStopped, "Closing must restore the previous clock state.");
                serviceType.GetMethod("Close").Invoke(service, null);
                Require((bool)stopClock.GetValue(null) == previouslyStopped, "Repeated close must be harmless.");
            }
        }
        finally { stopClock.SetValue(null, original); }
    }
}
