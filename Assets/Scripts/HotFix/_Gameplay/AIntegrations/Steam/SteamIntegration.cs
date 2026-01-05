using System.Collections.Generic;
using System.Threading;

namespace MarbleHero;

public class SteamIntegration : IPublisherIntegration
{
    static string[] TEXT;
    // static SteamUserStats steamStats;
    // static SteamUser steamUser;
    // static SteamApps steamApps;
    // static SteamFriends steamFriends;
    static Thread thread;
    static int accountId = -1;
    // static SteamLeaderboardHandle lbHandle = null;
    static LeaderboardTask task;
    static bool retrieveGlobal = true;
    static bool gettingTime;
    static int lbScore;
    static int startIndex;
    static int endIndex;
    static bool isUploadingScore;
    static Queue<StatTuple> statsToUpload = new();

    public enum LeaderboardTask
    {
        RETRIEVE,
        RETRIEVE_DAILY,
        UPLOAD,
        UPLOAD_DAILY
    }

    public class StatTuple
    {
        public string stat;
        public int score;

        public StatTuple(string statName, int scoreVal)
        {
            stat = statName;
            score = scoreVal;
        }
    }

    public SteamIntegration()
    {
        if (!Settings.isDev || Settings.isModded)
        {
            /*
            try
            {
                SteamAPI.loadLibraries();
                if (SteamAPI.init())
                {
                    log("[SUCCESS] Steam API initialized successfully.");
                    steamStats = new SteamUserStats(new SSCallback(this));
                    steamUser = new SteamUser(new SUCallback());
                    steamApps = new SteamApps();
                    steamFriends = new SteamFriends(new SFCallback());
                    log("BUILD ID: " + steamApps.getAppBuildId());
                    log("CURRENT LANG: " + steamApps.getCurrentGameLanguage());
                    SteamID id = steamApps.getAppOwner();
                    accountId = id.getAccountID();
                    log("ACCOUNT ID: " + accountId);
                    thread = new Thread(new SteamTicker());
                    thread.Name = "SteamTicker";
                    thread.Start();
                }
                else
                {
                    log("[FAILURE] Steam API failed to initialize correctly.");
                }
            }
            catch (SteamException e)
            {
                e.printStackTrace();
            }
        */
        }

        // if (SteamAPI.isSteamRunning())
        //     requestGlobalStats(365);
    }

    public bool isInitialized()
    {
        return false;
        // return (steamUser != null && steamStats != null);
    }

    public List<string> getAllCloudFiles()
    {
        List<string> files = new();
        
        /*var remoteStorage = new SteamRemoteStorage(new SRCallback());
        int numFiles = remoteStorage.getFileCount();
        log("Num of files: " + numFiles);
        for (int i = 0; i < numFiles; i++)
        {
            int[] sizes = new int[1];
            string file = remoteStorage.getFileNameAndSize(i, sizes);
            bool exists = remoteStorage.fileExists(file);
            if (exists)
                files.add(file);
            log("# " + i + " : name=" + file + ", size=" + sizes[0] + ", exists=" + (exists ? "yes" : "no"));
        }

        remoteStorage.dispose();*/
        return files;
    }

    public void deleteAllCloudFiles()
    {
        deleteCloudFiles(getAllCloudFiles());
        log("Deleted all Cloud Files");
    }

    void deleteCloudFiles(List<string> files)
    {
        /*var remoteStorage = new SteamRemoteStorage(new SRCallback());
        foreach (string file in files)
        {
            log("Deleting file: " + file);
            remoteStorage.fileDelete(file);
        }

        remoteStorage.dispose();*/
    }

    public static string basename(string path)
    {
        return path;
        // Path p = Path.get(path);
        // return p.getFileName().toString();
    }

    public void unlockAchievement(string id)
    {
        log("unlockAchievement: " + id);
        // if (steamStats != null)
        // {
        //     if (steamStats.setAchievement(id))
        //     {
        //         steamStats.storeStats();
        //     }
        //     else
        //     {
        //         log("[ERROR] Could not find achievement " + id);
        //     }
        // }
    }

    public static void removeAllAchievementsBeCarefulNotToPush()
    {
        /*if (Settings.isDev && Settings.isBeta && steamStats != null && steamStats.resetAllStats(true))
        {
            steamStats.storeStats();
        }*/
    }

