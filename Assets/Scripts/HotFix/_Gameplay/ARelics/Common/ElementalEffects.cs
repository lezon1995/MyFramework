using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// FireElemental - 火元素
/// 灼烧伤害+30%
/// </summary>
public class FireElemental : ARelic
{
    public static string ID = "FireElemental";

    public FireElemental() : base(ID, "FireElemental.png", RelicTier.RARE, LandingSound.HEAVY) { }

    // TODO: 需要实现灼烧增伤
    // public override void onEquip(APlayer p)
    // {
    //     p.BurningDamageMultiplier *= 1.3f;
    // }

    public override ARelic makeCopy() => new FireElemental();
}

/// <summary>
/// 对移动受损的敌人造成的伤害提高20%
/// </summary>
public class IceElemental : ARelic
{
    public static string ID = "IceElemental";

    public IceElemental() : base(ID, "IceElemental.png", RelicTier.RARE, LandingSound.SOLID) { }

    float damage;

    protected override void onRelicDefSet(RelicDef d)
    {
        damage = d.Args[0];
    }

    public override void onPlayerBeforeHandleImpactDamage(APlayer p, Ball ball, Brick brick, ref Dmg dmg)
    {
        brick.GetStat(Character.Stat.MS, out var stat);
        if (stat.BonusValue < 0 || brick.conditionState.Is(Character.Conditions.Frozen, Character.Conditions.Stunned))
        {
            dmg.addDmgRate(damage);
        }
    }

    public override void onPlayerBeforeHandleSkillDamage(APlayer p, Ball ball, Brick brick, ref Dmg dmg)
    {
        brick.GetStat(Character.Stat.MS, out var stat);
        if (stat.BonusValue < 0 || brick.conditionState.Is(Character.Conditions.Frozen, Character.Conditions.Stunned))
        {
            dmg.addDmgRate(damage);
        }
    }

    public override ARelic makeCopy() => new IceElemental();
}

/// <summary>
/// LightningElemental - 雷元素
/// 闪电链伤害+50%
/// </summary>
public class LightningElemental : ARelic
{
    public static string ID = "LightningElemental";

    public LightningElemental() : base(ID, "LightningElemental.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    // TODO: 需要实现闪电链增伤
    // public override void onEquip(APlayer p)
    // {
    //     p.LightningChainDamageMultiplier *= 1.5f;
    // }

    public override ARelic makeCopy() => new LightningElemental();
}

/// <summary>
/// 你会在身后留下一条毒气踪迹，对经过它的敌人造成每秒施加1层【中毒】。 踪迹持续3秒。
/// </summary>
public class PoisonElemental : ARelic, IEvent<OnBrickDeath>
{
    public static string ID = "PoisonElemental";

    public PoisonElemental() : base(ID, "PoisonElemental.png", RelicTier.RARE, LandingSound.SOLID) { }
    protected const string fx_path_flash = $"{GAMEPLAY_PATH}/Effects/Fx_Poison_DOT_Flash.prefab";
    protected const string fx_path_fog = $"{GAMEPLAY_PATH}/Effects/Fx_Poison_Fog.prefab";
    GameEffect effect;
    Queue<Sample> samples = new();
    List<Brick> tempBricks = new();
    Dictionary<Brick, float> brickTimes = new();
    float duration;
    float radius;
    float applyTimeGap;
    float sampleDistanceGap;
    float distanceAccumulated;
    float timeAccumulated;
    float sampleTimeGap;
    Vector3 lastPosition;

    protected override void onRelicDefSet(RelicDef d)
    {
        duration = d.Args[0];
        radius = d.Args[1];
        applyTimeGap = d.Args[2];
        sampleDistanceGap = 0.5F;
        sampleTimeGap = 0.5F;
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);

        var pos = p.getWorldPosition();
        effect = mEffectManager.createEffect(fx_path_fog);
        effect.setWorldPosition(pos);
        lastPosition = pos;
        distanceAccumulated = 0F;
        timeAccumulated = 0F;
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);

