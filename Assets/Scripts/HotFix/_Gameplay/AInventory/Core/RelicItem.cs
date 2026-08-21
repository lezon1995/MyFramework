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
    public class RelicItem : ClassObject, IInventoryItem
    {
        public int ItemId => Def.RelicDefId;
        public RelicType Type => Def.Type;
        public ItemKind Kind => ItemKind.Relic;
        public RelicDef Def;

        public string DisplayName => Def ? $"{Def.DisplayName.GetLocalizedString()}" : $"Relic#{Type}";
        public int SellPrice => Def.BasePrice;

        /// <summary>关联的 ARelic 实例（或其子类）；由 RelicService 解析回传。</summary>
        public ARelic UnderlyingRelic;

        public override void resetProperty()
        {
            base.resetProperty();
            Def = null;
            UnderlyingRelic = null;
        }
        
        /// <summary>工厂方法。系统内部创建都用它。</summary>
        public static RelicItem New(RelicDef def, ARelic underlying)
        {
            var item = CLASS<RelicItem>();
            item.Def = def;
            item.UnderlyingRelic = underlying;
            underlying.setDef(def);
            return item;
        }
        
        public static void Release(ref RelicItem item) => UN_CLASS(ref item);
        public static void Release(RelicItem item) => UN_CLASS(item);
    }
}