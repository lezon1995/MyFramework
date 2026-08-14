using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 根据当前波数和 Luck 属性，按 Brotato Wiki 的规则
    /// 计算并抽取出本次商店物品的稀有度（Tier1 ~ Tier4）。
    ///
    /// 公式（来自 Wiki - Rarity of Shop Items and Luck）：
    ///   Raw(T) = BaseChance(T) + ChancePerWave(T) * (CurrentWave - MinWave(T) + 1)   （Wave >= MinWave）
    ///          = 0                                                                （Wave <  MinWave）
    ///
    /// Wiki 文字："Chance per wave starts adding chance at the Min Wave."
    /// 即在 MinWave 当波就已经加了 1 个 CPW。
    ///
    /// 抽样顺序自 Tier4 起，到 Tier1 止；每一档的最终 (个体) 概率等于
    ///   Actual(T) = max(0, RawWithLuck(T) - Σ RawWithLuck(更高稀有度))
    /// 并被 (MaxChance(T) - Σ Actual(更高稀有度)) 进一步裁剪，避免越过 Max 帽。
    ///   Tier1 = 1 - Σ Actual(高稀有度)。
    ///
    /// 直观解释：每个 Tier 按 Raw 争抢整体概率空间；高稀有度先分到的部分
    /// 会从低稀有度的 Raw 中"扣"出来再裁剪。
    /// </summary>
    public sealed class RarityRollService
    {
        // Wiki 原始数据 —— 顺序按高 → 低，便于"先抽 T4 再 T3 再 T2" 的累减。
        // Tier1 放在末尾用占位 BaseChance=1 表示"剩余的都归 Tier1"。
        static RarityTierConfig[] DefaultTiers =
        {
            new(ItemRarity.Tier4, minWave: 8, baseChance: 0F, chancePerWave: 0.0023F, maxChance: 0.08F),
            new(ItemRarity.Tier3, minWave: 4, baseChance: 0F, chancePerWave: 0.0200F, maxChance: 0.25F),
            new(ItemRarity.Tier2, minWave: 2, baseChance: 0F, chancePerWave: 0.0600F, maxChance: 0.60F),
            new(ItemRarity.Tier1, minWave: 1, baseChance: 1F, chancePerWave: 0.0000F, maxChance: 1.00F),
        };

        RarityTierConfig[] _tiers;
        Random _rng;

        public RarityRollService(int seed = 0, RarityTierConfig[] tiers = null)
        {
            _tiers = tiers ?? DefaultTiers;
            _rng = new Random(seed == 0 ? Environment.TickCount : seed);
        }

        /// <summary>使用项目默认 RNG（无种子）重置。供 RewardController 等不会自己持有 RNG 的场景复用。</summary>
        public RarityRollService() : this(seed: 0, tiers: null)
        {
        }

        /// <summary>计算 (CurrentWave, Luck) 下各稀有度的最终真实概率（合计 = 1）。</summary>
        public void ComputeChances(int currentWave, float luck, ref Dictionary<ItemRarity, float> result)
        {
            result.Clear();
            float sumHigherRaw = 0F; // Σ RawWithLuck(更高稀有度) — 用于个体分摊
            float sumHigherActual = 0F; // Σ Actual(更高稀有度) — 用于 Max 裁剪

            // 自 Tier4 开始，逐档分配。
            for (int i = 0; i < _tiers.Length; i++)
            {
                var cfg = _tiers[i];
                if (cfg.Rarity == ItemRarity.Tier1)
                    continue;

                float rawWithLuck = ComputeRawChance(currentWave, luck, cfg);
                float rawMinusHigher = Mathf_clamp(rawWithLuck - sumHigherRaw, 0F, cfg.MaxChance);
                float budget = Mathf_clamp(cfg.MaxChance - sumHigherActual, 0F, cfg.MaxChance);
                float actual = Mathf_min(rawMinusHigher, budget);

                result[cfg.Rarity] = actual;
                sumHigherRaw += rawWithLuck;
                sumHigherActual += actual;
            }

            // Tier1 拿剩余。注意 Wiki 表格里 Tier1 在中后期恒为 40%（被 T2~T4 推下去的"地板"）。
            result[ItemRarity.Tier1] = Mathf_clamp(1F - sumHigherActual, 0F, 1F);
        }

        /// <summary>返回本次刷新抽取到的稀有度。</summary>
        public ItemRarity RollItem(int currentWave, float luck)
        {
            using var _ = new DicScope<ItemRarity, float>(out var chances);
            ComputeChances(currentWave, luck, ref chances);
            float roll = (float)_rng.NextDouble();
            float cumulative = 0F;

            // 注意：必须按 Tier4 → Tier3 → Tier2 → Tier1 顺序累加，与 Wiki 抽样顺序一致。
            Span<ItemRarity> rarities = stackalloc ItemRarity[4];
            rarities[0] = ItemRarity.Tier4;
            rarities[1] = ItemRarity.Tier3;
            rarities[2] = ItemRarity.Tier2;
            rarities[3] = ItemRarity.Tier1;
            foreach (var rarity in rarities)
            {
                cumulative += chances[rarity];
                if (roll < cumulative)
                    return rarity;
            }

            return ItemRarity.Tier1;
        }

        public ItemRarity RollReward(int currentWave, float luck)
        {
            switch (currentWave)
            {
                case 1:
                    return ItemRarity.Tier1;
                case 5:
                    return ItemRarity.Tier2;
                case 10:
                case 15:
                case 20:
                    return ItemRarity.Tier3;
                default:
                    if (currentWave >= 25 && currentWave % 5 == 0)
                    {
                        return ItemRarity.Tier4;
                    }

                    return RollItem(currentWave, luck);
            }
        }

        /// <summary>把 (CurrentWave, Luck) 拆成 4 个阈值，外部自己用 uniform [0,1) 抽样即可。
        /// 阈值顺序：Tier4, Tier3, Tier2, Tier1。剩余值均为 1，便于直接做 >= / &lt; 判断。</summary>
        public void FillCumulativeThresholds(int currentWave, float luck, float[] thresholds)
        {
            if (thresholds == null || thresholds.Length < 4)
                throw new ArgumentException("thresholds 长度必须 >= 4", nameof(thresholds));

            using var _ = new DicScope<ItemRarity, float>(out var chances);
            ComputeChances(currentWave, luck, ref chances);
            float cumulative = 0F;
            thresholds[0] = cumulative + chances[ItemRarity.Tier4];
            cumulative = thresholds[0];
            thresholds[1] = cumulative + chances[ItemRarity.Tier3];
            cumulative = thresholds[1];
            thresholds[2] = cumulative + chances[ItemRarity.Tier2];
            cumulative = thresholds[2];
            thresholds[3] = 1F;
        }

        /// <summary>只计算单个 Tier 的 raw 含 Luck 概率（不做累减）。用于 UI 展示 / 调试。</summary>
        static float ComputeRawChance(int currentWave, float luck, RarityTierConfig cfg)
        {
            if (currentWave < cfg.MinWave)
                return 0F;

            float steps = currentWave - cfg.MinWave + 1; // Wiki 文字："chance per wave starts adding at the Min Wave"
            float raw = cfg.BaseChance + cfg.ChancePerWave * steps;
            return raw * (1F + luck);
        }

        // 避免直接依赖 UnityEngine.Mathf 的工具 —— 用普通 Math.Clamp 兼容 IL2CPP/编辑器。
        static float Mathf_clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        static float Mathf_min(float a, float b) => a < b ? a : b;
    }
}