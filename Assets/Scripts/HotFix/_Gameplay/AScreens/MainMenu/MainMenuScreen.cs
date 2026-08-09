using UnityEngine;

namespace MoreMountains;

public enum MainMenuType
{
    PLAY, //开始游戏
    RESUME_GAME, //继续
    ABANDON_RUN, //放弃当前游戏
    INFO, //百科大全
    STATS, //统计内容
    SETTINGS, //设定
    PATCH_NOTES, //补丁内容清单
    QUIT, //退出
}

public partial class MainMenuScreen
{
    static UIStrings uiStrings = languagePack.getUIString("MainMenuScreen");
    public static string[] TEXT = uiStrings.TEXT;
    static string VERSION_INFO = Game.VERSION_NUM;
    public string newName;
    public bool isDarken;
    public bool superDarken;
    public Color screenColor = new(0.0F, 0.0F, 0.0F, 0.0F);
    public static float OVERLAY_ALPHA = 0.8F;
    Color overlayColor = new(0.0F, 0.0F, 0.0F, 0.0F);
    public bool fadedOut;
    public bool isFadingOut;
    public int windId;

    // public TitleBackground bg = new();
    // EarlyAccessPopup eaPopup;
    public CurScreen screen = CurScreen.MAIN_MENU;

    // public SaveSlotScreen saveSlotScreen = new();
    // public MenuPanelScreen panelScreen = new();
    // public StatsScreen statsScreen = new();
    // public DailyScreen dailyScreen = new();
    // public CardLibraryScreen cardLibraryScreen = new();
    // public LeaderboardScreen leaderboardsScreen = new();
    // public RelicViewScreen relicScreen = new();
    // public PotionViewScreen potionScreen = new();
    // public CreditsScreen creditsScreen = new();
    // public DoorUnlockScreen doorUnlockScreen = new();
    // public NeowNarrationScreen neowNarrateScreen = new();
    // public PatchNotesScreen patchNotesScreen = new();
    // public RunHistoryScreen runHistoryScreen;
    // public CharacterSelectScreen charSelectScreen = new();
    // public CustomModeScreen customModeScreen = new();
    // public ConfirmPopup abandonPopup = new(ConfirmPopup.ConfirmType.ABANDON_MAIN_MENU);
    // public InputSettingsScreen inputSettingsScreen = new();
    // public OptionsPanel optionPanel = new();
    // public SyncMessage syncMessage = new();
    // public bool isSettingsUp;
    // public ConfirmButton confirmButton = new(TEXT[1]);
    // public MenuCancelButton cancelButton = new();
    // public List<MenuButton> buttons = new();
    public bool abandonedRun;


    public enum CurScreen
    {
        CHAR_SELECT,
        RELIC_VIEW,
        POTION_VIEW,
        BANNER_DECK_VIEW,
        DAILY,
        TRIALS,
        SETTINGS,
        MAIN_MENU,
        SAVE_SLOT,
        STATS,
        RUN_HISTORY,
        CARD_LIBRARY,
        CREDITS,
        PATCH_NOTES,
        NONE,
        LEADERBOARD,
        ABANDON_CONFIRM,
        PANEL_MENU,
        INPUT_SETTINGS,
        CUSTOM,
        NEOW_SCREEN,
        DOOR_UNLOCK
    }

    public override void onCreate()
    {
        base.onCreate();
        Game.publisherIntegration.setRichPresenceDisplayInMenu();
        player = null;
        if (Settings.isDemo && Settings.isShowBuild)
            TipTracker.reset();

        music.changeBGM("MENU");
        if (Settings.AMBIANCE_ON)
            windId = sound.playAndLoop("WIND");
        else
            windId = sound.playAndLoop("WIND", 0.0F);

        UnlockTracker.refresh();
        // cardLibraryScreen.initialize();
        // charSelectScreen.initialize();
        // confirmButton.hide();
        updateAmbienceVolume();
        // runHistoryScreen = new RunHistoryScreen();
    }

