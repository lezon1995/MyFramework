using System;
using System.Collections.Generic;

namespace MarbleHero
{
    public class AUnlock : IComparable<AUnlock>
    {
        public string title;
        public string key;
        public UnlockType type;
        public APlayer player = null;
        public ACard card = null;
        // public ARelic relic = null;

        public enum UnlockType
        {
            CARD,
            RELIC,
            LOADOUT,
            CHARACTER,
            MISC
        }

        public int CompareTo(AUnlock u)
        {
            switch (type)
            {
                case UnlockType.CARD:
                    return string.Compare(card.cardID, u.card.cardID, StringComparison.Ordinal);
                case UnlockType.RELIC:
                    // return relic.relicId.CompareTo(u.relic.relicId);
                    return 0;
            }

            return string.Compare(title, u.title, StringComparison.Ordinal);
        }

        public void onUnlockScreenOpen()
        {
        }
    }
    
    public class UnlockTracker
    {
        public static Prefs unlockPref;
        public static Prefs seenPref;
        public static Prefs betaCardPref;
        public static Prefs bossSeenPref;
        public static Prefs relicSeenPref;
        public static Prefs pawnSeenPref;
        public static Prefs achievementPref;
        public static Prefs unlockProgress;
        public static Dictionary<string, string> unlockReqs = new();
        public static List<string> lockedCards = new();
        public static List<string> lockedCharacters = new();
        public static List<string> lockedLoadouts = new();
        public static List<string> lockedRelics = new();
        public static List<string> lockedPawns = new();
        public static int lockedRedCardCount;
        public static int unlockedRedCardCount;
        public static int lockedGreenCardCount;
        public static int unlockedGreenCardCount;
        public static int lockedBlueCardCount;
        public static int unlockedBlueCardCount;
        public static int lockedPurpleCardCount;
        public static int unlockedPurpleCardCount;
        public static int lockedRelicCount;
        public static int unlockedRelicCount;
        private static int STARTING_UNLOCK_COST = 300;

        public static void initialize()
        {
            achievementPref = SaveHelper.getPrefs("Achievements");
            unlockPref = SaveHelper.getPrefs("Unlocks");
            unlockProgress = SaveHelper.getPrefs("UnlockProgress");
            seenPref = SaveHelper.getPrefs("SeenCards");
            betaCardPref = SaveHelper.getPrefs("BetaCardPreference");
            bossSeenPref = SaveHelper.getPrefs("SeenBosses");
            relicSeenPref = SaveHelper.getPrefs("SeenRelics");
            pawnSeenPref = SaveHelper.getPrefs("SeenPawns");
            refresh();
        }

        public static void retroactiveUnlock()
        {
            List<string> cardKeys = new();
            List<string> relicKeys = new();
            List<AUnlock> bundle = new();
            appendRetroactiveUnlockList(APlayer.PlayerClass.IRONCLAD, unlockProgress.getInteger(APlayer.PlayerClass.IRONCLAD + "UnlockLevel", -1), bundle, cardKeys, relicKeys);
            appendRetroactiveUnlockList(APlayer.PlayerClass.THE_SILENT, unlockProgress.getInteger(APlayer.PlayerClass.THE_SILENT + "UnlockLevel", -1), bundle, cardKeys, relicKeys);
            appendRetroactiveUnlockList(APlayer.PlayerClass.DEFECT, unlockProgress.getInteger(APlayer.PlayerClass.DEFECT + "UnlockLevel", -1), bundle, cardKeys, relicKeys);
            appendRetroactiveUnlockList(APlayer.PlayerClass.WATCHER, unlockProgress.getInteger(APlayer.PlayerClass.WATCHER + "UnlockLevel", -1), bundle, cardKeys, relicKeys);
            bool changed = false;
            foreach (string k in cardKeys)
            {
                if (unlockPref.getInteger(k) != 2)
                {
                    unlockPref.putInteger(k, 2);
                    changed = true;
                    log("RETROACTIVE CARD UNLOCK:  " + k);
                }
            }

            foreach (string k in relicKeys)
            {
                if (unlockPref.getInteger(k) != 2)
                {
                    unlockPref.putInteger(k, 2);
                    changed = true;
                    log("RETROACTIVE RELIC UNLOCK: " + k);
                }
            }

            if (isCharacterLocked("Watcher") && !isCharacterLocked("Defect") && (isAchievementUnlocked("RUBY") ||
                                                                                 isAchievementUnlocked("EMERALD") || isAchievementUnlocked("SAPPHIRE")))
            {
                unlockPref.putInteger("Watcher", 2);
                lockedCharacters.Remove("Watcher");
                changed = true;
            }

            if (changed)
            {
                log("RETRO UNLOCKED, SAVING");
                unlockPref.flush();
            }
        }

