using System.Collections.Generic;

namespace MarbleHero
{
    public class DraftTrial : ATrial
    {
        public override bool keepsStarterCards()
        {
            return false;
        }

        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Draft"
            };
            return retVal;
        }
    }
}