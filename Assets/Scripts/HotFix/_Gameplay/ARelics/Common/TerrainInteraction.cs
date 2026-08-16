namespace MoreMountains;

/// <summary>
/// WallCrawler - 爬墙者
/// 球可以攀爬边界
/// [设计文案] 墙壁漫步者
/// </summary>
public class WallCrawler : ARelic
{
    public static string ID = "WallCrawler";

    public WallCrawler() : base(ID, "WallCrawler.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现攀爬边界
    // public override void onBallHitBorderLeft(APlayer p, Ball ball, BorderLeft border, ref Vector2 normal)
    // {
    //     ball.SetWallClimbing(true);
    //     ball.Direction = Vector2.down; // 沿着墙壁向下移动
    // }

    public override ARelic makeCopy() => new WallCrawler();
}

/// <summary>
/// BouncePad - 弹跳垫
/// 特定区域弹速增加
/// [设计文案] 弹跳加速区域
/// </summary>
public class BouncePad : ARelic
{
    public static string ID = "BouncePad";

    public BouncePad() : base(ID, "BouncePad.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现特定区域加速
    // public override void onBallEnterZone(APlayer p, Ball ball, Zone zone)
    // {
    //     if (zone.Type == ZoneType.BouncePad)
    //     {
    //         ball.GetStat(Ball.Stat.BallisticSpeed, out var stat);
    //         stat.AddPct(0.5f);
    //     }
    // }

    public override ARelic makeCopy() => new BouncePad();
}

/// <summary>
/// PortalWand - 传送棒
/// 球可以传送
/// [设计文案] 空间的魔法
/// </summary>
public class PortalWand : ARelic
{
    public static string ID = "PortalWand";

    public PortalWand() : base(ID, "PortalWand.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现传送功能
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (Random.value < 0.1f)
    //     {
    //         ball.TeleportToRandomPosition();
    //     }
    // }

    public override ARelic makeCopy() => new PortalWand();
}

/// <summary>
/// MagnetBall - 磁力球
/// 球吸引附近敌人
/// [设计文案] 磁力的吸引
/// </summary>
public class MagnetBall : ARelic
{
    public static string ID = "MagnetBall";

    public MagnetBall() : base(ID, "MagnetBall.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现吸引敌人
    // public override void onShootBall(Ball ball)
    // {
    //     ball.SetMagnet(true);
    //     ball.MagnetStrength = 2f;
    // }

    public override ARelic makeCopy() => new MagnetBall();
}

/// <summary>
/// GravityBoots - 重力靴
/// 可以改变重力方向
/// [设计文案] 重力操控
/// </summary>
public class GravityBoots : ARelic
{
    public static string ID = "GravityBoots";

    public GravityBoots() : base(ID, "GravityBoots.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现重力方向改变
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (Input.GetKeyDown(KeyCode.G))
    //     {
    //         p.GravityDirection = p.GravityDirection.Rotate(90f);
    //     }
    // }

    public override ARelic makeCopy() => new GravityBoots();
}

/// <summary>
/// TeleportBeacon - 传送信标
/// 球可以瞬移
/// [设计文案] 瞬间移动
/// </summary>
public class TeleportBeacon : ARelic
{
    public static string ID = "TeleportBeacon";

    public TeleportBeacon() : base(ID, "TeleportBeacon.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现瞬移功能
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (Random.value < 0.15f)
    //     {
    //         ball.TeleportToNearestEnemy();
    //     }
    // }

    public override ARelic makeCopy() => new TeleportBeacon();
}

/// <summary>
/// Wormhole - 虫洞
/// 球可以从任意位置出现
/// [设计文案] 时空虫洞
/// </summary>
public class Wormhole : ARelic
{
    public static string ID = "Wormhole";

    public Wormhole() : base(ID, "Wormhole.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现虫洞传送
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     if (Random.value < 0.2f)
    //     {
    //         ball.transform.position = GetRandomScreenPosition();
    //     }
    // }

    public override ARelic makeCopy() => new Wormhole();
}

/// <summary>
/// BouncyCastle - 蹦床城堡
/// 所有反弹伤害翻倍
/// [设计文案] 弹跳的乐园
/// </summary>
public class BouncyCastle : ARelic
{
    public static string ID = "BouncyCastle";

    public BouncyCastle() : base(ID, "BouncyCastle.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现反弹伤害翻倍
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //     stat.AddPct(1.0f);
    // }

    public override ARelic makeCopy() => new BouncyCastle();
}

/// <summary>
/// TrampolinePark - 蹦床公园
/// 反弹次数+3
/// [设计文案] 无限弹跳
/// </summary>
public class TrampolinePark : ARelic
{
    public static string ID = "TrampolinePark";

    public TrampolinePark() : base(ID, "TrampolinePark.png", RelicTier.SPECIAL, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现反弹次数增加
    // public override void onEquip(APlayer p)
    // {
    //     foreach (var ball in p.Balls)
    //     {
    //         ball.MaxBounceCount += 3;
    //     }
    // }

    public override ARelic makeCopy() => new TrampolinePark();
}

/// <summary>
/// WallJumpMaster - 墙壁跳跃大师
/// 反弹伤害递增
/// [设计文案] 墙壁的舞者
/// </summary>
public class WallJumpMaster : ARelic
{
    public static string ID = "WallJumpMaster";

    public WallJumpMaster() : base(ID, "WallJumpMaster.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现反弹伤害递增
    // private int bounceCount = 0;
    //
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     bounceCount++;
    //     ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //     stat.AddPct(0.05f * bounceCount);
    // }
    //
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     bounceCount = 0;
    // }

    public override ARelic makeCopy() => new WallJumpMaster();
}
