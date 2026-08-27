using System;
using UnityEngine;

namespace MoreMountains
{
    public enum ItemRarity
    {
        Tier1 = 0, //基础
        Tier2 = 1, //普通
        Tier3 = 2, //罕见
        Tier4 = 3, //稀有
    }

    [Serializable]
    public struct TagInfo
    {
        public MechanicTag[] MechanicTags;
        public StatusTag[] StatusTags;
    }

    public enum MechanicTag
    {
        None = 0,
        Duration, //持续：增加持续时间
        Range, //范围：增加效果范围
        Chance, //概率：增加触发概率
        Penetrate, //穿透：增加穿透次数
        MultiHit, //多段攻击：增加多端攻击次数
        Period, //周期：减少周期时间
        Summon, //召唤：增加召唤物个数
        Stack, //叠加：增加最大可叠加层数
        StackTrigger, //叠加触发：减少最大可叠加层数
        TrueDmg, //真伤：增加真实伤害倍率
    }

    public enum StatusTag
    {
        None = 0,
        Slowed, //减速
        Burning, //灼烧：层数越多，触发间隔越短
        Poisoned, //中毒：每秒受到中毒层数的伤害
        Bleeding, //流血：每次受到伤害触发流血伤害
        Electrified, //感电：每次受到伤害触发感电伤害
        Stunned, //眩晕=定身+无法攻击+无法释放技能
        Frozen, //冰冻=定身
        Charmed, //魅惑：敌人会追击最近的敌人
    }

    [CreateAssetMenu(fileName = "GameDesign", menuName = "MoreMountains/GameDesign", order = 0)]
    public class GameDesign : ScriptableObject
    {
        public float PlayerGreedIncreasementPerWave = 1.05F;
        public float globalExpCoefficient = 1F; //全局经验倍率
        public float globalDurationCoefficient = 1F; //全局时长倍率
        public float globalDurationMinutes = 30F; //单局目标时长（分钟）
        public int maxLevel = 18; //最大等级
        public int baseExpStandard = 10; //单砖块基础经验
        public int oneTurnKillBrickAvg = 5; //单回合摧毁砖块数量均值

        #region 模型默认前提

        public float oneGameTurnDuration = 45F; //单个回合时长（秒）
        public float turnsPerMinute => 60F / oneGameTurnDuration; //每分钟回合数
        public float reachMaxLevelAtProgress = 0.7F; //完成最大等级所占用的时间比例
        public float comboKillExpCoefficient = 0.25F; //连续摧毁经验占比
        public int comboKillCap = 10; //连续摧毁砖块封顶

        #endregion

        #region 一局有多少回合

        public float maxDuration => globalDurationMinutes * globalDurationCoefficient; //总时长（分钟）
        public int maxTurns => Mathf.CeilToInt(maxDuration * turnsPerMinute); //总回合数

        #endregion

        public int baseExpPerTurn => baseExpStandard * oneTurnKillBrickAvg; //单回合基础经验
        public float comboExpPerTurn => baseExpPerTurn * comboKillExpCoefficient; //单回合连击额外经验（期望）
        public float expPerTurn => baseExpPerTurn * (1 + comboKillExpCoefficient); //单回合总经验期望

        public int turnsToMaxLevel => Mathf.CeilToInt(maxTurns * reachMaxLevelAtProgress); //到达满级的目标回合数

        //满级所需总经验
        public int totalExpToMaxLevel => Mathf.CeilToInt(expPerTurn * turnsToMaxLevel * globalExpCoefficient);

        #region 等级经验曲线

        public int exp1_2 => Mathf.CeilToInt(totalExpToMaxLevel / ((maxLevel - 1) * (1 + (maxLevel - 2) / 4F)));
        public int expDelta => Mathf.CeilToInt(exp1_2 / 4F);

        public int getMaxExpAtLevel(int n)
        {
            return Mathf.RoundToInt(exp1_2 + (n - 1) * expDelta);
        }

        #endregion


        //连续摧毁奖励
        public int getExtraExpAtCombo(int combo)
        {
            return (Mathf.Clamp(combo, 1, comboKillCap) - 1) * Mathf.RoundToInt(baseExpStandard * comboKillExpCoefficient);
        }

        public static void initialize()
        {
            var path = $"{GAMEPLAY_PATH}/GameDesign.asset";
            gameDesign = resource.loadGameResource<GameDesign>(path).get();
        }

        public UniversalColor universalColor;
        public RarityColor[] rarityColor;

        public TagColor tagColor;
        
        [Serializable]
        public class RarityColor
        {
            public ItemRarity rarity;
            public Color bg;
            public Color border;
            public Color title;
            public Color iconBg;
        }

        [Serializable]
        public class UniversalColor
        {
            public Color enhanced;
            public Color reduced;
            public Color cursed;
            public Color unafforded;
            public Color statEntry;
            public Color statValueRaw = Color.gray2;
        }

        public RarityColor getRarityColor(ItemRarity rarity)
        {
            foreach (var c in rarityColor)
                if (c.rarity == rarity)
                    return c;

            return null;
        }

        public UniversalColor getUniversalColor()
        {
            return universalColor;
        }
        
        [Serializable]
        public class TagColor
        {
            public Color mechanic = Color.blueViolet;
            public Color status = Color.brown;
            public Color amplifier = Color.cornflowerBlue;
        }
    }
}