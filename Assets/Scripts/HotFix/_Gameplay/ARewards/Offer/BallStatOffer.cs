namespace MoreMountains
{
    /// <summary>
    /// 球的属性奖励卡实现。
    /// 持有 BallStatDef
    /// </summary>
    public sealed class BallStatOffer : ClassObject, IPurchasable, IArgs<BallStatDef, ItemRarity, float, float, DisplayConfig>
    {
        public BallStatDef Def;
        public ItemKind Kind => ItemKind.BallStatMod;
        public int ItemId => 0;

        public string DisplayName
        {
            get
            {
                var str = LocalizedStats.getName(Def.statKey);
                var name = str;
                string suffix = Rarity switch
                {
                    ItemRarity.Tier1 => "",
                    ItemRarity.Tier2 => "Ⅱ",
                    ItemRarity.Tier3 => "Ⅲ",
                    ItemRarity.Tier4 => "Ⅳ",
                    _ => ""
                };
                name += $" {suffix}";
                return name;
            }
        }

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
        public DisplayConfig DisplayConfig;
        public ItemRarity Rarity;

        public override void resetProperty()
        {
            Def = null;
            _sold = false;
            Sold = false;
            displayName = null;
            DisplayConfig = null;
            BonusFlat = 0F;
            BonusPct = 0F;
            Rarity = default;
            base.resetProperty();
        }

        public void MarkSold()
        {
            Sold = true;
        }

        public void onCreate(BallStatDef def, ItemRarity rarity, float bonusFlat, float bonusPct, DisplayConfig displayConfig)
        {
            Def = def;
            Rarity = rarity;
            BonusFlat = bonusFlat;
            BonusPct = bonusPct;
            DisplayConfig = displayConfig;
        }
    }
}