    public override void destroy()
    {
        newName = null;
        isDarken = false;
        superDarken = false;
        screenColor = default;
        overlayColor = default;
        fadedOut = false;
        isFadingOut = false;
        windId = 0;
        screen = default;
        abandonedRun = false;
            
        base.destroy();
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (isFadingOut)
        {
            InputHelper.justClickedLeft = false;
            InputHelper.justReleasedClickLeft = false;
            InputHelper.justClickedRight = false;
            InputHelper.justReleasedClickRight = false;
        }

        // abandonPopup.update();
        if (abandonedRun)
        {
            abandonedRun = false;
            setShowPlayButton(true);
        }

        if (Settings.isInfo && DevInputActionSet.deleteSteamCloud.isJustPressed())
            Game.publisherIntegration.deleteAllCloudFiles();

        // syncMessage.update();
        // cancelButton.update();
        // updateSettings();
        // if (screen != CurScreen.SAVE_SLOT)
        //     for (MenuButton b : buttons)
        // b.update();

        /*switch (screen)
        {
            case CurScreen.CHAR_SELECT:
                updateCharSelectController();
                charSelectScreen.update();
                break;
            case CurScreen.CARD_LIBRARY:
                cardLibraryScreen.update();
                break;
            case CurScreen.CUSTOM:
                customModeScreen.update();
                break;
            case CurScreen.PANEL_MENU:
                updateMenuPanelController();
                panelScreen.update();
                break;
            case CurScreen.DAILY:
                dailyScreen.update();
                break;
            case CurScreen.MAIN_MENU:
                updateMenuButtonController();
                break;
            case CurScreen.LEADERBOARD:
                leaderboardsScreen.update();
                break;
            case CurScreen.RELIC_VIEW:
                relicScreen.update();
                break;
            case CurScreen.POTION_VIEW:
                potionScreen.update();
                break;
            case CurScreen.STATS:
                statsScreen.update();
                break;
            case CurScreen.CREDITS:
                creditsScreen.update();
                break;
            case CurScreen.DOOR_UNLOCK:
                doorUnlockScreen.update();
                break;
            case CurScreen.NEOW_SCREEN:
                neowNarrateScreen.update();
                break;
            case CurScreen.PATCH_NOTES:
                patchNotesScreen.update();
                break;
            case CurScreen.RUN_HISTORY:
                runHistoryScreen.update();
                break;
            case CurScreen.INPUT_SETTINGS:
                inputSettingsScreen.update();
                break;
        }*/

        // saveSlotScreen.update();
        // bg.update();
        if (superDarken)
        {
            screenColor.a = MathHelper.popLerpSnap(screenColor.a, 1.0F, dt);
        }
        else if (isDarken)
        {
            screenColor.a = MathHelper.popLerpSnap(screenColor.a, 0.8F, dt);
        }
        else
        {
            screenColor.a = MathHelper.popLerpSnap(screenColor.a, 0.0F, dt);
        }

        // if (!statsScreen.screenUp)
        // updateRenameArea();

        if (!isFadingOut)
            handleInput();

        fadingOut(dt);
    }

    /*
    void updateMenuButtonController()
    {
        if (!Settings.isControllerMode || EarlyAccessPopup.isUp)
            return;
        bool anyHovered = false;
        int index = 0;
        for (MenuButton b :
        buttons)
        {
            if (b.hb.hovered)
            {
                anyHovered = true;
                break;
            }

            index++;
        }
        if (anyHovered)
        {
            if (CInputActionSet.down.isJustPressed() || CInputActionSet.altDown.isJustPressed())
            {
                index--;
                if (index < 0)
                    index = buttons.size() - 1;
                CInputHelper.setCursor(buttons.get(index).hb);
            }
            else if (CInputActionSet.up.isJustPressed() || CInputActionSet.altUp.isJustPressed())
            {
                index++;
                if (index > buttons.size() - 1)
                    index = 0;
                CInputHelper.setCursor(buttons.get(index).hb);
            }
        }
        else
        {
            index = buttons.size() - 1;
            CInputHelper.setCursor(buttons.get(index).hb);
        }
    }
    */

