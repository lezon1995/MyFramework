using System.Collections.Generic;

namespace MarbleHero
{
    public class InceptionTrial : ATrial
    {
        public override bool keepStarterRelic()
        {
            return false;
        }

        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Unceasing Top"
            };
            return retVal;
        }
    }
}