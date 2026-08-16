using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// ElasticRacket - 弹性球拍
/// 每次反弹伤害+3%
/// </summary>
public class ElasticRacket : ARelic
{
    public static string ID = "ElasticRacket";

    public ElasticRacket() : base(ID, "ElasticRacket.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
        stat.AddPct(0.03f);
    }

    public override ARelic makeCopy() => new ElasticRacket();
}

/// <summary>
/// Trampoline - 蹦床
/// 球从底部反弹时伤害翻倍
/// [设计文案] 神奇的蹦床，底部反弹时力量倍增
/// </summary>
public class Trampoline : ARelic
{
    public static string ID = "Trampoline";

    public Trampoline() : base(ID, "Trampoline.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现底部反弹伤害翻倍
    // public override void onBallHitBorderBot(APlayer p, Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn)
    // {
    //     ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //     stat.AddPct(1.0f); // 翻倍
    // }

    public override ARelic makeCopy() => new Trampoline();
}

/// <summary>
/// BouncyWall - 弹力墙
/// 球反弹时弹速+8%
/// </summary>
public class BouncyWall : ARelic
{
    public static string ID = "BouncyWall";

    public BouncyWall() : base(ID, "BouncyWall.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        ball.GetStat(Ball.Stat.BallisticSpeed, out var stat);
        stat.AddPct(0.08f);
    }

    public override ARelic makeCopy() => new BouncyWall();
}

/// <summary>
/// PinballWizard - 弹珠达人
/// 连续反弹超过3次时触发暴击
/// [设计文案] 弹珠游戏大师，连续反弹蓄积力量
/// </summary>
public class PinballWizard : ARelic
{
    public static string ID = "PinballWizard";

    public PinballWizard() : base(ID, "PinballWizard.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现连续反弹计数和暴击触发
    // private int consecutiveBounces = 0;
    //
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     consecutiveBounces++;
    //     if (consecutiveBounces >= 3)
    //     {
    //         ball.GetStat(Ball.Stat.CritChance, out var critStat);
    //         critStat.AddFlat(1.0f); // 100%暴击
    //     }
    // }
    //
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     consecutiveBounces = 0;
    // }

    public override ARelic makeCopy() => new PinballWizard();
}

/// <summary>
/// FlipperMaster - 翻转大师
/// 球反弹角度更精准
/// [设计文案] 翻转大师的技术，优化反弹角度
/// </summary>
public class FlipperMaster : ARelic
{
    public static string ID = "FlipperMaster";

    public FlipperMaster() : base(ID, "FlipperMaster.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现优化反弹角度的机制
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     // 优化反弹方向，使其更垂直于墙壁
    //     reflectDir = Vector2.Reflect(ball.Direction, normal);
    //     reflectDir = reflectDir.normalized * ball.Speed;
    // }

    public override ARelic makeCopy() => new FlipperMaster();
}

/// <summary>
/// ChaosMirror - 混乱之镜
/// 球反弹方向随机化
/// [设计文案] 混乱的镜子，打破常规的反弹规律
/// </summary>
public class ChaosMirror : ARelic
{
    public static string ID = "ChaosMirror";

    public ChaosMirror() : base(ID, "ChaosMirror.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现随机化反弹方向
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     float randomAngle = Random.Range(-45f, 45f);
    //     reflectDir = reflectDir.Rotate(randomAngle);
    // }

    public override ARelic makeCopy() => new ChaosMirror();
}

/// <summary>
/// PrecisionAngle - 精准角度
/// 反弹方向优化
/// [设计文案] 精确计算最佳反弹角度
/// </summary>
public class PrecisionAngle : ARelic
{
    public static string ID = "PrecisionAngle";

    public PrecisionAngle() : base(ID, "PrecisionAngle.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现优化反弹方向
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     // 找到最近的目标方向
    //     var nearestBrick = FindNearestBrick(ball.Position);
    //     if (nearestBrick != null)
    //     {
    //         var toTarget = (nearestBrick.Position - ball.Position).normalized;
    //         reflectDir = Vector2.Lerp(reflectDir, toTarget, 0.3f);
    //     }
    // }

    public override ARelic makeCopy() => new PrecisionAngle();
}

/// <summary>
/// WallRunner - 攀墙者
/// 球沿墙移动时速度不减
/// [设计文案] 攀墙者的技巧，让球在墙上如履平地
/// </summary>
public class WallRunner : ARelic
{
    public static string ID = "WallRunner";

    public WallRunner() : base(ID, "WallRunner.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现沿墙速度不减的机制
    // public override void onBallHitBorderLeft(APlayer p, Ball ball, BorderLeft border, ref Vector2 normal)
    // {
    //     ball.MaintainSpeedOnWall = true;
    // }

    public override ARelic makeCopy() => new WallRunner();
}

/// <summary>
/// ReflectionMaster - 反射大师
/// 反弹伤害+10%
/// </summary>
public class ReflectionMaster : ARelic
{
    public static string ID = "ReflectionMaster";

    public ReflectionMaster() : base(ID, "ReflectionMaster.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
        stat.AddPct(0.10f);
    }

    public override ARelic makeCopy() => new ReflectionMaster();
}

/// <summary>
/// RubberBand - 橡皮筋
/// 球反弹时额外获得一次穿透
/// [设计文案] 橡皮筋的弹性，给予球额外的穿透力
/// </summary>
public class RubberBand : ARelic
{
    public static string ID = "RubberBand";

    public RubberBand() : base(ID, "RubberBand.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现反弹时增加穿透
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     ball.AddPenetration(1);
    // }

    public override ARelic makeCopy() => new RubberBand();
}
