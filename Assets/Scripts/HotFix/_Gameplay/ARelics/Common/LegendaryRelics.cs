using MoreMountains.Tools;
using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// Excalibur - 圣剑
/// 所有属性+30%
/// [设计文案] 传说中的圣剑
/// </summary>
public class Excalibur : ARelic
{
    public static string ID = "Excalibur";

    public Excalibur() : base(ID, "Excalibur.png", RelicTier.SPECIAL, LandingSound.HEAVY) { }

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

    public Mjolnir() : base(ID, "Mjolnir.png", RelicTier.SPECIAL, LandingSound.HEAVY) { }

    // TODO: 需要实现闪电巨伤害
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     // 造成大量闪电伤害
    // }

    public override ARelic makeCopy() => new Mjolnir();
}

/// <summary>
/// 拾取金币时有几率对附近的敌人发射一枚追踪子弹
/// </summary>
public class AegisOfOlympus : ARelic
{
    public static string ID = "AegisOfOlympus";

    public AegisOfOlympus() : base(ID, "AegisOfOlympus.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    public override void onPickupGold(int amount)
    {
        if (brickManager.getRandomActiveBrick(out var brick))
        {
            if (_player.relicMetaHandleWeapons.TryGetValue(item.Slot, out var metaHandleWeapon))
            {
                if (metaHandleWeapon.CurrentWeapon is MissileProjectileWeapon weapon)
                {
                    weapon.SetStatsOwner(_player);
                    weapon.SetAimTarget(brick.transform);
                }
            }

            metaHandleWeapon.ShootStart();
        }
    }

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

    public WingsOfIcarus() : base(ID, "WingsOfIcarus.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

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

    public EyeOfProvidence() : base(ID, "EyeOfProvidence.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

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

    public PhilosopherStoneUltimate() : base(ID, "PhilosopherStoneUltimate.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    public override ARelic makeCopy() => new PhilosopherStoneUltimate();
}

/// <summary>
/// 原地不动时球速+50%
/// </summary>
public class CrownOfTheUniverse : ARelic, IEvent<DoMove>
{
    public static string ID = "CrownOfTheUniverse";

    public CrownOfTheUniverse() : base(ID, "CrownOfTheUniverse.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    float standStillTime;
    float bonusBallisticSpeed;
    string modKey;
    NumMod<float> mod;

    Timer timer;

    protected override void onRelicDefSet(RelicDef d)
    {
        standStillTime = d.Args[0];
        bonusBallisticSpeed = d.Args[1];
        modKey = $"{ID}_{GetHashCode()}";
        timer = standStillTime;
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);

        p.GetStat(Character.Stat.BallisticSpeed, out var bs);
        mod = bs.BonusFlat.AddFlat(bonusBallisticSpeed, modKey);
        p.Event.addListener<DoMove>(this);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
        p.GetStat(Character.Stat.BallisticSpeed, out var bs);
        mod = null;
        bs.BonusFlat.RemoveMod(modKey);
        p.Event.removeListener<DoMove>(this);
    }

    public override void onFixedUpdate(float dt)
    {
        if (timer.update(dt))
        {
            mod.Value = bonusBallisticSpeed;
        }
    }

    public void onEvent(DoMove e)
    {
        mod.Value = 0;
        timer.reset();
    }
}

/// <summary>
/// HolyGrail - 圣杯
/// 无限生命回复
/// [设计文案] 神圣的圣杯
/// </summary>
public class HolyGrail : ARelic
{
    public static string ID = "HolyGrail";

    public HolyGrail() : base(ID, "HolyGrail.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

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

    public InfinityGauntlet() : base(ID, "InfinityGauntlet.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    // TODO: 需要实现无限宝石效果
    // public override void onEquip(APlayer p)
    // {
    //     p.BuffAllStats(1.0f); // 全属性翻倍
    //     p.HealToFull();
    // }

    public override ARelic makeCopy() => new InfinityGauntlet();
}

/// <summary>
/// 基于已损失生命值，获得（0-40）AP，（0-30%）移动速度和（0-10%）吸血。发射球时会减少1%最大生命值
/// </summary>
public class CelestialCore : ARelic
{
    public static string ID = "CelestialCore";

    public CelestialCore() : base(ID, "CelestialCore.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    float maxAP;
    float maxMS;
    float maxLS;
    float minHealthPct;
    float selfDamagePerShoot;
    UniStats.Stat ap;
    UniStats.Stat ms;
    UniStats.Stat ls;
    string modKey;

    protected override void onRelicDefSet(RelicDef d)
    {
        maxAP = d.Args[0];
        maxMS = d.Args[1];
        maxLS = d.Args[2];
        minHealthPct = d.Args[3];
        selfDamagePerShoot = d.Args[4];
        modKey = $"{ID}_{GetHashCode()}";
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
        p.GetStat(Character.Stat.AP, out ap);
        p.GetStat(Character.Stat.MS, out ms);
        p.GetStat(Character.Stat.LifeSteal, out ls);

        var f = inverseLerp(minHealthPct, 1F, p.currentHealthPct);
        var apValue = lerp(0F, maxAP, f);
        var msValue = lerp(0F, maxMS, f);
        var lsValue = lerp(0F, maxLS, f);

        ap.BonusFlat.AddFlat(apValue, modKey);
        ms.BonusPct.AddFlat(msValue, modKey);
        ls.BonusFlat.AddFlat(lsValue, modKey);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);

        ap.BonusFlat.RemoveMod(modKey);
        ms.BonusPct.RemoveMod(modKey);
        ls.BonusFlat.RemoveMod(modKey);

        ap = null;
        ms = null;
        ls = null;
    }

    public override void onShootBall(APlayer p, Ball ball)
    {
        if (p.currentHealthPct > selfDamagePerShoot)
        {
            var dmg = Dmg.True(selfDamagePerShoot).AllPct();
            dmg.SetIgnoreInvincible(true);
            p.Health.Damage(ref dmg, p.gameObject, p, direction: Vector3.up);
        }
    }

    public override void onPlayerHealthChanged(APlayer p, int curHealth)
    {
        ap.BonusFlat.GetMod(modKey, out var modAP);
        ms.BonusPct.GetMod(modKey, out var modMS);
        ls.BonusFlat.GetMod(modKey, out var modLS);

        var f = inverseLerp(minHealthPct, 1F, p.currentHealthPct);
        var apValue = lerp(0F, maxAP, f);
        var msValue = lerp(0F, maxMS, f);
        var lsValue = lerp(0F, maxLS, f);

        modAP.Value = apValue;
        modMS.Value = msValue;
        modLS.Value = lsValue;
    }

    public override ARelic makeCopy() => new CelestialCore();
}