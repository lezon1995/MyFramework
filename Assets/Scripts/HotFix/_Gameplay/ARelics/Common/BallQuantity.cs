using System.Collections.Generic;
using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// MultiBall - 多球
/// 额外获得1个球
/// </summary>
public class MultiBall : ARelic
{
    public static string ID = "MultiBall";

    public MultiBall() : base(ID, "MultiBall.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    public override ARelic makeCopy() => new MultiBall();
}

/// <summary>
/// 技能伤害有25%几率让目标眩晕0.5秒
/// </summary>
public class BallSquad : ARelic
{
    public static string ID = "BallSquad";

    public BallSquad() : base(ID, "BallSquad.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    float stunChance;
    float stunDuration;

    protected override void onRelicDefSet(RelicDef d)
    {
        stunChance = d.Args[0];
        stunDuration = d.Args[1];
    }

    public override void onBallSkillHitBrick(APlayer p, Ball ball, Brick brick)
    {
        var chance = stunChance + p.triggerChance;
        if (randomHit(chance))
        {
            var y = brick.getSize().y;
            var offset = new Vector3(0, y * 0.5F, 0);
            effectManager.addLogic<StunnedEffect>().with(brick, stunDuration, offset);
        }
    }

    public override ARelic makeCopy() => new BallSquad();
}

/// <summary>
/// 当技能命中时，有20%几率恢复2%已损失生命值。
/// </summary>
public class BallSwarm : ARelic
{
    public static string ID = "BallSwarm";

    public BallSwarm() : base(ID, "BallSwarm.png", RelicTier.RARE, LandingSound.SOLID) { }

    float chance;
    float healAmount;

    protected override void onRelicDefSet(RelicDef d)
    {
        chance = d.Args[0];
        healAmount = d.Args[1];
    }

    public override void onBallSkillHitBrick(APlayer p, Ball ball, Brick brick)
    {
        if (randomHit(chance))
        {
            var heal = new Heal(healAmount, Heal.Algos.LostPct);
            effectManager.addLogic<HealingBulletEffect>().with(brick.transform, p.transform, heal);
        }
    }

    public override ARelic makeCopy() => new BallSwarm();
}

/// <summary>
/// 球在释放技能时，有25%几率再释放1次
/// </summary>
public class BallBarrage : ARelic
{
    public static string ID = "BallBarrage";

    public BallBarrage() : base(ID, "BallBarrage.png", RelicTier.RARE, LandingSound.SOLID) { }

    Chance chance;

    protected override void onRelicDefSet(RelicDef d)
    {
        chance = d.Args[0];
    }

    public override void onBallDoHitEffect(APlayer p, Ball ball, DoHitEffect e, float delay)
    {
        // if (chance.check())
        {
            effectManager.addLogic<BallDoHitEffect>().with(ball, e, delay);
        }
    }

    public override ARelic makeCopy() => new BallBarrage();
}

/// <summary>
/// 击杀敌人时，提高10%移速和20%攻速，持续4秒
/// </summary>
public class BallStorm : ARelic
{
    public static string ID = "BallStorm";

    public BallStorm() : base(ID, "BallStorm.png", RelicTier.SPECIAL, LandingSound.SOLID) { }

    float deltaMoveSpeed;
    float deltaAttackSpeed;
    float duration;

    List<float> timers = new();

    protected override void onRelicDefSet(RelicDef d)
    {
        deltaMoveSpeed = d.Args[0];
        deltaAttackSpeed = d.Args[1];
        duration = d.Args[2];
    }

    public override void onFixedUpdate(float dt)
    {
        for (var i = timers.Count - 1; i >= 0; i--)
        {
            timers[i] -= dt;
            if (timers[i] <= 0)
            {
                timers.RemoveAt(i);

                if (_player.GetStat(Character.Stat.MS, out var moveSpeed))
                {
                    if (moveSpeed.BonusPct.GetMod(ID, out var mod))
                    {
                        mod.Value -= deltaMoveSpeed;
                        if (mod.Value.isZero())
                        {
                            moveSpeed.BonusPct.RemoveMod(ID);
                        }
                    }
                }

                if (_player.GetStat(Character.Stat.AS, out var attackSpeed))
                {
                    if (attackSpeed.BonusPct.GetMod(ID, out var mod))
                    {
                        mod.Value -= deltaAttackSpeed;
                        if (mod.Value.isZero())
                        {
                            attackSpeed.BonusPct.RemoveMod(ID);
                        }
                    }
                }
            }
        }
    }

    public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    {
        if (p.GetStat(Character.Stat.MS, out var moveSpeed))
        {
            if (moveSpeed.BonusPct.GetMod(ID, out var mod))
            {
                mod.Value += deltaMoveSpeed;
            }
            else
            {
                moveSpeed.BonusPct.AddFlat(deltaMoveSpeed, ID);
            }
        }

        if (p.GetStat(Character.Stat.AS, out var attackSpeed))
        {
            if (attackSpeed.BonusPct.GetMod(ID, out var mod))
            {
                mod.Value += deltaAttackSpeed;
            }
            else
            {
                attackSpeed.BonusPct.AddFlat(deltaAttackSpeed, ID);
            }
        }

        timers.add(duration);
    }

