using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MarbleHero
{
    public class Game : ClassObject
    {
        public Game()
        {
        }

        public override void resetProperty()
        {
            base.resetProperty();
            prevDebugKeyDown = false;
        }

        public enum GameMode
        {
            SPLASH,
            MAIN_MENU,
            GAMEPLAY,
            DUNGEON_TRANSITION,
        }

        public static string VERSION_NUM = "[V2.3.4] (12-18-2022)";
        public static string TRUE_VERSION_NUM = "2022-12-18";

        //
        public static ScreenShake screenShake;
        public static ADungeon dungeon;
        public static SplashScreen splashScreen;
        public static MainMenuScreen mainMenuScreen;
        public static DungeonTransitionScreen transitionScreen;
        public static DebugPanel debugPanel;

        public bool prevDebugKeyDown;
        public static string nextDungeon;
        public static GameMode mode;
        public static bool isStartingOver;
        public static bool queueCredits;
        public static bool playCreditsBgm;
        public static bool MUTE_IF_BG;

        public static APlayer.PlayerClass chosenCharacter = APlayer.PlayerClass.IRONCLAD;
        public static bool loadingSave;

        public static SaveFile saveFile;
        public static Prefs saveSlotPref;
        public static Prefs playerPref;
        public static int saveSlot;
        public static string playerName;

        public static string alias;

        public static CharacterManager characterManager;
        public static int monstersSlain;
        public static int elites1Slain;
        public static int elites2Slain;
        public static int elites3Slain;
        public static int elitesModdedSlain = 0;
        public static int champion;
        public static int perfect;
        public static bool overkill;
        public static bool combo;
        public static bool cheater;
        public static int goldGained;
        public static int cardsPurged;
        public static int potionsBought;
        public static int mysteryMachine;
        public static float playtime;

        public static bool stopClock;

        public static ATrial trial;

        // static SteamInputHelper steamInputHelper;
        // public static SteamUtils clientUtils;
        public static Thread sInputDetectThread;
        static Color screenColor = Color.black;
        static Timer screenTimer = 2.0F;
        static bool isFadingIn = true;

        public static IPublisherIntegration publisherIntegration;

        // public static SteelSeries steelSeries;
        static bool displayCursor = true;
        static bool displayVersion = true;

        public static string preferenceDir;

        // SteamUtilsCallback clUtilsCallback;

        public void create()
        {
            if (Settings.isAlpha)
            {
                TRUE_VERSION_NUM += " ALPHA";
                VERSION_NUM += " ALPHA";
            }
            else if (Settings.isBeta)
            {
                VERSION_NUM += " BETA";
            }

            if (Settings.isDebug)
            {
                mDebugPanel = debugPanel = LT.LOAD<DebugPanel>();
            }

            try
            {
                // TwitchConfig.createConfig();
                var buildSettings = new BuildSettings("build.properties");

                log("DistributorPlatform=" + buildSettings.getDistributor());
                log("isModded=" + Settings.isModded);
                log("isBeta=" + Settings.isBeta);

                publisherIntegration = DistributorFactory.getEnabledDistributor(buildSettings.getDistributor());

                saveMigration();

                saveSlotPref = SaveHelper.getPrefs("SaveSlots");
                saveSlot = saveSlotPref.getInteger("DEFAULT_SLOT", 0);

                playerPref = SaveHelper.getPrefs("Player");
                playerName = saveSlotPref.getString(SaveHelper.slotName("PROFILE_NAME", saveSlot), "");
                if (string.IsNullOrEmpty(playerName))
                    playerName = playerPref.getString("name", "");

                alias = playerPref.getString("alias", "");
                if (string.IsNullOrEmpty(alias))
                {
                    alias = generateRandomAlias();
                    playerPref.putString("alias", alias);
                    playerPref.flush();
                }

                Settings.initialize(false);

                // camera = Camera.main;
                // languagePack = new();
                // Gdx.graphics.setCursor(Gdx.graphics.newCursor(new Pixmap(Gdx.files. internal ("images/blank.png")), 0, 0));
                music = new();
                sound = new();
                screenShake = new(getMainCamera());

                GameDesign.initialize();
                // GameDictionary.initialize();
                // ImageMaster.initialize();
                APower.initialize();
                // FontHelper.initialize();
                UnlockTracker.initialize();
                CardLibrary.initialize();
                RelicLibrary.initialize();
                InputHelper.initialize();
                TipTracker.initialize();
                ModHelper.initialize();
                // ShaderHelper.initializeShaders();
                UnlockTracker.retroactiveUnlock();
                // CInputHelper.loadSettings();

                // clientUtils = new SteamUtils(clUtilsCallback);
                // steamInputHelper = new SteamInputHelper();
                // steelSeries = new SteelSeries();
                metricData = new();
                characterManager = new();
                LT.LOAD(out splashScreen);
                mode = GameMode.SPLASH;
            }
            catch (Exception e)
            {
                logException(e);
                Application.Quit();
            }
        }

        public static void reloadPrefs()
        {
            playerPref = SaveHelper.getPrefs("Player");
            alias = playerPref.getString("alias", "");
            if (string.IsNullOrEmpty(alias))
            {
                alias = generateRandomAlias();
                playerPref.putString("alias", alias);
            }

            music.fadeOutBGM();
            mainMenuScreen.fadeOutMusic();
            InputActionSet.prefs = SaveHelper.getPrefs("InputSettings");
            InputActionSet.load();

            // CInputActionSet.prefs = SaveHelper.getPrefs("InputSettings_Controller");
            // CInputActionSet.load();

            // if (SteamInputHelper.numControllers == 1)
            // SteamInputHelper.initActions(SteamInputHelper.controllerHandles[0]);

            characterManager = new();
            Settings.initialize(true);
            UnlockTracker.initialize();

            CardLibrary.resetForReload();
            CardLibrary.initialize();

            RelicLibrary.resetForReload();
            RelicLibrary.initialize();

            TipTracker.initialize();
            // log("TEXTURE COUNT: " + Texture.getNumManagedTextures());
            screenColor.a = 0.0F;
            screenTimer = 0.01F;
            isFadingIn = false;
            isStartingOver = true;
        }

        public void update(float dt)
        {
            if (Settings.isDebug)
            {
                using var _ = new MyStringBuilderScope(out var sb);
                sb.addLine($"mode={mode.ToString()}");
                sb.addLine($"screenTimer={screenTimer.remain:F2}");
                sb.addLine($"room={room?.GetType().Name}");
                mDebugPanel.setDebugText(sb.ToString());
            }

            try
            {
                // if (!SteamInputHelper.alive)
                // CInputHelper.initializeIfAble();

                onUpdate(dt);
            }
            catch (Exception e)
            {
                logException(e);
                Application.Quit();
            }
        }

        public void fixedUpdate(float dt)
        {
            switch (mode)
            {
                case GameMode.SPLASH:
                    break;
                case GameMode.MAIN_MENU:
                    break;
                case GameMode.GAMEPLAY:
                    dungeon?.fixedUpdate(dt);
                    break;
                case GameMode.DUNGEON_TRANSITION:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void pause()
        {
        }

        public void resume()
        {
        }

        public void dispose()
        {
        }

        void onUpdate(float dt)
        {
            screenShake.update(dt);
            
            if (mode != GameMode.SPLASH)
                updateFade(dt);

            music.update(dt);
            sound.update(dt);

            // if (steelSeries.isEnabled)
            // steelSeries.update();

            if (Settings.isDebug)
            {
                if (DevInputActionSet.toggleCursor.isJustPressed())
                {
                    displayCursor = !displayCursor;
                }
                else if (DevInputActionSet.toggleVersion.isJustPressed())
                {
                    displayVersion = !displayVersion;
                }
            }

            // if (SteamInputHelper.numControllers == 1)
            // {
            //     SteamInputHelper.updateFirst();
            // }
            // else if (SteamInputHelper.numControllers == 999 && CInputHelper.controllers == null)
            // {
            //     CInputHelper.initializeIfAble();
            // }

            InputHelper.updateFirst();

            switch (mode)
            {
                case GameMode.SPLASH:
                    splashScreen.update(dt);
                    if (splashScreen.isDone)
                    {
                        LT.UNLOAD(ref splashScreen);
                        LT.SHOW(out mainMenuScreen);
                        mode = GameMode.MAIN_MENU;
                    }

                    break;
                case GameMode.MAIN_MENU:
                    mainMenuScreen.update(dt);
                    if (mainMenuScreen.fadedOut)
                    {
                        LT.HIDE(ref mainMenuScreen);
                        ADungeon.path.Clear();

                        if (trial == null && Settings.specialSeed != 0)
                            trial = TrialHelper.getTrialForSeed(SeedHelper.getString(Settings.specialSeed));

                        if (loadingSave)
                        {
                            ModHelper.setModsFalse();
                            player = createCharacter(chosenCharacter);
                            loadPlayerSave(player);
                        }
                        else
                        {
                            Settings.setFinalActAvailability();
                            log("Final Act Available: " + Settings.isFinalActAvailable);

                            if (trial == null)
                            {
                                if (Settings.isDailyRun)
                                {
                                    ADungeon.ascensionLevel = 0;
                                    ADungeon.isAscensionMode = false;
                                }

                                player = createCharacter(chosenCharacter);
                                foreach (var relic in player.relics)
                                {
                                    relic.updateDescription(player.chosenClass);
                                    relic.onEquip(player);
                                }

                                // foreach (var card in player.masterDeck.group)
                                // {
                                //     if (card.rarity != CardRarity.Basic)
                                //         CardHelper.obtain(card.cardID, card.rarity, card.color);
                                // }
                            }
                            else
                            {
                                Settings.isTrial = true;
                                Settings.isDailyRun = false;
                                setupTrialMods(trial, chosenCharacter);
                                setupTrialPlayer(trial);
                            }
                        }

                        mode = GameMode.GAMEPLAY;
                        nextDungeon = "Exordium";
                        transitionScreen = new("Exordium");

                        if (loadingSave)
                        {
                            transitionScreen.isComplete = true;
                            break;
                        }

                        monstersSlain = 0;
                        elites1Slain = 0;
                        elites2Slain = 0;
                        elites3Slain = 0;
                    }

                    break;
                case GameMode.GAMEPLAY:
                    if (transitionScreen != null)
                    {
                        if (transitionScreen.update(dt))
                        {
                            transitionScreen = null;
                            dungeon = loadDungeon();
                        }
                    }
                    else
                    {
                        dungeon.update(dt);
                    }

                    if (dungeon != null && ADungeon.isDungeonBeaten && ADungeon.fadeColor.a == 1.0F)
                    {
                        dungeon = null;
                        // ADungeon.scene.fadeOutAmbiance();
                        transitionScreen = new(nextDungeon);
                    }

                    break;
                case GameMode.DUNGEON_TRANSITION:
                    break;
                default:
                    break;
            }

            updateDebugSwitch();
            InputHelper.updateLast();

            // if (CInputHelper.controller != null)
            //     CInputHelper.updateLast();
            //
            // if (Settings.isInfo)
            //     fpsLogger.log();
        }

        public ADungeon loadDungeon()
        {
            ADungeon d;
            if (loadingSave)
            {
                d = getDungeon(saveFile.level_name, player, saveFile);
                loadPostCombat(saveFile);
                if (!saveFile.post_combat)
                    loadingSave = false;
            }
            else
            {
                d = getDungeon(nextDungeon, player);
                if (nextDungeon != "Exordium" || Settings.isShowBuild || !TipTracker.tips["NEOW_SKIP"])
                {
                    // ADungeon.dungeonMapScreen.open(true);
                    TipTracker.neverShowAgain("NEOW_SKIP");
                }
            }

            return d;
        }

        public ADungeon getDungeon(string key, APlayer p)
        {
            return key switch
            {
                "Exordium" => new Exordium(p, new List<string>()),
                // "TheCity" => new TheCity(p, ADungeon.specialOneTimeEventList),
                // "TheBeyond" => new TheBeyond(p, ADungeon.specialOneTimeEventList),
                // "TheEnding" => new TheEnding(p, ADungeon.specialOneTimeEventList),
                _ => null
            };
        }

        public ADungeon getDungeon(string key, APlayer p, SaveFile saveFile)
        {
            return key switch
            {
                "Exordium" => new Exordium(p, saveFile),
                // "TheCity" => new TheCity(p, saveFile),
                // "TheBeyond" => new TheBeyond(p, saveFile),
                // "TheEnding" => new TheEnding(p, saveFile),
                _ => null
            };
        }

        static void setupTrialMods(ATrial trial, APlayer.PlayerClass chosenClass)
        {
            if (trial.useRandomDailyMods())
            {
                long sourceTime = DateTime.UtcNow.Ticks;
                var rng = new Rand(sourceTime);
                Settings.seed = SeedHelper.generateUnoffensiveSeed(rng);
                ModHelper.setTodaysMods(Settings.seed, chosenClass);
            }
            else if (trial.dailyModIDs() != null)
            {
                ModHelper.setMods(trial.dailyModIDs());
                ModHelper.clearNulls();
            }
        }

        static void setupTrialPlayer(ATrial trial)
        {
            player = trial.setupPlayer(createCharacter(chosenCharacter));
            if (!trial.keepStarterRelic())
                player.relics.Clear();

            foreach (string relicID in trial.extraStartingRelicIDs())
            {
                ARelic relic = RelicLibrary.getRelic(relicID);
                relic.instantObtain(player, player.relics.Count, false);
                ADungeon.relicsToRemoveOnStart.Add(relic.relicId);
            }

            foreach (ARelic r in player.relics)
            {
                r.updateDescription(player.chosenClass);
                r.onEquip(player);
            }

            // if (!trial.keepsStarterCards())
            // player.masterDeck.clear();

            // foreach (string cardID in trial.extraStartingCardIDs())
            // {
            //     if (CardLibrary.getCard(cardID, out var card))
            //     {
            //         player.masterDeck.addToTop(card.makeCopy());
            //     }
            // }
        }

        static void loadPostCombat(SaveFile saveFile)
        {
            if (saveFile.post_combat)
            {
                room.isBattleOver = true;
                // ADungeon.overlayMenu.hideCombatPanels();
                ADungeon.loading_post_combat = true;
                room.smoked = saveFile.smoked;
                room.mugged = saveFile.mugged;

                room.evt?.postCombatLoad();

                if (room.monsters != null)
                {
                    room.monsters.monsters.Clear();
                    actionManager.actions.Clear();
                }

                if (!saveFile.smoked)
                {
                    foreach (RewardSave i in saveFile.combat_rewards)
                    {
                        switch (i.type)
                        {
                            case "CARD":
                                continue;
                            case "GOLD":
                                room.addGoldToRewards(i.amount);
                                continue;
                            case "RELIC":
                                room.addRelicToRewards(RelicLibrary.getRelic(i.id).makeCopy());
                                continue;
                            // case "POTION":
                            // room.addPotionToRewards(PotionHelper.getPotion(i.id));
                            // continue;
                            case "STOLEN_GOLD":
                                room.addStolenGoldToRewards(i.amount);
                                continue;
                            case "SAPPHIRE_KEY":
                                // room.addSapphireKey(room.rewards[^1]);
                                continue;
                            case "EMERALD_KEY":
                                // room.rewards.Add(new RewardItem(room.rewards[^1], RewardType.EMERALD_KEY));
                                continue;
                        }

                        log("Loading unknown type: " + i.type);
                    }
                }

                if (room is MonsterRoomBoss)
                {
                    // ADungeon.scene.fadeInAmbiance();
                    music.silenceTempBgmInstantly();
                    music.silenceBGMInstantly();
                    AMonster.playBossStinger();
                }
                else if (room is MonsterRoomElite)
                {
                    // ADungeon.scene.fadeInAmbiance();
                    music.fadeOutTempBGM();
                }

                saveFile.post_combat = false;
            }
        }

        public static string generateRandomAlias()
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
            using var _ = new MyStringBuilderScope(out var sb);
            for (int i = 0; i < 16; i++)
            {
                var index = MathUtils.random(0, alphabet.Length - 1);
                sb.add(alphabet[index]);
            }

            return sb.ToString();
        }

        public static void saveMigration()
        {
            if (!SaveHelper.saveExists())
            {
                var playerName = PlayerPrefHelper.GetString("Player", "name", string.Empty);
                var playTime = PlayerPrefHelper.GetInt("DataVagabond", "PLAYTIME", 0);
                if (string.IsNullOrEmpty(playerName) && playTime == 0)
                {
                    log("New player, no migration.");
                    return;
                }

                log("Migrating Save...");
                migrateHelper("Player");
                migrateHelper("Unlocks");
                migrateHelper("UnlockProgress");
                migrateHelper("Tips");
                migrateHelper("Sound");
                migrateHelper("SeenRelics");
                migrateHelper("SeenCards");
                migrateHelper("SeenBosses");
                migrateHelper("GameplaySettings");
                migrateHelper("DataVagabond");
                migrateHelper("DataTheSilent");
                migrateHelper("Achievements");
                if (MathUtils.randomBool(0.5F))
                    logWarning("Save Migration");
            }
            else
            {
                log("No migration");
            }

            return;

            void migrateHelper(string file)
            {
                // Preferences p = Gdx.app.getPreferences(file);
                // Prefs p2 = SaveHelper.getPrefs(file);
                // Map < string, ?> map = p.get();
                // for (Map.Entry < string, ?> c : map.entrySet())
                // p2.putString(c.getKey(), p.getString(c.getKey()));
                // p2.flush();
            }
        }

        static void loadPlayerSave(APlayer p)
        {
            saveFile = SaveAndContinue.loadSaveFile(p.chosenClass);
            ADungeon.loading_post_combat = false;
            Settings.seed = saveFile.seed;
            Settings.isFinalActAvailable = saveFile.is_final_act_on;
            Settings.hasRubyKey = saveFile.has_ruby_key;
            Settings.hasEmeraldKey = saveFile.has_emerald_key;
            Settings.hasSapphireKey = saveFile.has_sapphire_key;
            Settings.isDailyRun = saveFile.is_daily;
            if (Settings.isDailyRun)
                Settings.dailyDate = saveFile.daily_date;

            Settings.specialSeed = saveFile.special_seed;
            Settings.seedSet = saveFile.seed_set;
            Settings.isTrial = saveFile.is_trial;
            if (Settings.isTrial)
            {
                ModHelper.setTodaysMods(Settings.seed, player.chosenClass);
                APlayer.customMods = saveFile.custom_mods;
            }
            else if (Settings.isDailyRun)
            {
                ModHelper.setTodaysMods(Settings.specialSeed, player.chosenClass);
            }

            APlayer.customMods = saveFile.custom_mods ?? new();
            p.currentHealth = saveFile.current_health;
            p.maxHealth = saveFile.max_health;
            p.gold = saveFile.gold;
            p.displayGold = p.gold;
            // p.masterHandSize = saveFile.hand_size;
            // p.potionSlots = saveFile.potion_slots;
            // if (p.potionSlots == 0)
            // p.potionSlots = 3;
            // p.potions.Clear();
            // for (int i = 0; i < p.potionSlots; i++)
            // p.potions.Add(new PotionSlot(i));
            // p.masterMaxOrbs = saveFile.max_orbs;
            // p.energy = new EnergyManager(saveFile.red + saveFile.green + saveFile.blue);
            monstersSlain = saveFile.monsters_killed;
            elites1Slain = saveFile.elites1_killed;
            elites2Slain = saveFile.elites2_killed;
            elites3Slain = saveFile.elites3_killed;
            goldGained = saveFile.gold_gained;
            champion = saveFile.champions;
            perfect = saveFile.perfect;
            combo = saveFile.combo;
            overkill = saveFile.overkill;
            mysteryMachine = saveFile.mystery_machine;
            playtime = saveFile.play_time;
            ADungeon.ascensionLevel = saveFile.ascension_level;
            ADungeon.isAscensionMode = saveFile.is_ascension_mode;
            // p.masterDeck.clear();
            // foreach (CardSave s in saveFile.cards)
            // {
            //     log(s.id + ", " + s.upgrades);
            //     p.masterDeck.addToTop(CardLibrary.getCopy(s.id, /*s.upgrades,*/ s.misc));
            // }

            Settings.isEndless = saveFile.is_endless_mode;
            // int index = 0;
            // p.blights.Clear();
            // if (saveFile.blights != null)
            // {
            //     foreach (string b in saveFile.blights)
            //     {
            //         AbstractBlight blight = BlightHelper.getBlight(b);
            //         if (blight != null)
            //         {
            //             int incrementAmount = saveFile.endless_increments[index];
            //             for (int j = 0; j < incrementAmount; j++)
            //                 blight.incrementUp();
            //             blight.setIncrement(incrementAmount);
            //             blight.instantObtain(player, index, false);
            //         }
            //
            //         index++;
            //     }
            //
            //     if (saveFile.blight_counters != null)
            //     {
            //         index = 0;
            //         foreach (int integer in saveFile.blight_counters)
            //         {
            //             p.blights[index].setCounter(integer);
            //             p.blights[index].updateDescription(p.chosenClass);
            //             index++;
            //         }
            //     }
            // }

            p.relics.Clear();
            // index = 0;
            // foreach (string s in saveFile.relics)
            // {
            //     ARelic r = RelicLibrary.getRelic(s).makeCopy();
            //     r.instantObtain(p, index, false);
            //     if (index < saveFile.relic_counters.Count)
            //         r.setCounter(saveFile.relic_counters[index]);
            //     r.updateDescription(p.chosenClass);
            //     index++;
            // }

            // index = 0;
            // foreach (string s in saveFile.potions)
            // {
            //     AbstractPotion potion = PotionHelper.getPotion(s);
            //     if (potion != null)
            //         player.obtainPotion(index, potion);
            //     index++;
            // }

            // ACard tmpCard = null;
            // if (saveFile.bottled_flame != null)
            // {
            //     foreach (ACard abstractCard in player.masterDeck.group)
            //     {
            //         if (abstractCard.cardID == (saveFile.bottled_flame))
            //         {
            //             tmpCard = abstractCard;
            //             if (abstractCard.timesUpgraded == saveFile.bottled_flame_upgrade && abstractCard.misc == saveFile.bottled_flame_misc)
            //                 break;
            //         }
            //     }
            //
            //     if (tmpCard != null)
            //     {
            //         tmpCard.inBottleFlame = true;
            //         ((BottledFlame)player.getRelic("Bottled Flame")).card = tmpCard;
            //         ((BottledFlame)player.getRelic("Bottled Flame")).setDescriptionAfterLoading();
            //     }
            // }

            // tmpCard = null;
            // if (saveFile.bottled_lightning != null)
            // {
            //     foreach (ACard abstractCard in player.masterDeck.group)
            //     {
            //         if (abstractCard.cardID == (saveFile.bottled_lightning))
            //         {
            //             tmpCard = abstractCard;
            //             if (abstractCard.timesUpgraded == saveFile.bottled_lightning_upgrade && abstractCard.misc == saveFile.bottled_lightning_misc)
            //                 break;
            //         }
            //     }
            //
            //     if (tmpCard != null)
            //     {
            //         tmpCard.inBottleLightning = true;
            //         ((BottledLightning)player.getRelic("Bottled Lightning")).card = tmpCard;
            //         ((BottledLightning)player.getRelic("Bottled Lightning")).setDescriptionAfterLoading();
            //     }
            // }

            // tmpCard = null;
            // if (saveFile.bottled_tornado != null)
            // {
            //     foreach (ACard abstractCard in player.masterDeck.group)
            //     {
            //         if (abstractCard.cardID == (saveFile.bottled_tornado))
            //         {
            //             tmpCard = abstractCard;
            //             if (abstractCard.timesUpgraded == saveFile.bottled_tornado_upgrade && abstractCard.misc == saveFile.bottled_tornado_misc)
            //                 break;
            //         }
            //     }
            //
            //     if (tmpCard != null)
            //     {
            //         tmpCard.inBottleTornado = true;
            //         ((BottledTornado)player.getRelic("Bottled Tornado")).card = tmpCard;
            //         ((BottledTornado)player.getRelic("Bottled Tornado")).setDescriptionAfterLoading();
            //     }
            // }

            if (saveFile.daily_mods is { Count: > 0 })
                ModHelper.setMods(saveFile.daily_mods);

            metricData.clearData();
            metricData.campfire_rested = saveFile.metric_campfire_rested;
            metricData.campfire_upgraded = saveFile.metric_campfire_upgraded;
            metricData.purchased_purges = saveFile.metric_purchased_purges;
            metricData.potions_floor_spawned = saveFile.metric_potions_floor_spawned;
            metricData.current_hp_per_floor = saveFile.metric_current_hp_per_floor;
            metricData.max_hp_per_floor = saveFile.metric_max_hp_per_floor;
            metricData.gold_per_floor = saveFile.metric_gold_per_floor;
            metricData.path_per_floor = saveFile.metric_path_per_floor;
            metricData.path_taken = saveFile.metric_path_taken;
            metricData.items_purchased = saveFile.metric_items_purchased;
            metricData.items_purged = saveFile.metric_items_purged;
            metricData.card_choices = saveFile.metric_card_choices;
            metricData.event_choices = saveFile.metric_event_choices;
            metricData.damage_taken = saveFile.metric_damage_taken;
            metricData.boss_relics = saveFile.metric_boss_relics;

            if (saveFile.metric_potions_obtained != null)
                metricData.potions_obtained = saveFile.metric_potions_obtained;

            if (saveFile.metric_relics_obtained != null)
                metricData.relics_obtained = saveFile.metric_relics_obtained;

            if (saveFile.metric_campfire_choices != null)
                metricData.campfire_choices = saveFile.metric_campfire_choices;

            if (saveFile.metric_item_purchase_floors != null)
                metricData.item_purchase_floors = saveFile.metric_item_purchase_floors;

            if (saveFile.metric_items_purged_floors != null)
                metricData.items_purged_floors = saveFile.metric_items_purged_floors;

            if (saveFile.neow_bonus != null)
                metricData.neowBonus = saveFile.neow_bonus;

            if (saveFile.neow_cost != null)
                metricData.neowCost = saveFile.neow_cost;
        }

        static APlayer createCharacter(APlayer.PlayerClass selection)
        {
            APlayer p = characterManager.recreateCharacter(selection);
            // foreach (ACard c in p.masterDeck.group)
            // UnlockTracker.markCardAsSeen(c.cardID);
            return p;
        }


        void updateDebugSwitch()
        {
            if (!Settings.isDev)
                return;

            if (DevInputActionSet.toggleDebug.isJustPressed())
            {
                Settings.isDebug = !Settings.isDebug;
                return;
            }

            if (DevInputActionSet.toggleInfo.isJustPressed())
            {
                Settings.isInfo = !Settings.isInfo;
                return;
            }

            if (Settings.isDebug && DevInputActionSet.uploadData.isJustPressed())
            {
                // RelicLibrary.uploadRelicData();
                // CardLibrary.uploadCardData();
                // MonsterHelper.uploadEnemyData();
                // PotionHelper.uploadPotionData();
                // ModHelper.uploadModData();
                // BlightHelper.uploadBlightData();
                // BotDataUploader.uploadKeywordData();
                return;
            }

            if (!Settings.isDebug)
                return;

            if (DevInputActionSet.hideTopBar.isJustPressed())
            {
                Settings.hideTopBar = !Settings.hideTopBar;
                return;
            }

            if (DevInputActionSet.hidePopUps.isJustPressed())
            {
                Settings.hidePopupDetails = !Settings.hidePopupDetails;
                return;
            }

            if (DevInputActionSet.hideRelics.isJustPressed())
            {
                Settings.hideRelics = !Settings.hideRelics;
                return;
            }

            if (DevInputActionSet.hideCombatLowUI.isJustPressed())
            {
                Settings.hideLowerElements = !Settings.hideLowerElements;
                return;
            }

            if (DevInputActionSet.hideCards.isJustPressed())
            {
                Settings.hideCards = !Settings.hideCards;
                return;
            }

            if (DevInputActionSet.hideEndTurnButton.isJustPressed())
            {
                Settings.hideEndTurn = !Settings.hideEndTurn;
                if (ADungeon.getMonsters() == null)
                    return;

                foreach (AMonster m in ADungeon.getMonsters().monsters)
                    m.damage(new DamageInfo(ADungeon.player, m.currentHealth, DamageInfo.DamageType.HP_LOSS));

                return;
            }

            if (DevInputActionSet.hideCombatInfo.isJustPressed())
                Settings.hideCombatElements = !Settings.hideCombatElements;
        }


        public static void updateFade(float dt)
        {
            if (!screenTimer)
                return;

            var finished = screenTimer.update(dt);
            if (finished)
                screenTimer.kill();

            if (isFadingIn)
            {
                screenColor.a = MMLerp.fade.apply(1.0F, 0.0F, screenTimer.pct);
            }
            else
            {
                screenColor.a = MMLerp.fade.apply(0.0F, 1.0F, screenTimer.pct);
                if (isStartingOver && finished)
                {
                    // if (ADungeon.scene != null)
                    // ADungeon.scene.fadeOutAmbiance();

                    long startTime = TimeUtility.getNowTimeStampMS();
                    ADungeon.screen = CurrentScreen.NONE;
                    ADungeon.reset();
                    // FontHelper.cardTitleFont.getData().setScale(1.0F);
                    ARelic.relicPage = 0;
                    ModHelper.setModsFalse();
                    SeedHelper.cachedSeed = null;
                    Settings.seed = 0;
                    Settings.seedSet = false;
                    Settings.specialSeed = 0;
                    Settings.isTrial = false;
                    Settings.isDailyRun = false;
                    Settings.isEndless = false;
                    Settings.isFinalActAvailable = false;
                    Settings.hasRubyKey = false;
                    Settings.hasEmeraldKey = false;
                    Settings.hasSapphireKey = false;
                    // CustomModeScreen.finalActAvailable = false;
                    trial = null;
                    log("Dungeon Reset: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
                    startTime = TimeUtility.getNowTimeStampMS();
                    // ShopScreen.resetPurgeCost();
                    // tips.initialize();
                    metricData.clearData();
                    log("Shop Screen Rest, Tips Initialize, Metric Data Clear: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
                    startTime = TimeUtility.getNowTimeStampMS();
                    UnlockTracker.refresh();
                    log("Unlock Tracker Refresh:  " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
                    startTime = TimeUtility.getNowTimeStampMS();
                    LT.SHOW(out mainMenuScreen);
                    // mainMenuScreen.bg.slideDownInstantly();
                    saveSlotPref.putFloat(SaveHelper.slotName("COMPLETION", saveSlot), UnlockTracker.getCompletionPercentage());
                    saveSlotPref.putLong(SaveHelper.slotName("PLAYTIME", saveSlot), UnlockTracker.getTotalPlaytime());
                    saveSlotPref.flush();
                    log("New Main Menu Screen: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
                    startTime = TimeUtility.getNowTimeStampMS();
                    CardHelper.clear();
                    mode = GameMode.MAIN_MENU;
                    nextDungeon = "Exordium";
                    transitionScreen = new("Exordium");
                    TipTracker.refresh();
                    // log("[GC] BEFORE: " + SystemStats.getUsedMemory());
                    GC.Collect();
                    // log("[GC] AFTER: " + SystemStats.getUsedMemory());
                    log("New Transition Screen, Tip Tracker Refresh: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
                    startTime = TimeUtility.getNowTimeStampMS();
                    fadeIn(2.0F);
                    if (queueCredits)
                    {
                        queueCredits = false;
                        // mainMenuScreen.creditsScreen.open(playCreditsBgm);
                        mainMenuScreen.hideMenuButtons();
                    }
                }
            }
        }

        public static void fadeIn(float duration)
        {
            screenColor.a = 1.0F;
            screenTimer = duration;
            isFadingIn = true;
        }

        public static void fadeToBlack(float duration)
        {
            screenColor.a = 0.0F;
            screenTimer = duration;
            isFadingIn = false;
        }

        public static void startOver()
        {
            isStartingOver = true;
            fadeToBlack(2.0F);
        }

        public static void startOverButShowCredits()
        {
            isStartingOver = true;
            queueCredits = true;
            // doorUnlockScreenCheck();
            fadeToBlack(2.0F);
        }

        public static void resetScoreVars()
        {
            monstersSlain = 0;
            elites1Slain = 0;
            elites2Slain = 0;
            elites3Slain = 0;
            // if (dungeon != null)
            // ADungeon.bossCount = 0;
            champion = 0;
            perfect = 0;
            overkill = false;
            combo = false;
            goldGained = 0;
            cardsPurged = 0;
            potionsBought = 0;
            mysteryMachine = 0;
            playtime = 0.0F;
            stopClock = false;
        }
    }
}