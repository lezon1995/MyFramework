using System;
using UnityEngine;

namespace MarbleHero;

public class Exp : ClassObject
{
    const string XP = "_Xp";
    const string LEVEL = "_Level";
    const string XP_TOTAL = "_XpTotal";
    const string XP_REQUIRED = "_XpRequired";

    Action levelUpAction;
    
    ExpData data;
    bool saveOnQuit;
    bool saveOnDestroy;
    bool loadOnStart;

    public int maxLevel => data.Trait.MaxLevel;

    int level;
    float xp;
    float xpTotal;
    float xpRequired;

    public void setData(ExpData d)
    {
        data = d;

        for (int i = 0; i < d.Trait.MaxLevel; i++)
        {
            var curLevel = i;
            var nextLevel = i + 1;
            // Debug.LogError($"从{curLevel}级升到{nextLevel}级需要 {calculateXpRequiredToNextLevel(curLevel)}经验");
        }
    }

    public void setOnLevelUp(Action action)
    {
        levelUpAction = action;
    }
    
    public void setLevel(int value)
    {
        if (level != value)
        {
            var oldLevel = level;
            level = value;
            new OnLevelChange(oldLevel, value).trigger();
        }
    }

    public void setXpRequired(float value)
    {
        xpRequired = value;
        new OnXpRequiredChange((int)xpRequired).trigger();
    }

    public void setXp(float value)
    {
        xp = value;
        new OnXpChange((int)xp, xpRequired == 0 ? 0 : xp / xpRequired).trigger();
    }

    public void setXpTotal(float value)
    {
        xpTotal = value;
        new OnXpTotalChange((int)xpTotal).trigger();
    }

    void start()
    {
        if (loadOnStart)
        {
            load();
        }
        else
        {
            resetLevel();
        }
    }

    void OnApplicationQuit()
    {
        if (saveOnQuit)
        {
            save();
        }
    }

    void OnDestroy()
    {
        if (saveOnDestroy)
        {
            save();
        }
    }

    void updateAll(int level, float xpRequired, float xp, float xpTotal)
    {
        setLevel(level);
        setXpRequired(xpRequired);
        setXpTotal(xpTotal);
        setXp(xp);
    }

    public void save()
    {
        string key = data.Trait.Key;
        // PlayerPrefs.SetInt(key + LEVEL, Level);
        // PlayerPrefs.SetFloat(key + XP, Xp);
        // PlayerPrefs.SetFloat(key + XP_TOTAL, XpTotal);
        // PlayerPrefs.SetFloat(key + XP_REQUIRED, XpRequired);
        // PlayerPrefs.Save();
    }

    public void load()
    {
        // string key = Data.Trait.Key;
        // if (PlayerPrefs.HasKey(key + LEVEL))
        // {
        //     UpdateAll(
        //         PlayerPrefs.GetInt(key + LEVEL),
        //         PlayerPrefs.GetFloat(key + XP_REQUIRED),
        //         PlayerPrefs.GetFloat(key + XP),
        //         PlayerPrefs.GetFloat(key + XP_TOTAL));
        // }
        // else
        // {
        //     ResetLevel();
        // }
    }


    // [Button]
    public void addXp(float delta)
    {
        var maxLevel = data.Trait.MaxLevel;
        var newXp = xp + delta;
        var newXpTotal = xpTotal + delta;
        var newLevel = level;
        var newXpRequired = xpRequired;

        if (level >= maxLevel)
            return;

        if (newXp >= xpRequired && newLevel < maxLevel)
        {
            new OnAddXp((int)xpRequired, 1F).trigger();
        }
        else
        {
            new OnAddXp((int)newXp, newXpRequired == 0 ? 0 : newXp / newXpRequired).trigger();
        }

        while (newXp >= newXpRequired && newLevel < maxLevel)
        {
            newXp -= newXpRequired;
            newLevel++;
            newXpRequired = calculateXpRequiredToNextLevel(newLevel);
            new OnLevelUp((int)newXp, newLevel, Mathf.Clamp01(newXp / newXpRequired)).trigger();
            levelUpAction?.Invoke();
        }

        if (newLevel >= maxLevel)
        {
            newXpRequired = calculateXpRequiredToNextLevel(newLevel);
            newXp = 0;
            newXpTotal = calculateXpTotalToLevel(maxLevel);
            new OnMaxLevel().trigger();
        }

        updateAll(newLevel, newXpRequired, newXp, newXpTotal);
    }

    float calculateXpRequiredToNextLevel(int curLevel)
    {
        (int maxLevel, float startXpRequired, float maxLevelXpRequired, AnimationCurve xpCurve) = data.Trait;

        float t = (float)curLevel / (maxLevel - 1);
        float curveValue = xpCurve.Evaluate(t);
        float xpRequired = Mathf.Lerp(startXpRequired, maxLevelXpRequired, curveValue);
        return xpRequired;
    }

    float calculateXpTotalToLevel(int level)
    {
        (int maxLevel, float startXpRequired, float maxLevelXpRequired, AnimationCurve xpCurve) = data.Trait;

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

    // [Button]
    public void resetLevel()
    {
        var startXpRequired = data.Trait.StartXpRequired;
        updateAll(0, startXpRequired, 0, 0);
    }
}