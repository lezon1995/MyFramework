using System.Collections.Generic;

namespace MoreMountains
{
    public class AnyColorDraftTrial : ATrial
    {
        public override bool keepsStarterCards()
        {
            return false;
        }

        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Diverse",
                "Draft"
            };
            return retVal;
        }
    }
}