        private static void appendRetroactiveUnlockList(APlayer.PlayerClass c, int lvl, List<AUnlock> bundle, List<string> cardKeys, List<string> relicKeys)
        {
            while (lvl > 0)
            {
                bundle = getUnlockBundle(c, lvl - 1);
                foreach (AUnlock u in bundle)
                {
                    if (u.type == AUnlock.UnlockType.RELIC)
                    {
                        log(u.key + " should be unlocked.");
                        relicKeys.Add(u.key);
                        continue;
                    }

                    if (u.type == AUnlock.UnlockType.CARD)
                    {
                        log(u.key + " should be unlocked.");
                        cardKeys.Add(u.key);
                    }
                }

                lvl--;
            }
        }

        public static void refresh()
        {
            lockedCards.Clear();
            lockedCharacters.Clear();
            lockedLoadouts.Clear();
            lockedRelics.Clear();
            lockedPawns.Clear();
            addCard("Havoc");
            addCard("Sentinel");
            addCard("Exhume");
            addCard("Wild Strike");
            addCard("Evolve");
            addCard("Immolate");
            addCard("Heavy Blade");
            addCard("Spot Weakness");
            addCard("Limit Break");
            addCard("Concentrate");
            addCard("Setup");
            addCard("Grand e");
            addCard("Cloak And Dagger");
            addCard("Accuracy");
            addCard("Storm of Steel");
            addCard("Bane");
            addCard("Catalyst");
            addCard("Corpse Explosion");
            addCard("Rebound");
            addCard("Undo");
            addCard("Echo Form");
            addCard("Turbo");
            addCard("Sunder");
            addCard("Meteor Strike");
            addCard("Hyperbeam");
            addCard("Recycle");
            addCard("Core Surge");
            addCard("Prostrate");
            addCard("Blasphemy");
            addCard("Devotion");
            addCard("ForeignInfluence");
            addCard("Alpha");
            addCard("MentalFortress");
            addCard("SpiritShield");
            addCard("Wish");
            addCard("Wireheading");
            addCharacter("The Silent");
            addCharacter("Defect");
            addCharacter("Watcher");
            addRelic("Omamori");
            addRelic("Prayer Wheel");
            addRelic("Shovel");
            addRelic("Art of War");
            addRelic("The Courier");
            addRelic("Pandora's Box");
            addRelic("Blue Candle");
            addRelic("Dead Branch");
            addRelic("Singing Bowl");
            addRelic("Du-Vu Doll");
            addRelic("Smiling Mask");
            addRelic("Tiny Chest");
            addRelic("Cables");
            addRelic("DataDisk");
            addRelic("Emotion Chip");
            addRelic("Runic Capacitor");
            addRelic("Turnip");
            addRelic("Symbiotic Virus");
            addRelic("Akabeko");
            addRelic("Yang");
            addRelic("CeramicFish");
            addRelic("StrikeDummy");
            addRelic("TeardropLocket");
            addRelic("CloakClasp");
            countUnlockedCards();
        }

        public static int incrementUnlockRamp(int currentCost)
        {
            return currentCost switch
            {
                300 => 750,
                500 => 1000,
                750 => 1000,
                1000 => 1500,
                1500 => 2000,
                2000 => 2500,
                2500 => 3000,
                3000 => 3000,
                4000 => 4000,
                _ => currentCost + 250
            };
        }

        public static void resetUnlockProgress(APlayer.PlayerClass c)
        {
            unlockProgress.putInteger(c + "UnlockLevel", 0);
            unlockProgress.putInteger(c + "Progress", 0);
            unlockProgress.putInteger(c + "CurrentCost", 300);
            unlockProgress.putInteger(c + "TotalScore", 0);
            unlockProgress.putInteger(c + "HighScore", 0);
        }

        public static int getUnlockLevel(APlayer.PlayerClass c)
        {
            return unlockProgress.getInteger(c + "UnlockLevel", 0);
        }

