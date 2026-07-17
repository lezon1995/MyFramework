using System.Collections.Generic;
using Obfuz;
using UnityEngine;

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
    }

    public void addButton(MainMenuType type)
    {
        var item = MainMenuButtonPool.newItem();
        item.getRoot().setName(type.ToString());
        item.setName(type.ToString());
        switch (type)
        {
            case MainMenuType.PLAY:
                item.setOnClick(onPlayClick);
                break;
            case MainMenuType.RESUME_GAME:
                item.setOnClick(onResumeGameClick);
                break;
            case MainMenuType.ABANDON_RUN:
                break;
            case MainMenuType.INFO:
                break;
            case MainMenuType.STAT:
                break;
            case MainMenuType.SETTINGS:
                break;
            case MainMenuType.PATCH_NOTES:
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
        buttons[MainMenuType.STAT].setActive(show);
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
        addButton(MainMenuType.STAT);
        addButton(MainMenuType.INFO);
        addButton(MainMenuType.SETTINGS);
        addButton(MainMenuType.QUIT);
        addButton(MainMenuType.PATCH_NOTES);

        setShowPlayButton(!Game.characterManager.anySaveFileExists());
        setShowStatAndInfoButton(!Settings.isShowBuild /* && statsScreen.statScreenUnlocked()*/);
        setShowQuitAndPatchButton(!Settings.isMobile && !Settings.isConsoleBuild);
    }
}