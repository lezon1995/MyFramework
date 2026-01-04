using System.Collections.Generic;
using System.Linq;

namespace MarbleHero
{
    public class CharStat
    {
        // static AchievementStrings achievementStrings = Game.languagePack.getAchievementString("CharStat");
        // public static string[] NAMES = achievementStrings.NAMES;
        // public static string[] TEXT = achievementStrings.TEXT;
        public static string[] TEXT = Enumerable.Repeat("", 100).ToArray();
        Prefs pref;
        string info;
        string info2;
        int cardsUnlocked;
        int relicsUnlocked;
        int cardsDiscovered;
        int cardsToDiscover;
        public int FurthestAscent;
        public int HighestScore;
        public int HighestDaily;
        int totalFloorsClimbed;
        int numVictory;
        int numDeath;
        public int winStreak;
        public int bestWinStreak;
        public int enemyKilled;
        public int bossKilled;
        public long playTime;
        public long fastestTime;
        List<RunData> runs = new();
        public static string CARD_UNLOCK = "CARD_UNLOCK";
        public static string RELIC_UNLOCK = "RELIC_UNLOCK";
        public static string HIGHEST_FLOOR = "HIGHEST_FLOOR";
        public static string HIGHEST_SCORE = "HIGHEST_SCORE";
        public static string HIGHEST_DAILY = "HIGHEST_DAILY";
        public static string TOTAL_FLOORS = "TOTAL_FLOORS";
        public static string TOTAL_CRYSTALS_FED = "TOTAL_CRYSTALS_FED";
        public static string WIN_COUNT = "WIN_COUNT";
        public static string LOSE_COUNT = "LOSE_COUNT";
        public static string WIN_STREAK = "WIN_STREAK";
        public static string BEST_WIN_STREAK = "BEST_WIN_STREAK";
        public static string ASCENSION_LEVEL = "ASCENSION_LEVEL";
        public static string LAST_ASCENSION_LEVEL = "LAST_ASCENSION_LEVEL";
        public static string ENEMY_KILL = "ENEMY_KILL";
        public static string BOSS_KILL = "BOSS_KILL";
        public static string PLAYTIME = "PLAYTIME";
        public static string FASTEST_VICTORY = "FAST_VICTORY";

        public CharStat(List<CharStat> allChars)
        {
            cardsUnlocked = 0;
            relicsUnlocked = 0;
            FurthestAscent = 0;
            HighestScore = 0;
            totalFloorsClimbed = 0;
            numVictory = 0;
            numDeath = 0;
            enemyKilled = 0;
            bossKilled = 0;
            playTime = 0L;
            fastestTime = 999999999999L;
            int highestFloorTmp = 0;
            int highestDailyTmp = 0;
            foreach (CharStat stat in allChars)
            {
                cardsUnlocked += stat.cardsUnlocked;
                relicsUnlocked += stat.relicsUnlocked;
                if (stat.FurthestAscent > highestFloorTmp)
                {
                    FurthestAscent = stat.FurthestAscent;
                    highestFloorTmp = FurthestAscent;
                }

                if (stat.HighestDaily > highestDailyTmp)
                {
                    HighestDaily = stat.HighestDaily;
                    highestDailyTmp = HighestDaily;
                }

                if (stat.fastestTime < fastestTime && stat.fastestTime != 0L)
                    fastestTime = stat.fastestTime;
                totalFloorsClimbed += stat.totalFloorsClimbed;
                numVictory += stat.numVictory;
                numDeath += stat.numDeath;
                enemyKilled += stat.enemyKilled;
                bossKilled += stat.bossKilled;
                playTime += stat.playTime;
            }

            info = TEXT[0] + formatHMSM(playTime) + " NL ";
            info += TEXT[1] + numVictory + " NL ";
            info += TEXT[2] + numDeath + " NL ";
            info += TEXT[3] + totalFloorsClimbed + " NL ";
            info += TEXT[4] + bossKilled + " NL ";
            info += TEXT[5] + enemyKilled + " NL ";
            info2 = TEXT[7] + UnlockTracker.getCardsSeenString() + " NL ";
            int unlockedCardCount = UnlockTracker.unlockedRedCardCount + UnlockTracker.unlockedGreenCardCount + UnlockTracker.unlockedBlueCardCount + UnlockTracker.unlockedPurpleCardCount;
            int lockedCardCount = UnlockTracker.lockedRedCardCount + UnlockTracker.lockedGreenCardCount + UnlockTracker.lockedBlueCardCount + UnlockTracker.lockedPurpleCardCount;
            info2 += TEXT[8] + unlockedCardCount + "/" + lockedCardCount + " NL ";
            info2 += TEXT[9] + UnlockTracker.getRelicsSeenString() + " NL ";
            info2 += TEXT[10] + UnlockTracker.unlockedRelicCount + "/" + UnlockTracker.lockedRelicCount + " NL ";
            if (fastestTime != 999999999999L)
                info2 += TEXT[13] + formatHMSM(fastestTime) + " NL ";
        }