        public static int getCurrentProgress(APlayer.PlayerClass c)
        {
            return unlockProgress.getInteger(c + "Progress", 0);
        }

        public static int getCurrentScoreCost(APlayer.PlayerClass c)
        {
            return unlockProgress.getInteger(c + "CurrentCost", 300);
        }

        public static void addScore(APlayer.PlayerClass c, int scoreGained)
        {
            string key_unlock_level = c + "UnlockLevel";
            string key_progress = c + "Progress";
            string key_current_cost = c + "CurrentCost";
            string key_total_score = c + "TotalScore";
            string key_high_score = c + "HighScore";
            log("Keys");
            log(key_unlock_level);
            log(key_progress);
            log(key_current_cost);
            log(key_total_score);
            log(key_high_score);
            int p = unlockProgress.getInteger(key_progress, 0);
            p += scoreGained;
            if (p >= unlockProgress.getInteger(key_current_cost, 300))
            {
                log("[DEBUG] Level up!");
                int lvl = unlockProgress.getInteger(key_unlock_level, 0);
                lvl++;
                unlockProgress.putInteger(key_unlock_level, lvl);
                p -= unlockProgress.getInteger(key_current_cost, 300);
                unlockProgress.putInteger(key_progress, p);
                log("[DEBUG] Score Progress: " + key_progress);
                int current_cost = unlockProgress.getInteger(key_current_cost, 300);
                unlockProgress.putInteger(key_current_cost, incrementUnlockRamp(current_cost));
                if (p > unlockProgress.getInteger(key_current_cost, 300))
                {
                    unlockProgress.putInteger(key_progress, unlockProgress
                        .getInteger(key_current_cost, 300) - 1);
                    log("Overfloat maxes out next level");
                }
            }
            else
            {
                unlockProgress.putInteger(key_progress, p);
            }

            int total = unlockProgress.getInteger(key_total_score, 0);
            total += scoreGained;
            unlockProgress.putInteger(key_total_score, total);
            log("[DEBUG] Total score: " + total);
            int highscore = unlockProgress.getInteger(key_high_score, 0);
            if (scoreGained > highscore)
            {
                unlockProgress.putInteger(key_high_score, scoreGained);
                log("[DEBUG] New high score: " + scoreGained);
            }

            unlockProgress.flush();
        }

