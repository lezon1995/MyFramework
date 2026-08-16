namespace MoreMountains;

/// <summary>
/// WisdomScroll - 智慧卷轴
/// 经验+25%
/// </summary>
public class WisdomScroll : ARelic
{
    public static string ID = "WisdomScroll";

    public WisdomScroll() : base(ID, "WisdomScroll.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

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

    public ScholarHat() : base(ID, "ScholarHat.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

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

    public KnowledgeGem() : base(ID, "KnowledgeGem.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现升级属性奖励
    // public override void onLevelUp(APlayer p, int newLevel)
    // {
    //     p.AddBonusStatsOnLevelUp(1);
    // }

    public override ARelic makeCopy() => new KnowledgeGem();
}

/// <summary>
/// AncientTome - 古老典籍
/// 技能解锁更快
/// [设计文案] 加速技能学习
/// </summary>
public class AncientTome : ARelic
{
    public static string ID = "AncientTome";

    public AncientTome() : base(ID, "AncientTome.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现技能加速
    // public override void onEquip(APlayer p)
    // {
    //     p.SkillUnlockSpeed *= 1.5f;
    // }

    public override ARelic makeCopy() => new AncientTome();
}

/// <summary>
/// StudyGlasses - 学习眼镜
/// 经验+15%
/// </summary>
public class StudyGlasses : ARelic
{
    public static string ID = "StudyGlasses";

    public StudyGlasses() : base(ID, "StudyGlasses.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

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

    public MentorSpirit() : base(ID, "MentorSpirit.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现升级双倍属性
    // public override void onLevelUp(APlayer p, int newLevel)
    // {
    //     p.LevelUpStats *= 2f;
    // }

    public override ARelic makeCopy() => new MentorSpirit();
}

/// <summary>
/// Enlightenment - 启迪
/// 每3级获得额外技能
/// [设计文案] 智慧的火花
/// </summary>
public class Enlightenment : ARelic
{
    public static string ID = "Enlightenment";

    public Enlightenment() : base(ID, "Enlightenment.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现每3级技能
    // public override void onLevelUp(APlayer p, int newLevel)
    // {
    //     if (newLevel % 3 == 0)
    //     {
    //         p.UnlockBonusSkill();
    //     }
    // }

    public override ARelic makeCopy() => new Enlightenment();
}

/// <summary>
/// PhilosopherStone - 贤者之石
/// 经验转化为生命
/// [设计文案] 炼金术的奥秘
/// </summary>
public class PhilosopherStone : ARelic
{
    public static string ID = "PhilosopherStone";

    public PhilosopherStone() : base(ID, "PhilosopherStone.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现经验转生命
    // public override void onGainXP(APlayer p, int amount)
    // {
    //     p.Heal(amount * 0.01f);
    // }

    public override ARelic makeCopy() => new PhilosopherStone();
}

/// <summary>
/// LibraryCard - 图书证
/// 经验+20%
/// </summary>
public class LibraryCard : ARelic
{
    public static string ID = "LibraryCard";

    public LibraryCard() : base(ID, "LibraryCard.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现经验增益
    // public override void onEquip(APlayer p)
    // {
    //     p.XPMultiplier *= 1.2f;
    // }

    public override ARelic makeCopy() => new LibraryCard();
}

/// <summary>
/// BrainFood - 补脑食品
/// 经验+10%，幸运+5
/// </summary>
public class BrainFood : ARelic
{
    public static string ID = "BrainFood";

    public BrainFood() : base(ID, "BrainFood.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new BrainFood();
}
