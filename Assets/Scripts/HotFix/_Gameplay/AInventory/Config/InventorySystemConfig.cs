using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 背包系统策划配置 —— 容量与扩容上限。
    /// 单例 SO 模式：InventorySystem.init() 期间会读取。
    /// </summary>
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/InventorySystemConfig")]
    public sealed class InventorySystemConfig : ScriptableObject
    {
        static InventorySystemConfig sInstance;

        public static InventorySystemConfig Instance
        {
            get
            {
                if (sInstance == null)
                    sInstance = Resources.Load<InventorySystemConfig>(nameof(InventorySystemConfig));
                return sInstance;
            }
        }

        [Header("Bag Capacity")]
        [Tooltip("球背包默认格数")]
        public int BallBagCapacity  = 9;
        [Tooltip("遗物背包默认格数")]
        public int RelicBagCapacity = 15;

        [Header("Expansion Cap")]
        [Tooltip("球背包容量上限")]
        public int MaxBallBagCapacity  = 30;
        [Tooltip("遗物背包容量上限")]
        public int MaxRelicBagCapacity = 40;
    }
}
