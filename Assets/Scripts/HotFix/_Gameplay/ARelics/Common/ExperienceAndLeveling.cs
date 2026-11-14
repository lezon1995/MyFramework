using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// WisdomScroll - 智慧卷轴
/// 经验+25%
/// </summary>
public class WisdomScroll : ARelic
{
    public static string ID = "WisdomScroll";

    public WisdomScroll() : base(ID, "WisdomScroll.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现经验增益
    // public override void onEquip(APlayer p)
    // {
    //     p.XPMultiplier *= 1.25f;
    // }

    public override ARelic makeCopy() => new WisdomScroll();
}

/// <summary>
/// ScholarHat - 学者之帽
/// 升级所需经验-15%
/// </summary>
public class ScholarHat : ARelic
{
    public static string ID = "ScholarHat";

    public ScholarHat() : base(ID, "ScholarHat.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现经验需求降低
    // public override void onEquip(APlayer p)
    // {
    //     p.XPRequiredMultiplier *= 0.85f;
    // }

    public override ARelic makeCopy() => new ScholarHat();
}

/// <summary>
/// KnowledgeGem - 知识宝石
/// 每级额外获得属性
/// [设计文案] 升级时的额外奖励
/// </summary>
public class KnowledgeGem : ARelic
{
    public static string ID = "KnowledgeGem";

    public KnowledgeGem() : base(ID, "KnowledgeGem.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现升级属性奖励
    // public override void onLevelUp(APlayer p, int newLevel)
    // {
    //     p.AddBonusStatsOnLevelUp(1);
    // }

    public override ARelic makeCopy() => new KnowledgeGem();
}

/// <summary>
/// 当敌人受到DOT伤害时获得实际造成伤害*10%的金币
/// </summary>
public class AncientTome : ARelic
{
    public static string ID = "AncientTome";

    public AncientTome() : base(ID, "AncientTome.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    int minAmount, maxAmount;
    float convertRate;

    protected override void onRelicDefSet(RelicDef d)
    {
        convertRate = d.Args[0];
        minAmount = (int)d.Args[1];
        maxAmount = (int)d.Args[2];
    }

    public override void onCauseDotDamage(APlayer p, Brick brick, in Dmg dmg)
    {
        var amount = (int)Mathf.Clamp(dmg.DamageDealt * convertRate, minAmount, maxAmount);
        p.gainGold(amount);
        new GainCoinTextEvent(amount, brick.getTransform()).trigger();
    }

    public override ARelic makeCopy() => new AncientTome();
}

/// <summary>
/// StudyGlasses - 学习眼镜
/// 经验+15%
/// </summary>
public class StudyGlasses : ARelic
{
    public static string ID = "StudyGlasses";

    public StudyGlasses() : base(ID, "StudyGlasses.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    // TODO: 需要实现经验增益
    // public override void onEquip(APlayer p)
    // {
    //     p.XPMultiplier *= 1.15f;
    // }

    public override ARelic makeCopy() => new StudyGlasses();
}

/// <summary>
/// MentorSpirit - 导师之魂
/// 升级时获得双倍属性
/// [设计文案] 优秀的学生
/// </summary>
public class MentorSpirit : ARelic
{
    public static string ID = "MentorSpirit";

    public MentorSpirit() : base(ID, "MentorSpirit.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现升级双倍属性
    // public override void onLevelUp(APlayer p, int newLevel)
    // {
    //     p.LevelUpStats *= 2f;
    // }

    public override ARelic makeCopy() => new MentorSpirit();
}

/// <summary>
/// 撞击时附带0.08*AP的魔法伤害
/// </summary>
public class Enlightenment : ARelic
{
    public static string ID = "Enlightenment";

    public Enlightenment() : base(ID, "Enlightenment.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    float rate;
    
    protected override void onRelicDefSet(RelicDef d)
    {
        rate = d.Args[0];
    }

    public override void onBallCauseImpactDamage(APlayer p, Ball ball, Brick brick, in Dmg dmg)
    {
        if (brick.IsDead())
            return;

        var value = (int)(rate * p.ap);
        if (value > 0)
        {
            var d = Dmg.AP(value);
            d.SetEffect(Dmg.Effects.None);
            brick.Health.Damage(ref d, ball.gameObject, p, direction: dmg.Direction);
        }
    }
}

/// <summary>
/// PhilosopherStone - 贤者之石
/// 经验转化为生命
/// [设计文案] 炼金术的奥秘
/// </summary>
public class PhilosopherStone : ARelic
{
    public static string ID = "PhilosopherStone";

    public PhilosopherStone() : base(ID, "PhilosopherStone.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    // TODO: 需要实现经验转生命
    // public override void onGainXP(APlayer p, int amount)
    // {
    //     p.Heal(amount * 0.01f);
    // }

    public override ARelic makeCopy() => new PhilosopherStone();
}

/// <summary>
/// 你的分裂效果会额外分裂2个
/// </summary>
public class LibraryCard : ARelic
{
    public static string ID = "LibraryCard";

    public LibraryCard() : base(ID, "LibraryCard.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    BallFissionExtraPower power;

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
        power = p.addPower<BallFissionExtraPower>();
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
        p.removePower(power);
    }
}

/// <summary>
/// 你的裂变子球撞击时有20%几率复制自己
/// </summary>
public class LibraryBadge : ARelic
{
    public static string ID = "LibraryBadge";

    public LibraryBadge() : base(ID, "LibraryBadge.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

    float duplicateChance => chance + _player.triggerChance;
    float chance;
    
    protected override void onRelicDefSet(RelicDef d)
    {
        chance = d.Args[0];
    }

    public override void onBallDoHitEffect(APlayer p, Ball ball, DoHitEffect e, float delay)
    {
        if (ball.BallType is BallType.FissionMini)
        {
            if (randomHit(duplicateChance))
            {
                var arg = new BallDuplicateArg();
                arg.Type = ball.BallType;
                arg.Pos =ball.curPos;
                arg.Normal = e.dmg.HitNormal;
                arg.Level =ball. level;
                arg.Duration = ball.getDuration();
                effectManager.addLogic<BallDuplicateEffect>().with(_player, ball.SourceWeapon, arg);
            }
        }
    }
}

/// <summary>
/// 你的复制效果会额外复制1个
/// </summary>
public class BrainFood : ARelic
{
    public static string ID = "BrainFood";

    public BrainFood() : base(ID, "BrainFood.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }
    BallDuplicateExtraPower power;

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
        power = p.addPower<BallDuplicateExtraPower>();
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
        p.removePower(power);
    }
}