        public static void countUnlockedCards()
        {
            List<string> tmp = new();
            int count = 0;
            tmp.Add("Havoc");
            tmp.Add("Sentinel");
            tmp.Add("Exhume");
            tmp.Add("Wild Strike");
            tmp.Add("Evolve");
            tmp.Add("Immolate");
            tmp.Add("Heavy Blade");
            tmp.Add("Spot Weakness");
            tmp.Add("Limit Break");
            foreach (string s in tmp)
            {
                if (!isCardLocked(s))
                    count++;
            }

            lockedRedCardCount = tmp.Count;
            unlockedRedCardCount = count;
            tmp.Clear();
            count = 0;
            tmp.Add("Concentrate");
            tmp.Add("Setup");
            tmp.Add("Grand e");
            tmp.Add("Cloak And Dagger");
            tmp.Add("Accuracy");
            tmp.Add("Storm of Steel");
            tmp.Add("Bane");
            tmp.Add("Catalyst");
            tmp.Add("Corpse Explosion");
            foreach (string s in tmp)
            {
                if (!isCardLocked(s))
                    count++;
            }

            lockedGreenCardCount = tmp.Count;
            unlockedGreenCardCount = count;
            tmp.Clear();
            count = 0;
            tmp.Add("Rebound");
            tmp.Add("Undo");
            tmp.Add("Echo Form");
            tmp.Add("Turbo");
            tmp.Add("Sunder");
            tmp.Add("Meteor Strike");
            tmp.Add("Hyperbeam");
            tmp.Add("Recycle");
            tmp.Add("Core Surge");
            foreach (string s in tmp)
            {
                if (!isCardLocked(s))
                    count++;
            }

            lockedBlueCardCount = tmp.Count;
            unlockedBlueCardCount = count;
            tmp.Clear();
            count = 0;
            tmp.Add("Prostrate");
            tmp.Add("Blasphemy");
            tmp.Add("Devotion");
            tmp.Add("ForeignInfluence");
            tmp.Add("Alpha");
            tmp.Add("MentalFortress");
            tmp.Add("SpiritShield");
            tmp.Add("Wish");
            tmp.Add("Wireheading");
            foreach (string s in tmp)
            {
                if (!isCardLocked(s))
                    count++;
            }

            lockedPurpleCardCount = tmp.Count;
            unlockedPurpleCardCount = count;
            tmp.Clear();
            count = 0;
            tmp.Add("Omamori");
            tmp.Add("Prayer Wheel");
            tmp.Add("Shovel");
            tmp.Add("Art of War");
            tmp.Add("The Courier");
            tmp.Add("Pandora's Box");
            tmp.Add("Blue Candle");
            tmp.Add("Dead Branch");
            tmp.Add("Singing Bowl");
            tmp.Add("Du-Vu Doll");
            tmp.Add("Smiling Mask");
            tmp.Add("Tiny Chest");
            tmp.Add("Cables");
            tmp.Add("DataDisk");
            tmp.Add("Emotion Chip");
            tmp.Add("Runic Capacitor");
            tmp.Add("Turnip");
            tmp.Add("Symbiotic Virus");
            tmp.Add("Akabeko");
            tmp.Add("Yang");
            tmp.Add("CeramicFish");
            tmp.Add("StrikeDummy");
            tmp.Add("TeardropLocket");
            tmp.Add("CloakClasp");
            foreach (string s in tmp)
            {
                if (!isRelicLocked(s))
                    count++;
            }

            lockedRelicCount = tmp.Count;
            unlockedRelicCount = count;
            log("RED UNLOCKS:   " + unlockedRedCardCount + "/" + lockedRedCardCount);
            log("GREEN UNLOCKS: " + unlockedGreenCardCount + "/" + lockedGreenCardCount);
            log("BLUE UNLOCKS: " + unlockedBlueCardCount + "/" + lockedBlueCardCount);
            log("PURPLE UNLOCKS: " + unlockedPurpleCardCount + "/" + lockedPurpleCardCount);
            log("RELIC UNLOCKS: " + unlockedRelicCount + "/" + lockedRelicCount);
            log("CARDS SEEN:    " + seenPref.get().Count + "/" + CardLibrary.totalCardCount);
            log("RELICS SEEN:   " + relicSeenPref.get().Count + "/" + RelicLibrary.totalRelicCount);
            // log("Pawns SEEN:   " + pawnSeenPref.get().Count + "/" + PawnLibrary.totalPawnCount);
        }

        public static string getCardsSeenString()
        {
            return (CardLibrary.seenRedCards + CardLibrary.seenGreenCards + CardLibrary.seenBlueCards + CardLibrary.seenPurpleCards + CardLibrary.seenColorlessCards + CardLibrary.seenCurseCards) + "/" + CardLibrary.totalCardCount;
        }

        public static string getRelicsSeenString()
        {
            return RelicLibrary.seenRelics + "/" + RelicLibrary.totalRelicCount;
        }

        public static void addCard(string key)
        {
            if (unlockPref.getString(key) == "true")
            {
                unlockPref.putInteger(key, 2);
                log("Converting " + key + " from bool to int");
                unlockPref.flush();
            }
            else if (unlockPref.getString(key) == "false")
            {
                unlockPref.putInteger(key, 0);
                log("Converting " + key + " from bool to int");
                unlockPref.flush();
            }

            if (unlockPref.getInteger(key, 0) != 2)
                lockedCards.Add(key);
        }

        public static void addCharacter(string key)
        {
            if (unlockPref.getString(key) == "true")
            {
                unlockPref.putInteger(key, 2);
                log("Converting " + key + " from bool to int");
                unlockPref.flush();
            }
            else if (unlockPref.getString(key) == "false")
            {
                unlockPref.putInteger(key, 0);
                log("Converting " + key + " from bool to int");
                unlockPref.flush();
            }

            if (unlockPref.getInteger(key, 0) != 2)
                lockedCharacters.Add(key);
        }

        public static void addRelic(string key)
        {
            if (unlockPref.getInteger(key, 0) != 2)
                lockedRelics.Add(key);
        }

        public static void addPawn(string key)
        {
            if (unlockPref.getInteger(key, 0) != 2)
                lockedPawns.Add(key);
        }

