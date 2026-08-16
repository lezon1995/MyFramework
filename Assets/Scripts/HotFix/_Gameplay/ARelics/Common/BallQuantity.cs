namespace MoreMountains;

/// <summary>
/// MultiBall - 多球
/// 额外获得1个球
/// </summary>
public class MultiBall : ARelic
{
    public static string ID = "MultiBall";

    public MultiBall() : base(ID, "MultiBall.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new MultiBall();
}

/// <summary>
/// BallSquad - 球小队
/// 额外获得2个球
/// </summary>
public class BallSquad : ARelic
{
    public static string ID = "BallSquad";

    public BallSquad() : base(ID, "BallSquad.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new BallSquad();
}

/// <summary>
/// BallSwarm - 球群
/// 额外获得3个球
/// </summary>
public class BallSwarm : ARelic
{
    public static string ID = "BallSwarm";

    public BallSwarm() : base(ID, "BallSwarm.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new BallSwarm();
}

/// <summary>
/// BallBarrage - 球弹幕
/// 额外获得4个球
/// </summary>
public class BallBarrage : ARelic
{
    public static string ID = "BallBarrage";

    public BallBarrage() : base(ID, "BallBarrage.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new BallBarrage();
}

/// <summary>
/// BallStorm - 球风暴
/// 额外获得5个球
/// </summary>
public class BallStorm : ARelic
{
    public static string ID = "BallStorm";

    public BallStorm() : base(ID, "BallStorm.png", RelicTier.SPECIAL, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new BallStorm();
}

/// <summary>
/// BallRain - 球雨
/// 每秒额外发射1个球
/// [设计文案] 持续不断的球流
/// </summary>
public class BallRain : ARelic
{
    public static string ID = "BallRain";

    public BallRain() : base(ID, "BallRain.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现自动发射球
    // private float timer = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     timer += dt;
    //     if (timer >= 1f)
    //     {
    //         timer = 0f;
    //         p.ShootExtraBall();
    //     }
    // }

    public override ARelic makeCopy() => new BallRain();
}

/// <summary>
/// BallFountain - 球喷泉
/// 球数量上限+1
/// [设计文案] 源源不断的球
/// </summary>
public class BallFountain : ARelic
{
    public static string ID = "BallFountain";

    public BallFountain() : base(ID, "BallFountain.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new BallFountain();
}

/// <summary>
/// BallSwarmController - 球群控制器
/// 球循环发射
/// [设计文案] 自动化的球群控制
/// </summary>
public class BallSwarmController : ARelic
{
    public static string ID = "BallSwarmController";

    public BallSwarmController() : base(ID, "BallSwarmController.png", RelicTier.SPECIAL, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现自动循环发射
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     // 循环发射所有球
    // }

    public override ARelic makeCopy() => new BallSwarmController();
}

/// <summary>
/// BallFactory - 球工厂
/// 每波次补充球数量
/// [设计文案] 工业化的球生产
/// </summary>
public class BallFactory : ARelic
{
    public static string ID = "BallFactory";

    public BallFactory() : base(ID, "BallFactory.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现波次补充球
    // public override void atBattleStart()
    // {
    //     owner.RestoreAllBalls();
    // }

    public override ARelic makeCopy() => new BallFactory();
}

/// <summary>
/// BallReplication - 球复制
/// 球有概率分裂
/// [设计文案] 指数级增长
/// </summary>
public class BallReplication : ARelic
{
    public static string ID = "BallReplication";

    public BallReplication() : base(ID, "BallReplication.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现球分裂
    // public override void onShootBall(Ball ball)
    // {
    //     if (Random.value < 0.1f)
    //     {
    //         var copy = ball.Clone();
    //     }
    // }

    public override ARelic makeCopy() => new BallReplication();
}
