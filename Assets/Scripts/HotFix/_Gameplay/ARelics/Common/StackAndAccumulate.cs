using MoreMountains.Tools;
using UniStats;
using UnityEngine;

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

    public RageStack() : base(ID, "RageStack.png", RelicTier.RARE, LandingSound.HEAVY) { }

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

    public MomentumGain() : base(ID, "MomentumGain.png", RelicTier.RARE, LandingSound.SOLID) { }

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

    public WarriorSpirit() : base(ID, "WarriorSpirit.png", RelicTier.RARE, LandingSound.HEAVY) { }

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
/// 技能暴击会造成小型爆炸效果
/// </summary>
public class BeastMode : ARelic
{
    public static string ID = "BeastMode";

    public BeastMode() : base(ID, "BeastMode.png", RelicTier.RARE, LandingSound.HEAVY) { }

    public override void onBallSkillCrit(APlayer p, Ball ball, Brick brick)
    {
        effectManager.addLogic<CritSkillExplodeEffect>().with(ball, brick);
    }

    public override ARelic makeCopy() => new BeastMode();
}

/// <summary>
/// 球碰到玩家则会重置持续时间。
/// </summary>
public class RisingPower : ARelic
{
    public static string ID = "RisingPower";

    public RisingPower() : base(ID, "RisingPower.png", RelicTier.RARE, LandingSound.SOLID) { }

    public override void onShootBall(APlayer p, Ball ball)
    {
        ball.BounceLayers |= LayerManager.Player_Mask;
    }

    public override void onBallEnterPlayer(APlayer p, Ball ball, APlayer p2, ref Vector2 normal, ref bool needReflect)
    {
        effectManager.addLogic<RefreshBallDurationEffect>().with(ball);
    }
}

/// <summary>
/// EscalatingForce - 升级力量
/// 每次命中增加伤害
/// [设计文案] 积累的力量
/// </summary>
public class EscalatingForce : ARelic
{
    public static string ID = "EscalatingForce";

    public EscalatingForce() : base(ID, "EscalatingForce.png", RelicTier.RARE, LandingSound.SOLID) { }

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

    public InfiniteGrowth() : base(ID, "InfiniteGrowth.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

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

    public Overdrive() : base(ID, "Overdrive.png", RelicTier.RARE, LandingSound.HEAVY) { }

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
/// 撞击暴击会造成小型爆炸效果
/// </summary>
public class BattleRage : ARelic
{
    public static string ID = "BattleRage";

    public BattleRage() : base(ID, "BattleRage.png", RelicTier.RARE, LandingSound.HEAVY) { }

    public override void onBallImpactCrit(APlayer p, Ball ball, Brick brick, in Dmg dmg)
    {
        effectManager.addLogic<CritImpactExplodeEffect>().with(ball, brick);
    }

    public override ARelic makeCopy() => new BattleRage();
}

/// <summary>
/// 每损失1%的最大生命值，球速+2%
/// </summary>
public class BloodPact : ARelic
{
    public static string ID = "BloodPact";

    public BloodPact() : base(ID, "BloodPact.png", RelicTier.SPECIAL, LandingSound.HEAVY) { }

    float hpPerLost;
    float speedPerHpLost;
    NumMod<float> modMS;

    protected override void onRelicDefSet(RelicDef d)
    {
        hpPerLost = d.Args[0];
        speedPerHpLost = d.Args[1];
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
        p.GetStat(Character.Stat.BallisticSpeed, out var ms);
        var lostHealthPct = (1F - p.currentHealthPct).clamp01();
        var share = (lostHealthPct / hpPerLost).round();
        modMS = ms.BonusFlat.AddFlat(speedPerHpLost * share);
    }

    public override void onPlayerHealthChanged(APlayer p, int curHealth)
    {
        var lostHealthPct = (1F - p.currentHealthPct).clamp01();
        var share = (lostHealthPct / hpPerLost).round();
        modMS.Value = speedPerHpLost * share;
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
        p.GetStat(Character.Stat.BallisticSpeed, out var ms);
        ms.BonusFlat.RemoveMod(modMS);
        modMS = null;
    }
}

/// <summary>
/// 每损失1%的最大生命值，球存活时间的流逝速度-1%
/// </summary>
public class BloodLife : ARelic
{
    public static string ID = "BloodLife";

    public BloodLife() : base(ID, "BloodLife.png", RelicTier.SPECIAL, LandingSound.HEAVY) { }

    float hpPerLost;
    float speedPerHpLost;

    protected override void onRelicDefSet(RelicDef d)
    {
        hpPerLost = d.Args[0];
        speedPerHpLost = d.Args[1];
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
        var lostHealthPct = (1F - p.currentHealthPct).clamp01();
        var share = (lostHealthPct / hpPerLost).round();
        p.ballDurationElapsedSpeed.setExtra(-speedPerHpLost * share);
    }

    public override void onPlayerHealthChanged(APlayer p, int curHealth)
    {
        var lostHealthPct = (1F - p.currentHealthPct).clamp01();
        var share = (lostHealthPct / hpPerLost).round();
        p.ballDurationElapsedSpeed.setExtra(-speedPerHpLost * share);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
        p.ballDurationElapsedSpeed.setExtra(0);
    }
}