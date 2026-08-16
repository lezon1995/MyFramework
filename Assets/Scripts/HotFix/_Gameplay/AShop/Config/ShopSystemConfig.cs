using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 商店系统策划配置。
    /// </summary>
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/ShopSystemConfig")]
    public sealed class ShopSystemConfig : ScriptableObject
    {
        static ShopSystemConfig sInstance;

        public static ShopSystemConfig Instance
        {
            get
            {
                if (sInstance == null)
                {
                    string path = $"{GAMEPLAY_PATH}/Shop/ShopSystemConfig.asset";
                    sInstance = resource.loadGameResource<ShopSystemConfig>(path);
                }

                return sInstance;
            }
        }

        [Header("Board Size")]
        public int BallOfferCount  = 4;
        public int RelicOfferCount = 4;
        public int MixedOfferCount = 4;

        [Header("Reroll Cost")]
        public int BallBoardRerollCost  = 2;
        public int RelicBoardRerollCost = 1;
        public int MixedBoardRerollCost = 1;

        [Header("Offer Pool")]
        public List<BallDef>  BallOfferPool  = new();
        public List<RelicDef> RelicOfferPool = new();
    }
}
