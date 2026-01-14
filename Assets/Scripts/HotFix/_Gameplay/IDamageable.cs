using System;
using UnityEngine;

namespace MarbleHero;

public enum ResistDamageType
{
    None,
    Invulnerable,
    DashInvincible,
    ImmuneToDamage,
    Dodged,
    Dead,
    Disabled,
}

[Serializable]
public struct Dmg
{
    public Effects effect;
    public float value;
    public Types type;
    public Types actualType;
    public Algos algo;
    public bool isCrit;
    public float critRate;
    public Stat dmgRate;
    public bool isSelf;
    public float damageRaw;
    public int damageDealt;
    public bool triggerEffect;
    public Vector3 direction;
    public Vector2 hitNormal;

    public Mixed mix;

    public static Dmg physicDmg(float value) => new(value, Types.PHYSIC, false);
    public static Dmg magicDmg(float value) => new(value, Types.MAGIC, false);
    public static Dmg trueDmg(float value) => new(value, Types.TRUE, false);

    public Dmg(float v, Types t, bool crit)
    {
        effect = Effects.Hit;
        value = v;
        type = actualType = t;
        algo = Algos.FIXED;
        isCrit = crit;
        critRate = 2F;
        dmgRate = 1F;
        damageRaw = 0F;
        damageDealt = 0;
        triggerEffect = true;
        direction = Vector3.zero;
        hitNormal = Vector2.zero;
        isSelf = false;
        mix = default;
    }

    public Dmg(float v, Types t, Algos a)
    {
        effect = Effects.Hit;
        value = v;
        type = actualType = t;
        algo = a;
        isCrit = false;
        critRate = 2F;
        dmgRate = 1F;
        damageRaw = 0F;
        damageDealt = 0;
        triggerEffect = true;
        direction = Vector3.zero;
        hitNormal = Vector2.zero;
        isSelf = false;
        mix = default;
    }

    public Dmg Fixed()
    {
        algo = Algos.FIXED;
        return this;
    }

    public Dmg CurPct()
    {
        algo = Algos.CUR_PCT;
        return this;
    }

    public Dmg LostPct()
    {
        algo = Algos.LOST_PCT;
        return this;
    }

    public Dmg AllPct()
    {
        algo = Algos.ALL_PCT;
        return this;
    }

    public Dmg setCrit()
    {
        isCrit = true;
        critRate = 2F;
        return this;
    }

    public Dmg setCrit(float critDamage)
    {
        isCrit = true;
        critRate = critDamage;
        return this;
    }

    public bool hasHitEffect()
    {
        return (effect & Effects.Hit) != 0;
    }

    public Dmg setHitEffect()
    {
        effect = Effects.Hit;
        return this;
    }

    public bool hasSkillEffect()
    {
        return (effect & Effects.Skill) != 0;
    }

    public Dmg setSkillEffect()
    {
        effect = Effects.Skill;
        return this;
    }

    public Dmg addHitEffect()
    {
        effect |= Effects.Hit;
        return this;
    }

    public Dmg addSkillEffect()
    {
        effect |= Effects.Skill;
        return this;
    }

    public Dmg setDamageRaw(float damage)
    {
        damageRaw = damage;
        return this;
    }

    public Dmg setDamageDealt(int damage)
    {
        damageDealt = damage;
        return this;
    }

    public Dmg setDirection(Vector3 dir)
    {
        direction = dir;
        return this;
    }

    public Dmg setHitNormal(Vector2 normal)
    {
        hitNormal = normal;
        return this;
    }

    public Dmg setActualType(Types t)
    {
        actualType = t;
        return this;
    }

    public Dmg setDmgRate(float rate)
    {
        dmgRate = rate;
        return this;
    }

    public Dmg addDmgRate(float delta)
    {
        dmgRate.increase(delta);
        return this;
    }

    public Dmg setSelf()
    {
        isSelf = true;
        return this;
    }

    public Dmg setTriggerEffect(bool v)
    {
        triggerEffect = v;
        return this;
    }

    public enum Types
    {
        PHYSIC,
        MAGIC,
        TRUE,
    }

    public enum Algos
    {
        FIXED,
        CUR_PCT,
        LOST_PCT,
        ALL_PCT,
    }

    [Flags]
    public enum Effects
    {
        Hit = 1 << 0,
        Skill = 1 << 1,
    }

    [Serializable]
    public struct Mixed
    {
        public bool on;
        public float physicPct;
        public float magicPct;
        public float truePct;

        public bool off => !on;
        public float physicDamageDealt { get; set; }
        public float magicDamageDealt { get; set; }
        public float trueDamageDealt { get; set; }

        public int Sum()
        {
            return (int)(physicDamageDealt + magicDamageDealt + trueDamageDealt);
        }
    }
}

public interface IReusable
{
    void onAcquire();
    void onRelease();
}

public interface IDamageable<in Attacker>
{
    bool canTakeDamageThisFrame(out ResistDamageType resistType);
    void damage(Dmg dmg, GameObject instigator, Attacker source, out bool killed, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null);
    bool kill();
    bool isDead();
}

public interface IDmgCalculator
{
    float computeDamageAlgo(Dmg.Algos algo, float value, float curHealth, float maxHealth);
    float computeDamageCrit(Dmg dmg, float damage);
    int computeDamageRate(Dmg dmg, float damage);
    Dmg.Mixed computeDamageMix(Dmg.Mixed mix, float damage, float physicResist, float magicResist);
    int computeDamageDefence(Dmg.Types type, float damage, float physicResist, float magicResist);
}

public class DmgCalculator : IDmgCalculator
{
    public static IDmgCalculator Default = new DmgCalculator();

    public float computeDamageAlgo(Dmg.Algos algo, float value, float curHealth, float maxHealth)
    {
        return algo switch
        {
            Dmg.Algos.FIXED => value,
            Dmg.Algos.CUR_PCT => curHealth * value,
            Dmg.Algos.LOST_PCT => (maxHealth - curHealth) * value,
            Dmg.Algos.ALL_PCT => maxHealth * value,
            _ => value
        };
    }

    public float computeDamageCrit(Dmg dmg, float damage)
    {
        return dmg.isCrit switch
        {
            true => damage * dmg.critRate,
            false => damage,
        };
    }

    public int computeDamageRate(Dmg dmg, float damage)
    {
        return (int)(damage * dmg.dmgRate);
    }

    public Dmg.Mixed computeDamageMix(Dmg.Mixed mix, float damage, float physicResist, float magicResist)
    {
        var physicDmg = mix.physicPct * damage;
        if (physicDmg > 0)
            mix.physicDamageDealt = computeDamageDefence(Dmg.Types.PHYSIC, physicDmg, physicResist, magicResist);

        var magicDmg = mix.magicPct * damage;
        if (magicDmg > 0)
            mix.magicDamageDealt = computeDamageDefence(Dmg.Types.MAGIC, magicDmg, physicResist, magicResist);

        var trueDmg = mix.truePct * damage;
        if (trueDmg > 0)
            mix.trueDamageDealt = computeDamageDefence(Dmg.Types.TRUE, trueDmg, physicResist, magicResist);

        return mix;
    }

    public int computeDamageDefence(Dmg.Types type, float damage, float physicResist, float magicResist)
    {
        return type switch
        {
            Dmg.Types.PHYSIC => (int)(damage / (physicResist / 100 + 1)),
            Dmg.Types.MAGIC => (int)(damage / (magicResist / 100 + 1)),
            Dmg.Types.TRUE => (int)damage,
            _ => (int)damage
        };
    }
}