using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace MarbleHero
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class UIStrings : ILocalizedStringsSerialized
    {
        public string[] TEXT;
        public string[] EXTRA_TEXT;
        public Dictionary<string, string> TEXT_DICT;

        [JsonIgnore]
        public StringPair[] TEXT_LIST;

        public void beforeSerialized()
        {
            if (TEXT_LIST is { Length: > 0 })
                TEXT_DICT = TEXT_LIST.ToDictionary(pair => pair.key, pair => pair.value);
            else
                TEXT_DICT = null;
        }


        static UIStrings mock;
        public static UIStrings getMockUIString()
        {
            if (mock !=null)
                return mock;

            var retVal = new UIStrings();
            retVal.TEXT = new[] { "[MISSING_0]", "[MISSING_1]", "[MISSING_2]" };
            retVal.EXTRA_TEXT = new[] { "[MISSING_0]", "[MISSING_1]", "[MISSING_2]" };
            mock = retVal;
            return retVal;
        }
    }
}