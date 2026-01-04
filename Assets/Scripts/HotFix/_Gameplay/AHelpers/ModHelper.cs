using System.Collections.Generic;

namespace MarbleHero
{
    public class ModHelper
    {
        static Dictionary<string, ADailyMod> starterMods = new();
        static Dictionary<string, ADailyMod> genericMods = new();
        static Dictionary<string, ADailyMod> difficultyMods = new();
        static Dictionary<string, ADailyMod> legacyMods = new();
        public static List<ADailyMod> enabledMods = new();

        public static void initialize()
        {
            // addStarterMod(new Shiny());
            // addStarterMod(new Allstar());
            // addStarterMod(new Draft());
            // addStarterMod(new SealedDeck());
            // addStarterMod(new Insanity());
            // addStarterMod(new Heirloom());
            // addStarterMod(new Specialized());
            // addStarterMod(new Chimera());
            // addStarterMod(new CursedRun());
            // addGenericMod(new Diverse());
            // addGenericMod(new RedCards());
            // addGenericMod(new GreenCards());
            // addGenericMod(new BlueCards());
            // addGenericMod(new PurpleCards());
            // addGenericMod(new ColorlessCards());
            // addGenericMod(new TimeDilation());
            // addGenericMod(new Vintage());
            // addGenericMod(new Hoarder());
            // addGenericMod(new Flight());
            // addGenericMod(new CertainFuture());
            // addGenericMod(new ControlledChaos());
            // addDifficultyMod(new BigGameHunter());
            // addDifficultyMod(new Lethality());
            // addDifficultyMod(new NightTerrors());
            // addDifficultyMod(new Binary());
            // addDifficultyMod(new Midas());
            // addDifficultyMod(new Terminal());
            // addDifficultyMod(new DeadlyEvents());
            // addLegacyMod(new Brewmaster());
            // addLegacyMod(new Colossus());
        }

        static void addStarterMod(ADailyMod mod)
        {
            starterMods.Add(mod.modID, mod);
        }

        static void addGenericMod(ADailyMod mod)
        {
            genericMods.Add(mod.modID, mod);
        }

        static void addDifficultyMod(ADailyMod mod)
        {
            difficultyMods.Add(mod.modID, mod);
        }

        static void addLegacyMod(ADailyMod mod)
        {
            legacyMods.Add(mod.modID, mod);
        }

        public static void setMods(List<string> modIDs)
        {
            setModsFalse();
            foreach (string m in modIDs)
            {
                if (m != "Endless")
                    enabledMods.Add(getMod(m));
            }
        }

        public static ADailyMod getMod(string key)
        {
            starterMods.TryGetValue(key, out var mod);
            if (mod == null)
                genericMods.TryGetValue(key, out mod);
            if (mod == null)
                difficultyMods.TryGetValue(key, out mod);
            if (mod == null)
                legacyMods.TryGetValue(key, out mod);
            return mod;
        }

        public static List<string> getEnabledModIDs()
        {
            List<string> enabled = new();
            foreach (ADailyMod m in enabledMods)
            {
                if (m != null)
                    enabled.Add(m.modID);
            }

            return enabled;
        }

        static void setTheMods(Dictionary<string, ADailyMod> modMap, long daysSince1970, APlayer.PlayerClass characterClass)
        {
            List<ADailyMod> shuffledList = new();
            foreach (var (id, mod) in modMap)
            {
                if (mod.classToExclude != characterClass)
                    shuffledList.Add(mod);
            }

            int rotationConstant = 5;
            int modSelectionIndex = (int)(daysSince1970 % rotationConstant);
            if (modSelectionIndex < 0)
                modSelectionIndex += rotationConstant;
            int shuffleInterval = (int)(daysSince1970 / rotationConstant);
            // Collections.shuffle(shuffledList, new Random(shuffleInterval));
            enabledMods.Add(shuffledList[modSelectionIndex]);
        }

        public static void setTodaysMods(long daysSince1970, APlayer.PlayerClass chosenClass)
        {
            setModsFalse();
            setTheMods(starterMods, daysSince1970, chosenClass);
            setTheMods(genericMods, daysSince1970, chosenClass);
            setTheMods(difficultyMods, daysSince1970, chosenClass);
        }

        public static bool isModEnabled(string modID)
        {
            foreach (ADailyMod m in enabledMods)
            {
                if (m is { modID: not null } && m.modID == modID)
                    return true;
            }

            return false;
        }

        public static void setModsFalse()
        {
            enabledMods.Clear();
        }

        public static void uploadModData()
        {
            List<string> data = new();
            foreach (var (id, mod) in starterMods)
                data.Add(mod.gameDataUploadData());
            foreach (var (id, mod) in genericMods)
                data.Add(mod.gameDataUploadData());
            foreach (var (id, mod) in difficultyMods)
                data.Add(mod.gameDataUploadData());

            // BotDataUploader.uploadDataAsync(BotDataUploader.GameDataType.DAILY_MOD_DATA, ADailyMod.gameDataUploadHeader(), data);
        }

        public static void clearNulls()
        {
            for (int i = enabledMods.Count - 1; i >= 0; i--)
            {
                if (enabledMods[i] == null)
                {
                    enabledMods.RemoveAt(i);
                }
            }
        }
    }
}