using System.Collections.Generic;

namespace MoreMountains
{
    public class CustomTrial : ATrial
    {
        bool isKeepingStarterRelic = true;
        List<string> relicIds = new();
        bool isKeepingStarterCards = true;
        List<string> cardIds = new();
        bool _useRandomDailyMods;
        List<string> dailyModIds = new();
        int? maxHpOverride;

        public void setMaxHpOverride(int maxHp)
        {
            maxHpOverride = maxHp;
        }

        public void addStarterCards(List<string> moreCardIds)
        {
            cardIds.AddRange(moreCardIds);
        }

        public void setStarterCards(List<string> starterCards)
        {
            cardIds.Clear();
            cardIds.AddRange(starterCards);
            isKeepingStarterCards = false;
        }

        public void addStarterRelic(string relicId)
        {
            relicIds.Add(relicId);
        }

        public void addStarterRelics(List<string> moreRelics)
        {
            relicIds.AddRange(moreRelics);
        }

        public void setStarterRelics(List<string> starterRelics)
        {
            relicIds.Clear();
            relicIds.AddRange(starterRelics);
            isKeepingStarterRelic = false;
        }

        public void setShouldKeepStarterRelic(bool shouldKeep)
        {
            isKeepingStarterRelic = shouldKeep;
        }

        public void addDailyMod(string modId)
        {
            dailyModIds.Add(modId);
        }

        public void addDailyMods(List<string> moreDailyMods)
        {
            dailyModIds.AddRange(moreDailyMods);
        }

        public void setRandomDailyMods()
        {
            _useRandomDailyMods = true;
        }

        public override void setupPlayer(ref APlayer p)
        {
            if (maxHpOverride != null)
            {
                p.maxHealth = maxHpOverride.Value;
                p.currentHealth = maxHpOverride.Value;
            }
        }

        public override bool keepStarterRelic()
        {
            return isKeepingStarterRelic;
        }

        public override List<string> extraStartingRelicIDs()
        {
            return relicIds;
        }

        public override bool keepsStarterCards()
        {
            return isKeepingStarterCards;
        }

        public override List<string> extraStartingCardIDs()
        {
            return cardIds;
        }

        public override bool useRandomDailyMods()
        {
            return _useRandomDailyMods;
        }

        public override List<string> dailyModIDs()
        {
            return dailyModIds;
        }
    }
}