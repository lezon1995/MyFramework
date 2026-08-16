using UniStats;

namespace MoreMountains;

/// <summary>
/// SharpSphere - 锐利球体
/// 命中伤害+8%
/// </summary>
public class SharpSphere : ARelic
{
    public static string ID = "SharpSphere";

    public SharpSphere() : base(ID, "SharpSphere.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new SharpSphere();
}

/// <summary>
/// RubberBall - 橡皮球
/// 球弹速+5%
/// </summary>
public class RubberBall : ARelic
{
    public static string ID = "RubberBall";

    public RubberBall() : base(ID, "RubberBall.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new RubberBall();
}

/// <summary>
/// LuckyDice - 幸运骰子
/// 幸运+5
/// </summary>
public class LuckyDice : ARelic
{
    public static string ID = "LuckyDice";

    public LuckyDice() : base(ID, "LuckyDice.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new LuckyDice();
}

/// <summary>
/// TinyShield - 小护盾
/// 护甲+1
/// </summary>
public class TinyShield : ARelic
{
    public static string ID = "TinyShield";

    public TinyShield() : base(ID, "TinyShield.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new TinyShield();
}

/// <summary>
/// SpeedPotion - 速度药水
/// 移速+3%
/// </summary>
public class SpeedPotion : ARelic
{
    public static string ID = "SpeedPotion";

    public SpeedPotion() : base(ID, "SpeedPotion.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new SpeedPotion();
}

/// <summary>
/// CottonPadded - 棉垫
/// 闪避+2%
/// </summary>
public class CottonPadded : ARelic
{
    public static string ID = "CottonPadded";

    public CottonPadded() : base(ID, "CottonPadded.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new CottonPadded();
}

/// <summary>
/// IronCore - 铁核
/// 生命上限+10
/// </summary>
public class IronCore : ARelic
{
    public static string ID = "IronCore";

    public IronCore() : base(ID, "IronCore.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new IronCore();
}

/// <summary>
/// QuickReflexes - 快速反应
/// 攻速+5%
/// </summary>
public class QuickReflexes : ARelic
{
    public static string ID = "QuickReflexes";

    public QuickReflexes() : base(ID, "QuickReflexes.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new QuickReflexes();
}

/// <summary>
/// SteelBall - 钢球
/// 球伤害+5%
/// </summary>
public class SteelBall : ARelic
{
    public static string ID = "SteelBall";

    public SteelBall() : base(ID, "SteelBall.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new SteelBall();
}

/// <summary>
/// LightWeight - 轻量级
/// 球弹速+3%
/// </summary>
public class LightWeight : ARelic
{
    public static string ID = "LightWeight";

    public LightWeight() : base(ID, "LightWeight.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new LightWeight();
}
