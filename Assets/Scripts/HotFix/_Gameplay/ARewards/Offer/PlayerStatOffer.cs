namespace MoreMountains
{
    /// <summary>
    /// 玩家角色的属性奖励卡实现。
    /// 持有 StatDef；价格 / 显示名都从 def 读。
    /// Sold 标记后置灰，点击被 RewardController 拒绝。
    /// </summary>
    public sealed class PlayerStatOffer : ClassObject, IPurchasable
        , IArgs<PlayerStatDef, ItemRarity, float, float, DisplayConfig>
    {
        public PlayerStatDef Def;
        public ItemKind Kind => ItemKind.PlayerStatMod;
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

        public void onCreate(PlayerStatDef def, ItemRarity rarity, float bonusFlat, float bonusPct, DisplayConfig displayConfig)
        {
            Def = def;
            Rarity = rarity;
            BonusFlat = bonusFlat;
            BonusPct = bonusPct;
            DisplayConfig = displayConfig;
        }
    }
}