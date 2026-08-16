namespace MoreMountains;

/// <summary>
/// PrismSphere - 棱镜球
/// 球发出彩虹光
/// [设计文案] 美丽的光学效果
/// </summary>
public class PrismSphere : ARelic
{
    public static string ID = "PrismSphere";

    public PrismSphere() : base(ID, "PrismSphere.png", RelicTier.COMMON, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现彩虹光效果
    // public override void onShootBall(Ball ball)
    // {
    //     ball.AddEffect(BallEffect.Rainbow);
    // }

    public override ARelic makeCopy() => new PrismSphere();
}

/// <summary>
/// CrystalBall - 水晶球
/// 显示未来事件
/// [设计文案] 预知未来
/// </summary>
public class CrystalBall : ARelic
{
    public static string ID = "CrystalBall";

    public CrystalBall() : base(ID, "CrystalBall.png", RelicTier.UNCOMMON, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现预测功能
    // public override void onEquip(APlayer p)
    // {
    //     p.ShowDangerousBricks = true;
    // }

    public override ARelic makeCopy() => new CrystalBall();
}

/// <summary>
/// MagicLantern - 魔法灯笼
/// 照亮隐藏区域
/// [设计文案] 照亮黑暗
/// </summary>
public class MagicLantern : ARelic
{
    public static string ID = "MagicLantern";

    public MagicLantern() : base(ID, "MagicLantern.png", RelicTier.UNCOMMON, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现照亮功能
    // public override void onEquip(APlayer p)
    // {
    //     p.LightRadius *= 2f;
    // }

    public override ARelic makeCopy() => new MagicLantern();
}

/// <summary>
/// MirrorBall - 镜球
/// 产生闪光特效
/// [设计文案] 闪闪发光
/// </summary>
public class MirrorBall : ARelic
{
    public static string ID = "MirrorBall";

    public MirrorBall() : base(ID, "MirrorBall.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现闪光特效
    // public override void onShootBall(Ball ball)
    // {
    //     ball.AddEffect(BallEffect.Glitter);
    // }

    public override ARelic makeCopy() => new MirrorBall();
}

/// <summary>
/// DiscoBall - 迪斯科球
/// 音乐节拍增强
/// [设计文案] 派对时间
/// </summary>
public class DiscoBall : ARelic
{
    public static string ID = "DiscoBall";

    public DiscoBall() : base(ID, "DiscoBall.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现节拍增强
    // public override void onEquip(APlayer p)
    // {
    //     p.BeatSync = true;
    //     p.BeatDamageBonus = 0.2f;
    // }

    public override ARelic makeCopy() => new DiscoBall();
}

/// <summary>
/// AuroraSphere - 极光球
/// 美丽极光效果
/// [设计文案] 北极之光
/// </summary>
public class AuroraSphere : ARelic
{
    public static string ID = "AuroraSphere";

    public AuroraSphere() : base(ID, "AuroraSphere.png", RelicTier.UNCOMMON, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现极光效果
    // public override void onShootBall(Ball ball)
    // {
    //     ball.AddEffect(BallEffect.Aurora);
    // }

    public override ARelic makeCopy() => new AuroraSphere();
}

/// <summary>
/// StarDust - 星尘
/// 留下星光轨迹
/// [设计文案] 星光之路
/// </summary>
public class StarDust : ARelic
{
    public static string ID = "StarDust";

    public StarDust() : base(ID, "StarDust.png", RelicTier.COMMON, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现星光轨迹
    // public override void onShootBall(Ball ball)
    // {
    //     ball.AddEffect(BallEffect.StarTrail);
    // }

    public override ARelic makeCopy() => new StarDust();
}

/// <summary>
/// RainbowTrail - 彩虹尾迹
/// 球带有彩虹
/// [设计文案] 彩虹之路
/// </summary>
public class RainbowTrail : ARelic
{
    public static string ID = "RainbowTrail";

    public RainbowTrail() : base(ID, "RainbowTrail.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现彩虹尾迹
    // public override void onShootBall(Ball ball)
    // {
    //     ball.AddEffect(BallEffect.RainbowTrail);
    // }

    public override ARelic makeCopy() => new RainbowTrail();
}

/// <summary>
/// SparkleEffect - 闪亮特效
/// 华丽的粒子效果
/// [设计文案] 闪闪发光
/// </summary>
public class SparkleEffect : ARelic
{
    public static string ID = "SparkleEffect";

    public SparkleEffect() : base(ID, "SparkleEffect.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现闪亮粒子
    // public override void onShootBall(Ball ball)
    // {
    //     ball.AddEffect(BallEffect.Sparkle);
    // }

    public override ARelic makeCopy() => new SparkleEffect();
}

/// <summary>
/// FireworkBall - 烟花球
/// 爆炸时产生烟花
/// [设计文案] 庆祝时刻
/// </summary>
public class FireworkBall : ARelic
{
    public static string ID = "FireworkBall";

    public FireworkBall() : base(ID, "FireworkBall.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现烟花效果
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     SpawnFirework(brick.Position);
    // }

    public override ARelic makeCopy() => new FireworkBall();
}
