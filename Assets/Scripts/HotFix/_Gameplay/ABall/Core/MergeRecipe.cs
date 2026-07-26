using System;

namespace MoreMountains
{
    /// <summary>
    /// 升级 / 融合通用配方。
    /// 注意：
    ///   • 升级产物仍是同 def，等级+1（ResultDefId == 自身）
    ///   • 融合产物是不同 def，等级=1（ResultDefId 由策划填）
    /// 字段语义克制，避免在系统里硬编码 X=2。
    /// </summary>
    [Serializable]
    public struct MergeRecipe
    {
        /// <summary>合成所需的数量（升级默认 2，可策划改成 3）</summary>
        public int CombineCount;

        /// <summary>合成所需金币（升级默认 0，融合有值）</summary>
        public int GoldCost;

        /// <summary>合成产物的 def id；null/-1 表示"同 def 升级"语义</summary>
        public int ResultDefId;

        public bool IsUpgradeRecipe => ResultDefId <= 0;

        public static MergeRecipe UpgradeDefault(int combineCount = 2, int goldCost = 0)
        {
            return new() { CombineCount = combineCount, GoldCost = goldCost, ResultDefId = 0 };
        }

        public static MergeRecipe MergeTo(int resultDefId, int goldCost)
        {
            return new() { CombineCount = 2, GoldCost = goldCost, ResultDefId = resultDefId };
        }
    }
}