namespace MoreMountains;

/// <summary>
/// SwiftBoots - 疾风之靴
/// 移速+15%
/// </summary>
public class SwiftBoots : ARelic
{
    public static string ID = "SwiftBoots";

    public SwiftBoots() : base(ID, "SwiftBoots.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new SwiftBoots();
}

/// <summary>
/// Haste - 加速
/// 攻速+20%
/// </summary>
public class Haste : ARelic
{
    public static string ID = "Haste";

    public Haste() : base(ID, "Haste.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new Haste();
}

/// <summary>
/// WindWalker - 风行者是
/// 移动不受惩罚
/// [设计文案] 自由穿行于战场
/// </summary>
public class WindWalker : ARelic
{
    public static string ID = "WindWalker";

    public WindWalker() : base(ID, "WindWalker.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现移动不受惩罚
    // public override void onEquip(APlayer p)
    // {
    //     p.IgnoreMovementPenalty = true;
    // }

    public override ARelic makeCopy() => new WindWalker();
}

/// <summary>
/// QuickDraw - 快速拔枪
/// 发射间隔-15%
/// [设计文案] 更快的发射节奏
/// </summary>
public class QuickDraw : ARelic
{
    public static string ID = "QuickDraw";

    public QuickDraw() : base(ID, "QuickDraw.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new QuickDraw();
}

/// <summary>
/// Sprint - 冲刺
/// 移速+25%
/// </summary>
public class Sprint : ARelic
{
    public static string ID = "Sprint";

    public Sprint() : base(ID, "Sprint.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new Sprint();
}

/// <summary>
/// LightningReflexes - 闪电反应
/// 攻速+10%，闪避+5%
/// </summary>
public class LightningReflexes : ARelic
{
    public static string ID = "LightningReflexes";

    public LightningReflexes() : base(ID, "LightningReflexes.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new LightningReflexes();
}

/// <summary>
/// WindCloak - 风之斗篷
/// 移速+10%，闪避+8%
/// </summary>
public class WindCloak : ARelic
{
    public static string ID = "WindCloak";

    public WindCloak() : base(ID, "WindCloak.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new WindCloak();
}

/// <summary>
/// TurboMode - 涡轮模式
/// 弹速+30%
/// </summary>
public class TurboMode : ARelic
{
    public static string ID = "TurboMode";

    public TurboMode() : base(ID, "TurboMode.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new TurboMode();
}

/// <summary>
/// Momentum - 动量
/// 移动增加弹速
/// [设计文案] 移动的惯性转化为速度
/// </summary>
public class Momentum : ARelic
{
    public static string ID = "Momentum";

    public Momentum() : base(ID, "Momentum.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现移动增加弹速
    // private float movementAccumulator = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (p.IsMoving)
    //     {
    //         movementAccumulator += p.MovementSpeed * dt * 0.01f;
    //         if (movementAccumulator >= 1f)
    //         {
    //             movementAccumulator = 0f;
    //             foreach (var ball in p.Balls)
    //             {
    //                 ball.GetStat(Ball.Stat.BallisticSpeed, out var stat);
    //                 stat.AddPct(0.05f);
    //             }
    //         }
    //     }
    // }

    public override ARelic makeCopy() => new Momentum();
}

/// <summary>
/// Afterburner - 加速器
/// 攻速逐渐加快
/// [设计文案] 持续战斗使攻速不断提升
/// </summary>
public class Afterburner : ARelic
{
    public static string ID = "Afterburner";

    public Afterburner() : base(ID, "Afterburner.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现攻速逐渐增加
    // private float attackSpeedBonus = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     attackSpeedBonus += 0.01f * dt;
    //     foreach (var ball in p.Balls)
    //     {
    //         ball.GetStat(Ball.Stat.AS, out var stat);
    //         stat.SetBasePct(attackSpeedBonus);
    //     }
    // }
    //
    // public override void atBattleStart()
    // {
    //     attackSpeedBonus = 0f;
    // }

    public override ARelic makeCopy() => new Afterburner();
}
