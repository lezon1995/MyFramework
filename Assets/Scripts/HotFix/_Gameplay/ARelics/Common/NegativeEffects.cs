namespace MoreMountains;

/// <summary>
/// CursedCoin - 被诅咒的硬币
/// 金币+50%但经验-20%
/// [设计文案] 金钱的代价
/// </summary>
public class CursedCoin : ARelic
{
    public static string ID = "CursedCoin";

    public CursedCoin() : base(ID, "CursedCoin.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现金币增益和经验减益
    // public override void onEquip(APlayer p)
    // {
    //     p.GoldMultiplier *= 1.5f;
    //     p.XPMultiplier *= 0.8f;
    // }

    public override ARelic makeCopy() => new CursedCoin();
}

/// <summary>
/// BrokenShield - 破损护盾
/// 护甲-5但伤害+10%
/// [设计文案] 牺牲防御换取攻击
/// </summary>
public class BrokenShield : ARelic
{
    public static string ID = "BrokenShield";

    public BrokenShield() : base(ID, "BrokenShield.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new BrokenShield();
}

/// <summary>
/// RustedArmor - 生锈护甲
/// 护甲-3但移速+15%
/// [设计文案] 机动优于防护
/// </summary>
public class RustedArmor : ARelic
{
    public static string ID = "RustedArmor";

    public RustedArmor() : base(ID, "RustedArmor.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new RustedArmor();
}

/// <summary>
/// WeakenedHeart - 衰弱之心
/// 生命上限-20但伤害+15%
/// [设计文案] 脆弱但致命
/// </summary>
public class WeakenedHeart : ARelic
{
    public static string ID = "WeakenedHeart";

    public WeakenedHeart() : base(ID, "WeakenedHeart.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new WeakenedHeart();
}

/// <summary>
/// SlowCannon - 慢速大炮
/// 攻速-30%但伤害+40%
/// [设计文案] 慢而有力
/// </summary>
public class SlowCannon : ARelic
{
    public static string ID = "SlowCannon";

    public SlowCannon() : base(ID, "SlowCannon.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new SlowCannon();
}

/// <summary>
/// HeavyBurden - 重负
/// 移速-20%但生命上限+50
/// [设计文案] 沉重的力量
/// </summary>
public class HeavyBurden : ARelic
{
    public static string ID = "HeavyBurden";

    public HeavyBurden() : base(ID, "HeavyBurden.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new HeavyBurden();
}

/// <summary>
/// ChaosCurse - 混乱诅咒
/// 随机属性波动
/// [设计文案] 不可预测的力量
/// </summary>
public class ChaosCurse : ARelic
{
    public static string ID = "ChaosCurse";

    public ChaosCurse() : base(ID, "ChaosCurse.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现随机属性波动
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (Random.value < 0.1f)
    //     {
    //         var stat = Random.Range(0, 5);
    //         switch (stat)
    //         {
    //             case 0: p.BuffDamage(0.1f); break;
    //             case 1: p.BuffSpeed(0.1f); break;
    //             case 2: p.DeBuffDamage(0.1f); break;
    //             // ...
    //         }
    //     }
    // }

    public override ARelic makeCopy() => new ChaosCurse();
}

/// <summary>
/// DeathWish - 死亡之愿
/// 伤害+30%但无法生命偷取
/// [设计文案] 纯粹的伤害
/// </summary>
public class DeathWish : ARelic
{
    public static string ID = "DeathWish";

    public DeathWish() : base(ID, "DeathWish.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new DeathWish();
}

/// <summary>
/// MidasTouch - 点金术
/// 金币+100%但击杀不恢复生命
/// [设计文案] 黄金的代价
/// </summary>
public class MidasTouch : ARelic
{
    public static string ID = "MidasTouch";

    public MidasTouch() : base(ID, "MidasTouch.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new MidasTouch();
}

/// <summary>
/// PowerDrain - 力量流失
/// 属性逐渐下降
/// [设计文案] 衰减的力量
/// </summary>
public class PowerDrain : ARelic
{
    public static string ID = "PowerDrain";

    public PowerDrain() : base(ID, "PowerDrain.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现属性衰减
    // private float drainTimer = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     drainTimer += dt;
    //     if (drainTimer >= 10f)
    //     {
    //         drainTimer = 0f;
    //         p.DeBuffAllStats(0.02f);
    //     }
    // }

    public override ARelic makeCopy() => new PowerDrain();
}
