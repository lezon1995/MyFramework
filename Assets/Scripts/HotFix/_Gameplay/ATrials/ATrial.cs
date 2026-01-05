using System.Collections.Generic;

namespace MarbleHero
{
    public class ATrial
    {
        public string name;
        public APlayer.PlayerClass c;
        public int energy;
        public CardGroup deck;
        public List<ARelic> relics = new();

        public virtual APlayer setupPlayer(APlayer player)
        {
            return player;
        }

        public virtual bool keepStarterRelic()
        {
            return true;
        }

        public virtual List<string> extraStartingRelicIDs()
        {
            return null;
        }

        public virtual bool keepsStarterCards()
        {
            return true;
        }

        public virtual List<string> extraStartingCardIDs()
        {
            return null;
        }

        public virtual bool useRandomDailyMods()
        {
            return false;
        }

        public virtual List<string> dailyModIDs()
        {
            return null;
        }
    }
}