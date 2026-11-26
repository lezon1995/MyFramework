using System.Collections.Generic;
using Obfuz;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/MainMenuScreen.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class MainMenuScreen : LayoutScript
// auto generate classname end
{
    // auto generate member start
    protected myUGUIImageSimple overlay;
    protected myUGUIText debugText;
    protected WindowStructPool<MainMenuButton> MainMenuButtonPool;
    // auto generate member end

    protected Dictionary<MainMenuType, MainMenuButton> buttons = new();

    public MainMenuScreen()
    {
        // auto generate constructor start
        MainMenuButtonPool = new(this);
        // auto generate constructor end
        mNeedUpdate = false;
        mUnuseAllWhenHide = false;
    }

    public override void assignWindow()
    {
        // auto generate assignWindow start
        newObject(out overlay, "Overlay");
        newObject(out debugText, "DebugText");
        MainMenuButtonPool.assignTemplate(mRoot, "Content/MenuButtons/V/MainMenuButton");
        // auto generate assignWindow end
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end
        setMainMenuButtons();
    }

    public override void onGameState()
    {
        base.onGameState();
        screen = CurScreen.MAIN_MENU;
        isFadingOut = fadedOut = isDarken = superDarken = false;
        setOverlapRaycastTarget(false);
        setMenuInteractable(true);
        foreach (var button in buttons.Values)
            button.setActive(true);
        refreshMenuVisibility();
    }

    public void addButton(MainMenuType type)
    {
        var item = MainMenuButtonPool.newItem();
        item.getRoot().setName(type.ToString());
        item.setStringReference("MainMenuScreen", type.ToString());
        switch (type)
        {
            case MainMenuType.PLAY:
                item.setOnClick(onPlayClick);
                break;
            case MainMenuType.RESUME_GAME:
                item.setOnClick(onResumeGameClick);
                break;
            case MainMenuType.ABANDON_RUN:
                item.setOnClick(onAbandonClick);
                break;
            case MainMenuType.INFO:
                item.setOnClick(() => openPage<EncyclopediaScreen>(CurScreen.CARD_LIBRARY));
                break;
            case MainMenuType.STATS:
                item.setOnClick(() => openPage<StatisticsScreen>(CurScreen.STATS));
                break;
            case MainMenuType.SETTINGS:
                item.setOnClick(() => openPage<DesktopSettingsScreen>(CurScreen.SETTINGS));
                break;
            case MainMenuType.PATCH_NOTES:
                item.setOnClick(() => openPage<PatchNotesScreen>(CurScreen.PATCH_NOTES));
                break;
            case MainMenuType.QUIT:
                item.setOnClick(onQuitClick);
                break;
        }
        buttons[type] = item;
    }

    public void setShowPlayButton(bool show)
    {
        buttons[MainMenuType.PLAY].setActive(show);
        buttons[MainMenuType.ABANDON_RUN].setActive(!show);
        buttons[MainMenuType.RESUME_GAME].setActive(!show);
    }

    public void setShowStatAndInfoButton(bool show)
    {
        buttons[MainMenuType.STATS].setActive(show);
        buttons[MainMenuType.INFO].setActive(show);
    }

    public void setShowQuitAndPatchButton(bool show)
    {
        buttons[MainMenuType.QUIT].setActive(show);
        buttons[MainMenuType.PATCH_NOTES].setActive(show);
    }

    public void setOverlapColor(Color color)
    {
        overlay.setColor(color);
    }

    public void setOverlapRaycastTarget(bool b)
    {
        overlay.setUGUIRaycastTarget(b);
    }

    void setMainMenuButtons()
    {
        addButton(MainMenuType.ABANDON_RUN);
        addButton(MainMenuType.RESUME_GAME);
        addButton(MainMenuType.PLAY);
        addButton(MainMenuType.STATS);
        addButton(MainMenuType.INFO);
        addButton(MainMenuType.SETTINGS);
        addButton(MainMenuType.QUIT);
        addButton(MainMenuType.PATCH_NOTES);

        refreshMenuVisibility();
    }

    void refreshMenuVisibility()
    {
        setShowPlayButton(!characterManager.anySaveFileExists());
        setShowStatAndInfoButton(!Settings.isShowBuild /* && statsScreen.statScreenUnlocked()*/);
        setShowQuitAndPatchButton(!Settings.isMobile && !Settings.isConsoleBuild);
    }
}
