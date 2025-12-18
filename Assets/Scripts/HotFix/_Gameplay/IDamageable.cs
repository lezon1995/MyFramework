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
    public Types actualType { get; set; }
    public Algos algo;
    public bool isCrit;
    public float critRate;
    public float dmgRate;
    public bool isSelf { get; set; }
    public float damageRaw { get; set; }
    public float damageDealt { get; set; }
    public Vector3 direction { get; set; }

    public Mixed mix { get; set; }

    public static Dmg physicDmg(float value) => new(value, Types.PHYSIC, false);
    public static Dmg magicDmg(float value) => new(value, Types.MAGIC, false);
    public static Dmg trueDmg(float value) => new(value, Types.TRUE, false);

    public Dmg(float v, Types t, bool crit)
    {
        effect = Effects.Attack;
        value = v;
        type = actualType = t;
        algo = Algos.FIXED;
        isCrit = crit;
        critRate = 2F;
        dmgRate = 1F;
        damageRaw = 0F;
        damageDealt = 0F;
        direction = Vector3.zero;
        isSelf = false;
        mix = default;
    }

    public Dmg(float v, Types t, Algos a)
    {
        effect = Effects.Attack;
        value = v;
        type = actualType = t;
        algo = a;
        isCrit = false;
        critRate = 2F;
        dmgRate = 1F;
        damageRaw = 0F;
        damageDealt = 0F;
        direction = Vector3.zero;
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

    public bool hasAttackEffect()
    {
        return (effect & Effects.Attack) != 0;
    }

    public Dmg setAttackEffect()
    {
        effect = Effects.Attack;
        return this;
    }
    
    public bool hasAbilityEffect()
    {
        return (effect & Effects.Ability) != 0;
    }

    public Dmg setAbilityEffect()
    {
        effect = Effects.Ability;
        return this;
    }

    public Dmg addAttackEffect()
    {
        effect |= Effects.Attack;
        return this;
    }

    public Dmg addAbilityEffect()
    {
        effect |= Effects.Ability;
        return this;
    }

    public Dmg setDamageRaw(float damage)
    {
        damageRaw = damage;
        return this;
    }

    public Dmg setDamageDealt(float damage)
    {
        damageDealt = damage;
        return this;
    }

    public Dmg setDirection(Vector3 dir)
    {
        direction = dir;
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

    public Dmg setSelf()
    {
        isSelf = true;
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
        Attack = 1 << 0,
        Ability = 1 << 1,
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

        public float Sum()
        {
            return physicDamageDealt + magicDamageDealt + trueDamageDealt;
        }
    }
}

public interface IDamageable<in Attacker>
{
    bool canTakeDamageThisFrame(out ResistDamageType resistType);
    void damage(Dmg dmg, GameObject instigator, Attacker source, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null);
    bool kill();
    bool isDead();
}

public interface IDmgCalculator
{
    float computeDamageAlgo(Dmg.Algos algo, float value, float curHealth, float maxHealth);
    float computeDamageCrit(Dmg dmg, float damage);
    float computeDamageRate(Dmg dmg, float damage);
    Dmg.Mixed computeDamageMix(Dmg.Mixed mix, float damage, float physicResist, float magicResist);
    float computeDamageDefence(Dmg.Types type, float damage, float physicResist, float magicResist);
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

    public float computeDamageRate(Dmg dmg, float damage)
    {
        return damage * dmg.dmgRate;
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

    public float computeDamageDefence(Dmg.Types type, float damage, float physicResist, float magicResist)
    {
        return type switch
        {
            Dmg.Types.PHYSIC => damage / (physicResist / 100 + 1),
            Dmg.Types.MAGIC => damage / (magicResist / 100 + 1),
            Dmg.Types.TRUE => damage,
            _ => damage
        };
    }
}