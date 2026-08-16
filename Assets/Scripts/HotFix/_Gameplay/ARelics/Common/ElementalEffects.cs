namespace MoreMountains;

/// <summary>
/// FireElemental - 火元素
/// 灼烧伤害+30%
/// </summary>
public class FireElemental : ARelic
{
    public static string ID = "FireElemental";

    public FireElemental() : base(ID, "FireElemental.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现灼烧增伤
    // public override void onEquip(APlayer p)
    // {
    //     p.BurningDamageMultiplier *= 1.3f;
    // }

    public override ARelic makeCopy() => new FireElemental();
}

/// <summary>
/// IceElemental - 冰元素
/// 冻结时间+1秒
/// [设计文案] 冰冻的力量
/// </summary>
public class IceElemental : ARelic
{
    public static string ID = "IceElemental";

    public IceElemental() : base(ID, "IceElemental.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现冻结时间增加
    // public override void onEquip(APlayer p)
    // {
    //     p.FreezeDurationBonus += 1f;
    // }

    public override ARelic makeCopy() => new IceElemental();
}

/// <summary>
/// LightningElemental - 雷元素
/// 闪电链伤害+50%
/// </summary>
public class LightningElemental : ARelic
{
    public static string ID = "LightningElemental";

    public LightningElemental() : base(ID, "LightningElemental.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现闪电链增伤
    // public override void onEquip(APlayer p)
    // {
    //     p.LightningChainDamageMultiplier *= 1.5f;
    // }

    public override ARelic makeCopy() => new LightningElemental();
}

/// <summary>
/// PoisonElemental - 毒元素
/// 中毒伤害+40%
/// </summary>
public class PoisonElemental : ARelic
{
    public static string ID = "PoisonElemental";

    public PoisonElemental() : base(ID, "PoisonElemental.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现中毒增伤
    // public override void onEquip(APlayer p)
    // {
    //     p.PoisonDamageMultiplier *= 1.4f;
    // }

    public override ARelic makeCopy() => new PoisonElemental();
}

/// <summary>
/// EarthElemental - 土元素
/// 护甲+10
/// </summary>
public class EarthElemental : ARelic
{
    public static string ID = "EarthElemental";

    public EarthElemental() : base(ID, "EarthElemental.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new EarthElemental();
}

/// <summary>
/// WindElemental - 风元素
/// 移速+20%
/// </summary>
public class WindElemental : ARelic
{
    public static string ID = "WindElemental";

    public WindElemental() : base(ID, "WindElemental.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new WindElemental();
}

/// <summary>
/// WaterElemental - 水元素
/// 生命回复+100%
/// </summary>
public class WaterElemental : ARelic
{
    public static string ID = "WaterElemental";

    public WaterElemental() : base(ID, "WaterElemental.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现生命回复翻倍
    // public override void onEquip(APlayer p)
    // {
    //     p.HealthRegenMultiplier *= 2f;
    // }

    public override ARelic makeCopy() => new WaterElemental();
}

/// <summary>
/// LightElemental - 光元素
/// 暴击伤害+50%
/// </summary>
public class LightElemental : ARelic
{
    public static string ID = "LightElemental";

    public LightElemental() : base(ID, "LightElemental.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    public override ARelic makeCopy() => new LightElemental();
}

/// <summary>
/// DarkElemental - 暗元素
/// 生命偷取+20%
/// </summary>
public class DarkElemental : ARelic
{
    public static string ID = "DarkElemental";

    public DarkElemental() : base(ID, "DarkElemental.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    public override ARelic makeCopy() => new DarkElemental();
}

/// <summary>
/// ChaosElemental - 混沌元素
/// 所有元素伤害+15%
/// [设计文案] 混乱的元素力量
/// </summary>
public class ChaosElemental : ARelic
{
    public static string ID = "ChaosElemental";

    public ChaosElemental() : base(ID, "ChaosElemental.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现所有元素增伤
    // public override void onEquip(APlayer p)
    // {
    //     p.AllElementalDamageMultiplier *= 1.15f;
    // }

    public override ARelic makeCopy() => new ChaosElemental();
}