    public bool incrementStat(string id, int incrementAmt)
    {
        log("incrementStat: " + id);
        /*if (steamStats != null)
        {
            if (steamStats.setStatI(id, getStat(id) + incrementAmt))
                return true;
            log("Stat: " + id + " not found.");
            return false;
        }*/

        log("[ERROR] Could not find stat " + id);
        return false;
    }

    public int getStat(string id)
    {
        log("getStat: " + id);
        // if (steamStats != null)
            // return steamStats.getStatI(id, 0);
        return -1;
    }

    public bool setStat(string id, int value)
    {
        log("setStat: " + id);
        /*if (steamStats != null)
        {
            if (steamStats.setStatI(id, value))
            {
                log(id + " stat set to " + value);
                return true;
            }

            log("Stat: " + id + " not found.");
            return false;
        }*/

        log("[ERROR] Could not find stat " + id);
        return false;
    }

    public long getGlobalStat(string id)
    {
        log("getGlobalStat");
        // if (steamStats != null)
            // return steamStats.getGlobalStat(id, 0L);
        return -1L;
    }

    static void requestGlobalStats(int i)
    {
        log("requestGlobalStats");
        // if (steamStats != null)
            // steamStats.requestGlobalStats(i);
    }

    /*public void getLeaderboardEntries(APlayer.PlayerClass pClass, FilterButton.RegionSetting rSetting, FilterButton.LeaderboardType lType, int start, int end)
    {
        task = LeaderboardTask.RETRIEVE;
        startIndex = start;
        endIndex = end;
        gettingTime = lType == FilterButton.LeaderboardType.FASTEST_WIN;
        retrieveGlobal = rSetting == FilterButton.RegionSetting.GLOBAL;
        if (steamStats != null)
            steamStats.findLeaderboard(createGetLeaderboardString(pClass, lType));
    }*/

    public void getDailyLeaderboard(long date, int start, int end)
    {
        task = LeaderboardTask.RETRIEVE_DAILY;
        startIndex = start;
        endIndex = end;
        retrieveGlobal = true;
        gettingTime = false;
        /*if (steamStats != null)
        {
            StringBuilder leaderboardRetrieveString = new StringBuilder("DAILY_");
            leaderboardRetrieveString.Append(date);
            if (Settings.isBeta)
                leaderboardRetrieveString.Append("_BETA");
            steamStats.findOrCreateLeaderboard(leaderboardRetrieveString.ToString(), SteamUserStats.LeaderboardSortMethod.Descending, SteamUserStats.LeaderboardDisplayType.Numeric);
        }*/
    }

    /*
    static string createGetLeaderboardString(APlayer.PlayerClass pClass, FilterButton.LeaderboardType lType)
    {
        string retVal = "";
        switch (pClass)
        {
            case AVG_FLOOR:
                retVal = retVal + "IRONCLAD";
                break;
            case AVG_SCORE:
                retVal = retVal + "SILENT";
                break;
            case CONSECUTIVE_WINS:
                retVal = retVal + "DEFECT";
                break;
            case FASTEST_WIN:
                retVal = retVal + "WATCHER";
                break;
        }

        switch (lType)
        {
            case AVG_FLOOR:
                retVal = retVal + "_AVG_FLOOR";
                break;
            case AVG_SCORE:
                retVal = retVal + "_AVG_SCORE";
                break;
            case CONSECUTIVE_WINS:
                retVal = retVal + "_CONSECUTIVE_WINS";
                break;
            case FASTEST_WIN:
                retVal = retVal + "_FASTEST_WIN";
                break;
            case HIGH_SCORE:
                retVal = retVal + "_HIGH_SCORE";
                break;
            case SPIRE_LEVEL:
                retVal = retVal + "_SPIRE_LEVEL";
                break;
        }

        if (Settings.isBeta)
            retVal = retVal + "_BETA";
        return retVal;
    }
    */

    public void uploadLeaderboardScore(string name, int score)
    {
        /*if (steamUser == null || steamStats == null)
            return;
        if (isUploadingScore)
        {
            statsToUpload.Enqueue(new StatTuple(name, score));
        }
        else
        {
            log(string.Format("Uploading Steam Leaderboard score (%s: %d)", name, score));
            isUploadingScore = true;
            task = LeaderboardTask.UPLOAD;
            lbScore = score;
            steamStats.findLeaderboard(name);
        }*/
    }

