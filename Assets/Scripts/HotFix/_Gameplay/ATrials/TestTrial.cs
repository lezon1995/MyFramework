using System.Collections.Generic;

namespace MoreMountains
{
    public class TestTrial : ATrial
    {
        public override void setupPlayer(ref APlayer p)
        {
            p.maxHealth = 20;
            p.currentHealth = 10;
            p.gold = 777;
        }

        public override bool keepStarterRelic()
        {
            return false;
        }

        public override List<string> extraStartingRelicIDs()
        {
            List<string> retVal = new()
            {
                "Derp Rock",
                "Unceasing Top"
            };
            return retVal;
        }

        public override bool keepsStarterCards()
        {
            return true;
        }

        public override List<string> extraStartingCardIDs()
        {
            List<string> retVal = new()
            {
                "Demon Form",
                "Wraith Form v2",
                "Echo Form"
            };
            return retVal;
        }

        public override bool useRandomDailyMods()
        {
            return false;
        }

        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Diverse",
                "Lethality",
                "Time Dilation",
                "Cursed Run",
                "Elite Swarm"
            };
            return retVal;
        }
    }
}