using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 结算撞击伤害后，如果生命值低于10%则处决敌人，并额外获得2金币
/// </summary>
public class Executioner : ARelic
{
    public static string ID = "Executioner";

    public Executioner() : base(ID, "Executioner.png", RelicTier.RARE, LandingSound.HEAVY) { }

    float executionPct;
    int bonusCoin;

    protected override void onRelicDefSet(RelicDef d)
    {
        executionPct = d.Args[0];
        bonusCoin = (int)d.Args[1];
    }

    public override void onBallCauseImpactDamage(APlayer p, Ball ball, Brick brick, in Dmg dmg)
    {
        if (!dmg.IsLethal && brick.currentHealthPct <= executionPct)
        {
            brick.kill(p);
            p.gainGold(bonusCoin);
            var executeDmg = Dmg.True(999);
            new DmgTextEvent(executeDmg, brick.getTransform()).trigger();
            new GainCoinTextEvent(bonusCoin, brick.getTransform()).trigger();
        }
    }

    public override ARelic makeCopy() => new Executioner();
}

/// <summary>
/// FirstBlood - 先手必杀
/// 每波次第一个击杀获得双倍奖励
/// [设计文案] 第一个击杀的奖励翻倍
/// </summary>
public class FirstBlood : ARelic
{
    public static string ID = "FirstBlood";

    public FirstBlood() : base(ID, "FirstBlood.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现首杀奖励翻倍
    // private bool firstKillThisWave = true;
    //
    // public override void atBattleStart()
    // {
    //     firstKillThisWave = true;
    // }
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     if (firstKillThisWave)
    //     {
    //         firstKillThisWave = false;
    //         // 奖励翻倍：经验+100%, 金币+100%
    //     }
    // }

    public override ARelic makeCopy() => new FirstBlood();
}

/// <summary>
/// 发球时会额外发射一个球，但是球的伤害率-30%
/// </summary>
public class ComboMaster : ARelic
{
    public static string ID = "ComboMaster";
    Func<SpawnState, Vector3, int, int, SpawnState> getter;

    public ComboMaster() : base(ID, "ComboMaster.png", RelicTier.RARE, LandingSound.SOLID)
    {
        getter = SpawnStateGetter;
    }

    float dmgRateDecrement;

    protected override void onRelicDefSet(RelicDef d)
    {
        dmgRateDecrement = d.Args[0];
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
        foreach (var handleWeapon in p.mainHandleWeapons)
        {
            handleWeapon.ProjectilesPerShot = (handleWeapon.ProjectilesPerShot + 1).clamp(1, 3);
            handleWeapon.SpawnStateGetter = getter;

            if (handleWeapon.CurrentWeapon is ProjectileWeapon weapon)
            {
                weapon.ProjectilesPerShot = (weapon.ProjectilesPerShot + 1).clamp(1, 3);
                weapon.SpawnStateGetter = getter;
            }
        }
    }

    public override void onBallGetImpactDmg(APlayer p, Ball ball, ref Dmg dmg)
    {
        dmg.subDmgRate(dmgRateDecrement);
    }

    public override void onBallGetSkillDmg(APlayer p, Ball ball, ref Dmg dmg)
    {
        dmg.subDmgRate(dmgRateDecrement);
    }

    SpawnState SpawnStateGetter(SpawnState state, Vector3 direction, int index, int totalCount)
    {
        var originPos = state.Position;
        var left = direction.rotate(Quaternion.AngleAxis(90, Vector3.forward));
        var right = direction.rotate(Quaternion.AngleAxis(-90, Vector3.forward));
        var leftPos = originPos + left * 0.675F / 2F;
        var rightPos = originPos + right * 0.675F / 2F;
        switch (totalCount)
        {
            case 1:
                return new(originPos, direction);
            case 2:
                switch (index)
                {
                    case 0: return new(leftPos, direction);
                    case 1: return new(rightPos, direction);
                }

                break;
            case 3:
                switch (index)
                {
                    case 0: return new(leftPos, direction);
                    case 1: return new(originPos, direction);
                    case 2: return new(rightPos, direction);
                }

                break;
        }

        return new(originPos, direction);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);

