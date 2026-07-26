namespace MoreMountains
{
    /// <summary>
    /// 球的商品卡实现。
    /// 持有 BallDef；价格 / 显示名都从 def 读。
    /// Sold 标记后置灰，点击被 ShopController 拒绝。
    /// </summary>
    public sealed class BallOffer : IPurchasable
    {
        public BallDef Def { get; }
        public ItemKind Kind => ItemKind.Ball;
        public int ItemId => Def ? Def.BallDefId : 0;
        public string DisplayName => Def ? Def.DisplayName : "<missing ball def>";
        public int Price => Def ? Def.BasePrice : 0;
        public bool Sold { get; private set; }
        public bool Enabled => !Sold;

        public BallOffer(BallDef def)
        {
            Def = def;
        }

        public void MarkSold()
        {
            Sold = true;
        }
    }

    /// <summary>
    /// 遗物的商品卡实现。
    /// 持有 RelicDef（项目里目前没有统一的 ScriptableObject RelicDef，可以换成 SO，也可以直接从 RelicItem 读）。
    /// 这里用 SO 风格的字段定义，便于策划表生成。
    /// </summary>
    public sealed class RelicOffer : IPurchasable
    {
        public RelicDef Def { get; }
        public ItemKind Kind => ItemKind.Relic;
        public int ItemId => Def ? Def.RelicDefId : 0;
        public string DisplayName => Def ? Def.DisplayName : "<missing relic def>";
        public int Price => Def ? Def.BasePrice : 0;
        public bool Sold { get; private set; }
        public bool Enabled => !Sold;

        public RelicOffer(RelicDef def)
        {
            Def = def;
        }

        public void MarkSold()
        {
            Sold = true;
        }
    }
}