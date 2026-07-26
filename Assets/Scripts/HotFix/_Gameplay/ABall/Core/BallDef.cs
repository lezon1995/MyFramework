using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 球的静态定义 —— 由策划用 CSV / SO 维护。
    /// 系统初始化时把所有 BallDef 注册到 BallDefLibrary。
    /// </summary>
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/BallDef")]
    public sealed class BallDef : ScriptableObject
    {
        public int BallDefId => (int)Type;
        public BallType Type;
        public string DisplayName = "Ball";

        [Header("Price")]
        public int BasePrice = 10;   // 商店售价 / 售出回收基于它

        [Header("Level")]
        public int MaxLevel = 3;

        [Header("Upgrade Recipe")]
        public int UpgradeCombineCount = 2;
        public int UpgradeGoldCost;

        [Header("Merge Recipe")]
        /// <summary>融合后产物的 def id。0 / -1 表示不可融合。</summary>
        public int MergeResultDefId;
        public int MergeGoldCost = 100;

        [Header("Visual")]
        public Sprite Icon;
        public Color LevelColor = Color.white;
    }
}
