namespace MoreMountains;

/// <summary>
/// WideAngle - 广角
/// 球发射范围+30%
/// [设计文案] 更宽广的发射角度
/// </summary>
public class WideAngle : ARelic
{
    public static string ID = "WideAngle";

    public WideAngle() : base(ID, "WideAngle.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现增加发射范围
    // public override void onEquip(APlayer p)
    // {
    //     p.ShootAngleRange *= 1.3f;
    // }

    public override ARelic makeCopy() => new WideAngle();
}

/// <summary>
/// LongRange - 远程
/// 球飞行距离+50%
/// [设计文案] 更远的射程
/// </summary>
public class LongRange : ARelic
{
    public static string ID = "LongRange";

    public LongRange() : base(ID, "LongRange.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new LongRange();
}

/// <summary>
/// Scattershot - 散弹
/// 发射多枚小球
/// [设计文案] 一发变多发的散射
/// </summary>
public class Scattershot : ARelic
{
    public static string ID = "Scattershot";

    public Scattershot() : base(ID, "Scattershot.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现发射多枚小球
    // public override void onShootBall(Ball ball)
    // {
    //     // 发射3枚额外的小球
    //     for (int i = 0; i < 3; i++)
    //     {
    //         var angle = Random.Range(-30f, 30f);
    //         var newBall = ball.Clone();
    //         newBall.Direction = ball.Direction.Rotate(angle);
    //     }
    // }

    public override ARelic makeCopy() => new Scattershot();
}

/// <summary>
/// SniperScope - 狙击镜
/// 命中伤害+25%，命中率-20%
/// [设计文案] 精准但难以命中
/// </summary>
public class SniperScope : ARelic
{
    public static string ID = "SniperScope";

    public SniperScope() : base(ID, "SniperScope.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new SniperScope();
}

/// <summary>
/// SpreadShot - 扩散射击
/// 球分散成扇形
/// [设计文案] 扇形散射覆盖更广
/// </summary>
public class SpreadShot : ARelic
{
    public static string ID = "SpreadShot";

    public SpreadShot() : base(ID, "SpreadShot.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现扇形分散
    // public override void onShootBall(Ball ball)
    // {
    //     // 球在飞行一段距离后分散成5个
    // }

    public override ARelic makeCopy() => new SpreadShot();
}

/// <summary>
/// Railgun - 电磁炮
/// 单发高伤害，低射速
/// [设计文案] 毁灭性的单发射击
/// </summary>
public class Railgun : ARelic
{
    public static string ID = "Railgun";

    public Railgun() : base(ID, "Railgun.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现高伤害低射速
    // public override void onEquip(APlayer p)
    // {
    //     foreach (var ball in p.Balls)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var dmg);
    //         dmg.AddPct(0.5f);
    //         ball.GetStat(Ball.Stat.AS, out var spd);
    //         spd.AddPct(-0.3f);
    //     }
    // }

    public override ARelic makeCopy() => new Railgun();
}

/// <summary>
/// ShotgunBlast - 霰弹
/// 近距离高伤害
/// [设计文案] 贴脸时伤害翻倍
/// </summary>
public class ShotgunBlast : ARelic
{
    public static string ID = "ShotgunBlast";

    public ShotgunBlast() : base(ID, "ShotgunBlast.png", RelicTier.UNCOMMON, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现近距离增伤
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     var distance = Vector2.Distance(p.Position, brick.Position);
    //     if (distance < 3f) // 近距离
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(1.0f); // 伤害翻倍
    //     }
    // }

    public override ARelic makeCopy() => new ShotgunBlast();
}

/// <summary>
/// PrecisionBeam - 精准光束
/// 穿透伤害+50%
/// [设计文案] 穿透敌人不减伤
/// </summary>
public class PrecisionBeam : ARelic
{
    public static string ID = "PrecisionBeam";

    public PrecisionBeam() : base(ID, "PrecisionBeam.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现穿透不减伤
    // public override void onEquip(APlayer p)
    // {
    //     p.PenetrationDamageDecay = 0f; // 穿透不衰减
    // }

    public override ARelic makeCopy() => new PrecisionBeam();
}

/// <summary>
/// ArcShot - 弧形射击
/// 球沿弧线飞行
/// [设计文案] 弯曲的弹道
/// </summary>
public class ArcShot : ARelic
{
    public static string ID = "ArcShot";

    public ArcShot() : base(ID, "ArcShot.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现弧线飞行
    // public override void onShootBall(Ball ball)
    // {
    //     ball.SetCurvedPath(true);
    // }

    public override ARelic makeCopy() => new ArcShot();
}

/// <summary>
/// TrajectoryGuide - 弹道引导
/// 球飞行更精准
/// [设计文案] 优化的弹道轨迹
/// </summary>
public class TrajectoryGuide : ARelic
{
    public static string ID = "TrajectoryGuide";

    public TrajectoryGuide() : base(ID, "TrajectoryGuide.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现弹道优化
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     // 计算到最近敌人的方向
    //     var nearest = FindNearestBrick(ball.Position);
    //     if (nearest != null)
    //     {
    //         var targetDir = (nearest.Position - ball.Position).normalized;
    //         reflectDir = Vector3.Lerp(reflectDir, targetDir, 0.2f).normalized * ball.Speed;
    //     }
    // }

    public override ARelic makeCopy() => new TrajectoryGuide();
}
