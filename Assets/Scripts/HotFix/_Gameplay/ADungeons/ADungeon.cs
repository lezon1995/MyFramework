using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    //当前显示页面
    public enum CurrentScreen
    {
        NONE,
        PHASE,
        INITIALIZING_PLAYER,
        ENTERING_FIRST_ROOM,
        GAMEPLAY,
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
        public static string name;
        public static string levelNum;
        public static string id;
        public static int floorNum;
        public static int actNum;
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
        public static DungeonMapScreen dungeonMapScreen = new();

        #endregion

        public static OverlayMenu overlayMenu;
        public static OperationPanel operationPanel;

        public static CurrentScreen screen { get; set; }
        public static CurrentScreen previousScreen;

        public static bool isAscensionMode;
        public static int ascensionLevel = 0; //进阶等级
        public static bool ascensionCheck;

        protected ADungeon(string _name, string levelId, List<string> newSpecialOneTimeEventList)
        {
            _dungeon = this;
            _charSelectInfo = new();
            id = levelId;
            name = _name;
            // ascensionCheck = UnlockTracker.isAscensionUnlocked(p);
            _dungeon = this;
            _charSelectInfo = new();
            // topPanel.setPlayerName();
            actionManager = new();
            effectManager = new();
            // dynamicBanner = new DynamicBanner();
            unlocks.Clear();
            specialOneTimeEventList = newSpecialOneTimeEventList;

            isFadingIn = false;
            isFadingOut = false;
            waitingOnFadeOut = false;
            fadeTimer = 1.0F;

            isDungeonBeaten = false;
            isScreenUp = false;
        }

        protected ADungeon(string _name, SaveFile saveFile)
        {
            _dungeon = this;
            _charSelectInfo = new();
            id = saveFile.level_name;
            name = _name;
            // ascensionCheck = UnlockTracker.isAscensionUnlocked(p);
            // topPanel.setPlayerName();
            actionManager = new();
            effectManager = new();
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
                // SaveAndContinue.deleteSave(p.getSaveFilePath());
                logException(e);
                Application.Quit();
            }

            screen = CurrentScreen.NONE;
            isScreenUp = false;
        }

        public virtual void initialize()
        {
            initializeManagers();
            initializePhases();
            generateMonsters();
            initializeBoss();
            if (bossList.Count > 0)
                setBoss(bossList[0]);

            initializeEventList();
            initializeEventImg();
            initializeShrineList();
        }

        public virtual void initializeByFile(SaveFile saveFile)
        {
        }

        public virtual void initializeWithPlayer(APlayer p)
        {
            initializeRelicList(p);
            initializeCardPools(p);
            if (floorNum == 0)
                p.initializeStarterDeck();

            initializePotions(p);
            // BlightHelper.initialize();
            dungeonTransitionSetup(p);
        }

        public static void dungeonTransitionSetup(APlayer p)
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
                var healAmount = ((p.maxHealth - p.currentHealth) * 0.75F).round();
                p.Health.ReceiveHealth(new(healAmount), null, p);
            }

            // if (floorNum > 1)
            // topPanel.panelHealEffect();

            if (floorNum <= 1)
            {
                if (_dungeon is Exordium)
                {
                    if (ascensionLevel >= 14)
                        p.decreaseMaxHealth(p.getAscensionMaxHPLoss());

                    if (ascensionLevel >= 6)
                        p.currentHealth = (p.maxHealth * 0.9F).round();

                    if (ascensionLevel >= 10)
                    {
                        // p.masterDeck.addToTop(new AscendersBane());
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
                    break;
                case CurrentScreen.PHASE:
                    onPhaseUpdate(dt);
                    break;
                case CurrentScreen.INITIALIZING_PLAYER:
                    Game.initializePlayer(_charSelectInfo);
                    screen = CurrentScreen.ENTERING_FIRST_ROOM;
                    break;
                case CurrentScreen.ENTERING_FIRST_ROOM:
                    _dungeon.entryFirstRoom();
                    screen = CurrentScreen.GAMEPLAY;
                    break;
                case CurrentScreen.GAMEPLAY:
                    overlayMenu.update(dt);
                    room.updateObjects(dt);
                    room.update(dt);
                    break;
                case CurrentScreen.MAP:
                    // dungeonMapScreen.update();
                    // room.update(dt);
                    // scene.update();
                    // room.eventControllerInput();
                    break;
                case CurrentScreen.FTUE:
                    // ftue.update();
                    // InputHelper.justClickedRight = false;
                    // InputHelper.justClickedLeft = false;
                    // room.update(dt);
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
                    // room.update(dt);
                    break;
                case CurrentScreen.HAND_SELECT:
                    // handCardSelectScreen.update();
                    // room.update(dt);
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
            effectManager.updateRender(dt);
            cardInstanceIdGenerator = 0;
        }

        public void fixedUpdate(float dt)
        {
            switch (screen)
            {
                case CurrentScreen.NONE:
                    break;
                case CurrentScreen.PHASE:
                    onPhaseFixedUpdate(dt);
                    break;
                case CurrentScreen.INITIALIZING_PLAYER:
                    break;
                case CurrentScreen.ENTERING_FIRST_ROOM:
                    break;
                case CurrentScreen.GAMEPLAY:
                    room?.fixedUpdate(dt);
                    break;
                case CurrentScreen.MAP:
                    // room.fixedUpdate(dt);
                    break;
                case CurrentScreen.FTUE:
                    // room.fixedUpdate(dt);
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
                    // room.fixedUpdate(dt);
                    break;
                case CurrentScreen.HAND_SELECT:
                    // room.fixedUpdate(dt);
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

        public static void onModifyPower(APlayer p)
        {
            // if (p != null)
            // {
            //     p.hand.applyPowers();
            // }

            if (room.monsters != null)
                foreach (var m in room.monsters.monsters)
                    m.applyPowers();
        }
    }
}