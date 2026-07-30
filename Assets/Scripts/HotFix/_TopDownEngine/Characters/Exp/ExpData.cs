using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "TopDown Engine/Character/Exp/ExpData", fileName = "ExpData")]
    public class ExpData : SerializedScriptableObject
    {
        public Traits Trait;

        [Serializable]
        public class Traits
        {
            public string Key;
            public int MaxLevel = 18;
            public int StartXpRequired = 100;
            public int MaxLevelXpRequired = 300;
            public AnimationCurve XpCurve;

            public void Deconstruct(out string key, out int maxLevel, out int startXpRequired, out int maxLevelXpRequired, out AnimationCurve xpCurve)
            {
                key = Key;
                maxLevel = MaxLevel;
                startXpRequired = StartXpRequired;
                maxLevelXpRequired = MaxLevelXpRequired;
                xpCurve = XpCurve;
            }

            public void Deconstruct(out int maxLevel, out int startXpRequired, out int maxLevelXpRequired, out AnimationCurve xpCurve)
            {
                maxLevel = MaxLevel;
                startXpRequired = StartXpRequired;
                maxLevelXpRequired = MaxLevelXpRequired;
                xpCurve = XpCurve;
            }
        }
    }
}