using System.Collections.Generic;

namespace MarbleHero
{
    public class MyTrueFormTrial : ATrial
    {
        public override List<string> dailyModIDs()
        {
            List<string> retVal = new()
            {
                "Demon Form",
                "Wraith Form v2",
                "Echo Form",
                "DevaForm"
            };
            return retVal;
        }
    }
}