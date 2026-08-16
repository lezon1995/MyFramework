namespace MoreMountains;

/// <summary>
/// CrownOfThorns - 荆棘之冠
/// 受伤时周围敌人受伤
/// [设计文案] 伤害扩散
/// </summary>
public class CrownOfThorns : ARelic
{
    public static string ID = "CrownOfThorns";

    public CrownOfThorns() : base(ID, "CrownOfThorns.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现受伤范围伤害
    // public override void onLoseHp(int damageAmount)
    // {
    //     var enemies = GetNearbyEnemies(owner.Position, 3f);
    //     foreach (var enemy in enemies)
    //     {
    //         enemy.TakeDamage(damageAmount * 0.5f);
    //     }
    // }

    public override ARelic makeCopy() => new CrownOfThorns();
}

/// <summary>
/// RingOfFire - 火焰戒指
/// 周围持续造成伤害
/// [设计文案] 火焰领域
/// </summary>
public class RingOfFire : ARelic
{
    public static string ID = "RingOfFire";

    public RingOfFire() : base(ID, "RingOfFire.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现周围持续伤害
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     var enemies = GetNearbyEnemies(p.Position, 2f);
    //     foreach (var enemy in enemies)
    //     {
    //         enemy.TakeDamage(5f * dt);
    //     }
    // }

    public override ARelic makeCopy() => new RingOfFire();
}

/// <summary>
/// BootsOfHermes - 赫尔墨斯之靴
/// 无敌帧+30%
/// [设计文案] 神的祝福
/// </summary>
public class BootsOfHermes : ARelic
{
    public static string ID = "BootsOfHermes";

    public BootsOfHermes() : base(ID, "BootsOfHermes.png", RelicTier.SPECIAL, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现无敌帧增加
    // public override void onEquip(APlayer p)
    // {
    //     p.InvincibilityFramesMultiplier *= 1.3f;
    // }

    public override ARelic makeCopy() => new BootsOfHermes();
}

/// <summary>
/// ShieldOfJustice - 正义之盾
/// 完美格挡触发反击
/// [设计文案] 正义的守护
/// </summary>
public class ShieldOfJustice : ARelic
{
    public static string ID = "ShieldOfJustice";

    public ShieldOfJustice() : base(ID, "ShieldOfJustice.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现完美格挡反击
    // public override void onPerfectBlock(APlayer p)
    // {
    //     var nearest = FindNearestEnemy();
    //     if (nearest != null)
    //     {
    //         p.Attack(nearest, p.AttackDamage * 3f);
    //     }
    // }

    public override ARelic makeCopy() => new ShieldOfJustice();
}

/// <summary>
/// SwordOfDamocles - 达摩克利斯之剑
/// 高风险高回报
/// [设计文案] 悬顶之剑
/// </summary>
public class SwordOfDamocles : ARelic
{
    public static string ID = "SwordOfDamocles";

    public SwordOfDamocles() : base(ID, "SwordOfDamocles.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现高风险高回报
    // public override void onEquip(APlayer p)
    // {
    //     p.BuffDamage(1.0f); // +100%伤害
    //     p.DebuffHealth(0.5f); // -50%血量
    // }

    public override ARelic makeCopy() => new SwordOfDamocles();
}

/// <summary>
/// AmuletOfLife - 生命护符
/// 生命偷取+15%
/// </summary>
public class AmuletOfLife : ARelic
{
    public static string ID = "AmuletOfLife";

    public AmuletOfLife() : base(ID, "AmuletOfLife.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    public override ARelic makeCopy() => new AmuletOfLife();
}

/// <summary>
/// OrbOfProtection - 保护之球
/// 周围敌人攻击减半
/// [设计文案] 保护领域
/// </summary>
public class OrbOfProtection : ARelic
{
    public static string ID = "OrbOfProtection";

    public OrbOfProtection() : base(ID, "OrbOfProtection.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现敌人攻击减半
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     var enemies = GetNearbyEnemies(p.Position, 4f);
    //     foreach (var enemy in enemies)
    //     {
    //         enemy.AttackDamage *= 0.5f;
    //     }
    // }

    public override ARelic makeCopy() => new OrbOfProtection();
}

/// <summary>
/// CloakOfShadows - 暗影斗篷
/// 隐身时伤害翻倍
/// [设计文案] 暗影中的杀手
/// </summary>
public class CloakOfShadows : ARelic
{
    public static string ID = "CloakOfShadows";

    public CloakOfShadows() : base(ID, "CloakOfShadows.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现隐身增伤
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (p.IsInvisible)
    //     {
    //         p.DamageMultiplier = 2f;
    //     }
    //     else
    //     {
    //         p.DamageMultiplier = 1f;
    //     }
    // }

    public override ARelic makeCopy() => new CloakOfShadows();
}

/// <summary>
/// GauntletOfStrength - 力量护手
/// 伤害+50%
/// </summary>
public class GauntletOfStrength : ARelic
{
    public static string ID = "GauntletOfStrength";

    public GauntletOfStrength() : base(ID, "GauntletOfStrength.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new GauntletOfStrength();
}

/// <summary>
/// HeartOfTheMountain - 山之心
/// 每秒恢复最大生命的1%
/// [设计文案] 大山的守护
/// </summary>
public class HeartOfTheMountain : ARelic
{
    public static string ID = "HeartOfTheMountain";

    public HeartOfTheMountain() : base(ID, "HeartOfTheMountain.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现持续回血
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     p.Heal(p.MaxHealth * 0.01f * dt);
    // }

    public override ARelic makeCopy() => new HeartOfTheMountain();
}
