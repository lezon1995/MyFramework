using System.Collections.Generic;

namespace MarbleHero
{
    public class TipTracker
    {
        public static Prefs pref;
        public static Dictionary<string, bool> tips = new();
        public static string NEOW_SKIP = "NEOW_SKIP";
        public static string NEOW_INTRO = "NEOW_INTRO";
        public static string NO_FTUE_CHECK = "NO_FTUE";
        public static string COMBAT_TIP = "COMBAT_TIP";
        public static string BLOCK_TIP = "BLOCK TIP";
        public static string POWER_TIP = "POWER_TIP";
        public static string M_POWER_TIP = "M_POWER_TIP";
        public static string ENERGY_USE_TIP = "ENERGY_USE_TIP";
        public static int energyUseCounter;
        public static string SHUFFLE_TIP = "SHUFFLE_TIP";
        public static int shuffleCounter;
        public static int SHUFFLE_THRESHOLD = 1;
        public static string POTION_TIP = "POTION_TIP";
        public static string CARD_REWARD_TIP = "CARD_REWARD_TIP";
        public static string INTENT_TIP = "INTENT_TIP";
        public static int blockCounter;
        public static int BLOCK_THRESHOLD = 3;
        public static string RELIC_TIP = "RELIC_TIP";
        public static int relicCounter;

        public static void initialize()
        {
            pref = SaveHelper.getPrefs("Tips");
            refresh();
        }

        public static void refresh()
        {
            tips.Clear();
            tips["NEOW_SKIP"] = pref.getBoolean("NEOW_SKIP", false);
            tips["NEOW_INTRO"] = pref.getBoolean("NEOW_INTRO", false);
            tips["NO_FTUE"] = pref.getBoolean("NO_FTUE", false);
            tips["COMBAT_TIP"] = pref.getBoolean("COMBAT_TIP", false);
            tips["BLOCK TIP"] = pref.getBoolean("BLOCK TIP", false);
            tips["POWER_TIP"] = pref.getBoolean("POWER_TIP", false);
            tips["M_POWER_TIP"] = pref.getBoolean("M_POWER_TIP", false);
            tips["ENERGY_USE_TIP"] = pref.getBoolean("ENERGY_USE_TIP", false);
            if (tips["ENERGY_USE_TIP"])
                energyUseCounter = 9;
            else
                energyUseCounter = 0;

            tips["SHUFFLE_TIP"] = pref.getBoolean("SHUFFLE_TIP", false);
            if (tips["SHUFFLE_TIP"])
                shuffleCounter = 99;
            else
                shuffleCounter = 0;

            shuffleCounter = 0;
            tips["POTION_TIP"] = pref.getBoolean("POTION_TIP", false);
            tips["CARD_REWARD_TIP"] = pref.getBoolean("CARD_REWARD_TIP", false);
            tips["INTENT_TIP"] = pref.getBoolean("INTENT_TIP", false);
            blockCounter = 0;

            tips["RELIC_TIP"] = pref.getBoolean("RELIC_TIP", false);
            if (tips["RELIC_TIP"])
                relicCounter = 99;
            else
                relicCounter = 0;
        }

        public static void neverShowAgain(string key)
        {
            log(key + " will never be shown again!");
            pref.putBoolean(key, true);
            tips[key] = true;
            pref.flush();
        }

        public static void showAgain(string key)
        {
            log(key + " is reactivated");
            pref.putBoolean(key, false);
            tips[key] = false;
            pref.flush();
        }

        public static void disableAllFtues()
        {
            neverShowAgain("BLOCK TIP");
            neverShowAgain("CARD_REWARD_TIP");
            neverShowAgain("COMBAT_TIP");
            neverShowAgain("ENERGY_USE_TIP");
            neverShowAgain("INTENT_TIP");
            neverShowAgain("M_POWER_TIP");
            neverShowAgain("NO_FTUE");
            neverShowAgain("POTION_TIP");
            neverShowAgain("POWER_TIP");
            neverShowAgain("RELIC_TIP");
            neverShowAgain("SHUFFLE_TIP");
        }

        public static void reset()
        {
            foreach (var (key, flag) in tips)
                showAgain(key);
        }
    }
}