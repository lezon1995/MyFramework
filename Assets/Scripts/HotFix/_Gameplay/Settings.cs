using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero
{
    public enum GameLanguage
    {
        ENG,
        DUT,
        EPO,
        PTB,
        ZHS,
        ZHT,
        FIN,
        FRA,
        DEU,
        GRE,
        IND,
        ITA,
        JPN,
        KOR,
        NOR,
        POL,
        RUS,
        SPA,
        SRP,
        SRB,
        THA,
        TUR,
        UKR,
        VIE,
        WWW
    }
    /// Framework options. Helps editor methods to put references in the Inspector automatically.
    // Uncomment this option in case of unintentional deletion from the options file.
    public static class Settings
    {
        public static bool isDev = true;
        public static bool isBeta;
        public static bool isAlpha;
        public static bool isModded;
        public static bool isControllerMode;
        public static bool isMobile;
        public static bool testFonts;
        public static bool isDebug = true;
        public static bool isInfo;
        public static bool isTestingNeow;
        public static bool usesTrophies;
        public static bool isConsoleBuild;
        public static bool usesProfileSaves;
        public static bool isTouchScreen;
        public static bool isDemo;
        public static bool isShowBuild;
        public static bool isPublisherBuild;
        public static GameLanguage language;
        public static bool lineBreakViaCharacter;
        public static bool usesOrdinal = true;
        public static bool leftAlignCards;
        public static bool manualLineBreak;
        public static bool removeAtoZSort;
        public static bool manualAndAutoLineBreak;
        public static Prefs soundPref;
        public static Prefs dailyPref;
        public static Prefs gamePref;
        public static bool isDailyRun;
        public static bool hasDoneDailyToday;
        public static long dailyDate;
        public static long totalPlayTime;
        public static bool isFinalActAvailable;
        public static bool hasRubyKey;
        public static bool hasEmeraldKey;
        public static bool hasSapphireKey;
        public static bool isEndless;
        public static bool isTrial;
        public static long specialSeed;
        public static string trialName;
        public static bool IS_FULLSCREEN { get; private set; }
        public static bool IS_WINDOWS_FULLSCREEN { get; private set; }
        public static bool IS_V_SYNC { get; private set; }
        public static int MAX_FPS { get; private set; }
        public static int M_W { get; private set; }
        public static int M_H { get; private set; }
        public static int SAVED_WIDTH { get; private set; }
        public static int SAVED_HEIGHT { get; private set; }
        public static int WIDTH { get; private set; }
        public static int HEIGHT { get; private set; }
        public static bool is16x10, is4x3, isTwoSixteen;
        public static bool isLetterbox;
        public static int HORIZ_LETTERBOX_AMT;
        public static int VERT_LETTERBOX_AMT;
        // public static List<DisplayOption> displayOptions;
        public static int displayIndex;
        public static float scale { get; private set; }
        public static float renderScale { get; private set; }
        public static float xScale { get; private set; }
        public static float yScale { get; private set; }
        public static float FOUR_BY_THREE_OFFSET_Y;
        public static float LETTERBOX_OFFSET_Y;
        public static long seed;
        public static bool seedSet;
        public static long seedSourceTimestamp;
        public static bool isBackgrounded;
        public static float bgVolume;
        public static string MASTER_VOLUME_PREF = "Master Volume";
        public static string MUSIC_VOLUME_PREF = "Music Volume";
        public static string SOUND_VOLUME_PREF = "Sound Volume";
        public static string AMBIENCE_ON_PREF = "Ambience On";
        public static string MUTE_IF_BG_PREF = "Mute in Bg";
        public static float DEFAULT_MASTER_VOLUME = 0.5F;
        public static float DEFAULT_MUSIC_VOLUME = 0.5F;
        public static float DEFAULT_SOUND_VOLUME = 0.5F;
        public static float MASTER_VOLUME;
        public static float MUSIC_VOLUME;
        public static float SOUND_VOLUME;
        public static bool AMBIANCE_ON;
        public static string SCREEN_SHAKE_PREF = "Screen Shake";
        public static string SUM_DMG_PREF = "Summed Damage";
        public static string BLOCKED_DMG_PREF = "Blocked Damage";
        public static string HAND_CONF_PREF = "Hand Confirmation";
        public static string EFFECTS_PREF = "Particle Effects";
        public static string FAST_MODE_PREF = "Fast Mode";
        public static string UPLOAD_PREF = "Upload Data";
        public static string PLAYTESTER_ART = "Playtester Art";
        public static string SHOW_CARD_HOTKEYS_PREF = "Show Card keys";
        public static string BIG_TEXT_PREF = "Bigger Text";
        public static string LONG_PRESS_PREF = "Long-press Enabled";
        public static string CONTROLLER_ENABLED_PREF = "Controller Enabled";
        public static string TOUCHSCREEN_ENABLED_PREF = "Touchscreen Enabled";
        public static string LAST_DAILY = "LAST_DAILY";
        public static bool SHOW_DMG_SUM;
        public static bool SHOW_DMG_BLOCK;
        public static bool FAST_HAND_CONF;
        public static bool FAST_MODE;
        public static bool CONTROLLER_ENABLED;
        public static bool TOUCHSCREEN_ENABLED;
        public static bool DISABLE_EFFECTS;
        public static bool UPLOAD_DATA;
        public static bool SCREEN_SHAKE;
        public static bool PLAYTESTER_ART_MODE;
        public static bool SHOW_CARD_HOTKEYS;
        public static bool USE_LONG_PRESS;
        public static bool BIG_TEXT_MODE;
        //public static Color CREAM_COLOR = new Color(-597249);
        //public static Color LIGHT_YELLOW_COLOR = new Color(-1202177);
        //public static Color RED_TEXT_COLOR = new Color(-10132481);
        //public static Color GREEN_TEXT_COLOR = new Color(2147418367);
        //public static Color BLUE_TEXT_COLOR = new Color(-2016482305);
        //public static Color GOLD_COLOR = new Color(-272084481);
        //public static Color PURPLE_COLOR = new Color(-293409025);
        // public static Color TOP_PANEL_SHADOW_COLOR = new Color(64);
        public static Color HALF_TRANSPARENT_WHITE_COLOR = new Color(1F, 1F, 1F, 0.5F);
        public static Color QUARTER_TRANSPARENT_WHITE_COLOR = new Color(1F, 1F, 1F, 0.25F);
        public static Color TWO_THIRDS_TRANSPARENT_BLACK_COLOR = new Color(0F, 0F, 0F, 0.66F);
        public static Color HALF_TRANSPARENT_BLACK_COLOR = new Color(0F, 0F, 0F, 0.5F);
        public static Color QUARTER_TRANSPARENT_BLACK_COLOR = new Color(0F, 0F, 0F, 0.25F);
        //public static Color RED_RELIC_COLOR = new Color(-10132545);
        //public static Color GREEN_RELIC_COLOR = new Color(2147418303);
        //public static Color BLUE_RELIC_COLOR = new Color(-2016482369);
        //public static Color PURPLE_RELIC_COLOR = new Color(-935526465);
        public static float POST_ATTACK_WAIT_DUR = 0.1F;
        public static float WAIT_BEFORE_BATTLE_TIME = 1F;
        public static float ACTION_DUR_XFAST = 0.1F;
        public static float ACTION_DUR_FASTER = 0.2F;
        public static float ACTION_DUR_FAST = 0.25F;
        public static float ACTION_DUR_MED = 0.5F;
        public static float ACTION_DUR_LONG = 1F;
        public static float ACTION_DUR_XLONG = 1.5F;
        public static float CARD_DROP_END_Y;
        public static float SCROLL_SPEED;
        public static float MAP_SCROLL_SPEED;
        public static float SCROLL_LERP_SPEED = 12F;
        public static float SCROLL_SNAP_BACK_SPEED = 10F;
        public static float DEFAULT_SCROLL_LIMIT;
        public static float MAP_DST_Y;
        public static float CLICK_SPEED_THRESHOLD = 0.4F;
        public static float CLICK_DIST_THRESHOLD;
        public static float POTION_W;
        public static float POTION_Y;
        // public static Color DISCARD_COLOR = Color.valueOf("8a769bff");
        // public static Color DISCARD_GLOW_COLOR = Color.valueOf("553a66ff");
        public static Color SHADOW_COLOR = new Color(0F, 0F, 0F, 0.5F);
        public static float CARD_SOUL_SCALE = 0.12F;
        public static float CARD_LERP_SPEED = 6F;
        public static float CARD_SNAP_THRESHOLD;
        public static float UI_SNAP_THRESHOLD;
        public static float CARD_SCALE_LERP_SPEED = 7.5F;
        public static float CARD_SCALE_SNAP_THRESHOLD = 0.003F;
        public static float UI_LERP_SPEED = 9F;
        public static float ORB_LERP_SPEED = 6F;
        public static float MOUSE_LERP_SPEED = 20F;
        public static float POP_LERP_SPEED = 8F;
        public static float FADE_LERP_SPEED = 12F;
        public static float SLOW_COLOR_LERP_SPEED = 3F;
        public static float FADE_SNAP_THRESHOLD = 0.01F;
        public static float ROTATE_LERP_SPEED = 12F;
        public static float SCALE_SNAP_THRESHOLD = 0.003F;
        public static float HOVER_BUTTON_RISE_AMOUNT;
        public static float CARD_VIEW_SCALE = 0.75F;
        public static float CARD_VIEW_PAD_X;
        public static float CARD_VIEW_PAD_Y;
        public static float OPTION_Y;
        public static float EVENT_Y;
        public static int MAX_ASCENSION_LEVEL = 20;
        public static float POST_COMBAT_WAIT_TIME = 0.25F;
        public static int MAX_HAND_SIZE = 10;
        public static int NUM_POTIONS = 3;
        public static int NORMAL_POTION_DROP_RATE = 40;
        public static int ELITE_POTION_DROP_RATE = 40;
        public static int BOSS_GOLD_AMT = 100;
        public static int BOSS_GOLD_JITTER = 5;
        public static int ACHIEVEMENT_COUNT = 46;
        public static int NORMAL_RARE_DROP_RATE = 3;
        public static int NORMAL_UNCOMMON_DROP_RATE = 40;
        public static int ELITE_RARE_DROP_RATE = 10;
        public static int ELITE_UNCOMMON_DROP_RATE = 50;
        public static int UNLOCK_PER_CHAR_COUNT = 5;
        public static bool hideTopBar;
        public static bool hidePopupDetails;
        public static bool hideRelics;
        public static bool hideLowerElements;
        public static bool hideCards;
        public static bool hideEndTurn;
        public static bool hideCombatElements;
        public static string SENDTODEVS = "sendToDevs";


        public static void initialize(bool reloaded)
        {
            // if (!reloaded)
            //     initializeDisplay();
            initializeSoundPref();
            initializeGamePref(reloaded);
        }

        /*static void initializeDisplay()
        {
            log("Initializing display settings...");
            DisplayConfig config = DisplayConfig.readConfig();
            M_W = Screen.width;
            M_H = Screen.height;
            WIDTH = config.getWidth();
            HEIGHT = config.getHeight();
            MAX_FPS = config.getMaxFPS();
            SAVED_WIDTH = WIDTH;
            SAVED_HEIGHT = HEIGHT;
            IS_FULLSCREEN = config.getIsFullscreen();
            IS_WINDOWS_FULLSCREEN = config.getWFS();
            IS_V_SYNC = config.getIsVsync();
            float aspectRatio = (float)WIDTH / HEIGHT;
            bool isUltraWide = false;
            isLetterbox = aspectRatio is > 2.34F or < 1.3332F;
            switch (aspectRatio)
            {
                case > 1.32F and < 1.34F:
                    is4x3 = true;
                    break;
                case > 1.59F and < 1.61F:
                    is16x10 = true;
                    break;
                case >= 1.78F:
                {
                    if (aspectRatio > 1.78F)
                        isUltraWide = true;
                    break;
                }
            }

            if (isLetterbox)
            {
                switch (aspectRatio)
                {
                    case < 1.333F:
                        HEIGHT = MathUtils.round(WIDTH * 0.75F);
                        HORIZ_LETTERBOX_AMT = (M_H - HEIGHT) / 2;
                        HORIZ_LETTERBOX_AMT += 2;
                        scale = WIDTH / 1920F;
                        xScale = scale;
                        renderScale = scale;
                        yScale = HEIGHT / 1080F;
                        is4x3 = true;
                        break;
                    case > 2.34F:
                        WIDTH = MathUtils.round(HEIGHT * 2.3333F);
                        VERT_LETTERBOX_AMT = (M_W - WIDTH) / 2;
                        VERT_LETTERBOX_AMT++;
                        scale = (int)(HEIGHT * 1.77778F) / 1920F;
                        xScale = WIDTH / 1920F;
                        renderScale = xScale;
                        yScale = scale;
                        setXOffset();
                        break;
                }
            }
            else if (is4x3)
            {
                scale = WIDTH / 1920F;
                xScale = scale;
                yScale = HEIGHT / 1080F;
                renderScale = yScale;
            }
            else if (isUltraWide)
            {
                scale = (int)(HEIGHT * 1.7777778F) / 1920F;
                xScale = WIDTH / 1920F;
                renderScale = xScale;
                yScale = scale;
                setXOffset();
                isLetterbox = true;
            }
            else
            {
                scale = WIDTH / 1920F;
                xScale = scale;
                yScale = scale;
                renderScale = scale;
            }

            SCROLL_SPEED = 75F * scale;
            MAP_SCROLL_SPEED = 75F * scale;
            DEFAULT_SCROLL_LIMIT = 50F * yScale;
            MAP_DST_Y = 150F * scale;
            CLICK_DIST_THRESHOLD = 30F * scale;
            CARD_DROP_END_Y = HEIGHT * 0.81F;
            POTION_W = isMobile ? (64F * scale) : (56F * scale);
            POTION_Y = isMobile ? (HEIGHT - 42F * scale) : (HEIGHT - 30F * scale);
            OPTION_Y = HEIGHT / 2F - 32F * yScale;
            EVENT_Y = HEIGHT / 2F - 128F * scale;
            CARD_VIEW_PAD_X = 40F * scale;
            CARD_VIEW_PAD_Y = 40F * scale;
            HOVER_BUTTON_RISE_AMOUNT = 8F * scale;
            CARD_SNAP_THRESHOLD = scale;
            UI_SNAP_THRESHOLD = scale;
            FOUR_BY_THREE_OFFSET_Y = 140F * yScale;
        }*/

        static void setXOffset()
        {
            if (scale == 1F)
            {
                LETTERBOX_OFFSET_Y = 0F;
                return;
            }

            float offsetScale = xScale - 1F;
            if (offsetScale < 0F)
            {
                LETTERBOX_OFFSET_Y = 0F;
                return;
            }

            LETTERBOX_OFFSET_Y = (WIDTH - 1920) * offsetScale;
        }

        static void initializeSoundPref()
        {
            log("Initializing sound settings...");
            soundPref = SaveHelper.getPrefs("Sound");
            try
            {
                soundPref.getBoolean("Ambience On");
                soundPref.getBoolean("Mute in Bg");
            }
            catch (Exception)
            {
                soundPref.putBoolean("Ambience On", soundPref.getBoolean("Ambience On", true));
                soundPref.putBoolean("Mute in Bg", soundPref.getBoolean("Mute in Bg", true));
                soundPref.flush();
            }

            AMBIANCE_ON = soundPref.getBoolean("Ambience On", true);
            Game.MUTE_IF_BG = soundPref.getBoolean("Mute in Bg", true);
        }

        static void initializeGamePref(bool reloaded)
        {
            log("Initializing game settings...");
            gamePref = SaveHelper.getPrefs("GameplaySettings");
            dailyPref = SaveHelper.getPrefs("Daily");
            try
            {
                gamePref.getBoolean("Summed Damage");
                gamePref.getBoolean("Blocked Damage");
                gamePref.getBoolean("Hand Confirmation");
                gamePref.getBoolean("Upload Data");
                gamePref.getBoolean("Particle Effects");
                gamePref.getBoolean("Fast Mode");
                gamePref.getBoolean("Show Card keys");
                gamePref.getBoolean("Bigger Text");
                gamePref.getBoolean("Long-press Enabled");
                gamePref.getBoolean("Screen Shake");
                gamePref.getBoolean("Playtester Art");
                gamePref.getBoolean("Controller Enabled");
                gamePref.getBoolean("Touchscreen Enabled");
            }
            catch (Exception e)
            {
                gamePref.putBoolean("Summed Damage", gamePref.getBoolean("Summed Damage", false));
                gamePref.putBoolean("Blocked Damage", gamePref.getBoolean("Blocked Damage", false));
                gamePref.putBoolean("Hand Confirmation", gamePref.getBoolean("Hand Confirmation", false));
                gamePref.putBoolean("Upload Data", gamePref.getBoolean("Upload Data", true));
                gamePref.putBoolean("Particle Effects", gamePref.getBoolean("Particle Effects", false));
                gamePref.putBoolean("Fast Mode", gamePref.getBoolean("Fast Mode", false));
                gamePref.putBoolean("Show Card keys", gamePref.getBoolean("Show Card keys", false));
                gamePref.putBoolean("Bigger Text", gamePref.getBoolean("Bigger Text", false));
                gamePref.putBoolean("Long-press Enabled", gamePref.getBoolean("Long-press Enabled", false));
                gamePref.putBoolean("Screen Shake", gamePref.getBoolean("Screen Shake", true));
                gamePref.putBoolean("Playtester Art", gamePref.getBoolean("Playtester Art", false));
                gamePref.putBoolean("Controller Enabled", gamePref.getBoolean("Controller Enabled", true));
                gamePref.putBoolean("Touchscreen Enabled", gamePref.getBoolean("Touchscreen Enabled", false));
                if (!reloaded)
                    setLanguage(gamePref.getString("LANGUAGE", GameLanguage.ENG.ToString()), true);
                gamePref.flush();
            }

            SHOW_DMG_SUM = gamePref.getBoolean("Summed Damage", false);
            SHOW_DMG_BLOCK = gamePref.getBoolean("Blocked Damage", false);
            FAST_HAND_CONF = gamePref.getBoolean("Hand Confirmation", false);
            UPLOAD_DATA = gamePref.getBoolean("Upload Data", true);
            DISABLE_EFFECTS = gamePref.getBoolean("Particle Effects", false);
            FAST_MODE = gamePref.getBoolean("Fast Mode", false);
            SHOW_CARD_HOTKEYS = gamePref.getBoolean("Show Card keys", false);
            BIG_TEXT_MODE = gamePref.getBoolean("Bigger Text", false);
            USE_LONG_PRESS = gamePref.getBoolean("Long-press Enabled", false);
            SCREEN_SHAKE = gamePref.getBoolean("Screen Shake", true);
            PLAYTESTER_ART_MODE = gamePref.getBoolean("Playtester Art", false);
            CONTROLLER_ENABLED = gamePref.getBoolean("Controller Enabled", true);
            TOUCHSCREEN_ENABLED = gamePref.getBoolean("Touchscreen Enabled", false);

            if (TOUCHSCREEN_ENABLED || isConsoleBuild)
                isTouchScreen = true;
            if (!reloaded)
                setLanguage(gamePref.getString("LANGUAGE", GameLanguage.ZHS.ToString()), true);
        }

        public static void setLanguage(GameLanguage key, bool initial)
        {
            language = key;
            if (initial)
            {
                switch (language)
                {
                    case GameLanguage.ZHS:
                    case GameLanguage.ZHT:
                        manualAndAutoLineBreak = true;
                        lineBreakViaCharacter = true;
                        usesOrdinal = false;
                        removeAtoZSort = true;
                        break;
                    case GameLanguage.JPN:
                        lineBreakViaCharacter = true;
                        usesOrdinal = false;
                        if (isConsoleBuild)
                        {
                            manualLineBreak = true;
                            leftAlignCards = true;
                        }
                        else
                        {
                            manualAndAutoLineBreak = true;
                            manualLineBreak = false;
                            leftAlignCards = false;
                        }

                        removeAtoZSort = true;
                        break;
                    case GameLanguage.ENG:
                        lineBreakViaCharacter = false;
                        usesOrdinal = true;
                        break;
                    case GameLanguage.DUT:
                    case GameLanguage.EPO:
                    case GameLanguage.PTB:
                    case GameLanguage.FIN:
                    case GameLanguage.FRA:
                    case GameLanguage.DEU:
                    case GameLanguage.GRE:
                    case GameLanguage.IND:
                    case GameLanguage.ITA:
                    case GameLanguage.KOR:
                    case GameLanguage.NOR:
                    case GameLanguage.POL:
                    case GameLanguage.RUS:
                    case GameLanguage.SPA:
                    case GameLanguage.SRP:
                    case GameLanguage.SRB:
                    case GameLanguage.THA:
                    case GameLanguage.UKR:
                    case GameLanguage.TUR:
                    case GameLanguage.VIE:
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                        break;
                    default:
                        logError("[ERROR] Unspecified language: " + key);
                        lineBreakViaCharacter = false;
                        usesOrdinal = true;
                        break;
                }
            }

            gamePref.putString("LANGUAGE", key.ToString());
        }

        public static void setLanguage(string langStr, bool initial)
        {
            try
            {
                var langKey = Enum.Parse<GameLanguage>(langStr, true);
                setLanguage(langKey, initial);
            }
            catch (Exception)
            {
                setLanguageLegacy(langStr, initial);
            }
        }

        public static void setLanguageLegacy(string key, bool initial)
        {
            GameLanguage lang;
            switch (key)
            {
                case "English":
                    lang = GameLanguage.ENG;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = true;
                    }

                    break;
                case "Brazilian Portuguese":
                    lang = GameLanguage.PTB;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Chinese (Simplified)":
                    lang = GameLanguage.ZHS;
                    if (initial)
                    {
                        lineBreakViaCharacter = true;
                        usesOrdinal = false;
                    }

                    break;
                case "Chinese (Traditional)":
                    lang = GameLanguage.ZHT;
                    if (initial)
                    {
                        lineBreakViaCharacter = true;
                        usesOrdinal = false;
                    }

                    break;
                case "Finnish":
                    lang = GameLanguage.FIN;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "French":
                    lang = GameLanguage.FRA;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "German":
                    lang = GameLanguage.DEU;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Greek":
                    lang = GameLanguage.GRE;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Italian":
                    lang = GameLanguage.ITA;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Indonesian":
                    lang = GameLanguage.IND;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Japanese":
                    lang = GameLanguage.JPN;
                    if (initial)
                    {
                        lineBreakViaCharacter = true;
                        usesOrdinal = false;
                    }

                    break;
                case "Korean":
                    lang = GameLanguage.KOR;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Norwegian":
                    lang = GameLanguage.NOR;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Polish":
                    lang = GameLanguage.POL;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Russian":
                    lang = GameLanguage.RUS;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Spanish":
                    lang = GameLanguage.SPA;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Serbian-Cyrillic":
                    lang = GameLanguage.SRP;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Serbian-Latin":
                    lang = GameLanguage.SRB;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Thai":
                    lang = GameLanguage.THA;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Turkish":
                    lang = GameLanguage.TUR;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Ukrainian":
                    lang = GameLanguage.UKR;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                case "Vietnamese":
                    lang = GameLanguage.VIE;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = false;
                    }

                    break;
                default:
                    lang = GameLanguage.ENG;
                    if (initial)
                    {
                        lineBreakViaCharacter = false;
                        usesOrdinal = true;
                    }

                    break;
            }

            language = lang;
            gamePref.putString("LANGUAGE", key);
        }

        public static bool isStandardRun()
        {
            return !isDailyRun && !isTrial && !seedSet;
        }

        public static bool treatEverythingAsUnlocked()
        {
            return isDailyRun || isTrial;
        }

        public static void setFinalActAvailability()
        {
            isFinalActAvailable = ((Game.playerPref.getBoolean(APlayer.PlayerClass.IRONCLAD + "_WIN", false)
                                    && Game.playerPref.getBoolean(APlayer.PlayerClass.THE_SILENT + "_WIN", false)
                                    && Game.playerPref.getBoolean(APlayer.PlayerClass.DEFECT + "_WIN", false)
                                    && !isDailyRun && !isTrial)
                /*|| CustomModeScreen.finalActAvailable*/);
        }
    }
}