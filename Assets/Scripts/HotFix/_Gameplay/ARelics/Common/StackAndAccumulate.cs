namespace MoreMountains;

/// <summary>
/// RageStack - 怒气叠加
/// 每击杀一个敌人伤害+2%（可叠加10层）
/// [设计文案] 杀戮积累愤怒
/// </summary>
public class RageStack : ARelic
{
    public static string ID = "RageStack";
    private int stackCount = 0;

    public RageStack() : base(ID, "RageStack.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现怒气叠加
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     if (stackCount < 10)
    //     {
    //         stackCount++;
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.02f);
    //     }
    // }
    //
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     stackCount = 0;
    // }

    public override ARelic makeCopy() => new RageStack();
}

/// <summary>
/// MomentumGain - 动量积累
/// 持续射击伤害递增
/// [设计文案] 越打越强
/// </summary>
public class MomentumGain : ARelic
{
    public static string ID = "MomentumGain";

    public MomentumGain() : base(ID, "MomentumGain.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现持续射击增伤
    // private int consecutiveHits = 0;
    //
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     consecutiveHits++;
    //     var bonus = consecutiveHits * 0.01f;
    //     ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //     stat.AddPct(bonus);
    // }
    //
    // public override void onBallReturn(APlayer p)
    // {
    //     consecutiveHits = 0;
    // }

    public override ARelic makeCopy() => new MomentumGain();
}

/// <summary>
/// WarriorSpirit - 战士之魂
/// 每波次击杀增加属性
/// [设计文案] 战斗的荣耀
/// </summary>
public class WarriorSpirit : ARelic
{
    public static string ID = "WarriorSpirit";

    public WarriorSpirit() : base(ID, "WarriorSpirit.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现波次击杀属性
    // private int waveKills = 0;
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     waveKills++;
    //     if (waveKills % 5 == 0)
    //     {
    //         p.AddStatBonus(Stats.AD, 1);
    //     }
    // }
    //
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     waveKills = 0;
    // }

    public override ARelic makeCopy() => new WarriorSpirit();
}

/// <summary>
/// BeastMode - 野兽模式
/// 血量越低属性越高
/// [设计文案] 野兽的觉醒
/// </summary>
public class BeastMode : ARelic
{
    public static string ID = "BeastMode";

    public BeastMode() : base(ID, "BeastMode.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现低血量属性增益
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     var healthPercent = p.HealthPercent;
    //     var bonus = (1f - healthPercent) * 0.5f; // 血量越低加成越高
    //     p.DamageMultiplier = 1f + bonus;
    //     p.SpeedMultiplier = 1f + bonus;
    // }

    public override ARelic makeCopy() => new BeastMode();
}

/// <summary>
/// RisingPower - 上升之力
/// 战斗越久伤害越高
/// [设计文案] 持久战的强者
/// </summary>
public class RisingPower : ARelic
{
    public static string ID = "RisingPower";

    public RisingPower() : base(ID, "RisingPower.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现战斗时间累积伤害
    // private float battleTime = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     battleTime += dt;
    //     var bonus = battleTime * 0.001f; // 每秒+0.1%
    //     foreach (var ball in p.Balls)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(bonus);
    //     }
    // }
    //
    // public override void atBattleStart()
    // {
    //     battleTime = 0f;
    // }

    public override ARelic makeCopy() => new RisingPower();
}

/// <summary>
/// EscalatingForce - 升级力量
/// 每次命中增加伤害
/// [设计文案] 积累的力量
/// </summary>
public class EscalatingForce : ARelic
{
    public static string ID = "EscalatingForce";

    public EscalatingForce() : base(ID, "EscalatingForce.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现命中累积伤害
    // private int totalHits = 0;
    //
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     totalHits++;
    //     var bonus = totalHits * 0.005f; // 每次命中+0.5%
    //     ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //     stat.AddPct(bonus);
    // }

    public override ARelic makeCopy() => new EscalatingForce();
}

/// <summary>
/// InfiniteGrowth - 无限成长
/// 属性持续增长
/// [设计文案] 永无止境
/// </summary>
public class InfiniteGrowth : ARelic
{
    public static string ID = "InfiniteGrowth";

    public InfiniteGrowth() : base(ID, "InfiniteGrowth.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现持续属性增长
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     p.AddStatBonus(Stats.AD, 0.001f * dt);
    //     p.AddStatBonus(Stats.AS, 0.001f * dt);
    // }

    public override ARelic makeCopy() => new InfiniteGrowth();
}

/// <summary>
/// Overdrive - 过载
/// 持续战斗获得增益
/// [设计文案] 超频模式
/// </summary>
public class Overdrive : ARelic
{
    public static string ID = "Overdrive";

    public Overdrive() : base(ID, "Overdrive.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现战斗增益
    // private float combatTime = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     combatTime += dt;
    //     if (combatTime > 10f)
    //     {
    //         p.BuffAllStats(0.05f);
    //     }
    // }

    public override ARelic makeCopy() => new Overdrive();
}

/// <summary>
/// BattleRage - 战斗狂热
/// 战斗中获得攻速
/// [设计文案] 战斗的狂热
/// </summary>
public class BattleRage : ARelic
{
    public static string ID = "BattleRage";

    public BattleRage() : base(ID, "BattleRage.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现战斗攻速增益
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     ball.GetStat(Ball.Stat.AS, out var stat);
    //     stat.AddPct(0.02f);
    // }

    public override ARelic makeCopy() => new BattleRage();
}

/// <summary>
/// BloodPact - 血之契约
/// 消耗生命换取力量
/// [设计文案] 鲜血的代价
/// </summary>
public class BloodPact : ARelic
{
    public static string ID = "BloodPact";

    public BloodPact() : base(ID, "BloodPact.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现消耗生命获得力量
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     p.TakeDamage(1f * dt); // 每秒消耗1点生命
    //     p.BuffDamage(0.01f * dt); // 获得等效伤害增益
    // }

    public override ARelic makeCopy() => new BloodPact();
}
