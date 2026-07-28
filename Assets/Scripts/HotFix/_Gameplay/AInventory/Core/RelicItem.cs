using System;

namespace MoreMountains
{
    /// <summary>
    /// 遗物背包中的"物品"抽象。
    /// ARelic 是项目里已有的 partial 类（见 Assets/Scripts/HotFix/_Gameplay/ARelics/ARelic.cs）。
    /// 这里定义一个 RelicItem 类作为背包的 item 适配层，
    /// 业务层（RelicService）负责把 ARelic 子类包装成 RelicItem 再入包。
    /// 这样：
    ///   1) 我们不修改 ARelic 任何字段/方法（最小侵入）
    ///   2) 遗物可以用 RelicItem 作 slot 排序的唯一身份（与球对称）
    /// </summary>
    public class RelicItem : IInventoryItem
    {
        public string RelicId { get; }
        public string DisplayName { get; }
        public int SellPrice { get; }
        public int ItemId { get; }
        public ItemKind Kind => ItemKind.Relic;

        /// <summary>关联的 ARelic 实例（或其子类）；由 RelicService 解析回传。</summary>
        public ARelic UnderlyingRelic { get; }

        public RelicItem(ARelic underlying, int sellPrice)
        {
            UnderlyingRelic = underlying ?? throw new ArgumentNullException(nameof(underlying));
            RelicId = string.IsNullOrEmpty(underlying.relicId) ? underlying.GetType().Name : underlying.relicId;
            DisplayName = underlying.name ?? RelicId;
            SellPrice = Math.Max(0, sellPrice);
            ItemId = ComputeHashId(RelicId);
        }

        static int ComputeHashId(string s)
        {
            unchecked
            {
                int hash = 17;
                foreach (var c in s) hash = hash * 31 + c;
                return hash;
            }
        }
    }
}