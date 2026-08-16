namespace MoreMountains;

/// <summary>
/// HeartOfGold - 黄金之心
/// 生命上限+20
/// </summary>
public class HeartOfGold : ARelic
{
    public static string ID = "HeartOfGold";

    public HeartOfGold() : base(ID, "HeartOfGold.png", RelicTier.UNCOMMON, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new HeartOfGold();
}

/// <summary>
/// RegenerationRing - 再生戒指
/// 每秒恢复1%最大生命
/// [设计文案] 持续的生命力流动
/// </summary>
public class RegenerationRing : ARelic
{
    public static string ID = "RegenerationRing";

    public RegenerationRing() : base(ID, "RegenerationRing.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现持续回血
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     p.Heal(p.MaxHealth * 0.01f * dt);
    // }

    public override ARelic makeCopy() => new RegenerationRing();
}

/// <summary>
/// StoneSkin - 石肤
/// 护甲+8
/// </summary>
public class StoneSkin : ARelic
{
    public static string ID = "StoneSkin";

    public StoneSkin() : base(ID, "StoneSkin.png", RelicTier.UNCOMMON, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new StoneSkin();
}

/// <summary>
/// BarrierWard - 屏障护符
/// 战斗开始时获得护盾
/// [设计文案] 开始的保护屏障
/// </summary>
public class BarrierWard : ARelic
{
    public static string ID = "BarrierWard";

    public BarrierWard() : base(ID, "BarrierWard.png", RelicTier.UNCOMMON, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现战斗开始获得护盾
    // public override void atBattleStart()
    // {
    //     owner.AddShield(3);
    // }

    public override ARelic makeCopy() => new BarrierWard();
}

/// <summary>
/// LifeDrain - 生命汲取
/// 造成伤害的5%转化为生命
/// [设计文案] 将伤害转化为治疗
/// </summary>
public class LifeDrain : ARelic
{
    public static string ID = "LifeDrain";

    public LifeDrain() : base(ID, "LifeDrain.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现伤害转化为治疗
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     var damage = ball.CalculateDamage();
    //     p.Heal(damage * 0.05f);
    // }

    public override ARelic makeCopy() => new LifeDrain();
}

/// <summary>
/// ImmortalSoul - 不死之魂
/// 免疫致命伤害（每波次一次）
/// [设计文案] 死亡只是暂时的
/// </summary>
public class ImmortalSoul : ARelic
{
    public static string ID = "ImmortalSoul";

    public ImmortalSoul() : base(ID, "ImmortalSoul.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现免疫致命伤害
    // private bool hasProtection = true;
    //
    // public override void atBattleStart()
    // {
    //     hasProtection = true;
    // }
    //
    // public override int onLoseHpLast(int damageAmount)
    // {
    //     if (hasProtection && damageAmount >= owner.CurrentHealth)
    //     {
    //         hasProtection = false;
    //         return owner.CurrentHealth - 1;
    //     }
    //     return damageAmount;
    // }

    public override ARelic makeCopy() => new ImmortalSoul();
}

/// <summary>
/// VampireFang - 吸血鬼之牙
/// 生命偷取+8%
/// </summary>
public class VampireFang : ARelic
{
    public static string ID = "VampireFang";

    public VampireFang() : base(ID, "VampireFang.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new VampireFang();
}

/// <summary>
/// ThornsArmor - 荆棘护甲
/// 受到伤害时反弹10%
/// [设计文案] 以牙还牙的防御
/// </summary>
public class ThornsArmor : ARelic
{
    public static string ID = "ThornsArmor";

    public ThornsArmor() : base(ID, "ThornsArmor.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现伤害反弹
    // public override int onAttacked(DamageInfo info, int damageAmount)
    // {
    //     // 反弹伤害给攻击者
    //     info.Attacker.TakeDamage(damageAmount * 0.1f);
    //     return damageAmount;
    // }

    public override ARelic makeCopy() => new ThornsArmor();
}

/// <summary>
/// HealingLight - 治疗之光
/// 击杀敌人时恢复生命
/// [设计文案] 神圣的治愈力量
/// </summary>
public class HealingLight : ARelic
{
    public static string ID = "HealingLight";

    public HealingLight() : base(ID, "HealingLight.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现击杀回血
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     p.Heal(2);
    // }

    public override ARelic makeCopy() => new HealingLight();
}

/// <summary>
/// ShieldGenerator - 护盾发生器
/// 每15秒生成护盾
/// [设计文案] 科技的防护力量
/// </summary>
public class ShieldGenerator : ARelic
{
    public static string ID = "ShieldGenerator";
    private float timer = 0f;

    public ShieldGenerator() : base(ID, "ShieldGenerator.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现定时生成护盾
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     timer += dt;
    //     if (timer >= 15f)
    //     {
    //         timer = 0f;
    //         p.AddShield(1);
    //     }
    // }

    public override ARelic makeCopy() => new ShieldGenerator();
}
