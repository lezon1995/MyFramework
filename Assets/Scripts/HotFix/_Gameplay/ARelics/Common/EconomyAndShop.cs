namespace MoreMountains;

/// <summary>
/// MoneyBag - 钱袋
/// 金币+20%
/// </summary>
public class MoneyBag : ARelic
{
    public static string ID = "MoneyBag";

    public MoneyBag() : base(ID, "MoneyBag.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现金币增益
    // public override void onEquip(APlayer p)
    // {
    //     p.GoldMultiplier *= 1.2f;
    // }

    public override ARelic makeCopy() => new MoneyBag();
}

/// <summary>
/// DiscountCoupon - 折扣券
/// 商店价格-15%
/// </summary>
public class DiscountCoupon : ARelic
{
    public static string ID = "DiscountCoupon";

    public DiscountCoupon() : base(ID, "DiscountCoupon.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现商店折扣
    // public override void onEquip(APlayer p)
    // {
    //     p.ShopPriceMultiplier *= 0.85f;
    // }

    public override ARelic makeCopy() => new DiscountCoupon();
}

/// <summary>
/// TreasureHunter - 寻宝者
/// 稀有物品出现概率+10%
/// </summary>
public class TreasureHunter : ARelic
{
    public static string ID = "TreasureHunter";

    public TreasureHunter() : base(ID, "TreasureHunter.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现稀有物品概率
    // public override void onEquip(APlayer p)
    // {
    //     p.RareItemChanceBonus += 0.1f;
    // }

    public override ARelic makeCopy() => new TreasureHunter();
}

/// <summary>
/// GoldenTouch - 点金手
/// 击杀敌人额外获得金币
/// [设计文案] 杀死敌人就有钱
/// </summary>
public class GoldenTouch : ARelic
{
    public static string ID = "GoldenTouch";

    public GoldenTouch() : base(ID, "GoldenTouch.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现击杀金币
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     p.AddGold(1);
    // }

    public override ARelic makeCopy() => new GoldenTouch();
}

/// <summary>
/// TaxCollector - 税务员
/// 每波次结束时获得金币
/// [设计文案] 波次结束的奖励
/// </summary>
public class TaxCollector : ARelic
{
    public static string ID = "TaxCollector";

    public TaxCollector() : base(ID, "TaxCollector.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现波次金币
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     p.AddGold(5);
    // }

    public override ARelic makeCopy() => new TaxCollector();
}

/// <summary>
/// MerchantSoul - 商人灵魂
/// 商店物品价格-10%
/// </summary>
public class MerchantSoul : ARelic
{
    public static string ID = "MerchantSoul";

    public MerchantSoul() : base(ID, "MerchantSoul.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现商店折扣
    // public override void onEquip(APlayer p)
    // {
    //     p.ShopPriceMultiplier *= 0.9f;
    // }

    public override ARelic makeCopy() => new MerchantSoul();
}

/// <summary>
/// LuckyCoin - 幸运硬币
/// 金币+30
/// </summary>
public class LuckyCoin : ARelic
{
    public static string ID = "LuckyCoin";

    public LuckyCoin() : base(ID, "LuckyCoin.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new LuckyCoin();
}

/// <summary>
/// BankAccount - 银行账户
/// 携带金币上限翻倍
/// [设计文案] 存更多的钱
/// </summary>
public class BankAccount : ARelic
{
    public static string ID = "BankAccount";

    public BankAccount() : base(ID, "BankAccount.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现金币上限
    // public override void onEquip(APlayer p)
    // {
    //     p.MaxGold *= 2f;
    // }

    public override ARelic makeCopy() => new BankAccount();
}

/// <summary>
/// Investment - 投资
/// 每10秒获得金币
/// [设计文案] 被动收入
/// </summary>
public class Investment : ARelic
{
    public static string ID = "Investment";
    private float timer = 0f;

    public Investment() : base(ID, "Investment.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现定时金币
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     timer += dt;
    //     if (timer >= 10f)
    //     {
    //         timer = 0f;
    //         p.AddGold(2);
    //     }
    // }

    public override ARelic makeCopy() => new Investment();
}

/// <summary>
/// BlackMarket - 黑市
/// 可以用金币刷新商店
/// [设计文案] 有钱就能刷新
/// </summary>
public class BlackMarket : ARelic
{
    public static string ID = "BlackMarket";

    public BlackMarket() : base(ID, "BlackMarket.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现商店刷新
    // public override void onEquip(APlayer p)
    // {
    //     p.CanRerollShop = true;
    //     p.ShopRerollCost = 10; // 重刷新需要10金币
    // }

    public override ARelic makeCopy() => new BlackMarket();
}
