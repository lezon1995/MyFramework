using System.Collections.Generic;

namespace MoreMountains
{
    public class SlowpokeTrial : ATrial
    {
        public override List<string> dailyModIDs()
        {
            List<string> retVal = new();
            retVal.Add("Time Dilation");
            return retVal;
        }
    }
}