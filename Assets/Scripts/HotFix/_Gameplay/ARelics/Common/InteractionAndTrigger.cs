using UnityEngine;

namespace MoreMountains;

/// <summary>
/// ReactiveArmor - 反应护甲
/// 受伤时反弹伤害
/// [设计文案] 以牙还牙
/// </summary>
public class ReactiveArmor : ARelic
{
    public static string ID = "ReactiveArmor";

    public ReactiveArmor() : base(ID, "ReactiveArmor.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现受伤反弹
    // public override void onLoseHp(int damageAmount)
    // {
    //     // 对攻击来源造成反弹伤害
    // }

    public override ARelic makeCopy() => new ReactiveArmor();
}

/// <summary>
/// ThornedSkin - 荆棘皮肤
/// 近距离反弹伤害
/// [设计文案] 刺痛的防护
/// </summary>
public class ThornedSkin : ARelic
{
    public static string ID = "ThornedSkin";

    public ThornedSkin() : base(ID, "ThornedSkin.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现近距离反弹
    // public override void onLoseHp(int damageAmount)
    // {
    //     var nearestEnemy = FindNearestEnemy();
    //     if (nearestEnemy != null)
    //     {
    //         nearestEnemy.TakeDamage(damageAmount * 0.3f);
    //     }
    // }

    public override ARelic makeCopy() => new ThornedSkin();
}

/// <summary>
/// MagicMirror - 魔法镜
/// 反射敌方攻击
/// [设计文案] 镜像反击
/// </summary>
public class MagicMirror : ARelic
{
    public static string ID = "MagicMirror";

    public MagicMirror() : base(ID, "MagicMirror.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现攻击反射
    // public override int onAttacked(DamageInfo info, int damageAmount)
    // {
    //     info.Attacker.TakeDamage(damageAmount);
    //     return 0; // 完全反射
    // }

    public override ARelic makeCopy() => new MagicMirror();
}

/// <summary>
/// CounterStrike - 反击
/// 闪避后立即攻击
/// [设计文案] 敏捷的反击
/// </summary>
public class CounterStrike : ARelic
{
    public static string ID = "CounterStrike";

    public CounterStrike() : base(ID, "CounterStrike.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现闪避反击
    // public override void onDodge(APlayer p)
    // {
    //     var nearest = FindNearestEnemy();
    //     if (nearest != null)
    //     {
    //         p.Attack(nearest, p.AttackDamage * 0.5f);
    //     }
    // }

    public override ARelic makeCopy() => new CounterStrike();
}

/// <summary>
/// Riposte - 格挡反击
/// 完美闪避后造成双倍伤害
/// [设计文案] 精准的反击
/// </summary>
public class Riposte : ARelic
{
    public static string ID = "Riposte";

    public Riposte() : base(ID, "Riposte.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现完美闪避反击
    // public override void onPerfectDodge(APlayer p)
    // {
    //     var nearest = FindNearestEnemy();
    //     if (nearest != null)
    //     {
    //         p.Attack(nearest, p.AttackDamage * 2f);
    //     }
    // }

    public override ARelic makeCopy() => new Riposte();
}

/// <summary>
/// LuckyDodge - 幸运闪避
/// 闪避时触发暴击
/// [设计文案] 闪避带来好运
/// </summary>
public class LuckyDodge : ARelic
{
    public static string ID = "LuckyDodge";

    public LuckyDodge() : base(ID, "LuckyDodge.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现闪避暴击
    // public override void onDodge(APlayer p)
    // {
    //     foreach (var ball in p.Balls)
    //     {
    //         ball.GetStat(Ball.Stat.CritChance, out var stat);
    //         stat.AddFlat(1.0f); // 100%暴击
    //     }
    // }

    public override ARelic makeCopy() => new LuckyDodge();
}

/// <summary>
/// Opportunist - 机会主义者
/// 敌人露出破绽时伤害翻倍
/// [设计文案] 等待时机
/// </summary>
public class Opportunist : ARelic
{
    public static string ID = "Opportunist";

    public Opportunist() : base(ID, "Opportunist.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现破绽增伤
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (brick.IsVulnerable)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(1.0f);
    //     }
    // }

    public override ARelic makeCopy() => new Opportunist();
}

/// <summary>
/// FirstStrike - 先发制人
/// 战斗开始时伤害+30%
/// [设计文案] 先发制人
/// </summary>
public class FirstStrike : ARelic
{
    public static string ID = "FirstStrike";

    public FirstStrike() : base(ID, "FirstStrike.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现战斗开始增伤
    // private bool firstStrikeUsed = false;
    //
    // public override void atBattleStart()
    // {
    //     if (!firstStrikeUsed)
    //     {
    //         owner.BuffDamage(0.3f);
    //         firstStrikeUsed = true;
    //     }
    // }
    //
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     firstStrikeUsed = false;
    // }

    public override ARelic makeCopy() => new FirstStrike();
}

/// <summary>
/// LastStand - 背水一战
/// 生命低于30%时伤害翻倍
/// [设计文案] 置之死地
/// </summary>
public class LastStand : ARelic
{
    public static string ID = "LastStand";

    public LastStand() : base(ID, "LastStand.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现低血量增伤
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (p.HealthPercent < 0.3f)
    //     {
    //         p.DamageMultiplier = 2f;
    //     }
    //     else
    //     {
    //         p.DamageMultiplier = 1f;
    //     }
    // }

    public override ARelic makeCopy() => new LastStand();
}

/// <summary>
/// SecondWind - 第二呼吸
/// 生命低于50%时攻速翻倍
/// [设计文案] 绝境逢生
/// </summary>
public class SecondWind : ARelic
{
    public static string ID = "SecondWind";

    public SecondWind() : base(ID, "SecondWind.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现低血量攻速翻倍
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (p.HealthPercent < 0.5f)
    //     {
    //         foreach (var ball in p.Balls)
    //         {
    //             ball.GetStat(Ball.Stat.AS, out var stat);
    //             stat.AddPct(1.0f); // 翻倍
    //         }
    //     }
    // }

    public override ARelic makeCopy() => new SecondWind();
}
