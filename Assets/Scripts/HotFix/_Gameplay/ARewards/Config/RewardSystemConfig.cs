using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 升级奖励系统策划配置。
    /// </summary>
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/RewardSystemConfig")]
    public sealed class RewardSystemConfig : ScriptableObject
    {
        static RewardSystemConfig sInstance;

        public static RewardSystemConfig Instance
        {
            get
            {
                if (sInstance == null)
                {
                    string path = $"{GAMEPLAY_PATH}/Rewards/RewardSystemConfig.asset";
                    sInstance = resource.loadGameResource<RewardSystemConfig>(path);
                }

                return sInstance;
            }
        }

        [Header("Board Size")]
        public int BallOfferCount = 4;

        public int RelicOfferCount = 4;
        public int MixedOfferCount = 4;

        [Header("Reroll Cost")]
        public int BallBoardRerollCost = 2;

        public int RelicBoardRerollCost = 2;
        public int MixedBoardRerollCost = 2;

        [Header("Offer Pool")]
        public List<BallStatConfig> BallStatModOfferPool = new();

        public List<PlayerStatConfig> PlayerStatModOfferPool = new();
    }

    [Serializable]
    public class BallStatConfig
    {
        public BallStatModDef def;
        public List<RarityConfig> configs;
        public RarityConfig getConfig(ItemRarity rarity) => configs[(int)rarity];

        [Serializable]
        public class RarityConfig
        {
            [HorizontalGroup]
            public ItemRarity rarity;

            [HorizontalGroup]
            public float bonusFlat;

            [HorizontalGroup]
            public float bonusPct;
        }
    }

    [Serializable]
    public class PlayerStatConfig
    {
        public PlayerStatModDef def;
        public List<RarityConfig> configs;
        public RarityConfig getConfig(ItemRarity rarity) => configs[(int)rarity];
        [Serializable]
        public class RarityConfig
        {
            [HorizontalGroup]
            public ItemRarity rarity;

            [HorizontalGroup]
            public float bonusFlat;

            [HorizontalGroup]
            public float bonusPct;
        }
    }
}