using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 奖励类型
    /// </summary>
    public enum RewardType
    {
        StatBoost,      // 属性提升
        NewWeapon,      // 新武器
        NewSkill,       // 新技能
        Heal,           // 治疗
        Shield,         // 护盾
        Gold,           // 金币
        Exp,            // 经验
        Item,           // 道具
    }

    /// <summary>
    /// 单个奖励项
    /// </summary>
    [Serializable]
    public class WaveReward
    {
        public RewardType type;
        public string rewardId;
        public string rewardName;
        public string description;
        public float value;
        public Sprite icon;
    }

    /// <summary>
    /// 波次间奖励管理器
    /// </summary>
    public class WaveRewardManager : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// 是否有奖励选择正在进行
        /// </summary>
        public bool IsSelectingReward { get; set; }

        /// <summary>
        /// 当前可选择的奖励列表
        /// </summary>
        public List<WaveReward> CurrentRewards { get; } = new();

        /// <summary>
        /// 已选择的奖励数量
        /// </summary>
        public int SelectedRewardCount { get; set; }
        #endregion

        #region Events
        public event Action<List<WaveReward>> OnRewardsGenerated;
        public event Action<WaveReward> OnRewardSelected;
        public event Action OnRewardSelectionComplete;
        #endregion

        void Awake()
        {
        }

        /// <summary>
        /// 生成波次奖励选项
        /// </summary>
        /// <param name="waveNumber">当前波次编号</param>
        /// <param name="optionCount">生成的选项数量</param>
        public void GenerateRewards(int waveNumber, int optionCount = 3)
        {
            CurrentRewards.Clear();
            SelectedRewardCount = 0;

            // 根据波次生成不同类型的奖励
            List<WaveReward> allRewards = new();

            // 1. 属性提升奖励（所有波次都有）
            GenerateStatRewards(allRewards, waveNumber);

            // 2. 特殊奖励（根据波次增加）
            if (waveNumber % 3 == 0)
            {
                GenerateHealRewards(allRewards);
            }

            if (waveNumber % 5 == 0)
            {
                GenerateGoldRewards(allRewards, waveNumber);
            }

            // 随机选择
            ShuffleRewards(allRewards);

            for (int i = 0; i < Mathf.Min(optionCount, allRewards.Count); i++)
            {
                CurrentRewards.Add(allRewards[i]);
            }

            IsSelectingReward = true;
            OnRewardsGenerated?.Invoke(CurrentRewards);
        }

        /// <summary>
        /// 选择一个奖励
        /// </summary>
        public void SelectReward(int index)
        {
            if (index < 0 || index >= CurrentRewards.Count)
            {
                Debug.LogWarning($"[WaveRewardManager] Invalid reward index: {index}");
                return;
            }

            var selectedReward = CurrentRewards[index];
            ApplyReward(selectedReward);
            SelectedRewardCount++;

            OnRewardSelected?.Invoke(selectedReward);

            Debug.Log($"[WaveRewardManager] Selected reward: {selectedReward.rewardName} ({selectedReward.type})");

            // 如果选择了所有奖励，结束选择
            if (SelectedRewardCount >= CurrentRewards.Count)
            {
                EndRewardSelection();
            }
        }

        /// <summary>
        /// 跳过奖励选择
        /// </summary>
        public void SkipRewardSelection()
        {
            Debug.Log("[WaveRewardManager] Reward selection skipped.");
            EndRewardSelection();
        }

        /// <summary>
        /// 结束奖励选择
        /// </summary>
        public void EndRewardSelection()
        {
            IsSelectingReward = false;
            CurrentRewards.Clear();
            OnRewardSelectionComplete?.Invoke();
        }

        void GenerateStatRewards(List<WaveReward> rewards, int waveNumber)
        {
            // 根据波次调整属性提升的数值
            float multiplier = 1f + (waveNumber - 1) * 0.1f;

            rewards.Add(new WaveReward
            {
                type = RewardType.StatBoost,
                rewardId = "health_boost",
                rewardName = "生命提升",
                description = $"最大生命值 +{Mathf.RoundToInt(20 * multiplier)}",
                value = 20 * multiplier
            });

            rewards.Add(new WaveReward
            {
                type = RewardType.StatBoost,
                rewardId = "damage_boost",
                rewardName = "伤害提升",
                description = $"攻击力 +{Mathf.RoundToInt(5 * multiplier)}",
                value = 5 * multiplier
            });

            rewards.Add(new WaveReward
            {
                type = RewardType.StatBoost,
                rewardId = "speed_boost",
                rewardName = "移速提升",
                description = $"移动速度 +{Mathf.RoundToInt(10 * multiplier)}%",
                value = 10 * multiplier
            });

            rewards.Add(new WaveReward
            {
                type = RewardType.StatBoost,
                rewardId = "attack_speed_boost",
                rewardName = "攻速提升",
                description = $"攻击速度 +{Mathf.RoundToInt(8 * multiplier)}%",
                value = 8 * multiplier
            });

            rewards.Add(new WaveReward
            {
                type = RewardType.StatBoost,
                rewardId = "defense_boost",
                rewardName = "防御提升",
                description = $"防御力 +{Mathf.RoundToInt(3 * multiplier)}",
                value = 3 * multiplier
            });

            rewards.Add(new WaveReward
            {
                type = RewardType.StatBoost,
                rewardId = "crit_boost",
                rewardName = "暴击提升",
                description = $"暴击率 +{Mathf.RoundToInt(3 * multiplier)}%",
                value = 3 * multiplier
            });
        }

        void GenerateHealRewards(List<WaveReward> rewards)
        {
            rewards.Add(new WaveReward
            {
                type = RewardType.Heal,
                rewardId = "heal_25",
                rewardName = "生命恢复",
                description = "恢复25%最大生命值",
                value = 25
            });

            rewards.Add(new WaveReward
            {
                type = RewardType.Heal,
                rewardId = "heal_full",
                rewardName = "完全治疗",
                description = "恢复至满血",
                value = 100
            });
        }

        void GenerateGoldRewards(List<WaveReward> rewards, int waveNumber)
        {
            int goldAmount = 50 + waveNumber * 10;
            rewards.Add(new WaveReward
            {
                type = RewardType.Gold,
                rewardId = "gold_bonus",
                rewardName = "金币奖励",
                description = $"获得 {goldAmount} 金币",
                value = goldAmount
            });
        }

        void ShuffleRewards(List<WaveReward> rewards)
        {
            System.Random random = new System.Random();
            int n = rewards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (rewards[k], rewards[n]) = (rewards[n], rewards[k]);
            }
        }

        void ApplyReward(WaveReward reward)
        {
            if (player == null)
            {
                Debug.LogWarning("[WaveRewardManager] Player is null!");
                return;
            }

            switch (reward.type)
            {
                case RewardType.StatBoost:
                    ApplyStatBoost(reward);
                    break;
                case RewardType.Heal:
                    ApplyHeal(reward);
                    break;
                case RewardType.Gold:
                    ApplyGold(reward);
                    break;
                case RewardType.Shield:
                    ApplyShield(reward);
                    break;
                // 可以继续添加其他类型
            }
        }

        void ApplyStatBoost(WaveReward reward)
        {
            if (player == null || player.Stats == null) 
                return;

            switch (reward.rewardId)
            {
                case "health_boost":
                    player.Stats.AddStatBonus(Character.Stat.HealthMax, reward.value);
                    break;
                case "damage_boost":
                    player.Stats.AddStatBonus(Character.Stat.AD, reward.value);
                    break;
                case "speed_boost":
                    player.Stats.AddStatBonus(Character.Stat.MS, reward.value);
                    break;
                case "attack_speed_boost":
                    player.Stats.AddStatBonus(Character.Stat.AS, reward.value);
                    break;
                case "defense_boost":
                    player.Stats.AddStatBonus(Character.Stat.AR, reward.value);
                    break;
                case "crit_boost":
                    player.Stats.AddStatBonus(Character.Stat.CritChance, reward.value);
                    break;
            }
        }

        void ApplyHeal(WaveReward reward)
        {
            if (player == null || player.Health == null) 
                return;

            int healAmount;
            if (reward.value >= 100)
            {
                healAmount = player.maxHealth;
            }
            else
            {
                healAmount = (int)(player.maxHealth * (reward.value / 100f));
            }

            player.Health.ReceiveHealth(new Heal { Value = healAmount }, null, player);
        }

        void ApplyGold(WaveReward reward)
        {
            if (player == null) 
                return;

            player.gainGold((int)reward.value);
        }

        void ApplyShield(WaveReward reward)
        {
            if (player == null) 
                return;

            // player.AddShield(reward.value);
        }

        public void OnDestroy()
        {
        }
    }

    // 辅助类
    public static class WaveRewardExtensions
    {
        public static WaveReward Clone(this WaveReward reward)
        {
            return new WaveReward
            {
                type = reward.type,
                rewardId = reward.rewardId,
                rewardName = reward.rewardName,
                description = reward.description,
                value = reward.value,
                icon = reward.icon
            };
        }
    }
}
