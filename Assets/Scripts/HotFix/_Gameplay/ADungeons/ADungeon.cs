using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.AutoBattleEngine.Gameplay.Cards;
using MoreMountains.AutoBattleEngine.Gameplay.Helpers;
using MoreMountains.AutoBattleEngine.Gameplay.Helpers.Input;
using MoreMountains.AutoBattleEngine.Gameplay.Rooms;
using MoreMountains.AutoBattleEngine.Gameplay.Saves;
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
        static ILogger logger = Log.GetLogger<ADungeon>();

        public readonly DungeonData Data;

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
        public static OverlayMenu overlayMenu { get; set; }
        public static CurrentScreen screen { get; set; }
        public static CurrentScreen previousScreen;

        public static bool isAscensionMode;
        public static int ascensionLevel = 0; //进阶等级
        public static bool ascensionCheck;

        protected ADungeon(DungeonData data, APlayer p, List<string> newSpecialOneTimeEventList)
        {
            _dungeon = this;
            Data = data;
            id = data.Id;
            name = data.Name;
            player = p;
            ascensionCheck = UnlockTracker.isAscensionUnlocked(p);
            _dungeon = this;
            long startTime = TimeUtility.getNowTimeStampMS();
            // topPanel.setPlayerName();
            // actionManager = new GameActionManager();
            overlayMenu = new OverlayMenu(p);
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

            Data.generateMonsters();
            Data.initializeBoss();
            if (bossList.Count > 0)
                setBoss(bossList[0]);

            Data.initializeEventList();
            Data.initializeEventImg();

            Data.initializeShrineList();

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

        protected ADungeon(DungeonData data, APlayer p, SaveFile saveFile)
        {
            Data = data;
            id = saveFile.level_name;
            name = data.Name;
            player = p;
            ascensionCheck = UnlockTracker.isAscensionUnlocked(p);
            _dungeon = this;
            long startTime = TimeUtility.getNowTimeStampMS();
            // topPanel.setPlayerName();
            // actionManager = new GameActionManager();
            overlayMenu = new OverlayMenu(p);
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
                ExceptionHandler.handleException(e, logger);
                Application.Quit();
            }

            Data.initializeEventImg();
            Data.initializeShrineList();
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
            // topPanel.unhoverHitboxes();
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
                    room.eventControllerInput();
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

            for (var i = 0; i < topLevelEffects.Count;)
            {
                var e = topLevelEffects[i];
                e.update(dt);
                if (e.isDone)
                    topLevelEffects.RemoveAt(i);
                else
                    i++;
            }

            for (var i = 0; i < effectList.Count;)
            {
                var e = effectList[i];
                e.update(dt);
                if (e.isDone)
                    effectList.RemoveAt(i);
                else
                    i++;
            }

            effectList.AddRange(effectsQueue);
            effectsQueue.Clear();

            topLevelEffects.AddRange(topLevelEffectsQueue);
            topLevelEffectsQueue.Clear();

            overlayMenu.update(dt);
            cardInstanceIdGenerator = 0;
        }

        public void fixedUpdate(float dt)
        {
            switch (screen)
            {
                case CurrentScreen.NONE:
                case CurrentScreen.MAP:
                    // dungeonMapScreen.update();
                    room.fixedUpdate(dt);
                    // scene.update();
                    room.eventControllerInput();
                    break;
                case CurrentScreen.FTUE:
                    // ftue.update();
                    InputHelper.justClickedRight = false;
                    InputHelper.justClickedLeft = false;
                    room.fixedUpdate(dt);
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
                    room.fixedUpdate(dt);
                    break;
                case CurrentScreen.HAND_SELECT:
                    // handCardSelectScreen.update();
                    room.fixedUpdate(dt);
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
        }

        public void loadSave(SaveFile saveFile)
        {
            floorNum = saveFile.floor_num;
            actNum = saveFile.act_num;
            Settings.seed = saveFile.seed;
            LoadSeeds(saveFile);
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

        public void checkForPactAchievement()
        {
            if (player != null)
                if (player.exhaustPile.size() >= 20)
                    UnlockTracker.unlockAchievement("THE_PACT");
        }

        public static void onModifyPower()
        {
            if (player != null)
            {
                player.hand.applyPowers();
                // if (player.hasPower("Focus"))
                // foreach (var o in player.orbs)
                // o.updateDescription();
            }

            if (room.monsters != null)
                foreach (var m in room.monsters.monsters)
                    m.applyPowers();
        }
    }
}