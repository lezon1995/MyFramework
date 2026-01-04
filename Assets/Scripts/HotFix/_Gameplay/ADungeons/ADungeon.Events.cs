using System.Collections.Generic;

namespace MarbleHero
{
    public struct OnDungeonMapGenerated
    {
    }

    public struct OpenMapPanel
    {
    }

    public abstract partial class ADungeon
    {
        public static List<string> eventList = new();
        public static List<string> shrineList = new();
        public static List<string> specialOneTimeEventList = new();

        public static float colorlessRareChance;
        public static float shopRoomChance;
        public static float restRoomChance;
        public static float eventRoomChance;
        public static float eliteRoomChance;
        public static float treasureRoomChance;
        public static int smallChestChance;
        public static int mediumChestChance;
        public static int largeChestChance;
        public static int commonRelicChance;
        public static int uncommonRelicChance;
        public static int rareRelicChance;
        public static float cardUpgradedChance;
        
        protected abstract void initializeEventList();
        protected abstract void initializeEventImg();
        protected abstract void initializeShrineList();

        public virtual void initializeLevelSpecificChances()
        {
            shopRoomChance =  0.05F;
            restRoomChance =  0.12F;
            treasureRoomChance =  0.0F;
            eventRoomChance =  0.22F;
            eliteRoomChance =  0.08F;
            smallChestChance =  50;
            mediumChestChance =  33;
            largeChestChance =  17;
            commonRelicChance =  50;
            uncommonRelicChance =  33;
            rareRelicChance =  17;
            colorlessRareChance =  0.3F;
            cardUpgradedChance =  0.0F;
        }

        protected void initializeSpecialOneTimeEventList()
        {
            specialOneTimeEventList.Clear();
            specialOneTimeEventList.Add("Accursed Blacksmith");
            specialOneTimeEventList.Add("Bonfire Elementals");
            specialOneTimeEventList.Add("Designer");
            specialOneTimeEventList.Add("Duplicator");
            specialOneTimeEventList.Add("FaceTrader");
            specialOneTimeEventList.Add("Fountain of Cleansing");
            specialOneTimeEventList.Add("Knowing Skull");
            specialOneTimeEventList.Add("Lab");
            specialOneTimeEventList.Add("N'loth");
            if (isNoteForYourselfAvailable())
                specialOneTimeEventList.Add("NoteForYourself");
            specialOneTimeEventList.Add("SecretPortal");
            specialOneTimeEventList.Add("The Joust");
            specialOneTimeEventList.Add("WeMeetAgain");
            specialOneTimeEventList.Add("The Woman in Blue");
        }

        bool isNoteForYourselfAvailable()
        {
            if (Settings.isDailyRun)
            {
                log("Note For Yourself is disabled due to Daily Run");
                return false;
            }

            if (ascensionLevel >= 15)
            {
                log("Note For Yourself is disabled beyond Ascension 15+");
                return false;
            }

            if (ascensionLevel == 0)
            {
                log("Note For Yourself is enabled due to No Ascension");
                return true;
            }

            if (ascensionLevel < player.getPrefs().getInteger("ASCENSION_LEVEL"))
            {
                log("Note For Yourself is enabled as it's less than Highest Unlocked Ascension");
                return true;
            }

            log("Note For Yourself is disabled as requirements aren't met");
            return false;
        }
    }
}