        public static void unlockAchievement(string key)
        {
            if (Settings.isModded || Settings.isShowBuild || !Settings.isStandardRun())
                return;
            // Game.publisherIntegration?.unlockAchievement(key);
            if (!achievementPref.getBoolean(key, false))
            {
                achievementPref.putBoolean(key, true);
                log("Achievement Unlocked: " + key);
            }

            if (allAchievementsExceptPlatinumUnlocked() && !isAchievementUnlocked("ETERNAL_ONE"))
            {
                // Game.publisherIntegration?.unlockAchievement("ETERNAL_ONE");
                achievementPref.putBoolean("ETERNAL_ONE", true);
                log("Achievement Unlocked: ETERNAL_ONE");
            }

            achievementPref.flush();
        }

        public static bool allAchievementsExceptPlatinumUnlocked()
        {
            return (achievementPref.data.Count >= 45);
        }

        public static bool isAchievementUnlocked(string key)
        {
            return achievementPref.getBoolean(key, false);
        }

        public static void unlockLuckyDay()
        {
            if (Settings.isModded)
                return;
            string key = "LUCKY_DAY";
            // Game.publisherIntegration?.unlockAchievement(key);
            if (!achievementPref.getBoolean(key, false))
            {
                achievementPref.putBoolean(key, true);
                achievementPref.flush();
                log("Achievement Unlocked: " + key);
            }
        }

        public static void hardUnlock(string key)
        {
            if (Settings.isShowBuild)
                return;
            if (unlockPref.getInteger(key, 0) == 1)
            {
                unlockPref.putInteger(key, 2);
                unlockPref.flush();
                log("Hard Unlock: " + key);
            }
        }

        public static void hardUnlockOverride(string key)
        {
            if (Settings.isShowBuild)
                return;
            unlockPref.putInteger(key, 2);
            unlockPref.flush();
            log("Hard Unlock: " + key);
        }

        public static bool isCardLocked(string key)
        {
            return lockedCards.Contains(key);
        }

        public static void unlockCard(string key)
        {
            seenPref.putInteger(key, 1);
            seenPref.flush();
            unlockPref.putInteger(key, 2);
            unlockPref.flush();
            lockedCards.Remove(key);
            if (CardLibrary.getCard(key, out var card))
            {
                card.isSeen = true;
                card.unlock();
            }
        }

        public static bool isCharacterLocked(string key)
        {
            if (key == "The Silent" && Settings.isDemo)
                return false;

            if (Settings.isAlpha)
                return false;

            return lockedCharacters.Contains(key);
        }

        public static bool isAscensionUnlocked(APlayer p)
        {
            // int victories = StatsScreen.getVictory(p.getCharStat());
            // if (victories > 0)
            {
                if (!achievementPref.getBoolean("ASCEND_0", false))
                    unlockAchievement("ASCEND_0");

                // if (!achievementPref.getBoolean("ASCEND_10", false))
                // StatsScreen.retroactiveAscend10Unlock(p.getPrefs());

                // if (!achievementPref.getBoolean("ASCEND_20", false))
                // StatsScreen.retroactiveAscend20Unlock(p.getPrefs());

                // return true;
            }

            return false;
        }

        public static bool isRelicLocked(string key)
        {
            return lockedRelics.Contains(key);
        }

        public static bool isPawnLocked(string key)
        {
            return lockedPawns.Contains(key);
        }

        public static void markCardAsSeen(string key)
        {
            CardLibrary.getCard(key, out var card);
            if (card is { isSeen: false })
            {
                card.isSeen = true;
                seenPref.putInteger(key, 1);
                seenPref.flush();
            }
            else
            {
                log("Already seen: " + key);
            }
        }

        public static bool isCardSeen(string key)
        {
            return seenPref.getInteger(key, 0) != 0;
        }

        public static void markRelicAsSeen(string key)
        {
            var relic = RelicLibrary.getRelic(key);
            if (relic is { isSeen: false })
            {
                relic.isSeen = true;
                relicSeenPref.putInteger(key, 1);
                relicSeenPref.flush();
            }
            else if (Settings.isDebug)
            {
                log("Already seen: " + key);
            }
        }

        public static bool isRelicSeen(string key)
        {
            return relicSeenPref.getInteger(key, 0) == 1;
        }


