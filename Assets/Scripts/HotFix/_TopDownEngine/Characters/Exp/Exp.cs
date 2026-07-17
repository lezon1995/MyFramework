using System;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    public class Exp : MonoBehaviour
    {
        const string XP = "_Xp";
        const string LEVEL = "_Level";
        const string XP_TOTAL = "_XpTotal";
        const string XP_REQUIRED = "_XpRequired";

        Action LevelUpAction;
        
        public ExpData Data;
        public bool saveOnQuit;
        public bool saveOnDestroy;
        public bool loadOnStart;

        public int LevelMax => Data.Trait.MaxLevel;
        public int maxLevel => Data.Trait.MaxLevel;
        public float progress => Xp / XpRequired;
        public int currentExp => (int)Xp;
        public int currentLevelRequiredExp => (int)XpRequired;

        public int Level;
        public float Xp;
        public float XpTotal;
        public float XpRequired;

        public void SetData(ExpData d)
        {
            Data = d;

            for (int i = 0; i < d.Trait.MaxLevel; i++)
            {
                var curLevel = i;
                var nextLevel = i + 1;
                // Debug.LogError($"从{curLevel}级升到{nextLevel}级需要 {calculateXpRequiredToNextLevel(curLevel)}经验");
            }
        }

        
        public void SetOnLevelUp(Action action)
        {
            LevelUpAction = action;
        }
        
        public void SetLevel(int value)
        {
            if (Level != value)
            {
                var oldLevel = Level;
                Level = value;
                new OnLevelChange(oldLevel, value).trigger();
            }
        }

        public void SetXpRequired(float value)
        {
            XpRequired = value;
            new OnXpRequiredChange(XpRequired).trigger();
        }

        public void SetXp(float value)
        {
            Xp = value;
            new OnXpChange(Xp, XpTotal == 0 ? 0 : Xp / XpTotal).trigger();
        }

        public void SetXpTotal(float value)
        {
            XpTotal = value;
            new OnXpTotalChange(XpTotal).trigger();
        }

        void Start()
        {
            if (loadOnStart)
            {
                Load();
            }
            else
            {
                ResetLevel();
            }
        }

        void OnApplicationQuit()
        {
            if (saveOnQuit)
            {
                Save();
            }
        }

        void OnDestroy()
        {
            if (saveOnDestroy)
            {
                Save();
            }
        }

        void UpdateAll(int level, float xpRequired, float xp, float xpTotal)
        {
            SetLevel(level);
            SetXpRequired(xpRequired);
            SetXp(xp);
            SetXpTotal(xpTotal);
        }

        public void Save()
        {
            string key = Data.Trait.Key;
            PlayerPrefs.SetInt(key + LEVEL, Level);
            PlayerPrefs.SetFloat(key + XP, Xp);
            PlayerPrefs.SetFloat(key + XP_TOTAL, XpTotal);
            PlayerPrefs.SetFloat(key + XP_REQUIRED, XpRequired);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            string key = Data.Trait.Key;
            if (PlayerPrefs.HasKey(key + LEVEL))
            {
                UpdateAll(
                    PlayerPrefs.GetInt(key + LEVEL),
                    PlayerPrefs.GetFloat(key + XP_REQUIRED),
                    PlayerPrefs.GetFloat(key + XP),
                    PlayerPrefs.GetFloat(key + XP_TOTAL));
            }
            else
            {
                ResetLevel();
            }
        }


        [Button]
        public void AddXp(float delta)
        {
            var maxLevel = Data.Trait.MaxLevel;
            var newXp = Xp + delta;
            var newXpTotal = XpTotal + delta;
            var newLevel = Level;
            var newXpRequired = XpRequired;

            if (Level >= maxLevel)
                return;

            if (newXp >= XpRequired && newLevel < maxLevel)
            {
                new OnAddXp((int)XpRequired, 1F).trigger();
            }
            else
            {
                new OnAddXp((int)newXp, newXpRequired == 0 ? 0 : newXp / newXpRequired).trigger();
            }

            while (newXp >= newXpRequired && newLevel < maxLevel)
            {
                newXp -= newXpRequired;
                newLevel++;
                newXpRequired = CalculateXpRequiredToNextLevel(newLevel);
                new OnLevelUp((int)newXp, newLevel, Mathf.Clamp01(newXp / newXpRequired)).trigger();
                LevelUpAction?.Invoke();
            }

            if (newLevel >= maxLevel)
            {
                newXpRequired = CalculateXpRequiredToNextLevel(newLevel);
                newXp = 0;
                newXpTotal = CalculateXpTotalToLevel(maxLevel);
                new OnMaxLevel().trigger();
            }

            UpdateAll(newLevel, newXpRequired, newXp, newXpTotal);
        }

        float CalculateXpRequiredToNextLevel(int level)
        {
            (int maxLevel, float startXpRequired, float maxLevelXpRequired, AnimationCurve xpCurve) = Data.Trait;

            float t = (float)level / (maxLevel - 1);
            float curveValue = xpCurve.Evaluate(t);
            float xpRequired = Mathf.Lerp(startXpRequired, maxLevelXpRequired, curveValue);
            return xpRequired;
        }

        float CalculateXpTotalToLevel(int level)
        {
            (int maxLevel, float startXpRequired, float maxLevelXpRequired, AnimationCurve xpCurve) = Data.Trait;

            float totalXP = 0;

            for (int i = 1; i <= level; i++)
            {
                float t = (float)i / (maxLevel - 1);
                float curveValue = xpCurve.Evaluate(t);
                float xpRequiredForLevel = Mathf.Lerp(startXpRequired, maxLevelXpRequired, curveValue);
                totalXP += xpRequiredForLevel;
            }

            return totalXP;
        }

        [Button]
        public void ResetLevel()
        {
            var startXpRequired = Data.Trait.StartXpRequired;
            UpdateAll(1, startXpRequired, 0, 0);
        }
    }
}