    /*
    void updateCharSelectController()
    {
        if (!Settings.isControllerMode || isFadingOut)
            return;
        bool anyHovered = false;
        int index = 0;
        foreach (var b in charSelectScreen.options)
        {
            if (b.hb.hovered)
            {
                anyHovered = true;
                break;
            }

            index++;
        }

        if (!anyHovered)
        {
            index = 0;
            CInputHelper.setCursor(charSelectScreen.options.get(index).hb);
            charSelectScreen.options.get(index).hb.clicked = true;
        }
        else
        {
            if (CInputActionSet.left.isJustPressed() || CInputActionSet.altLeft.isJustPressed())
            {
                index--;
                if (index < 0)
                    index = charSelectScreen.options.size() - 1;
                CInputHelper.setCursor(charSelectScreen.options.get(index).hb);
                charSelectScreen.options.get(index).hb.clicked = true;
            }
            else if (CInputActionSet.right.isJustPressed() || CInputActionSet.altRight.isJustPressed())
            {
                index++;
                if (index > charSelectScreen.options.size() - 1)
                    index = 0;
                CInputHelper.setCursor(charSelectScreen.options.get(index).hb);
                charSelectScreen.options.get(index).hb.clicked = true;
            }

            if (charSelectScreen.options.get(index).locked)
            {
                charSelectScreen.confirmButton.hide();
            }
            else
            {
                charSelectScreen.confirmButton.show();
            }
        }
    }
    */

    /*
    void updateMenuPanelController()
    {
        if (!Settings.isControllerMode)
            return;
        bool anyHovered = false;
        int index = 0;
        foreach (MainMenuPanelButton b in panelScreen.panels)
        {
            if (b.hb.hovered)
            {
                anyHovered = true;
                break;
            }

            index++;
        }

        if (anyHovered)
        {
            if (CInputActionSet.left.isJustPressed() || CInputActionSet.altLeft.isJustPressed())
            {
                index--;
                if (index < 0)
                    index = panelScreen.panels.size() - 1;
                if (panelScreen.panels.get(index).pColor == MainMenuPanelButton.PanelColor.GRAY)
                    index--;
                CInputHelper.setCursor(panelScreen.panels.get(index).hb);
            }
            else if (CInputActionSet.right.isJustPressed() || CInputActionSet.altRight.isJustPressed())
            {
                index++;
                if (index > panelScreen.panels.size() - 1)
                    index = 0;
                if (panelScreen.panels.get(index).pColor == MainMenuPanelButton.PanelColor.GRAY)
                    index = 0;
                CInputHelper.setCursor(panelScreen.panels.get(index).hb);
            }
        }
        else
        {
            index = 0;
            CInputHelper.setCursor(panelScreen.panels.get(index).hb);
        }
    }
    */

    /*
    void updateSettings()
    {
        if (saveSlotScreen.shown)
            return;
        if (!EarlyAccessPopup.isUp && InputHelper.pressedEscape && screen == CurScreen.MAIN_MENU && !isFadingOut)
            if (!isSettingsUp)
            {
                GameCursor.hidden = false;
                sound.play("END_TURN");
                isSettingsUp = true;
                darken();
                InputHelper.pressedEscape = false;
                statsScreen.hide();
                dailyScreen.hide();
                cancelButton.hide();
                Game.cancelButton.show(TEXT[2]);
                screen = CurScreen.SETTINGS;
                panelScreen.panels.clear();
                hideMenuButtons();
            }
            else if (!EarlyAccessPopup.isUp)
            {
                isSettingsUp = false;
                Game.cancelButton.hide();
                screen = CurScreen.MAIN_MENU;
                if (screen == CurScreen.MAIN_MENU)
                    cancelButton.hide();
            }

        if (isSettingsUp)
            optionPanel.update();
        Game.cancelButton.update();
    }
    */

    /*
    void updateRenameArea()
    {
        if (screen == CurScreen.MAIN_MENU)
            nameEditHb.update();

        if (screen == CurScreen.MAIN_MENU && ((nameEditHb.hovered && InputHelper.justClickedLeft) || CInputActionSet.map.isJustPressed()))
        {
            InputHelper.justClickedLeft = false;
            nameEditHb.hovered = false;
            saveSlotScreen.open(Game.playerName);
            screen = CurScreen.SAVE_SLOT;
        }

        if (bg.slider <= 0.1F && Game.saveSlotPref.getInteger("DEFAULT_SLOT", -1) == -1 && screen == CurScreen.MAIN_MENU)
        {
            if (!setDefaultSlot())
            {
                log("No saves detected, opening Save Slot screen automatically.");
                Game.playerPref.putBoolean("ftuePopupShown", true);
                saveSlotScreen.open(Game.playerName);
                screen = CurScreen.SAVE_SLOT;
            }
        }
    }
    */

