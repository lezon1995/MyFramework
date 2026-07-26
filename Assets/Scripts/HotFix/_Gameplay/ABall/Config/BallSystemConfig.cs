using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 球管理全局配置 —— 容量、升级默认值。
    /// 注意 Merge 配方由 BallDef 自带（per-def），不在这里硬编码。
    /// </summary>
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/BallSystemConfig")]
    public sealed class BallSystemConfig : ScriptableObject
    {
        static BallSystemConfig sInstance;

        public static BallSystemConfig Instance
        {
            get
            {
                if (sInstance == null)
                    sInstance = Resources.Load<BallSystemConfig>(nameof(BallSystemConfig));
                return sInstance;
            }
        }

        [Header("Slot")]
        [Tooltip("发射槽位数（默认 3，可运行时扩容）")]
        public int SlotCount = 3;

        [Tooltip("扩容上限（防越界）")]
        public int MaxSlotCount = 8;

        [Header("Level & Upgrade")]
        [Tooltip("球最大等级（默认 3）")]
        public int DefaultMaxLevel = 3;

        [Tooltip("升级 X 合 1（默认 2）")]
        public int UpgradeCombineCount = 2;

        [Tooltip("升级是否扣金币（默认 0）")]
        public int UpgradeGoldCost = 0;

        [Header("Price")]
        [Tooltip("出售时回收比例，百分数（默认 50%）")]
        [Range(0, 100)]
        public int SellRefundRate = 50;
    }
}