using UniStats;

namespace MoreMountains;

/// <summary>
/// 暴击时，球的存活时间延长1秒
/// </summary>
public class CriticalEye : ARelic
{
    public static string ID = "CriticalEye";

    public CriticalEye() : base(ID, "CriticalEye.png", RelicTier.COMMON, LandingSound.SOLID) { }

    float deltaDuration;

    protected override void onRelicDefSet(RelicDef d)
    {
        deltaDuration = d.Args[0];
    }

    public override void onBallImpactCrit(APlayer p, Ball ball, Brick brick, in Dmg dmg)
    {
        ball.extendDuration(deltaDuration);
    }

    public override void onBallSkillCrit(APlayer p, Ball ball, Brick brick)
    {
        ball.extendDuration(deltaDuration);
    }

    public override ARelic makeCopy() => new CriticalEye();
}

/// <summary>
/// 暴击率+20%
/// </summary>
public class CriticalCore : ARelic
{
    public static string ID = "CriticalCore";
    public CriticalCore() : base(ID, "CriticalCore.png", RelicTier.COMMON, LandingSound.SOLID) { }
}

/// <summary>
/// SharpClaws - 锋利爪子
/// 暴击伤害+20%
/// </summary>
public class SharpClaws : ARelic
{
    public static string ID = "SharpClaws";

    public SharpClaws() : base(ID, "SharpClaws.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    public override ARelic makeCopy() => new SharpClaws();
}

/// <summary>
/// VampiricFangs - 吸血獠牙
/// 暴击时恢复生命
/// [设计文案] 吸血鬼的獠牙，暴击吸取生命
/// </summary>
public class VampiricFangs : ARelic
{
    public static string ID = "VampiricFangs";

    public VampiricFangs() : base(ID, "VampiricFangs.png", RelicTier.RARE, LandingSound.SOLID) { }

    // TODO: 需要实现暴击回血
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (ball.IsCriticalHit)
    //     {
    //         p.Heal(ball.Damage * 0.1f);
    //     }
    // }

    public override ARelic makeCopy() => new VampiricFangs();
}

/// <summary>
/// LightningStrike - 雷击
/// 暴击时有概率触发闪电
/// [设计文案] 雷神之力，暴击引发闪电
/// </summary>
public class LightningStrike : ARelic
{
    public static string ID = "LightningStrike";

    public LightningStrike() : base(ID, "LightningStrike.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现暴击闪电
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (ball.IsCriticalHit && UnityEngine.Random.value < 0.3f)
    //     {
    //         // 释放闪电
    //     }
    // }

    public override ARelic makeCopy() => new LightningStrike();
}

/// <summary>
/// 你的DOT伤害可以暴击
/// </summary>
public class CriticalMass : ARelic
{
    public static string ID = "CriticalMass";

    public CriticalMass() : base(ID, "CriticalMass.png", RelicTier.RARE, LandingSound.SOLID) { }

    public override void onPlayerBeforeHandleDamage(APlayer p, Brick brick, ref Dmg dmg)
    {
        if (!dmg.IsDOT)
            return;

        if (randomHit(_player.critChance))
            dmg.Crit();
    }
}

/// <summary>
/// 电锯的攻击间隔-33%
/// </summary>
public class ChainSawBattery : ARelic
{
    public static string ID = "ChainSawBattery";

    public ChainSawBattery() : base(ID, "ChainSawBattery.png", RelicTier.RARE, LandingSound.SOLID) { }

    public override void onShootBall(APlayer p, Ball ball)
    {
        if (ball is Ball_ChainSaw chainSaw)
        {
            chainSaw.dmgGap -= 1;
        }
    }
}

/// <summary>
/// 球速提高100%，持续时间减少50%
/// </summary>
public class AssassinDagger : ARelic
{
    public static string ID = "AssassinDagger";

    public AssassinDagger() : base(ID, "AssassinDagger.png", RelicTier.RARE, LandingSound.SOLID) { }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
    }

    public override ARelic makeCopy() => new AssassinDagger();
}

/// <summary>
/// LuckyStar - 幸运星
/// 暴击时随机获得增益
/// [设计文案] 星星的祝福，暴击带来好运
/// </summary>
public class LuckyStar : ARelic
{
    public static string ID = "LuckyStar";

    public LuckyStar() : base(ID, "LuckyStar.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现暴击随机增益
    // private enum RandomBuff { Speed, Damage, Shield, Heal }
    //
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (ball.IsCriticalHit)
    //     {
    //         var buff = (RandomBuff)UnityEngine.Random.Range(0, 4);
    //         switch (buff)
    //         {
    //             case RandomBuff.Speed: p.BuffSpeed(0.2f, 3f); break;
    //             case RandomBuff.Damage: ball.BuffDamage(0.2f); break;
    //             case RandomBuff.Shield: p.AddShield(1); break;
    //             case RandomBuff.Heal: p.Heal(5); break;
    //         }
    //     }
    // }

    public override ARelic makeCopy() => new LuckyStar();
}

/// <summary>
/// CrimsonEdge - 猩红之刃
/// 暴击造成流血
/// [设计文案] 血红的刀刃，暴击留下伤口
/// </summary>
public class CrimsonEdge : ARelic
{
    public static string ID = "CrimsonEdge";

    public CrimsonEdge() : base(ID, "CrimsonEdge.png", RelicTier.RARE, LandingSound.SOLID) { }

    public override void onKillBrick(APlayer p, Brick brick) { }

    public override ARelic makeCopy() => new CrimsonEdge();
}

/// <summary>
/// PrecisionStrike - 精准打击
/// 暴击率+15%，暴击伤害-10%
/// [设计文案] 精准的射手，更多暴击但威力稍减
/// </summary>
public class PrecisionStrike : ARelic
{
    public static string ID = "PrecisionStrike";

    public PrecisionStrike() : base(ID, "PrecisionStrike.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    public override ARelic makeCopy() => new PrecisionStrike();
}

/// <summary>
/// 生命值低于50%时，球速+50%
/// </summary>
public class Bloodlust : ARelic
{
    public static string ID = "Bloodlust";

    public Bloodlust() : base(ID, "Bloodlust.png", RelicTier.RARE, LandingSound.HEAVY) { }

    float healthPct, ballSpeedRate;

    protected override void onRelicDefSet(RelicDef d)
    {
        ballSpeedRate = d.Args[0];
        healthPct = d.Args[1];
    }

    public override void onEnterBloodied(APlayer p)
    {
        if (p.GetStat(Character.Stat.BallisticSpeed, out var stat))
        {
            stat.BonusPct.AddFlat(ballSpeedRate, ID);
        }
    }

    public override void onExitBloodied(APlayer p)
    {
        if (p.GetStat(Character.Stat.BallisticSpeed, out var stat))
        {
            stat.BonusPct.RemoveMod(ID);
        }
    }

    public override ARelic makeCopy() => new Bloodlust();
}