        public static void markPawnAsSeen(string key)
        {
            // var pawn = PawnLibrary.getPawn(key);
            // if (pawn is { isSeen: false })
            // {
            //     pawn.isSeen = true;
            //     pawnSeenPref.putInteger(key, 1);
            //     pawnSeenPref.flush();
            // }
            // else if (Settings.isDebug)
            // {
            //     log("Already seen: " + key);
            // }
        }

        public static bool isPawnSeen(string key)
        {
            return pawnSeenPref.getInteger(key, 0) == 1;
        }

        public static void markBossAsSeen(string originalName)
        {
            if (bossSeenPref.getInteger(originalName) != 1)
            {
                bossSeenPref.putInteger(originalName, 1);
                bossSeenPref.flush();
            }
        }

        public static bool isBossSeen(string key)
        {
            return bossSeenPref.getInteger(key, 0) == 1;
        }

        public static List<AUnlock> getUnlockBundle(APlayer.PlayerClass c, int unlockLevel)
        {
            List<AUnlock> list = new();
            switch (c)
            {
                case APlayer.PlayerClass.IRONCLAD:
                    switch (unlockLevel)
                    {
                        case 0:
                            //list.Add(new HeavyBladeUnlock());
                            //list.Add(new SpotWeaknessUnlock());
                            //list.Add(new LimitBreakUnlock());
                            break;
                        case 1:
                            //list.Add(new OmamoriUnlock());
                            //list.Add(new PrayerWheelUnlock());
                            //list.Add(new ShovelUnlock());
                            break;
                        case 2:
                            //list.Add(new WildStrikeUnlock());
                            //list.Add(new EvolveUnlock());
                            //list.Add(new ImmolateUnlock());
                            break;
                        case 3:
                            //list.Add(new HavocUnlock());
                            //list.Add(new SentinelUnlock());
                            //list.Add(new ExhumeUnlock());
                            break;
                        case 4:
                            //list.Add(new BlueCandleUnlock());
                            //list.Add(new DeadBranchUnlock());
                            //list.Add(new SingingBowlUnlock());
                            break;
                    }

                    break;
                case APlayer.PlayerClass.THE_SILENT:
                    switch (unlockLevel)
                    {
                        case 0:
                            //list.Add(new BaneUnlock());
                            //list.Add(new CatalystUnlock());
                            //list.Add(new CorpseExplosionUnlock());
                            break;
                        case 1:
                            //list.Add(new DuvuDollUnlock());
                            //list.Add(new SmilingMaskUnlock());
                            //list.Add(new TinyChestUnlock());
                            break;
                        case 2:
                            //list.Add(new CloakAndDaggerUnlock());
                            //list.Add(new AccuracyUnlock());
                            //list.Add(new StormOfSteelUnlock());
                            break;
                        case 3:
                            //list.Add(new ArtOfWarUnlock());
                            //list.Add(new CourierUnlock());
                            //list.Add(new PandorasBoxUnlock());
                            break;
                        case 4:
                            //list.Add(new ConcentrateUnlock());
                            //list.Add(new SetupUnlock());
                            //list.Add(new GrandeUnlock());
                            break;
                    }

                    break;
                case APlayer.PlayerClass.DEFECT:
                    switch (unlockLevel)
                    {
                        case 0:
                            //list.Add(new ReboundUnlock());
                            //list.Add(new UndoUnlock());
                            //list.Add(new EchoFormUnlock());
                            break;
                        case 1:
                            //list.Add(new TurboUnlock());
                            //list.Add(new SunderUnlock());
                            //list.Add(new MeteorStrikeUnlock());
                            break;
                        case 2:
                            //list.Add(new HyperbeamUnlock());
                            //list.Add(new RecycleUnlock());
                            //list.Add(new NovaUnlock());
                            break;
                        case 3:
                            //list.Add(new CablesUnlock());
                            //list.Add(new TurnipUnlock());
                            //list.Add(new RunicCapacitorUnlock());
                            break;
                        case 4:
                            //list.Add(new EmotionChipUnlock());
                            //list.Add(new VirusUnlock());
                            //list.Add(new DataDiskUnlock());
                            break;
                    }

                    break;
                case APlayer.PlayerClass.WATCHER:
                    switch (unlockLevel)
                    {
                        case 0:
                            //list.Add(new ProstrateUnlock());
                            //list.Add(new BlasphemyUnlock());
                            //list.Add(new DevotionUnlock());
                            break;
                        case 1:
                            //list.Add(new ForeignInfluenceUnlock());
                            //list.Add(new AlphaUnlock());
                            //list.Add(new MentalFortressUnlock());
                            break;
                        case 2:
                            //list.Add(new ClarityUnlock());
                            //list.Add(new WishUnlock());
                            //list.Add(new ForesightUnlock());
                            break;
                        case 3:
                            //list.Add(new AkabekoUnlock());
                            //list.Add(new YangUnlock());
                            //list.Add(new CeramicFishUnlock());
                            break;
                        case 4:
                            //list.Add(new StrikeDummyUnlock());
                            //list.Add(new TeardropUnlock());
                            //list.Add(new CloakClaspUnlock());
                            break;
                    }

                    break;
            }

            return list;
        }

