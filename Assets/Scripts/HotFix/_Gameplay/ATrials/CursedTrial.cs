using System.Collections.Generic;

namespace MarbleHero
{
    public class CursedTrial : ATrial
    {
        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Cursed Run"
            };
            return retVal;
        }
    }
}