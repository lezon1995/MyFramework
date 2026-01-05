using System.Collections.Generic;

namespace MarbleHero
{
    public class LoseMaxHpTrial : ATrial
    {
        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Night Terrors",
                "Terminal"
            };
            return retVal;
        }
    }
}