        mEffectManager.destroyEffect(ref effect);
        samples.Clear();
        tempBricks.Clear();
        brickTimes.Clear();
    }

    public override void onUpdate(float dt)
    {
        effect?.setWorldPosition(_player.getWorldPosition());

        foreach (var sample in samples)
        {
            Drawing.Draw.editor.xy.Circle(sample.pos, radius, Color.magenta);
        }
    }

    public override void onFixedUpdate(float dt)
    {
        var now = Time.fixedTime;
        var pos = _player.getWorldPosition();
        TryAddSample(dt, pos, now);
        TryRemoveExpiredSamples(now);

        tempBricks.Clear();
        using var _ = new ListScope<Collider2D>(out var colliders);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(BRICK_LAYER_MASK);
        foreach (var sample in samples)
        {
            var count = Physics2D.OverlapCircle(sample.pos, radius, filter, colliders);
            for (var i = 0; i < count; i++)
            {
                if (colliders[i].TryGetComponent(out Brick b))
                {
                    if (b.IsDead())
                        continue;

                    tempBricks.addUnique(b);
                }
            }
        }

        foreach (var b in tempBricks)
        {
            bool needApply = false;
            if (brickTimes.ContainsKey(b))
            {
                brickTimes[b] += dt;
                if (brickTimes[b] >= applyTimeGap)
                {
                    brickTimes[b] -= applyTimeGap;
                    needApply = true;
                }
            }
            else
            {
                needApply = true;
                brickTimes[b] = 0F;
                b.Event.addListener<OnBrickDeath>(this);
            }

            if (needApply)
            {
                PoisonBurningEffect.Apply(_player, b);
                var eff = mEffectManager.createEffect(fx_path_flash, 0.5333334F, b.gameObject);
                eff.setPosition(Vector3.zero);
                eff.setScale(Vector3.one);
            }
        }

        lastPosition = pos;
    }

    public void onEvent(OnBrickDeath e)
    {
        var b = e.brick;
        b.Event.removeListener<OnBrickDeath>(this);
        brickTimes.Remove(b);
    }

    void TryRemoveExpiredSamples(float now)
    {
        while (samples.TryPeek(out var sample) && (now - sample.timestamp) > duration)
        {
            samples.Dequeue();
        }
    }

    void TryAddSample(float dt, Vector3 pos, float now)
    {
        var deltaDistance = (pos - lastPosition).magnitude;
        bool hasSampledByDistance = false;
        distanceAccumulated += deltaDistance;
        while (distanceAccumulated >= sampleDistanceGap)
        {
            distanceAccumulated -= sampleDistanceGap;
            samples.Enqueue(new(pos, now));
            hasSampledByDistance = true;
            timeAccumulated = 0F;
        }

        if (!hasSampledByDistance)
        {
            timeAccumulated += dt;
            if (timeAccumulated >= sampleTimeGap)
            {
                timeAccumulated -= sampleTimeGap;
                samples.Enqueue(new(pos, now));
                distanceAccumulated = (distanceAccumulated - sampleDistanceGap).clampMin();
            }
        }
    }

    struct Sample
    {
        public Vector3 pos;
        public float timestamp;

        public Sample(Vector3 p, float t)
        {
            pos = p;
            timestamp = t;
        }
    }


    public override ARelic makeCopy() => new PoisonElemental();
}

/// <summary>
/// EarthElemental - 土元素
/// 护甲+10
/// </summary>
public class EarthElemental : ARelic
{
    public static string ID = "EarthElemental";

    public EarthElemental() : base(ID, "EarthElemental.png", RelicTier.RARE, LandingSound.HEAVY) { }

    public override ARelic makeCopy() => new EarthElemental();
}

/// <summary>
/// WindElemental - 风元素
/// 移速+20%
/// </summary>
public class WindElemental : ARelic
{
    public static string ID = "WindElemental";

    public WindElemental() : base(ID, "WindElemental.png", RelicTier.RARE, LandingSound.SOLID) { }

    public override ARelic makeCopy() => new WindElemental();
}

/// <summary>
/// WaterElemental - 水元素
/// 生命回复+100%
/// </summary>
public class WaterElemental : ARelic
{
    public static string ID = "WaterElemental";

    public WaterElemental() : base(ID, "WaterElemental.png", RelicTier.RARE, LandingSound.SOLID) { }

    // TODO: 需要实现生命回复翻倍
    // public override void onEquip(APlayer p)
    // {
    //     p.HealthRegenMultiplier *= 2f;
    // }

    public override ARelic makeCopy() => new WaterElemental();
}

/// <summary>
/// LightElemental - 光元素
/// 暴击伤害+50%
/// </summary>
public class LightElemental : ARelic
{
    public static string ID = "LightElemental";

    public LightElemental() : base(ID, "LightElemental.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    public override ARelic makeCopy() => new LightElemental();
}

/// <summary>
/// DarkElemental - 暗元素
/// 治疗效果+20%
/// </summary>
public class DarkElemental : ARelic
{
    public static string ID = "DarkElemental";

    public DarkElemental() : base(ID, "DarkElemental.png", RelicTier.RARE, LandingSound.MAGICAL) { }

    public override ARelic makeCopy() => new DarkElemental();
}

/// <summary>
/// 球暴击时施加1次额外的撞击特效，暴击率+5%
/// </summary>
public class ChaosElemental : ARelic
{
    public static string ID = "ChaosElemental";

    public ChaosElemental() : base(ID, "ChaosElemental.png", RelicTier.SPECIAL, LandingSound.MAGICAL) { }

    public override void onBallImpactCrit(APlayer p, Ball ball, Brick brick, in Dmg dmg)
    {
        var e = new DoHitEffect(ball, brick, in dmg);
        effectManager.addLogic<BallDoHitEffect>().with(ball, e, 0.1F);
    }

    public override ARelic makeCopy() => new ChaosElemental();
}