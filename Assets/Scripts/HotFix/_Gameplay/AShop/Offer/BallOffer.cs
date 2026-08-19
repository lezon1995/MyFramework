namespace MoreMountains
{
    /// <summary>
    /// 球的商品卡实现。
    /// 持有 BallDef；价格 / 显示名都从 def 读。
    /// Sold 标记后置灰，点击被 ShopController 拒绝。
    /// </summary>
    public sealed class BallOffer : ClassObject, IPurchasable, IArgs<BallDef>
    {
        public BallDef Def;
        public ItemKind Kind => ItemKind.Ball;
        public int ItemId => Def ? Def.BallDefId : 0;
        public string DisplayName => Def ? Def.DisplayName.GetLocalizedString() : "<missing ball def>";
        public int Price => Def ? Def.BasePrice : 0;

        bool _sold;

        public bool Sold
        {
            get => _sold;
            private set => _sold = value;
        }

        public bool Enabled => !Sold;

        public override void resetProperty()
        {
            Def = null;
            _sold = false;
            Sold = false;
            base.resetProperty();
        }

        public void MarkSold()
        {
            Sold = true;
        }
        
        public void onCreate(BallDef def)
        {
            Def = def;
        }
    }

    /// <summary>
    /// 遗物的商品卡实现。
    /// 持有 RelicDef（项目里目前没有统一的 ScriptableObject RelicDef，可以换成 SO，也可以直接从 RelicItem 读）。
    /// 这里用 SO 风格的字段定义，便于策划表生成。
    /// </summary>
    public sealed class RelicOffer : ClassObject, IPurchasable, IArgs<RelicDef>
    {
        public RelicDef Def;
        public ItemKind Kind => ItemKind.Relic;
        public int ItemId => Def ? Def.RelicDefId : 0;
        public string DisplayName => Def ? Def.DisplayName.GetLocalizedString() : "<missing relic def>";
        public string DisplayDesc => Def ? Def.DisplayDescription.GetLocalizedString() : "<missing relic def>";
        public int Price => Def ? Def.BasePrice : 0;
        bool _sold;

        public bool Sold
        {
            get => _sold;
            private set => _sold = value;
        }

        public bool Enabled => !Sold;

        public override void resetProperty()
        {
            Def = null;
            _sold = false;
            Sold = false;
            base.resetProperty();
        }

        public void MarkSold()
        {
            Sold = true;
        }

        public void onCreate(RelicDef def)
        {
            Def = def;
        }
    }
}