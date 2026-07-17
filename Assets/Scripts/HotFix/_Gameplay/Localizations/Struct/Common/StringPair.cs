using System;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    [Serializable]
    public class StringPair
    {
        [HorizontalGroup, HideLabel]
        public string key;

        [HorizontalGroup, HideLabel]
        public string value;
    }
}