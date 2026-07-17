using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MoreMountains
{
    public class SaveHelper
    {
        const string FileSeparator = "/";

        public static void initialize()
        {
        }

        static bool isGog()
        {
            return false;
            // return Game.publisherIntegration.getType() == DistributorFactory.Distributor.GOG;
        }

        static string getSaveDir()
        {
            if (Settings.isBeta || isGog())
                return "betaPreferences";
            return "preferences";
        }

        public static bool doesPrefExist(string name)
        {
            return File.Exists(getSaveDir() + FileSeparator + name);
        }

        public static void deletePrefs(int slot)
        {
            string dir = getSaveDir() + FileSeparator;
            deleteFile(dir + slotName("DataVagabond", slot));
            deleteFile(dir + slotName("DataTheSilent", slot));
            deleteFile(dir + slotName("DataDefect", slot));
            deleteFile(dir + slotName("DataWatcher", slot));
            deleteFile(dir + slotName("Achievements", slot));
            deleteFile(dir + slotName("Daily", slot));
            deleteFile(dir + slotName("SeenBosses", slot));
            deleteFile(dir + slotName("SeenCards", slot));
            deleteFile(dir + slotName("BetaCardPreference", slot));
            deleteFile(dir + slotName("SeenRelics", slot));
            deleteFile(dir + slotName("UnlockProgress", slot));
            deleteFile(dir + slotName("Unlocks", slot));
            deleteFile(dir + slotName("GameplaySettings", slot));
            deleteFile(dir + slotName("InputSettings", slot));
            deleteFile(dir + slotName("InputSettings_Controller", slot));
            deleteFile(dir + slotName("Sound", slot));
            deleteFile(dir + slotName("Player", slot));
            deleteFile(dir + slotName("Tips", slot));

            dir = "runs" + FileSeparator;
            deleteFolder(dir + slotName("IRONCLAD", slot));
            deleteFolder(dir + slotName("THE_SILENT", slot));
            deleteFolder(dir + slotName("DEFECT", slot));
            deleteFolder(dir + slotName("WATCHER", slot));

            deleteFolder(dir + slotName("DAILY", slot));
            dir = "saves" + FileSeparator;
            deleteFile(dir + slotName("IRONCLAD.autosave", slot));
            deleteFile(dir + slotName("DEFECT.autosave", slot));
            deleteFile(dir + slotName("THE_SILENT.autosave", slot));
            deleteFile(dir + slotName("WATCHER.autosave", slot));

            deleteFile(dir + slotName("IRONCLAD.autosave.backUp", slot));
            deleteFile(dir + slotName("DEFECT.autosave.backUp", slot));
            deleteFile(dir + slotName("THE_SILENT.autosave.backUp", slot));
            deleteFile(dir + slotName("WATCHER.autosave.backUp", slot));
            if (Settings.isBeta || isGog())
            {
                deleteFile(dir + slotName("IRONCLAD.autosaveBETA", slot));
                deleteFile(dir + slotName("DEFECT.autosaveBETA", slot));
                deleteFile(dir + slotName("THE_SILENT.autosaveBETA", slot));
                deleteFile(dir + slotName("WATCHER.autosaveBETA", slot));

                deleteFile(dir + slotName("IRONCLAD.autosaveBETA.backUp", slot));
                deleteFile(dir + slotName("DEFECT.autosaveBETA.backUp", slot));
                deleteFile(dir + slotName("THE_SILENT.autosaveBETA.backUp", slot));
                deleteFile(dir + slotName("WATCHER.autosaveBETA.backUp", slot));
            }

            Game.saveSlotPref.putString(slotName("PROFILE_NAME", slot), "");
            Game.saveSlotPref.putFloat(slotName("COMPLETION", slot), 0.0F);
            Game.saveSlotPref.putLong(slotName("PLAYTIME", slot), 0L);
            Game.saveSlotPref.flush();
            if (slot == Game.saveSlot || Game.saveSlot == -1)
            {
                bool newDefaultSet = false;
                for (int i = 0; i < 3; i++)
                {
                    var name = Game.saveSlotPref.getString(slotName("PROFILE_NAME", i), "");
                    if (!string.IsNullOrEmpty(name))
                    {
                        log("Current slot deleted, DEFAULT_SLOT is now " + i);
                        Game.saveSlotPref.putInteger("DEFAULT_SLOT", i);
                        newDefaultSet = true;
                        // SaveSlotScreen.slotDeleted = true;
                        break;
                    }
                }

                if (!newDefaultSet)
                {
                    log("All slots deleted, DEFAULT_SLOT is now -1");
                    Game.saveSlotPref.putInteger("DEFAULT_SLOT", -1);
                }

                Game.saveSlotPref.flush();
            }
        }

        static void deleteFile(string fileName)
        {
            log("Deleting " + fileName);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                log(fileName + " deleted.");
            }

            if (File.Exists(fileName + ".backUp"))
            {
                File.Delete(fileName + ".backUp");
                log(fileName + ".backUp deleted.");
            }
        }

        static void deleteFolder(string dirName)
        {
            log("Deleting " + dirName);
            if (Directory.Exists(dirName))
            {
                Directory.Delete(dirName);
                log(dirName + " deleted.");
            }
        }

        public static string slotName(string name, int slot)
        {
            switch (slot)
            {
                case 0:
                    return name;
            }

            name = slot + "_" + name;
            return name;
        }

        public static Prefs getPrefs(string name)
        {
            switch (Game.saveSlot)
            {
                case 0:
                    break;
                default:
                    name = Game.saveSlot + "_" + name;
                    break;
            }

            string prefName = getSaveDir() + FileSeparator + name;
            Prefs retVal = new Prefs(prefName);
            string filepath = retVal.FilePath;
            string jsonStr = null;
            try
            {
                jsonStr = loadJson(filepath);
                if (string.IsNullOrWhiteSpace(jsonStr))
                {
                    logError("Empty Pref file: name=" + name + ", filepath=" + filepath);
                    handleCorruption(jsonStr, filepath, name);
                    retVal = getPrefs(name);
                }
                else
                {
                    retVal.data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
                }
            }
            catch (Exception e)
            {
                logException(e, "Corrupt Pref file");
                handleCorruption(jsonStr, filepath, name);
                retVal = getPrefs(name);
            }

            return retVal;
        }

        static void handleCorruption(string jsonStr, string filepath, string name)
        {
            preserveCorruptFile(filepath);
            if (File.Exists(filepath + ".backUp"))
            {
                File.Move(filepath + ".backUp", filepath);
                log("Original corrupted, backup loaded for " + filepath);
            }
        }

        public static void preserveCorruptFile(string filePath)
        {
            File.Move(filePath, "sendToDevs" + FileSeparator + filePath + ".corrupt");
        }

        public static string loadJson(string filepath)
        {
            if (File.Exists(filepath))
                return File.ReadAllText(filepath);

            AsyncSaver.save(filepath, JsonConvert.SerializeObject(new Dictionary<string, string>()));
            return "{}";
        }

        public static bool saveExists()
        {
            var sb = new StringBuilder();
            sb.Append(getSaveDir()).Append(FileSeparator);
            switch (Game.saveSlot)
            {
                case 0:
                    sb.Append("Player");
                    return File.Exists(sb.ToString());
                case 1:
                case 2:
                case 3:
                    sb.Append(Game.saveSlot).Append("_STSPlayer");
                    return File.Exists(sb.ToString());
            }

            sb.Append("Player");
            return File.Exists(sb.ToString());
        }

        public static void saveIfAppropriate(SaveType saveType)
        {
            if (!shouldSave())
                return;
            
            var saveFile = new SaveFile(saveType);
            SaveAndContinue.save(saveFile);
            // ADungeon.effectList.add(new GameSavedEffect());
        }

        public static bool shouldSave()
        {
            if (ADungeon.nextRoom != null && ADungeon.nextRoom.room is TrueVictoryRoom)
                return false;
            return !Settings.isDemo;
        }

        public static bool shouldDeleteSave()
        {
            return !Settings.isDemo;
        }
    }
}