        public CharStat(APlayer c)
        {
            pref = c.getPrefs();
            cardsUnlocked = calculateCardsUnlocked(c);
            cardsDiscovered = getSeenCardCount(c);
            cardsToDiscover = getCardCountForChar(c);
            relicsUnlocked = pref.getInteger("RELIC_UNLOCK", 0);
            FurthestAscent = pref.getInteger("HIGHEST_FLOOR", 0);
            HighestDaily = pref.getInteger("HIGHEST_DAILY", 0);
            totalFloorsClimbed = pref.getInteger("TOTAL_FLOORS", 0);
            numVictory = pref.getInteger("WIN_COUNT", 0);
            numDeath = pref.getInteger("LOSE_COUNT", 0);
            winStreak = pref.getInteger("WIN_STREAK", 0);
            bestWinStreak = pref.getInteger("BEST_WIN_STREAK", 0);
            enemyKilled = pref.getInteger("ENEMY_KILL", 0);
            bossKilled = pref.getInteger("BOSS_KILL", 0);
            playTime = pref.getLong("PLAYTIME", 0L);
            fastestTime = pref.getLong("FAST_VICTORY", 0L);
            HighestScore = pref.getInteger("HIGHEST_SCORE", 0);
            info = TEXT[0] + formatHMSM(playTime) + " NL ";
            info += TEXT[7] + cardsDiscovered + "/" + cardsToDiscover + " NL ";
            info += TEXT[8] + cardsUnlocked + "/" + UnlockTracker.lockedRedCardCount + " NL ";
            if (fastestTime != 0L)
                info += TEXT[13] + formatHMSM(fastestTime) + " NL ";
            info += TEXT[23] + HighestScore + " NL ";
            if (bestWinStreak > 0)
                info += TEXT[22] + bestWinStreak + " NL ";
            info2 = TEXT[17] + numVictory + " NL ";
            info2 += TEXT[18] + numDeath + " NL ";
            info2 += TEXT[19] + totalFloorsClimbed + " NL ";
            info2 += TEXT[20] + bossKilled + " NL ";
            info2 += TEXT[21] + enemyKilled + " NL ";

            // StringBuilder sb = new();
            // sb.Append("runs").Append("/");
            // if (Game.saveSlot != 0)
            //     sb.Append(Game.saveSlot).Append("_");
            // sb.Append(c.chosenClass.ToString()).Append("/");
            // foreach (var filePath in Directory.GetFiles(sb.ToString()))
            // {
            //     try
            //     {
            //         var json = File.ReadAllText(filePath);
            //         var data = JsonConvert.DeserializeObject<RunData>(json);
            //         runs.Add(data);
            //     }
            //     catch (Exception e)
            //     {
            //         File.Delete(filePath);
            //         logger.Warn("Deleted corrupt .run file, preventing crash!", e);
            //     }
            // }
        }

        int calculateCardsUnlocked(APlayer c) => c.getUnlockedCardCount();
        int getSeenCardCount(APlayer c) => c.getSeenCardCount();
        int getCardCountForChar(APlayer c) => c.getCardCount();

        public void highestScore(int score)
        {
            if (score > HighestScore)
            {
                HighestScore = score;
                pref.putInteger("HIGHEST_SCORE", HighestScore);
                pref.flush();
            }
        }

        public void furthestAscent(int floor)
        {
            if (floor > FurthestAscent)
            {
                FurthestAscent = floor;
                pref.putInteger("HIGHEST_FLOOR", FurthestAscent);
                pref.flush();
            }
        }

        public void highestDaily(int score)
        {
            if (score > HighestDaily)
            {
                HighestDaily = score;
                pref.putInteger("HIGHEST_DAILY", HighestDaily);
                pref.flush();
            }
        }

        public void incrementFloorClimbed()
        {
            totalFloorsClimbed++;
            pref.putInteger("TOTAL_FLOORS", totalFloorsClimbed);
            pref.flush();
        }

