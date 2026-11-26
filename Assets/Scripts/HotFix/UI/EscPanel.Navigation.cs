using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UIEventSystem = UnityEngine.EventSystems.EventSystem;

namespace MoreMountains;

public partial class EscPanel
{
    MainMenuPage childPage;
    CanvasGroup interaction;
    public bool HasChildPage => childPage != null;
    static string MenuText(string zh, string en) => MenuPreferences.IsChinese ? zh : en;

    void SetInteraction(bool enabled)
    {
        if (interaction == null)
        {
            interaction = mRoot.getGameObject().GetComponent<CanvasGroup>();
            if (interaction == null) interaction = mRoot.getGameObject().AddComponent<CanvasGroup>();
        }
        interaction.interactable = enabled;
        interaction.blocksRaycasts = enabled;
    }

    public void OnPauseOpened()
    {
        SetInteraction(true);
        mTransform.Find("MenuOptionView/V/BtnContinue").GetComponent<Button>().Select();
    }

    public void OnPauseClosed()
    {
        if (childPage != null) childPage.close();
        childPage = null;
        SetInteraction(true);
        if (_blockerGO != null) _blockerGO.SetActive(false);
        var selected = UIEventSystem.current?.currentSelectedGameObject;
        if (selected != null && selected.transform.IsChildOf(mTransform))
            UIEventSystem.current.SetSelectedGameObject(null);
    }

    T OpenChild<T>() where T : MainMenuPage
    {
        if (!EscPanelService.Instance.Opened || childPage != null || Game.RunExitPending) return null;
        var page = LT.LOAD_TOP<T>();
        if (page == null) return null;
        childPage = page;
        SetInteraction(false);
        page.Open(() =>
        {
            childPage = null;
            SetInteraction(true);
        });
        return page;
    }

    public void ShowSettings() => OpenChild<DesktopSettingsScreen>();

    public void ShowRestartConfirmation()
    {
        var popup = OpenChild<MainMenuConfirmScreen>();
        if (popup == null) return;
        popup.Configure(MenuText("重新开始？", "Restart run?"),
            MenuText("将放弃当前进度并删除本角色的本局存档，重新进入角色选择。\n角色解锁和累计统计将保留。",
                "Discard this run and delete its save, then choose a character for a new run.\nUnlocks and lifetime statistics are kept."),
            () => RequestExit(popup, true));
    }

    public void ShowReturnConfirmation()
    {
        var popup = OpenChild<MainMenuConfirmScreen>();
        if (popup == null) return;
        popup.Configure(MenuText("返回主菜单？", "Return to main menu?"),
            MenuText("当前战斗和购物进度不会保存。已有自动存档会保留，可以从主菜单继续。\n没有自动存档时，需要重新开始。",
                "Current combat and shopping progress will not be saved. Existing autosaves are kept for Continue.\nWithout an autosave, a new run is required."),
            () => RequestExit(popup, false));
    }

    void RequestExit(MainMenuConfirmScreen popup, bool restart)
    {
        if (Game.RunExitPending) return;
        try
        {
            if (Game.mode != Game.GameMode.GAMEPLAY) throw new InvalidOperationException("Run is no longer active.");
            var current = EscPanelService.Instance.CurrentPlayer;
            if (current == null) throw new InvalidOperationException("No active player.");
            if (restart)
            {
                var path = SaveAndContinue.getPlayerSavePath(current.chosenClass.ToString());
                SaveAndContinue.deleteSave(path);
                if (File.Exists(path) || File.Exists(path + ".backUp")) throw new IOException("Save still exists.");
            }
            // 下一次 Game.update 执行切换，避免在按钮回调中销毁正在遍历的 UI/房间。
            Game.RequestRunExit(restart);
            popup.close();
            SetInteraction(false);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Run exit failed: " + exception.Message);
            popup.ShowError(MenuText("操作失败，请检查存档权限后重试。", "Unable to continue. Check save permissions and retry."));
        }
    }

    public override bool onESCDown()
    {
        if (!EscPanelService.Instance.Opened) return false;
        if (!HasChildPage && !Game.RunExitPending && MainMenuPage.LastBackFrame != Time.frameCount)
            EscPanelService.Instance.Close();
        return true;
    }

    public override void destroy()
    {
        if (EscPanelService.Instance.Binder == binder)
            EscPanelService.Instance.Unregister();
        if (_blockerGO != null) UnityEngine.Object.Destroy(_blockerGO);
        childPage = null;
        interaction = null;
        base.destroy();
    }
}
