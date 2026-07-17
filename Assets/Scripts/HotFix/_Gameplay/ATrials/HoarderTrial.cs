using System.Collections.Generic;

namespace MoreMountains
{
    public class HoarderTrial : ATrial
    {
        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Hoarder"
            };
            return retVal;
        }
    }
}