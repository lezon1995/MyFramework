namespace MoreMountains;

/// <summary>
/// Excalibur - 圣剑
/// 所有属性+30%
/// [设计文案] 传说中的圣剑
/// </summary>
public class Excalibur : ARelic
{
    public static string ID = "Excalibur";

    public Excalibur() : base(ID, "Excalibur.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new Excalibur();
}

/// <summary>
/// Mjolnir - 雷神之锤
/// 闪电造成巨大伤害
/// [设计文案] 雷霆之力
/// </summary>
public class Mjolnir : ARelic
{
    public static string ID = "Mjolnir";

    public Mjolnir() : base(ID, "Mjolnir.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现闪电巨伤害
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     // 造成大量闪电伤害
    // }

    public override ARelic makeCopy() => new Mjolnir();
}

/// <summary>
/// AegisOfOlympus - 奥林匹斯之盾
/// 免疫所有负面效果
/// [设计文案] 神的庇护
/// </summary>
public class AegisOfOlympus : ARelic
{
    public static string ID = "AegisOfOlympus";

    public AegisOfOlympus() : base(ID, "AegisOfOlympus.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现免疫负面效果
    // public override void onEquip(APlayer p)
    // {
    //     p.ImmuneToDebuffs = true;
    // }

    public override ARelic makeCopy() => new AegisOfOlympus();
}

/// <summary>
/// WingsOfIcarus - 伊卡洛斯之翼
/// 飞行（无敌）+30%移速
/// [设计文案] 接近太阳的翅膀
/// </summary>
public class WingsOfIcarus : ARelic
{
    public static string ID = "WingsOfIcarus";

    public WingsOfIcarus() : base(ID, "WingsOfIcarus.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现飞行和无敌
    // public override void onEquip(APlayer p)
    // {
    //     p.IsFlying = true;
    //     p.IsInvincible = true;
    // }

    public override ARelic makeCopy() => new WingsOfIcarus();
}

/// <summary>
/// EyeOfProvidence - 普罗维登斯之眼
/// 全知（全屏显示敌人）
/// [设计文案] 神的全视之眼
/// </summary>
public class EyeOfProvidence : ARelic
{
    public static string ID = "EyeOfProvidence";

    public EyeOfProvidence() : base(ID, "EyeOfProvidence.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现全屏显示敌人
    // public override void onEquip(APlayer p)
    // {
    //     p.RevealAllEnemies = true;
    // }

    public override ARelic makeCopy() => new EyeOfProvidence();
}

/// <summary>
/// PhilosopherStoneUltimate - 贤者之石终极版
/// 所有属性+50%
/// [设计文案] 炼金术的极致
/// </summary>
public class PhilosopherStoneUltimate : ARelic
{
    public static string ID = "PhilosopherStoneUltimate";

    public PhilosopherStoneUltimate() : base(ID, "PhilosopherStoneUltimate.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    public override ARelic makeCopy() => new PhilosopherStoneUltimate();
}

/// <summary>
/// CrownOfTheUniverse - 宇宙之冠
/// 获得其他所有遗物的效果（减半）
/// [设计文案] 统御一切的力量
/// </summary>
public class CrownOfTheUniverse : ARelic
{
    public static string ID = "CrownOfTheUniverse";

    public CrownOfTheUniverse() : base(ID, "CrownOfTheUniverse.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现所有遗物效果减半
    // public override void onEquip(APlayer p)
    // {
    //     foreach (var relic in p.OtherRelics)
    //     {
    //         // 应用每个遗物效果的一半
    //     }
    // }

    public override ARelic makeCopy() => new CrownOfTheUniverse();
}

/// <summary>
/// HolyGrail - 圣杯
/// 无限生命回复
/// [设计文案] 神圣的圣杯
/// </summary>
public class HolyGrail : ARelic
{
    public static string ID = "HolyGrail";

    public HolyGrail() : base(ID, "HolyGrail.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现无限回复
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     p.Heal(p.MaxHealth * dt); // 全额回复
    // }

    public override ARelic makeCopy() => new HolyGrail();
}

/// <summary>
/// InfinityGauntlet - 无限手套
/// 集齐六颗宝石的力量
/// [设计文案] 无限的力量
/// </summary>
public class InfinityGauntlet : ARelic
{
    public static string ID = "InfinityGauntlet";

    public InfinityGauntlet() : base(ID, "InfinityGauntlet.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现无限宝石效果
    // public override void onEquip(APlayer p)
    // {
    //     p.BuffAllStats(1.0f); // 全属性翻倍
    //     p.HealToFull();
    // }

    public override ARelic makeCopy() => new InfinityGauntlet();
}

/// <summary>
/// CelestialCore - 天体核心
/// 时间倒流（重置当前波次）
/// [设计文案] 操控时间的力量
/// </summary>
public class CelestialCore : ARelic
{
    public static string ID = "CelestialCore";

    public CelestialCore() : base(ID, "CelestialCore.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现时间倒流
    // private bool canRewind = true;
    //
    // public override void onPlayerTurnBegin(APlayer p)
    // {
    //     if (canRewind)
    //     {
    //         // 显示时间倒流按钮
    //     }
    // }
    //
    // public void TriggerRewind(APlayer p)
    // {
    //     if (canRewind)
    //     {
    //         canRewind = false;
    //         // 重置当前波次
    //     }
    // }

    public override ARelic makeCopy() => new CelestialCore();
}