    public void uploadDailyLeaderboardScore(string name, int score)
    {
        /*
        if (!TimeHelper.isOfflineMode())
        {
            if (steamUser == null || steamStats == null)
            {
                log("User is NOT connected to Steam, unable to upload daily score.");
                return;
            }

            if (isUploadingScore)
            {
                statsToUpload.add(new StatTuple(name, score));
            }
            else
            {
                log(string.Format("Uploading [DAILY] Steam Leaderboard score (%s: %d)", name, score));
                isUploadingScore = true;
                task = LeaderboardTask.UPLOAD_DAILY;
                lbScore = score;
                steamStats.findOrCreateLeaderboard(name, SteamUserStats.LeaderboardSortMethod.Descending, SteamUserStats.LeaderboardDisplayType.Numeric);
            }
        }
    */
    }

    void didCompleteCallback(bool success)
    {
        log("didCompleteCallback");
        isUploadingScore = false;
        if (statsToUpload.Count > 0)
        {
            // StatTuple uploadMe = statsToUpload.remove();
            // uploadLeaderboardScore(uploadMe.stat, uploadMe.score);
        }
    }

    static void uploadLeaderboardHelper()
    {
        log("uploadLeaderboardHelper");
        // steamStats.uploadLeaderboardScore(lbHandle, SteamUserStats.LeaderboardUploadScoreMethod.KeepBest, lbScore, new int[0]);
    }

    static void uploadDailyLeaderboardHelper()
    {
        log("uploadDailyLeaderboardHelper");
        // steamStats.uploadLeaderboardScore(lbHandle, SteamUserStats.LeaderboardUploadScoreMethod.KeepBest, lbScore, new int[0]);
    }

    /*
    static void getLeaderboardEntryHelper()
    {
        if (task == LeaderboardTask.RETRIEVE)
        {
            if (retrieveGlobal)
            {
                log("Downloading GLOBAL entries: " + startIndex + " - " + endIndex);
                if (Game.mainMenuScreen.leaderboardsScreen.viewMyScore)
                {
                    steamStats.downloadLeaderboardEntries(lbHandle, SteamUserStats.LeaderboardDataRequest.GlobalAroundUser, -9, 10);
                    Game.mainMenuScreen.leaderboardsScreen.viewMyScore = false;
                }
                else
                {
                    steamStats.downloadLeaderboardEntries(lbHandle, SteamUserStats.LeaderboardDataRequest.Global, startIndex, endIndex);
                }
            }
            else
            {
                log("Downloading FRIEND entries: " + startIndex + " - " + endIndex);
                steamStats.downloadLeaderboardEntries(lbHandle, SteamUserStats.LeaderboardDataRequest.Friends, startIndex, endIndex);
            }
        }
        else if (task == LeaderboardTask.RETRIEVE_DAILY)
        {
            if (Game.mainMenuScreen.dailyScreen.viewMyScore)
            {
                steamStats.downloadLeaderboardEntries(lbHandle, SteamUserStats.LeaderboardDataRequest.GlobalAroundUser, -9, 10);
                Game.mainMenuScreen.dailyScreen.viewMyScore = false;
            }
            else
            {
                log("Downloading GLOBAL entries: " + startIndex + " - " + endIndex);
                steamStats.downloadLeaderboardEntries(lbHandle, SteamUserStats.LeaderboardDataRequest.Global, startIndex, endIndex);
            }
        }
    }
    */

    public void setRichPresenceDisplayPlaying(int floor, int ascension, string character)
    {
        /*
        if (TEXT == null)
            TEXT = (Game.languagePack.getUIString("RichPresence")).TEXT;
        if (Settings.isDailyRun)
        {
            string msg = string.Format(TEXT[0], floor);
            log("Setting Rich Presence: " + msg);
            setRichPresenceData("status", msg);
        }
        else if (Settings.isTrial)
        {
            string msg = string.Format(TEXT[1], floor);
            log("Setting Rich Presence: " + msg);
            setRichPresenceData("status", msg);
        }
        else if (Settings.language == Settings.GameLanguage.ENG || Settings.language == Settings.GameLanguage.DEU || Settings.language == Settings.GameLanguage.THA || Settings.language == Settings.GameLanguage.TUR || Settings.language == Settings.GameLanguage.KOR || Settings.language == Settings.GameLanguage.RUS || Settings.language == Settings.GameLanguage.SPA || Settings.language == Settings.GameLanguage.DUT)
        {
            string msg = string.Format(TEXT[4] + character + TEXT[2], ascension, floor);
            log("Setting Rich Presence: " + msg);
            setRichPresenceData("status", msg);
        }
        else
        {
            string msg = string.Format(character + TEXT[2] + TEXT[4], floor, ascension);
            log("Setting Rich Presence: " + msg);
            setRichPresenceData("status", msg);
        }
        */

        setRichPresenceData("steam_display", "#Status");
    }

