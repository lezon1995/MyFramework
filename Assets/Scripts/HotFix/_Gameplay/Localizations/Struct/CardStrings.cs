using System;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class CardStrings
    {
        public string NAME;
        public string DESCRIPTION;
        public string UPGRADE_DESCRIPTION;
        public string[] EXTENDED_DESCRIPTION;

        public static CardStrings getMockCardString()
        {
            var retVal = new CardStrings();
            retVal.NAME = "[MISSING_TITLE]";
            retVal.DESCRIPTION = "[MISSING_DESCRIPTION]";
            retVal.UPGRADE_DESCRIPTION = "[MISSING_DESCRIPTION+]";
            retVal.EXTENDED_DESCRIPTION = new[] { "[MISSING_0]", "[MISSING_1]", "[MISSING_2]" };
            return retVal;
        }
    }
}