    public override ARelic makeCopy() => new BallStorm();
}

/// <summary>
/// 冲刺后，冲刺轨迹上会触发多段雷电打击伤害，每段打击造成0.2AP的魔法伤害
/// </summary>
public class BallRain : ARelic
{
    public static string ID = "BallRain";

    public BallRain() : base(ID, "BallRain.png", RelicTier.RARE, LandingSound.SOLID) { }

    float delay;
    float rate;
    Countdown interval;
    bool started;
    int counter;

    protected override void onRelicDefSet(RelicDef d)
    {
        delay = d.Args[0];
        rate = d.Args[1];
        interval = (int)d.Args[2];
    }

    public override void onPlayerDashing(APlayer p, Vector3 pos)
    {
        if (interval.update())
        {
            interval = (int)def.Args[2];
            var damage = (int)(_player.ap * rate);
            effectManager.addLogic<LightningRangeStrikeEffect>().with(_player, pos, delay, damage);
        }

        counter++;
    }

    public override void onPlayerStartDash(APlayer p, Vector3 start, Vector3 end)
    {
        interval = (int)def.Args[2];
        started = true;
        counter = 0;
    }

    public override void onPlayerStopDash(APlayer p)
    {
        started = false;
        // log($"dash counter = {counter}");
    }

    public override ARelic makeCopy() => new BallRain();
}

/// <summary>
/// 球每次反弹时，AD+4，AP+8，最多叠加10次
/// </summary>
public class BallFountain : ARelic
{
    public static string ID = "BallFountain";

    public BallFountain() : base(ID, "BallFountain.png", RelicTier.RARE, LandingSound.SOLID) { }

    float deltaAD, deltaAP;

    protected override void onRelicDefSet(RelicDef d)
    {
        deltaAD = d.Args[0];
        deltaAP = d.Args[1];
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        if (ball.GetStat(Ball.Stat.AD, out var ad))
        {
            if (ad.BonusFlat.GetMod(ID, out var mod))
            {
                mod.Value += deltaAD;
            }
            else
            {
                ad.BonusFlat.AddFlat(deltaAD, ID);
            }
        }

        if (ball.GetStat(Ball.Stat.AP, out var ap))
        {
            if (ap.BonusFlat.GetMod(ID, out var mod))
            {
                mod.Value += deltaAP;
            }
            else
            {
                ap.BonusFlat.AddFlat(deltaAP, ID);
            }
        }
    }

    public override ARelic makeCopy() => new BallFountain();
}

/// <summary>
/// 击杀敌人时触发连锁闪电，对4个目标造成魔法伤害，若目标被此伤害击杀，则连锁目标+1
/// </summary>
public class BallSwarmController : ARelic
{
    public static string ID = "BallSwarmController";

    public BallSwarmController() : base(ID, "BallSwarmController.png", RelicTier.SPECIAL, LandingSound.SOLID) { }

    int count;
    float damageRate;
    float cooldown;
    float timeElapsed;

    protected override void onRelicDefSet(RelicDef d)
    {
        count = (int)d.Args[0];
        damageRate = d.Args[1];
        cooldown = d.Args[2];
        timeElapsed = cooldown;
    }

    public override void onFixedUpdate(float dt)
    {
        base.onFixedUpdate(dt);
        timeElapsed = Mathf.Clamp(timeElapsed + dt, 0F, cooldown);
    }

    public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    {
        if (timeElapsed >= cooldown)
        {
            timeElapsed = 0F;
            int damage = (int)(damageRate * p.ap);
            effectManager.addLogic<ElectricityStrikeChainEffect>().with(p, brick, count, damage);
        }
    }

    public override ARelic makeCopy() => new BallSwarmController();
}

/// <summary>
/// 球每次撞击砖块时，球速提高25%
/// </summary>
public class BallFactory : ARelic
{
    public static string ID = "BallFactory";

    public BallFactory() : base(ID, "BallFactory.png", RelicTier.RARE, LandingSound.SOLID) { }

    float delta;

    protected override void onRelicDefSet(RelicDef d)
    {
        delta = d.Args[0];
    }

    public override void onBallImpactHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    {
        if (ball.GetStat(Ball.Stat.BallisticSpeed, out var stat))
        {
            if (stat.BonusPct.GetMod(ID, out var mod))
            {
                mod.Value += delta;
            }
            else
            {
                stat.BonusPct.AddFlat(delta, ID);
            }
        }
    }

    public override ARelic makeCopy() => new BallFactory();
}

/// <summary>
/// 撞击特效率+50%，球速修改+10%
/// </summary>
public class BallReplication : ARelic
{
    public static string ID = "BallReplication";

    public BallReplication() : base(ID, "BallReplication.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
    }

    public override ARelic makeCopy() => new BallReplication();
}