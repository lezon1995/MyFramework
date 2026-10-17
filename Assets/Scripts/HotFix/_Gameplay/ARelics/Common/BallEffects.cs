using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 获得治疗时对最近的敌人造成(200+AP)%治疗量的魔法伤害
/// </summary>
public class BallOfFlames : ARelic
{
    public static string ID = "BallOfFlames";

    public BallOfFlames() : base(ID, "BallOfFlames.png", RelicTier.UNCOMMON, LandingSound.HEAVY) { }

    float rate;
    float duration;

    protected override void onRelicDefSet(RelicDef d)
    {
        rate = d.Args[0];
        duration = d.Args[1];
    }

    public override void onAfterHeal(APlayer p, Heal heal)
    {
        if (brickManager.getTheNearestActiveBrick(_player.getWorldPosition(), out var brick))
        {
            var f = rate + p.ap / 100F;
            var value = (int)(heal.ActualHealAmount * f);
            if (value > 0)
            {
                effectManager.addLogic<LaserLineEffect>().with(p, brick, value, duration);
            }
        }
    }

    public override ARelic makeCopy() => new BallOfFlames();
}

/// <summary>
/// IceBall - 冰霜球
/// 球命中时减速目标
/// [设计文案] 冰冷的球体，可以减缓敌人的移动速度
/// </summary>
public class IceBall : ARelic
{
    public static string ID = "IceBall";

    public IceBall() : base(ID, "IceBall.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现减速效果机制
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     brick.AddStatusEffect(StatusEffect.Slow, 0.3f, 3f);
    // }

    public override ARelic makeCopy() => new IceBall();
}

/// <summary>
/// ThunderBall - 雷电球
/// 球命中时造成眩晕
/// [设计文案] 带有雷电力量的球，可以短暂眩晕敌人
/// </summary>
public class ThunderBall : ARelic
{
    public static string ID = "ThunderBall";

    public ThunderBall() : base(ID, "ThunderBall.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现眩晕效果机制
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     brick.AddStatusEffect(StatusEffect.Stun, 1f, 0.5f);
    // }

    public override ARelic makeCopy() => new ThunderBall();
}

/// <summary>
/// PoisonBall - 毒球
/// 球命中时附加中毒
/// [设计文案] 剧毒的球体，中毒的敌人会持续掉血
/// </summary>
public class PoisonBall : ARelic
{
    public static string ID = "PoisonBall";

    public PoisonBall() : base(ID, "PoisonBall.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现中毒效果机制
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     brick.AddStatusEffect(StatusEffect.Poison, 5f, 3f);
    // }

    public override ARelic makeCopy() => new PoisonBall();
}

/// <summary>
/// LightningBall - 闪电球
/// 球命中时释放闪电链
/// [设计文案] 闪电在敌人之间跳跃，造成连锁伤害
/// </summary>
public class LightningBall : ARelic
{
    public static string ID = "LightningBall";

    public LightningBall() : base(ID, "LightningBall.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现闪电链效果
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     // 在附近敌人间释放闪电链
    //     // Damage = ball.Damage * 0.3, 跳跃3个敌人
    // }

    public override ARelic makeCopy() => new LightningBall();
}

/// <summary>
/// 你的激光类的球在撞击障碍和墙壁时也能触发撞击特效
/// </summary>
public class LaserBall : ARelic
{
    public static string ID = "LaserBall";

    public LaserBall() : base(ID, "LaserBall.png", RelicTier.RARE, LandingSound.SOLID) { }

    static bool isLaserType(Ball ball)
    {
        return ball.BallType
            is BallType.LaserBeam
            or BallType.LaserBeam_Crossed
            or BallType.LaserBeam_V
            or BallType.LaserBeam_H;
    }

    public override void onBallHitObstacle(APlayer p, Ball ball, Obstacle obstacle, ref Vector2 normal)
    {
        tryTrigger(ball);
    }

    public override void onBallHitBorder(APlayer p, Ball ball, Border border)
    {
        tryTrigger(ball);
    }

    static void tryTrigger(Ball ball)
    {
        if (isLaserType(ball))
        {
            var e = new DoHitEffect(ball);
            ball.Event.trigger(e);
        }
    }
}

/// <summary>
/// 你的随机激光会优先打击生命值最高的单位
/// </summary>
public class LaserGuidance : ARelic
{
    public static string ID = "LaserGuidance";

    public LaserGuidance() : base(ID, "LaserGuidance.png", RelicTier.RARE, LandingSound.SOLID) { }

    static bool isLaserType(Ball ball)
    {
        return ball.BallType is BallType.LaserBeam;
    }

    public override void onShootBall(APlayer p, Ball ball)
    {
        if (isLaserType(ball))
        {
            ball.addPower<BallRandomLaserGuidancePower>();
        }
    }
}

/// <summary>
/// ExplosiveBall - 爆炸球
/// 球命中时造成范围伤害
/// [设计文案] 爆炸性球体，命中时造成AOE伤害
/// </summary>
public class ExplosiveBall : ARelic
{
    public static string ID = "ExplosiveBall";

    public ExplosiveBall() : base(ID, "ExplosiveBall.png", RelicTier.UNCOMMON, LandingSound.HEAVY) { }

    // TODO: 需要实现爆炸范围伤害
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     // 在命中点造成爆炸范围伤害
    //     // Radius = 2, Damage = ball.Damage * 0.5
    // }

    public override ARelic makeCopy() => new ExplosiveBall();
}

/// <summary>
/// HomingBall - 追踪球
/// 球自动追踪最近目标
/// [设计文案] 智能球体，会自动追踪最近的敌人
/// </summary>
public class HomingBall : ARelic
{
    public static string ID = "HomingBall";

    public HomingBall() : base(ID, "HomingBall.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现追踪机制
    // public override void onShootBall(Ball ball)
    // {
    //     ball.SetHoming(true);
    //     ball.HomingStrength = 0.1f;
    // }

    public override ARelic makeCopy() => new HomingBall();
}

/// <summary>
/// RicochetBall - 弹跳球
/// 球额外反弹2次
/// [设计文案] 超级弹力球，可以额外反弹更多次
/// </summary>
public class RicochetBall : ARelic
{
    public static string ID = "RicochetBall";

    public RicochetBall() : base(ID, "RicochetBall.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现额外反弹次数
    // public override void onShootBall(Ball ball)
    // {
    //     ball.MaxBounceCount += 2;
    // }

    public override ARelic makeCopy() => new RicochetBall();
}

/// <summary>
/// SplittingBall - 分裂球
/// 球分裂成多个小球
/// [设计文案] 分裂球在飞行一定距离后分裂成多个小球
/// </summary>
public class SplittingBall : ARelic
{
    public static string ID = "SplittingBall";

    public SplittingBall() : base(ID, "SplittingBall.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现分裂机制
    // public override void onShootBall(Ball ball)
    // {
    //     ball.SetSplitting(true);
    //     ball.SplitCount = 3;
    // }

    public override ARelic makeCopy() => new SplittingBall();
}