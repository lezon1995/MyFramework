using UniStats;

namespace MoreMountains;

/// <summary>
/// CursedCoin - 被诅咒的硬币
/// 金币+50%但经验-20%
/// [设计文案] 金钱的代价
/// </summary>
public class CursedCoin : ARelic
{
    public static string ID = "CursedCoin";

    public CursedCoin() : base(ID, "CursedCoin.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

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

    public BrokenShield() : base(ID, "BrokenShield.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

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

    public RustedArmor() : base(ID, "RustedArmor.png", RelicTier.UNCOMMON, LandingSound.SOLID) { }

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

    public WeakenedHeart() : base(ID, "WeakenedHeart.png", RelicTier.RARE, LandingSound.SOLID) { }

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

    public SlowCannon() : base(ID, "SlowCannon.png", RelicTier.RARE, LandingSound.HEAVY) { }

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

    public HeavyBurden() : base(ID, "HeavyBurden.png", RelicTier.RARE, LandingSound.HEAVY) { }

    public override ARelic makeCopy() => new HeavyBurden();
}

/// <summary>
/// 受到治疗和护盾时，护甲魔抗+2，持续6秒
/// </summary>
public class ChaosCurse : ARelic
{
    public static string ID = "ChaosCurse";

    public ChaosCurse() : base(ID, "ChaosCurse.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    float deltaAR;
    float deltaMR;
    float duration;
    Timer timer;
    string modKey;

    protected override void onRelicDefSet(RelicDef d)
    {
        deltaAR = d.Args[0];
        deltaMR = d.Args[1];
        duration = d.Args[2];
        modKey = $"{ID}_{GetHashCode()}";
    }

    public override void onFixedUpdate(float dt)
    {
        if (timer.update(dt))
        {
            _player.GetStat(Character.Stat.AR, out var ar);
            _player.GetStat(Character.Stat.MR, out var mr);
            ar.BonusFlat.RemoveMod(modKey);
            mr.BonusFlat.RemoveMod(modKey);
        }
    }

    public override void onAfterHeal(APlayer p, Heal heal)
    {
        ApplyEffect(p);
    }

    public override void onShield(APlayer p, int amount)
    {
        ApplyEffect(p);
    }

    void ApplyEffect(APlayer p)
    {
        timer = duration;
        p.GetStat(Character.Stat.AR, out var ar);
        p.GetStat(Character.Stat.MR, out var mr);
        if (ar.BonusFlat.GetMod(modKey, out var modAR))
        {
            modAR.Value += deltaAR;
        }
        else
        {
            ar.BonusFlat.AddFlat(deltaAR, modKey);
        }

        if (mr.BonusFlat.GetMod(modKey, out var modMR))
        {
            modMR.Value += deltaMR;
        }
        else
        {
            mr.BonusFlat.AddFlat(deltaMR, modKey);
        }
    }


    public override ARelic makeCopy() => new ChaosCurse();
}

/// <summary>
/// 当球消失时会触发一道落雷
/// </summary>
public class DeathWish : ARelic
{
    public static string ID = "DeathWish";

    public DeathWish() : base(ID, "DeathWish.png", RelicTier.RARE, LandingSound.HEAVY) { }

    public override void onBallDeath(APlayer p, Ball ball)
    {
        effectManager.addLogic<LightningStrikeEffect2>().with(ball);
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

    public MidasTouch() : base(ID, "MidasTouch.png", RelicTier.RARE, LandingSound.SOLID) { }

    public override ARelic makeCopy() => new MidasTouch();
}

/// <summary>
/// 受到伤害时，AD+1，AP+1，AR+1，MR+1，持续6秒，最多叠加20层
/// </summary>
public class PowerDrain : ARelic
{
    public static string ID = "PowerDrain";

    public PowerDrain() : base(ID, "PowerDrain.png", RelicTier.RARE, LandingSound.SOLID) { }

    int deltaAD;
    int deltaAP;
    int deltaAR;
    int deltaMR;
    float duration;
    int maxLayer;
    string modKey;

    int curLayer;
    Timer timer;

    protected override void onRelicDefSet(RelicDef d)
    {
        deltaAD = (int)d.Args[0];
        deltaAP = (int)d.Args[1];
        deltaAR = (int)d.Args[2];
        deltaMR = (int)d.Args[3];
        duration = d.Args[4];
        maxLayer = (int)d.Args[5];
        modKey = $"{ID}_{GetHashCode()}";
        curLayer = 0;
    }

    public override void onFixedUpdate(float dt)
    {
        if (timer.update(dt))
        {
            _player.GetStat(Character.Stat.AD, out var ad);
            _player.GetStat(Character.Stat.AP, out var ap);
            _player.GetStat(Character.Stat.AR, out var ar);
            _player.GetStat(Character.Stat.MR, out var mr);
            ad.BonusFlat.RemoveMod(modKey);
            ap.BonusFlat.RemoveMod(modKey);
            ar.BonusFlat.RemoveMod(modKey);
            mr.BonusFlat.RemoveMod(modKey);
        }
    }

    public override void onTakeDamage(APlayer p, in Dmg dmg)
    {
        timer = duration;
        curLayer = (curLayer + 1).clamp(0, maxLayer);
        p.GetStat(Character.Stat.AD, out var ad);
        p.GetStat(Character.Stat.AP, out var ap);
        p.GetStat(Character.Stat.AR, out var ar);
        p.GetStat(Character.Stat.MR, out var mr);

        if (ad.BonusFlat.GetMod(modKey, out var modAD))
        {
            modAD.Value = deltaAD * curLayer;
        }
        else
        {
            ad.BonusFlat.AddFlat(deltaAD, modKey);
        }

        if (ap.BonusFlat.GetMod(modKey, out var modAP))
        {
            modAP.Value = deltaAP * curLayer;
        }
        else
        {
            ap.BonusFlat.AddFlat(deltaAP, modKey);
        }

        if (ar.BonusFlat.GetMod(modKey, out var modAR))
        {
            modAR.Value = deltaAR * curLayer;
        }
        else
        {
            ar.BonusFlat.AddFlat(deltaAR, modKey);
        }

        if (mr.BonusFlat.GetMod(modKey, out var modMR))
        {
            modMR.Value = deltaMR * curLayer;
        }
        else
        {
            mr.BonusFlat.AddFlat(deltaMR, modKey);
        }
    }

    public override ARelic makeCopy() => new PowerDrain();
}