        public static void addCardUnlockToList(Dictionary<string, AUnlock> map, string key, AUnlock unlock)
        {
            if (isCardLocked(key))
                map.Add(key, unlock);
        }

        public static void addRelicUnlockToList(Dictionary<string, AUnlock> map, string key, AUnlock unlock)
        {
            if (isRelicLocked(key))
                map.Add(key, unlock);
        }

        public static float getCompletionPercentage()
        {
            float totalPercent = 0.0F;
            totalPercent += getAscensionProgress() * 0.3F;
            totalPercent += getUnlockProgress() * 0.25F;
            totalPercent += getAchievementProgress() * 0.35F;
            totalPercent += getSeenCardsProgress() * 0.05F;
            totalPercent += getSeenRelicsProgress() * 0.05F;
            return totalPercent * 100.0F;
        }

        private static float getAscensionProgress()
        {
            // List<Prefs> allCharacterPrefs = Game.characterManager.getAllPrefs();
            List<Prefs> allCharacterPrefs = new();
            int sum = 0;
            foreach (Prefs p in allCharacterPrefs)
                sum += p.getInteger("ASCENSION_LEVEL", 0);
            float retVal = sum / 60.0F;
            log("Ascension Progress: " + retVal);
            if (retVal > 1.0F)
                retVal = 1.0F;
            return retVal;
        }

        private static float getUnlockProgress()
        {
            int sum = Math.Min(getUnlockLevel(APlayer.PlayerClass.IRONCLAD), 5);
            sum += Math.Min(getUnlockLevel(APlayer.PlayerClass.THE_SILENT), 5);
            sum += Math.Min(getUnlockLevel(APlayer.PlayerClass.DEFECT), 5);
            sum += Math.Min(getUnlockLevel(APlayer.PlayerClass.WATCHER), 5);
            float retVal = sum / 15.0F;
            log("Unlock IC: " + getUnlockLevel(APlayer.PlayerClass.IRONCLAD));
            log("Unlock Silent: " + getUnlockLevel(APlayer.PlayerClass.THE_SILENT));
            log("Unlock Defect: " + getUnlockLevel(APlayer.PlayerClass.DEFECT));
            log("Unlock Watcher: " + getUnlockLevel(APlayer.PlayerClass.WATCHER));
            log("Unlock Progress: " + retVal);
            if (retVal > 1.0F)
                retVal = 1.0F;
            return retVal;
        }

        private static float getAchievementProgress()
        {
            int sum = 0;
            // foreach (AchievementItem item in StatsScreen.achievements.items)
            // {
            // if (item.isUnlocked)
            // sum++;
            // }

            // float retVal = sum / StatsScreen.achievements.items.Count;
            float retVal = 0;
            log("Achievement Progress: " + retVal);
            if (retVal > 1.0F)
                retVal = 1.0F;
            return retVal;
        }

        private static float getSeenCardsProgress()
        {
            int sum = 0;
            foreach (var (id, card) in CardLibrary.cards)
            {
                if (card.isSeen)
                    sum++;
            }

            float retVal = sum * 1F / CardLibrary.cards.Count;
            log("Seen Cards Progress: " + retVal);
            if (retVal > 1.0F)
                retVal = 1.0F;
            return retVal;
        }

        private static float getSeenRelicsProgress()
        {
            float retVal = RelicLibrary.seenRelics * 1F / RelicLibrary.totalRelicCount;
            log("Seen Relics Progress: " + retVal);
            if (retVal > 1.0F)
                retVal = 1.0F;
            return retVal;
        }

        public static long getTotalPlaytime()
        {
            return Settings.totalPlayTime;
        }
    }
}