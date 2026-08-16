namespace MoreMountains;

/// <summary>
/// CriticalEye - 暴击之眼
/// 暴击率+10%
/// </summary>
public class CriticalEye : ARelic
{
    public static string ID = "CriticalEye";

    public CriticalEye() : base(ID, "CriticalEye.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new CriticalEye();
}

/// <summary>
/// SharpClaws - 锋利爪子
/// 暴击伤害+20%
/// </summary>
public class SharpClaws : ARelic
{
    public static string ID = "SharpClaws";

    public SharpClaws() : base(ID, "SharpClaws.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

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

    public VampiricFangs() : base(ID, "VampiricFangs.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

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

    public LightningStrike() : base(ID, "LightningStrike.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

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
/// CriticalMass - 暴击质变
/// 暴击率超过50%时额外+15%
/// [设计文案] 临界点的突破，高暴击带来额外收益
/// </summary>
public class CriticalMass : ARelic
{
    public static string ID = "CriticalMass";

    public CriticalMass() : base(ID, "CriticalMass.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现高暴击率时的额外暴击率
    // public override void onEquip(APlayer p)
    // {
    //     CheckCritBonus(p);
    // }
    //
    // private void CheckCritBonus(APlayer p)
    // {
    //     var critChance = p.GetStat(Stats.CritChance);
    //     if (critChance >= 0.5f)
    //     {
    //         p.AddStatBonus(Stats.CritChance, 0.15f);
    //     }
    // }

    public override ARelic makeCopy() => new CriticalMass();
}

/// <summary>
/// AssassinDagger - 刺客匕首
/// 暴击伤害+40%
/// </summary>
public class AssassinDagger : ARelic
{
    public static string ID = "AssassinDagger";

    public AssassinDagger() : base(ID, "AssassinDagger.png", RelicTier.RARE, LandingSound.SOLID)
    {
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

    public LuckyStar() : base(ID, "LuckyStar.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

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

    public CrimsonEdge() : base(ID, "CrimsonEdge.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现暴击流血
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (ball.IsCriticalHit)
    //     {
    //         brick.AddStatusEffect(StatusEffect.Bleeding, 3f, 5f);
    //     }
    // }

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

    public PrecisionStrike() : base(ID, "PrecisionStrike.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new PrecisionStrike();
}

/// <summary>
/// Bloodlust - 嗜血
/// 暴击时伤害增益可叠加
/// [设计文案] 杀戮的渴望，暴击积累力量
/// </summary>
public class Bloodlust : ARelic
{
    public static string ID = "Bloodlust";

    public Bloodlust() : base(ID, "Bloodlust.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现暴击伤害叠加
    // private float damageBonus = 0f;
    //
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (ball.IsCriticalHit)
    //     {
    //         damageBonus += 0.02f;
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(damageBonus);
    //     }
    // }
    //
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     damageBonus = 0f;
    // }

    public override ARelic makeCopy() => new Bloodlust();
}
