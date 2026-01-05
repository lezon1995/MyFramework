using System.Collections.Generic;

namespace MarbleHero
{
    public class StarterDeckTrial : ATrial
    {
        public override List<string> extraStartingRelicIDs()
        {
            List<string> retVal = new();
            retVal.Add("Busted Crown");
            return retVal;
        }

        public override List<string> dailyModIDs()
        {
            List<string> retVal = new();
            retVal.Add("Binary");
            return retVal;
        }
    }
}