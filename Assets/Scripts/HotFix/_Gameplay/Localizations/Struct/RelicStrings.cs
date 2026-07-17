using System;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class RelicStrings
    {
        public string NAME;
        public string FLAVOR;
        public string[] DESCRIPTIONS;
        
        public static RelicStrings getMockRelicString()
        {
            var retVal = new RelicStrings();
            retVal.NAME = "[MISSING_TITLE]";
            retVal.FLAVOR = "[MISSING_DESCRIPTION]";
            retVal.DESCRIPTIONS = new[] { "[MISSING_0]", "[MISSING_1]", "[MISSING_2]" };
            return retVal;
        }
    }
}