namespace MoreMountains;

public interface IReusable
{
    bool inUse { get; set; }
    void onAcquire();
    void onRelease();
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
            Dmg.Algos.Fixed => value,
            Dmg.Algos.CurPct => curHealth * value,
            Dmg.Algos.LostPct => (maxHealth - curHealth) * value,
            Dmg.Algos.AllPct => maxHealth * value,
            _ => value
        };
    }

    public float computeDamageCrit(Dmg dmg, float damage)
    {
        return dmg.IsCrit switch
        {
            true => damage * dmg.CritRate,
            false => damage,
        };
    }

    public int computeDamageRate(Dmg dmg, float damage)
    {
        return (int)(damage * dmg.DmgRate);
    }

    public Dmg.Mixed computeDamageMix(Dmg.Mixed mix, float damage, float physicResist, float magicResist)
    {
        var physicDmg = mix.PctAD * damage;
        if (physicDmg > 0)
            mix.DamageDealtAD = computeDamageDefence(Dmg.Types.AD, physicDmg, physicResist, magicResist);

        var magicDmg = mix.PctAP * damage;
        if (magicDmg > 0)
            mix.DamageDealtAP = computeDamageDefence(Dmg.Types.AP, magicDmg, physicResist, magicResist);

        var trueDmg = mix.PctTrue * damage;
        if (trueDmg > 0)
            mix.DamageDealtTrue = computeDamageDefence(Dmg.Types.True, trueDmg, physicResist, magicResist);

        return mix;
    }

    public int computeDamageDefence(Dmg.Types type, float damage, float physicResist, float magicResist)
    {
        return type switch
        {
            Dmg.Types.AD => (int)(damage / (physicResist / 100 + 1)),
            Dmg.Types.AP => (int)(damage / (magicResist / 100 + 1)),
            Dmg.Types.True => (int)damage,
            _ => (int)damage
        };
    }
}