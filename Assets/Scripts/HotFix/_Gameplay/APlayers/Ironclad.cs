using System.Collections.Generic;

namespace MoreMountains
{
    public class Ironclad : APlayer
    {
        public override PlayerClass chosenClass => PlayerClass.IRONCLAD;
        
        // static CharacterStrings characterStrings = Game.languagePack.getCharacterString("Ironclad");
        // public static string[] NAMES = characterStrings.NAMES;
        // public static string[] TEXT = characterStrings.TEXT;
        Prefs prefs;
        CharStat charStat;

        protected override void OnAwake()
        {
            base.OnAwake();
            charStat = new CharStat(this);
        }

        public override List<string> getStartingRelics()
        {
            List<string> retVal = new();
            // retVal.add(FreeBall.ID);
            // retVal.add(Origami.ID);
            // retVal.add(BurlapBag.ID);
            // retVal.add(BrokenTripod.ID);
            // retVal.add(AmmoSupply.ID);
            // retVal.add(SideBorderPortal.ID);
            // retVal.add(LakeMirror.ID);
            // retVal.add(RhombicDarts.ID);
            // retVal.add(MilkShake.ID);
            // retVal.add(Rattle.ID);
            // retVal.add(ImpactHammer.ID);
            // retVal.add(RoughCelling.ID);
            // retVal.add(RoughWall.ID);
            
            // retVal.add(RoundBattery.ID);
            // retVal.add(UnstableBattery.ID);
            // retVal.add(ExtremelyUnstableBattery.ID);
            // retVal.add(BaseMagazine.ID);

            foreach (var relicId in retVal)
                UnlockTracker.markRelicAsSeen(relicId);
            return retVal;
        }

        public override string getPortraitImageName()
        {
            return "ironcladPortrait.jpg";
        }

        public override ACard getStartCardForEvent()
        {
            return null;
            // return new Bash();
        }


        public override string getTitle(PlayerClass plyrClass)
        {
            return null;
            // return uiStrings.TEXT[1];
        }

        public CardColor getCardColor()
        {
            return CardColor.Red;
        }

        public override string getAchievementKey()
        {
            return "RUBY";
        }

        public override List<ACard> getCardPool(List<ACard> tmpPool)
        {
            CardLibrary.addRedCards(tmpPool);
            if (ModHelper.isModEnabled("Green Cards"))
                CardLibrary.addGreenCards(tmpPool);

            if (ModHelper.isModEnabled("Blue Cards"))
                CardLibrary.addBlueCards(tmpPool);

            if (ModHelper.isModEnabled("Purple Cards"))
                CardLibrary.addPurpleCards(tmpPool);

            return tmpPool;
        }

        public override string getLeaderboardCharacterName()
        {
            return "IRONCLAD";
        }

        public override int getAscensionMaxHPLoss()
        {
            return 5;
        }

        // public override BitmapFont getEnergyNumFont()
        // {
        //     return FontHelper.energyNumFontRed;
        // }

        public override Prefs getPrefs()
        {
            if (prefs == null)
                logError("prefs need to be initialized first!");
            return prefs;
        }

        public override void loadPrefs()
        {
            prefs = SaveHelper.getPrefs("DataVagabond");
        }

        public override int getUnlockedCardCount()
        {
            return UnlockTracker.unlockedRedCardCount;
        }

        public override int getSeenCardCount()
        {
            return CardLibrary.seenRedCards;
        }

        public override int getCardCount()
        {
            return CardLibrary.redCards;
        }

        public override string getWinStreakKey()
        {
            return "win_streak_ironclad";
        }

        public override string getLeaderboardWinStreakKey()
        {
            return "IRONCLAD_CONSECUTIVE_WINS";
        }

        public override void doCharSelectScreenSelectEffect()
        {
            // Game.sound.playA("ATTACK_HEAVY", MathUtils.random(-0.2F, 0.2F));
            // Game.screenShake.shake(ScreenShake.ShakeIntensity.MED, ScreenShake.ShakeDur.SHORT, true);
        }

        public override string getCustomModeCharacterButtonSoundKey()
        {
            return "ATTACK_HEAVY";
        }

        // public override CharacterStrings getCharacterString()
        // {
        //     return Game.languagePack.getCharacterString("Ironclad");
        // }

        public override string getLocalizedCharacterName()
        {
            return null;
            // return NAMES[0];
        }

        public override void refreshCharStat()
        {
            charStat = new(this);
        }
    }
}