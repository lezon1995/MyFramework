using System;
using System.IO;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;
using UIEventSystem = UnityEngine.EventSystems.EventSystem;

namespace MoreMountains;

public partial class MainMenuScreen
{
    public void BeginNewRun() => onPlayClick();
    MainMenuPage activePage;
    CanvasGroup menuInteraction;

    void setMenuInteractable(bool value)
    {
        if (menuInteraction == null)
        {
            var content = mTransform.Find("Content").gameObject;
            menuInteraction = content.MMGetOrAddComponent<CanvasGroup>();
        }
        menuInteraction.interactable = value;
        menuInteraction.blocksRaycasts = value;
    }

    T openPage<T>(CurScreen target) where T : MainMenuPage
    {
        if (isFadingOut || activePage != null) return null;
        var page = LT.LOAD_TOP<T>();
        if (page == null) return null;
        activePage = page;
        screen = target;
        setMenuInteractable(false);
        page.Open(() =>
        {
            activePage = null;
            screen = CurScreen.MAIN_MENU;
            setMenuInteractable(true);
            refreshMenuVisibility();
        });
        return page;
    }

    void onAbandonClick()
    {
        if (!characterManager.anySaveFileExists()) { refreshMenuVisibility(); return; }
        // 捕获点击时的角色和槽位路径；不触碰角色解锁、累计统计或其他槽位。
        var path = SaveAndContinue.getPlayerSavePath(characterManager.loadChosenCharacter().ToString());
        var popup = openPage<MainMenuConfirmScreen>(CurScreen.ABANDON_CONFIRM);
        if (popup == null) return;
        popup.Configure(MenuPreferences.IsChinese ? "放弃本局？" : "Abandon this run?",
            MenuPreferences.IsChinese
                ? "这将删除当前角色本局的存档和备份，无法撤销。\n角色解锁与累计统计将保留。"
                : "This deletes the current character's run save and backup permanently.\nUnlocks and lifetime statistics are kept.",
            () =>
            {
                try
                {
                    SaveAndContinue.deleteSave(path);
                    if (File.Exists(path) || File.Exists(path + ".backUp"))
                        throw new IOException("Save files still exist.");
                    Game.loadingSave = false;
                    Settings.seed = 0;
                    Settings.seedSet = false;
                    popup.close();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Could not abandon run: " + exception.Message);
                    popup.ShowError(MenuPreferences.IsChinese ? "删除失败，请检查存档文件权限后重试。" : "Unable to delete save. Check file permissions and retry.");
                }
            });
    }

    void confirmQuit()
    {
        var popup = openPage<MainMenuConfirmScreen>(CurScreen.NONE);
        if (popup == null) return;
        popup.Configure(MenuPreferences.IsChinese ? "退出游戏？" : "Quit game?",
            MenuPreferences.IsChinese ? "确定退出到桌面？" : "Return to desktop?", () =>
            {
                PlayerPrefs.Save();
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
    }

    public override bool onESCDown()
    {
        // 最上层页面会优先消费 Esc；不在这里再次关闭同一页面。
        if (activePage == null && !isFadingOut)
            openPage<DesktopSettingsScreen>(CurScreen.SETTINGS);
        return true;
    }

    void ensureMenuSelection()
    {
        if (activePage != null || isFadingOut || !isVisible()) return;
        var selected = UIEventSystem.current?.currentSelectedGameObject;
        if (selected != null && selected.activeInHierarchy && selected.transform.IsChildOf(mTransform)) return;
        var button = mTransform.Find("Content").GetComponentInChildren<Button>();
        if (button != null) button.Select();
    }

    public override void onHide()
    {
        if (activePage != null) activePage.close();
        base.onHide();
    }
}
