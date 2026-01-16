using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero
{
    public class Ironclad : APlayer
    {
        public override PlayerClass chosenClass => PlayerClass.IRONCLAD;
        
        // static CharacterStrings characterStrings = Game.languagePack.getCharacterString("Ironclad");
        // public static string[] NAMES = characterStrings.NAMES;
        // public static string[] TEXT = characterStrings.TEXT;
        Prefs prefs;
        CharStat charStat;

        public override void onCtor()
        {
            base.onCtor();
            
            charStat = new CharStat(this);
            initializeClass(getLoadout());
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

        public override Color getCardTrailColor()
        {
            return new(1.0F, 0.4F, 0.1F, 1.0F);
        }

        public override CharSelectInfo getLoadout()
        {
            return new("", "", 100, 100, 99, 5, this, getStartingRelics(), getStartingDeck(), false);
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

        public override CharStat getCharStat()
        {
            return charStat;
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

        public override bool saveFileExists()
        {
            return SaveAndContinue.saveExistsAndNotCorrupted(chosenClass.ToString());
        }

        public override string getWinStreakKey()
        {
            return "win_streak_ironclad";
        }

        public override string getLeaderboardWinStreakKey()
        {
            return "IRONCLAD_CONSECUTIVE_WINS";
        }

        // public override void renderStatScreen(SpriteBatch sb, float screenX, float renderY)
        // {
        //     StatsScreen.renderHeader(sb, StatsScreen.NAMES[2], screenX, renderY);
        //     charStat.render(sb, screenX, renderY);
        // }

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
            charStat = new CharStat(this);
        }

        public override APlayer newInstance()
        {
            return CLASS<Ironclad>();
        }

        // public override TextureAtlas.AtlasRegion getOrb()
        // {
        //     return ACard.orb_red;
        // }

        // public override void damage(DamageInfo info)
        // {
        //     if (info.owner != null && info.type != DamageInfo.DamageType.THORNS && info.output - currentBlock > 0)
        //     {
        //         AnimationState.TrackEntry e = state.setAnimation(0, "Hit", false);
        //         state.addAnimation(0, "Idle", true, 0.0F);
        //         e.setTimeScale(0.6F);
        //     }
        //
        //     base.damage(info);
        // }

        public override string getSpireHeartText()
        {
            return null;
            // return SpireHeart.DESCRIPTIONS[8];
        }

        public override string getVampireText()
        {
            return null;
            // return Vampires.DESCRIPTIONS[0];
        }
    }
}