        public void incrementDeath()
        {
            numDeath++;
            if (!ADungeon.isAscensionMode)
            {
                winStreak = 0;
                pref.putInteger("WIN_STREAK", winStreak);
            }

            pref.putInteger("LOSE_COUNT", numDeath);
            pref.flush();
        }

        public int getVictoryCount()
        {
            return numVictory;
        }

        public int getDeathCount()
        {
            return numDeath;
        }

        public void unlockAscension()
        {
            pref.putInteger("ASCENSION_LEVEL", 1);
            pref.putInteger("LAST_ASCENSION_LEVEL", 1);
        }

        public void incrementAscension()
        {
            if (!Settings.isTrial)
            {
                int derp = pref.getInteger("ASCENSION_LEVEL", 1);
                if (derp == ADungeon.ascensionLevel)
                {
                    derp++;
                    if (derp <= 20)
                    {
                        pref.putInteger("ASCENSION_LEVEL", derp);
                        pref.putInteger("LAST_ASCENSION_LEVEL", derp);
                        pref.flush();
                        log("ASCENSION LEVEL IS NOW: " + derp);
                    }
                    else
                    {
                        pref.putInteger("ASCENSION_LEVEL", 20);
                        pref.putInteger("LAST_ASCENSION_LEVEL", 20);
                        pref.flush();
                        log("MAX ASCENSION");
                    }
                }
                else
                {
                    log("Played Ascension that wasn't Max");
                }
            }
        }

        public void incrementVictory()
        {
            numVictory++;
            if (!ADungeon.isAscensionMode)
            {
                winStreak++;
                pref.putInteger("WIN_STREAK", winStreak);
                if (winStreak > pref.getInteger("BEST_WIN_STREAK", 0))
                    pref.putInteger("BEST_WIN_STREAK", winStreak);
            }

            pref.putInteger("WIN_COUNT", numVictory);
            pref.flush();
        }

        public void incrementBossSlain()
        {
            bossKilled++;
            pref.putInteger("BOSS_KILL", bossKilled);
            pref.flush();
        }

        public void incrementEnemySlain()
        {
            enemyKilled++;
            pref.putInteger("ENEMY_KILL", enemyKilled);
            pref.flush();
        }

        public void incrementPlayTime(long time)
        {
            playTime += time;
            pref.putLong("PLAYTIME", playTime);
            pref.flush();
        }

        public static string formatHMSM(float t)
        {
            string res;
            long duration = (long)t;
            int seconds = (int)(duration % 60L);
            duration /= 60L;
            int minutes = (int)(duration % 60L);
            int hours = (int)t / 3600;
            if (hours > 0)
            {
                res = string.Format(TEXT[24], hours, minutes, seconds);
            }
            else
            {
                res = string.Format(TEXT[25], minutes, seconds);
            }

            return res;
        }

        public static string formatHMSM(long t)
        {
            string res;
            long duration = t;
            int seconds = (int)(duration % 60L);
            duration /= 60L;
            int minutes = (int)(duration % 60L);
            int hours = (int)t / 3600;
            if (hours > 0)
            {
                res = string.Format(TEXT[26], hours, minutes, seconds);
            }
            else
            {
                res = string.Format(TEXT[27], minutes, seconds);
            }

            return res;
        }

        public static string formatHMSM(int t)
        {
            string res;
            long duration = t;
            int seconds = (int)(duration % 60L);
            duration /= 60L;
            int minutes = (int)(duration % 60L);
            int hours = t / 3600;
            res = string.Format(TEXT[28], hours, minutes, seconds);
            return res;
        }

        public void updateFastestVictory(long newTime)
        {
            if (newTime < fastestTime || fastestTime == 0L)
            {
                fastestTime = newTime;
                pref.putLong("FAST_VICTORY", fastestTime);
                pref.flush();
                log("Fastest victory time updated to: " + fastestTime);
            }
            else
            {
                log("Did not save fastest victory.");
            }
        }

        // public void render(SpriteBatch sb, float screenX, float renderY)
        // {
        //     FontHelper.renderSmartText(sb, FontHelper.panelNameFont, info, screenX + 75.0F * Settings.scale, renderY + 766.0F * Settings.yScale, 9999.0F, 38.0F * Settings.scale, Settings.CREAM_COLOR);
        //     if (info2 != null)
        //         FontHelper.renderSmartText(sb, FontHelper.panelNameFont, info2, screenX + 675.0F * Settings.scale, renderY + 766.0F * Settings.yScale, 9999.0F, 38.0F * Settings.scale, Settings.CREAM_COLOR);
        // }
    }
}