    bool setDefaultSlot()
    {
        if (!Game.playerPref.getString("name", "").isEmpty())
        {
            log("Migration to Save Slot schema detected, setting DEFAULT_SLOT to 0.");
            Game.saveSlot = 0;
            Game.saveSlotPref.putInteger("DEFAULT_SLOT", 0);
            Game.saveSlotPref.flush();
            return true;
        }

        return false;
    }

    void handleInput()
    {
        // confirmButton.update();
    }

    public void fadeOutMusic()
    {
        music.fadeOutBGM();
        if (Settings.AMBIANCE_ON)
            sound.fadeOut("WIND", windId);
    }

    void fadingOut(float dt)
    {
        if (isFadingOut && !fadedOut)
        {
            overlayColor.a += dt;
            if (overlayColor.a > 1.0F)
            {
                overlayColor.a = 1.0F;
                fadedOut = true;
            }
        }
        else if (overlayColor.a != 0.0F)
        {
            overlayColor.a -= dt * 2.0F;
            if (overlayColor.a < 0.0F)
            {
                overlayColor.a = 0.0F;
                setOverlapRaycastTarget(false);
            }
        }

        setOverlapColor(overlayColor);
    }

    public void updateAmbienceVolume()
    {
        if (Settings.AMBIANCE_ON)
        {
            sound.adjustVolume("WIND", windId);
        }
        else
        {
            sound.adjustVolume("WIND", windId, 0.0F);
        }
    }

    public void muteAmbienceVolume()
    {
        if (Settings.AMBIANCE_ON)
            sound.adjustVolume("WIND", windId, 0.0F);
    }

    public void unmuteAmbienceVolume()
    {
        sound.adjustVolume("WIND", windId);
    }

    public void darken()
    {
        isDarken = true;
    }

    public void lighten()
    {
        isDarken = false;
    }

    public void fadeOut()
    {
        isFadingOut = true;
        setOverlapRaycastTarget(true);
    }

    public void hideMenuButtons()
    {
        // foreach (MenuButton b in buttons)
        //     b.hide();
    }
    
    protected void onPlayClick()
    {
        if (Settings.seed == 0)
        {
            setRandomSeed();
        }
        else
        {
            Settings.seedSet = true;
        }

        Game.mainMenuScreen.screen = CurScreen.NONE;
        Game.mainMenuScreen.hideMenuButtons();
        Game.mainMenuScreen.darken();
        Game.loadingSave = false;
        Game.chosenCharacter = APlayer.PlayerClass.IRONCLAD;
        Game.mainMenuScreen.fadeOut();
        Game.mainMenuScreen.fadeOutMusic();
        Settings.isDailyRun = false;
        Settings.isTrial = false;
        ModHelper.setModsFalse();
        ADungeon.generateSeeds();

        // if (Game.steelSeries.isEnabled)
        // Game.steelSeries.event_character_chosen(Game.chosenCharacter);

        // if (Settings.isDemo || Settings.isPublisherBuild)
        // {
        //     BotDataUploader poster = new BotDataUploader();
        //     poster.setValues(BotDataUploader.GameDataType.DEMO_EMBARK, null, null);
        //     Thread t = new Thread(poster);
        //     t.setName("LeaderboardPoster");
        //     t.start();
        // }
        return;

        void setRandomSeed()
        {
            long sourceTime = TimeUtility.getNowTimeStampMS();
            Rand rng = new Rand(sourceTime);
            Settings.seedSourceTimestamp = sourceTime;
            Settings.seed = SeedHelper.generateUnoffensiveSeed(rng);
            Settings.seedSet = false;
        }
    }
    
    protected void onResumeGameClick()
    {
        Game.mainMenuScreen.screen = CurScreen.NONE;
        Game.mainMenuScreen.hideMenuButtons();
        Game.mainMenuScreen.darken();
        Game.loadingSave = true;
        Game.chosenCharacter = characterManager.loadChosenCharacter();
        Game.mainMenuScreen.fadeOut();
        Game.mainMenuScreen.fadeOutMusic();
        Settings.isDailyRun = false;
        Settings.isTrial = false;
        ModHelper.setModsFalse();
        // if (Game.steelSeries.isEnabled)
        // Game.steelSeries.event_character_chosen(Game.chosenCharacter);
    }
    
    protected void onQuitClick()
    {
        Application.Quit();
    }
}