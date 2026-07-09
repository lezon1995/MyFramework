using System;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMText : Text
    {
        public static Action OnLanguageChanged;
        
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}