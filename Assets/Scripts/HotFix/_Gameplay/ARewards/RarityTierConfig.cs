using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 商店物品稀有度档位定义（参考 Brotato Wiki - Rarity of Shop Items and Luck）。
    /// 数据按 Wiki 表格硬编码：MinWave / BaseChance / ChancePerWave / MaxChance 四列。
    /// </summary>
    [Serializable]
    public struct RarityTierConfig
    {
        public ItemRarity Rarity;

        [Tooltip("该稀有度首次出现的最小波次（小于此波次时概率为 0）。")]
        public int MinWave;

        [Tooltip("起始基准概率（0~1）。Tier2/3/4 默认 0；Tier1 默认 1。")]
        [Range(0F, 1F)]
        public float BaseChance;

        [Tooltip("从 MinWave 起每多 1 波增加的原始概率（0~1）。")]
        [Range(0F, 1F)]
        public float ChancePerWave;

        [Tooltip("该稀有度概率上限（0~1），用于最终截断。")]
        [Range(0F, 1F)]
        public float MaxChance;

        public RarityTierConfig(ItemRarity rarity, int minWave, float baseChance, float chancePerWave, float maxChance)
        {
            Rarity = rarity;
            MinWave = minWave;
            BaseChance = baseChance;
            ChancePerWave = chancePerWave;
            MaxChance = maxChance;
        }
    }
}