    public void setRichPresenceDisplayPlaying(int floor, string character)
    {
        /*
        if (TEXT == null)
            TEXT = (Game.languagePack.getUIString("RichPresence")).TEXT;
        if (Settings.isDailyRun)
        {
            string msg = string.Format(TEXT[0], floor);
            log("Setting Rich Presence: " + msg);
            setRichPresenceData("status", msg);
        }
        else if (Settings.isTrial)
        {
            string msg = string.Format(TEXT[1], floor);
            log("Setting Rich Presence: " + msg);
            setRichPresenceData("status", msg);
        }
        else
        {
            string msg = string.Format(character + TEXT[2], floor);
            log("Setting Rich Presence: " + msg);
            setRichPresenceData("status", msg);
        }
        */

        setRichPresenceData("steam_display", "#Status");
    }

    public void setRichPresenceDisplayInMenu()
    {
        // if (TEXT == null)
            // TEXT = (Game.languagePack.getUIString("RichPresence")).TEXT;
        // log("Setting Rich Presence: " + string.Format(TEXT[3]));
        // setRichPresenceData("status", TEXT[3]);
        setRichPresenceData("steam_display", "#Status");
    }

    public int getNumUnlockedAchievements()
    {
        int retVal = 0;
        List<string> keys = new();
        keys.add("ADRENALINE");
        keys.add("ASCEND_0");
        keys.add("ASCEND_10");
        keys.add("ASCEND_20");
        keys.add("AUTOMATON");
        keys.add("BARRICADED");
        keys.add("CATALYST");
        keys.add("CHAMP");
        keys.add("COLLECTOR");
        keys.add("COME_AT_ME");
        keys.add("COMMON_SENSE");
        keys.add("CROW");
        keys.add("DONUT");
        keys.add("EMERALD");
        keys.add("EMERALD_PLUS");
        keys.add("FOCUSED");
        keys.add("GHOST_GUARDIAN");
        keys.add("GUARDIAN");
        keys.add("IMPERVIOUS");
        keys.add("INFINITY");
        keys.add("JAXXED");
        keys.add("LUCKY_DAY");
        keys.add("MINIMALIST");
        keys.add("NEON");
        keys.add("NINJA");
        keys.add("ONE_RELIC");
        keys.add("PERFECT");
        keys.add("PLAGUE");
        keys.add("POWERFUL");
        keys.add("PURITY");
        keys.add("RUBY");
        keys.add("RUBY_PLUS");
        keys.add("SAPPHIRE");
        keys.add("SAPPHIRE_PLUS");
        keys.add("AMETHYST");
        keys.add("AMETHYST_PLUS");
        keys.add("SHAPES");
        keys.add("SHRUG_IT_OFF");
        keys.add("SLIME_BOSS");
        keys.add("SPEED_CLIMBER");
        keys.add("THE_ENDING");
        keys.add("THE_PACT");
        keys.add("TIME_EATER");
        keys.add("TRANSIENT");
        keys.add("YOU_ARE_NOTHING");
        // foreach (string s in keys)
        // {
        //     if (steamStats.isAchieved(s, false))
        //         retVal++;
        // }

        return retVal;
    }

    public DistributorFactory.Distributor getType()
    {
        return DistributorFactory.Distributor.STEAM;
    }

    void setRichPresenceData(string key, string value)
    {
        // if (steamFriends != null && !steamFriends.setRichPresence(key, value))
            // log("Failed to set Steam Rich Presence: key=" + key + " value=" + value);
    }

    public void dispose()
    {
        // if (isInitialized())
            // SteamAPI.shutdown();
    }
}