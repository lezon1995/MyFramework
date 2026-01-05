using System.Collections.Generic;

namespace MarbleHero
{
    public class SneckoTrial : ATrial
    {
        public override bool keepStarterRelic()
        {
            return false;
        }

        public override List<string> dailyModIDs()
        {
            List<string> retVal = new();
            retVal.Add("Snecko Eye");
            return retVal;
        }
    }
}