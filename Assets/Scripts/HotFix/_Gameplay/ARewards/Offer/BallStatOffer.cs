namespace MoreMountains
{
    /// <summary>
    /// 球的属性奖励卡实现。
    /// 持有 BallStatDef
    /// </summary>
    public sealed class BallStatOffer : ClassObject, IPurchasable, IArgs<BallStatModDef, ItemRarity, float, float>
    {
        public BallStatModDef Def;
        public ItemKind Kind => ItemKind.BallStatMod;
        public int ItemId => 0;
        public string DisplayName => Def ? Def.statKey : "<missing ball stat mod def>";
        public int Price => 0;
        bool _sold;

        public bool Sold
        {
            get => _sold;
            private set => _sold = value;
        }

        public bool Enabled => !Sold;
        
        public string displayName;
        public float BonusFlat;
        public float BonusPct;
        public ItemRarity Rarity;

        public override void resetProperty()
        {
            Def = null;
            _sold = false;
            Sold = false;
            displayName = null;
            BonusFlat = 0F;
            BonusPct = 0F;
            Rarity = default;
            base.resetProperty();
        }

        public void MarkSold()
        {
            Sold = true;
        }

        public void onCreate(BallStatModDef def, ItemRarity rarity, float bonusFlat, float bonusPct)
        {
            Def = def;
            Rarity = rarity;
            BonusFlat = bonusFlat;
            BonusPct = bonusPct;
        }
    }
}