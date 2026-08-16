namespace MoreMountains;

/// <summary>
/// ExtendedFlight - 延长飞行
/// 球持续时间+30%
/// </summary>
public class ExtendedFlight : ARelic
{
    public static string ID = "ExtendedFlight";

    public ExtendedFlight() : base(ID, "ExtendedFlight.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new ExtendedFlight();
}

/// <summary>
/// EternalBall - 永恒之球
/// 球存在时间翻倍
/// </summary>
public class EternalBall : ARelic
{
    public static string ID = "EternalBall";

    public EternalBall() : base(ID, "EternalBall.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new EternalBall();
}

/// <summary>
/// QuickReturn - 快速返回
/// 球提前返回
/// [设计文案] 更快的返回节奏
/// </summary>
public class QuickReturn : ARelic
{
    public static string ID = "QuickReturn";

    public QuickReturn() : base(ID, "QuickReturn.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现快速返回
    // public override void onEquip(APlayer p)
    // {
    //     p.BallReturnSpeed *= 1.5f;
    // }

    public override ARelic makeCopy() => new QuickReturn();
}

/// <summary>
/// LongLasting - 持久
/// 球持续时间+50%
/// </summary>
public class LongLasting : ARelic
{
    public static string ID = "LongLasting";

    public LongLasting() : base(ID, "LongLasting.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new LongLasting();
}

/// <summary>
/// FadingEcho - 消逝回声
/// 球逐渐衰减但伤害增加
/// [设计文案] 牺牲持久换取力量
/// </summary>
public class FadingEcho : ARelic
{
    public static string ID = "FadingEcho";

    public FadingEcho() : base(ID, "FadingEcho.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现衰减但增伤的机制
    // public override void onShootBall(Ball ball)
    // {
    //     ball.AddModifier(new BallModifier
    //     {
    //         DurationMultiplier = 0.7f,
    //         DamageMultiplier = 1.3f
    //     });
    // }

    public override ARelic makeCopy() => new FadingEcho();
}

/// <summary>
/// Overcharged - 过载
/// 球时间越长伤害越高
/// [设计文案] 蓄力的力量
/// </summary>
public class Overcharged : ARelic
{
    public static string ID = "Overcharged";

    public Overcharged() : base(ID, "Overcharged.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现时间累积伤害
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     foreach (var ball in p.Balls)
    //     {
    //         ball.AccumulatedDamageBonus += 0.01f * dt;
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(ball.AccumulatedDamageBonus);
    //     }
    // }

    public override ARelic makeCopy() => new Overcharged();
}

/// <summary>
/// StableOrbit - 稳定轨道
/// 球轨道更稳定
/// [设计文案] 减少轨迹偏差
/// </summary>
public class StableOrbit : ARelic
{
    public static string ID = "StableOrbit";

    public StableOrbit() : base(ID, "StableOrbit.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现稳定轨道
    // public override void onEquip(APlayer p)
    // {
    //     p.BallTrajectoryVariance *= 0.5f;
    // }

    public override ARelic makeCopy() => new StableOrbit();
}

/// <summary>
/// EnduringBlow - 持久打击
/// 命中伤害随时间增加
/// [设计文案] 越战越强
/// </summary>
public class EnduringBlow : ARelic
{
    public static string ID = "EnduringBlow";

    public EnduringBlow() : base(ID, "EnduringBlow.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现时间累积伤害
    // private float timeAccumulator = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     timeAccumulator += dt;
    //     if (timeAccumulator >= 5f)
    //     {
    //         foreach (var ball in p.Balls)
    //         {
    //             ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //             stat.AddPct(0.05f);
    //         }
    //         timeAccumulator = 0f;
    //     }
    // }

    public override ARelic makeCopy() => new EnduringBlow();
}

/// <summary>
/// TimelessShot - 无时间射击
/// 球不受时间限制
/// [设计文案] 永恒的球体
/// </summary>
public class TimelessShot : ARelic
{
    public static string ID = "TimelessShot";

    public TimelessShot() : base(ID, "TimelessShot.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现球不受时间限制
    // public override void onShootBall(Ball ball)
    // {
    //     ball.IgnoreTimeLimit = true;
    // }

    public override ARelic makeCopy() => new TimelessShot();
}

/// <summary>
/// MomentumShift - 动量转移
/// 球速度随距离增加
/// [设计文案] 距离产生力量
/// </summary>
public class MomentumShift : ARelic
{
    public static string ID = "MomentumShift";

    public MomentumShift() : base(ID, "MomentumShift.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现距离累积伤害
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     foreach (var ball in p.Balls)
    //     {
    //         var distance = Vector2.Distance(ball.StartPosition, ball.Position);
    //         var bonus = distance * 0.01f; // 每单位距离+1%
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(Mathf.Min(bonus, 0.5f)); // 最多+50%
    //     }
    // }

    public override ARelic makeCopy() => new MomentumShift();
}
