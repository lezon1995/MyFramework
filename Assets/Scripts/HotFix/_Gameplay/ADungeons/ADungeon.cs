using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarbleHero
{
    //当前显示页面
    public enum CurrentScreen
    {
        NONE,
        MASTER_DECK_VIEW,
        SETTINGS,
        INPUT_SETTINGS,
        GRID,
        MAP,
        FTUE,
        CHOOSE_ONE,
        HAND_SELECT,
        SHOP,
        COMBAT_REWARD,
        DISCARD_VIEW,
        EXHAUST_VIEW,
        GAME_DECK_VIEW,
        BOSS_REWARD,
        DEATH,
        CARD_REWARD,
        TRANSFORM,
        VICTORY,
        UNLOCK,
        DOOR_UNLOCK,
        CREDITS,
        NO_INTERACT,
        NEOW_UNLOCK
    }

    public abstract partial class ADungeon
    {
        public static string name { get; set; }
        public static string levelNum;
        public static string id;
        public static int floorNum;
        public static int actNum;
        public static APlayer player { get; set; }
        public static List<AUnlock> unlocks = new();
        public static bool turnPhaseEffectActive;
        protected static float shrineChance = 0.25F;
        public static string lastCombatMetricKey;
        public static ACard transformedCard;
        public static bool loading_post_combat;
        public static bool is_victory;

        public static bool isScreenUp;


        #region --------------------------------------------------------------------------------------------

        //爬塔地图页面
        public static DungeonMapScreen dungeonMapScreen = new DungeonMapScreen();

        #endregion

        public static OverlayMenu overlayMenu;
        public static CurrentScreen screen { get; set; }
        public static CurrentScreen previousScreen;

        public static bool isAscensionMode;
        public static int ascensionLevel = 0; //进阶等级
        public static bool ascensionCheck;

        protected ADungeon(string _name, string levelId, APlayer p, List<string> newSpecialOneTimeEventList)
        {
            _dungeon = this;
            id = levelId;
            name = _name;
            player = p;
            ascensionCheck = UnlockTracker.isAscensionUnlocked(p);
            _dungeon = this;
            long startTime = TimeUtility.getNowTimeStampMS();
            // topPanel.setPlayerName();
            actionManager = new();
            effectManager = new();
            LT.SHOW(out overlayMenu);
            // dynamicBanner = new DynamicBanner();
            unlocks.Clear();
            specialOneTimeEventList = newSpecialOneTimeEventList;

            isFadingIn = false;
            isFadingOut = false;
            waitingOnFadeOut = false;
            fadeTimer = 1.0F;

            isDungeonBeaten = false;
            isScreenUp = false;
            dungeonTransitionSetup();

            generateMonsters();
            initializeBoss();
            if (bossList.Count > 0)
                setBoss(bossList[0]);

            initializeEventList();
            initializeEventImg();
            initializeShrineList();

            initializeCardPools();
            if (floorNum == 0)
                p.initializeStarterDeck();

            initializePotions();
            // BlightHelper.initialize();

            if (id == "Exordium")
            {
                screen = CurrentScreen.NONE;
                isScreenUp = false;
            }
            else
            {
                screen = CurrentScreen.MAP;
                isScreenUp = true;
            }

            log("Content generation time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
        }

        protected ADungeon(string _name, APlayer p, SaveFile saveFile)
        {
            id = saveFile.level_name;
            name = _name;
            player = p;
            ascensionCheck = UnlockTracker.isAscensionUnlocked(p);
            _dungeon = this;
            long startTime = TimeUtility.getNowTimeStampMS();
            // topPanel.setPlayerName();
            actionManager = new();
            effectManager = new();
            LT.SHOW(out overlayMenu);
            // dynamicBanner = new DynamicBanner();
            // isFadingIn = false;
            // isFadingOut = false;
            // waitingOnFadeOut = false;
            // fadeTimer = 1.0F;
            isDungeonBeaten = false;
            isScreenUp = false;
            firstRoomChosen = true;
            unlocks.Clear();
            try
            {
                loadSave(saveFile);
            }
            catch (Exception e)
            {
                log("Exception occurred while loading save!");
                log("Deleting save due to crash!");
                SaveAndContinue.deleteSave(player.getSaveFilePath());
                logException(e);
                Application.Quit();
            }

            // Data.initializeEventImg();
            // Data.initializeShrineList();
            initializeCardPools();
            initializePotions();
            // BlightHelper.initialize();
            screen = CurrentScreen.NONE;
            isScreenUp = false;
            log("Dungeon load time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
        }

        public virtual void Initialize(int seed)
        {
        }

        public static void dungeonTransitionSetup()
        {
            actNum++;
            int counter = cardRng.counter switch
            {
                > 0 and < 250 => 250,
                > 250 and < 500 => 500,
                > 500 and < 750 => 750,
                _ => 0
            };
            if (counter > 0)
                cardRng.setCounter(counter);

            log("CardRng Counter: " + cardRng.counter);
            path.Clear();
            EventHelper.resetProbabilities();
            eventList.Clear();
            shrineList.Clear();
            monsterList.Clear();
            eliteMonsterList.Clear();
            bossList.Clear();
            ARoom.blizzardPotionMod = 0;

            if (ascensionLevel >= 5)
            {
                var healAmount = round((player.maxHealth - player.currentHealth) * 0.75F);
                player.heal(ref healAmount, false);
            }
            else
            {
                var healAmount = player.maxHealth;
                player.heal(ref healAmount, false);
            }

            // if (floorNum > 1)
            // topPanel.panelHealEffect();

            if (floorNum <= 1)
            {
                if (_dungeon is Exordium)
                {
                    if (ascensionLevel >= 14)
                        player.decreaseMaxHealth(player.getAscensionMaxHPLoss());

                    if (ascensionLevel >= 6)
                        player.currentHealth = round(player.maxHealth * 0.9F);

                    if (ascensionLevel >= 10)
                    {
                        // player.masterDeck.addToTop(new AscendersBane());
                        UnlockTracker.markCardAsSeen("AscendersBane");
                    }

                    Game.playtime = 0.0F;
                }
            }

            // dungeonMapScreen.map.atBoss = false;
        }

        public void update(float dt)
        {
            if (!Game.stopClock)
                Game.playtime += dt;

            // if (Game.screenTimer > 0.0F)
            // {
            //     InputHelper.justClickedLeft = false;
            //     CInputActionSet.select.unpress();
            // }

            // topPanel.update();
            // dynamicBanner.update();
            updateFading(dt);
            room.updateObjects(dt);

            if (isScreenUp)
            {
                // topGradientColor.a = MathHelper.fadeLerpSnap(topGradientColor.a, 0.25F);
                // botGradientColor.a = MathHelper.fadeLerpSnap(botGradientColor.a, 0.25F);
            }
            else
            {
                // topGradientColor.a = MathHelper.fadeLerpSnap(topGradientColor.a, 0.1F);
                // botGradientColor.a = MathHelper.fadeLerpSnap(botGradientColor.a, 0.1F);
            }

            switch (screen)
            {
                case CurrentScreen.NONE:
                case CurrentScreen.MAP:
                    // dungeonMapScreen.update();
                    room.update(dt);
                    // scene.update();
                    // room.eventControllerInput();
                    break;
                case CurrentScreen.FTUE:
                    // ftue.update();
                    InputHelper.justClickedRight = false;
                    InputHelper.justClickedLeft = false;
                    room.update(dt);
                    break;
                case CurrentScreen.MASTER_DECK_VIEW:
                    // deckViewScreen.update();
                    break;
                case CurrentScreen.GAME_DECK_VIEW:
                    // gameDeckViewScreen.update();
                    break;
                case CurrentScreen.DISCARD_VIEW:
                    // discardPileViewScreen.update();
                    break;
                case CurrentScreen.EXHAUST_VIEW:
                    // exhaustPileViewScreen.update();
                    break;
                case CurrentScreen.SETTINGS:
                    // settingsScreen.update();
                    break;
                case CurrentScreen.INPUT_SETTINGS:
                    // inputSettingsScreen.update();
                    break;
                case CurrentScreen.GRID:
                    // dungeonMapScreen.update();
                    break;
                case CurrentScreen.CHOOSE_ONE:
                    // gridSelectScreen.update();
                    // if (PeekButton.isPeeking)
                    // room.update(dt);
                    break;
                case CurrentScreen.CARD_REWARD:
                    // cardRewardScreen.update();
                    // if (PeekButton.isPeeking)
                    // room.update(dt);
                    break;
                case CurrentScreen.COMBAT_REWARD:
                    // combatRewardScreen.update();
                    break;
                case CurrentScreen.BOSS_REWARD:
                    // bossRelicScreen.update();
                    room.update(dt);
                    break;
                case CurrentScreen.HAND_SELECT:
                    // handCardSelectScreen.update();
                    room.update(dt);
                    break;
                case CurrentScreen.SHOP:
                    // shopScreen.update();
                    break;
                case CurrentScreen.DEATH:
                    // deathScreen.update();
                    break;
                case CurrentScreen.VICTORY:
                    // victoryScreen.update();
                    break;
                case CurrentScreen.UNLOCK:
                    // unlockScreen.update();
                    break;
                case CurrentScreen.NEOW_UNLOCK:
                    // gUnlockScreen.update();
                    break;
                case CurrentScreen.CREDITS:
                    // creditsScreen.update();
                    break;
                case CurrentScreen.DOOR_UNLOCK:
                    // Game.mainMenuScreen.doorUnlockScreen.update();
                    break;
                case CurrentScreen.TRANSFORM:
                case CurrentScreen.NO_INTERACT:
                default:
                    log("ERROR: UNKNOWN SCREEN TO UPDATE: " + screen);
                    break;
            }

            turnPhaseEffectActive = false;
            effectManager.update(dt);
            overlayMenu.update(dt);
            cardInstanceIdGenerator = 0;
        }

        public void fixedUpdate(float dt)
        {
            switch (screen)
            {
                case CurrentScreen.NONE:
                case CurrentScreen.MAP:
                    room.fixedUpdate(dt);
                    break;
                case CurrentScreen.FTUE:
                    room.fixedUpdate(dt);
                    break;
                case CurrentScreen.MASTER_DECK_VIEW:
                    break;
                case CurrentScreen.GAME_DECK_VIEW:
                    break;
                case CurrentScreen.DISCARD_VIEW:
                    break;
                case CurrentScreen.EXHAUST_VIEW:
                    break;
                case CurrentScreen.SETTINGS:
                    break;
                case CurrentScreen.INPUT_SETTINGS:
                    break;
                case CurrentScreen.GRID:
                    break;
                case CurrentScreen.CHOOSE_ONE:
                    // if (PeekButton.isPeeking)
                    // room.update(dt);
                    break;
                case CurrentScreen.CARD_REWARD:
                    // if (PeekButton.isPeeking)
                    // room.update(dt);
                    break;
                case CurrentScreen.COMBAT_REWARD:
                    break;
                case CurrentScreen.BOSS_REWARD:
                    room.fixedUpdate(dt);
                    break;
                case CurrentScreen.HAND_SELECT:
                    room.fixedUpdate(dt);
                    break;
                case CurrentScreen.SHOP:
                    break;
                case CurrentScreen.DEATH:
                    break;
                case CurrentScreen.VICTORY:
                    break;
                case CurrentScreen.UNLOCK:
                    break;
                case CurrentScreen.NEOW_UNLOCK:
                    break;
                case CurrentScreen.CREDITS:
                    break;
                case CurrentScreen.DOOR_UNLOCK:
                    break;
                case CurrentScreen.TRANSFORM:
                case CurrentScreen.NO_INTERACT:
                default:
                    log("ERROR: UNKNOWN SCREEN TO UPDATE: " + screen);
                    break;
            }
        }

        public void loadSave(SaveFile saveFile)
        {
            floorNum = saveFile.floor_num;
            actNum = saveFile.act_num;
            Settings.seed = saveFile.seed;
            loadSeeds(saveFile);
            monsterList = saveFile.monster_list;
            eliteMonsterList = saveFile.elite_monster_list;
            bossList = saveFile.boss_list;
            setBoss(saveFile.boss);
            commonRelicPool = saveFile.common_relics;
            uncommonRelicPool = saveFile.uncommon_relics;
            rareRelicPool = saveFile.rare_relics;
            shopRelicPool = saveFile.shop_relics;
            bossRelicPool = saveFile.boss_relics;
            path = saveFile.path.Select(point => (point.x, point.y)).ToList();
            // bossCount = saveFile.spirit_count;
            eventList = saveFile.event_list;
            specialOneTimeEventList = saveFile.one_time_event_list;
            EventHelper.setChances(saveFile.event_chances);
            ARoom.blizzardPotionMod = saveFile.potion_chance;
            // ShopScreen.purgeCost = saveFile.purgeCost;
            CardHelper.obtainedCards = saveFile.obtained_cards;
            if (saveFile.daily_mods != null)
                ModHelper.setMods(saveFile.daily_mods);
        }

        public static void onModifyPower()
        {
            // if (player != null)
            // {
            //     player.hand.applyPowers();
            // }

            if (room.monsters != null)
                foreach (var m in room.monsters.monsters)
                    m.applyPowers();
        }
    }
}