        foreach (var handleWeapon in p.mainHandleWeapons)
        {
            handleWeapon.ProjectilesPerShot = (handleWeapon.ProjectilesPerShot - 1).clamp(1, 3);
            if (handleWeapon.ProjectilesPerShot == 1)
            {
                handleWeapon.SpawnStateGetter = null;
            }

            if (handleWeapon.CurrentWeapon is ProjectileWeapon weapon)
            {
                weapon.ProjectilesPerShot = (weapon.ProjectilesPerShot - 1).clamp(1, 3);
                if (weapon.ProjectilesPerShot == 1)
                {
                    weapon.SpawnStateGetter = null;
                }
            }
        }
    }

    // TODO: 需要实现连击计数和增伤
    // private int comboCount = 0;
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     comboCount++;
    //     if (comboCount >= 10)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.2f);
    //     }
    // }
    //
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     comboCount = 0;
    // }

    public override ARelic makeCopy() => new ComboMaster();
}

/// <summary>
/// Massacre - 大屠杀
/// 快速击杀5个敌人后获得短暂无敌
/// [设计文案] 杀戮的狂潮，击杀后获得保护
/// </summary>
public class Massacre : ARelic
{
    public static string ID = "Massacre";

    public Massacre() : base(ID, "Massacre.png", RelicTier.RARE, LandingSound.HEAVY) { }

    // TODO: 需要实现快速击杀后无敌
    // private int killCount = 0;
    // private bool isInvincible = false;
    // private float invincibleTimer = 0f;
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     killCount++;
    //     if (killCount >= 5 && !isInvincible)
    //     {
    //         isInvincible = true;
    //         invincibleTimer = 2f; // 无敌2秒
    //         killCount = 0;
    //     }
    // }
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (isInvincible)
    //     {
    //         invincibleTimer -= dt;
    //         if (invincibleTimer <= 0f)
    //             isInvincible = false;
    //     }
    // }

    public override ARelic makeCopy() => new Massacre();
}

/// <summary>
/// LethalPrecision - 致命精准
/// 暴击必定击杀普通敌人
/// [设计文案] 一击必杀的艺术
/// </summary>
public class LethalPrecision : ARelic
{
    public static string ID = "LethalPrecision";

    public LethalPrecision() : base(ID, "LethalPrecision.png", RelicTier.RARE, LandingSound.SOLID) { }

    // TODO: 需要实现暴击必杀机制
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (ball.IsCriticalHit && brick.IsNormalEnemy)
    //     {
    //         brick.CurrentHealth = 0; // 直接击杀
    //     }
    // }

    public override ARelic makeCopy() => new LethalPrecision();
}

/// <summary>
/// Slayer - 杀手
/// 击杀精英敌人后伤害+15%（持续至波次结束）
/// [设计文案] 精英猎手，获得击杀奖励
/// </summary>
public class Slayer : ARelic
{
    public static string ID = "Slayer";

    public Slayer() : base(ID, "Slayer.png", RelicTier.RARE, LandingSound.HEAVY) { }

    // TODO: 需要实现精英击杀增伤
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     if (brick.IsElite)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.15f);
    //     }
    // }
    //
    // public override void atBattleStart()
    // {
    //     // 清除之前的增伤效果
    // }

    public override ARelic makeCopy() => new Slayer();
}

/// <summary>
/// Destroyer - 毁灭者
/// 击杀敌人时产生小爆炸
/// [设计文案] 毁灭的力量，击杀引发爆炸
/// </summary>
public class Destroyer : ARelic
{
    public static string ID = "Destroyer";

    public Destroyer() : base(ID, "Destroyer.png", RelicTier.UNCOMMON, LandingSound.HEAVY) { }

