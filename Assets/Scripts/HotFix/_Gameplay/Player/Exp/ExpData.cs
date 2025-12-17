using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ExpData", menuName = "MarbleHero/ExpData")]
public class ExpData : ScriptableObject
{
    public Traits Trait;

    [Serializable]
    public class Traits
    {
        public string Key;
        public int MaxLevel = 18;
        public float StartXpRequired = 100;
        public float MaxLevelXpRequired = 300;
        public AnimationCurve XpCurve;

        public void Deconstruct(out string key, out int maxLevel, out float startXpRequired, out float maxLevelXpRequired, out AnimationCurve xpCurve)
        {
            key = Key;
            maxLevel = MaxLevel;
            startXpRequired = StartXpRequired;
            maxLevelXpRequired = MaxLevelXpRequired;
            xpCurve = XpCurve;
        }

        public void Deconstruct(out int maxLevel, out float startXpRequired, out float maxLevelXpRequired, out AnimationCurve xpCurve)
        {
            maxLevel = MaxLevel;
            startXpRequired = StartXpRequired;
            maxLevelXpRequired = MaxLevelXpRequired;
            xpCurve = XpCurve;
        }
    }
}