    // TODO: 需要实现击杀爆炸
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     // 在砖块位置产生小爆炸
    //     // Radius = 1, Damage = ball.Damage * 0.3
    // }

    public override ARelic makeCopy() => new Destroyer();
}

/// <summary>
/// Reaper - 收割者
/// 每击杀10个敌人恢复5%最大生命
/// [设计文案] 生命的收割者，用敌人的生命治愈自己
/// </summary>
public class Reaper : ARelic
{
    public static string ID = "Reaper";

    public Reaper() : base(ID, "Reaper.png", RelicTier.RARE, LandingSound.SOLID) { }

    // TODO: 需要实现击杀回血
    // private int killCount = 0;
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     killCount++;
    //     if (killCount >= 10)
    //     {
    //         killCount = 0;
    //         p.Heal(p.MaxHealth * 0.05f);
    //     }
    // }

    public override ARelic makeCopy() => new Reaper();
}

/// <summary>
/// Hunter - 猎人
/// 对特定类型敌人伤害+25%
/// [设计文案] 专业的猎人，针对特定目标
/// </summary>
public class Hunter : ARelic
{
    public static string ID = "Hunter";

    public Hunter() : base(ID, "Hunter.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现对特定敌人类型的增伤
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (brick.BrickType == TargetType) // 假设有目标类型
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.25f);
    //     }
    // }

    public override ARelic makeCopy() => new Hunter();
}

/// <summary>
/// 每次造成撞击伤害时会提供1点AP
/// 每次造成魔法伤害时会提供1点AD
/// </summary>
public class Annihilator : ARelic, IEvent<OnBallDeath>
{
    public static string ID = "Annihilator";

    public Annihilator() : base(ID, "Annihilator.png", RelicTier.SPECIAL, LandingSound.HEAVY) { }

    int deltaAD, deltaAP;
    Dictionary<Ball, int> ballAdDict = new();
    Dictionary<Ball, int> ballApDict = new();

    protected override void onRelicDefSet(RelicDef d)
    {
        deltaAD = (int)d.Args[0];
        deltaAP = (int)d.Args[1];
    }

    public override void onBallImpactHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    {
        if (p.GetStat(Character.Stat.AP, out var ap))
        {
            if (ap.BonusFlat.GetMod(ID, out var mod))
            {
                mod.Value += deltaAP;
            }
            else
            {
                ap.BonusFlat.AddFlat(deltaAP, ID);
            }

            if (!ballApDict.TryAdd(ball, deltaAP))
            {
                ballApDict[ball] += deltaAP;
            }
            else
            {
                ball.Event.addListener<OnBallDeath>(this);
            }
        }
    }

    public override void onBallSkillHitBrick(APlayer p, Ball ball, Brick brick)
    {
        if (p.GetStat(Character.Stat.AD, out var ad))
        {
            if (ad.BonusFlat.GetMod(ID, out var mod))
            {
                mod.Value += deltaAD;
            }
            else
            {
                ad.BonusFlat.AddFlat(deltaAD, ID);
            }

            if (!ballAdDict.TryAdd(ball, deltaAD))
            {
                ballAdDict[ball] += deltaAD;
            }
            else
            {
                ball.Event.addListener<OnBallDeath>(this);
            }
        }
    }

    public void onEvent(OnBallDeath e)
    {
        var ball = e.ball;
        ball.Event.removeListener<OnBallDeath>(this);

        ballApDict.Remove(ball, out var ballAp);
        ballAdDict.Remove(ball, out var ballAd);

        if (_player.GetStat(Character.Stat.AP, out var ap))
        {
            if (ap.BonusFlat.GetMod(ID, out var mod))
            {
                mod.Value -= ballAp;
            }
        }

        if (_player.GetStat(Character.Stat.AD, out var ad))
        {
            if (ad.BonusFlat.GetMod(ID, out var mod))
            {
                mod.Value -= ballAd;
            }
        }
    }

    public override ARelic makeCopy() => new Annihilator();
}