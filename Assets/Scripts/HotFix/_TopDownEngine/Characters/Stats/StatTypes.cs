using System;
using System.Collections;

namespace MoreMountains
{
    public static class StatKey
    {
        public static IEnumerable Names = new string[]
        {
            string.Empty ,
            Stats.HealthMax ,
            Stats.HealthRegen ,
            Stats.ManaMax ,
            Stats.ManaRegen ,
            Stats.AD ,
            Stats.AR ,
            Stats.AD_PT ,
            Stats.AD_PT_Rate ,
            Stats.AP ,
            Stats.MR ,
            Stats.AP_PT ,
            Stats.AP_PT_Rate ,
            Stats.AS ,
            Stats.CD ,
            Stats.MS ,
            Stats.CritChance ,
            Stats.CritDamage ,
            Stats.DmgRate ,
            Stats.AF ,
            Stats.LifeSteal ,
        };
    }

    public static class StatExtensions
    {
        public static string Key(this Character.Stat stat)
        {
            return stat switch
            {
                Character.Stat.HealthMax => Stats.HealthMax,
                Character.Stat.HealthRegen => Stats.HealthRegen,
                Character.Stat.ManaMax => Stats.ManaMax,
                Character.Stat.ManaRegen => Stats.ManaRegen,
                Character.Stat.AD => Stats.AD,
                Character.Stat.AR => Stats.AR,
                Character.Stat.AD_PT => Stats.AD_PT,
                Character.Stat.AD_PT_Rate => Stats.AD_PT_Rate,
                Character.Stat.AP => Stats.AP,
                Character.Stat.MR => Stats.MR,
                Character.Stat.AP_PT => Stats.AP_PT,
                Character.Stat.AP_PT_Rate => Stats.AP_PT_Rate,
                Character.Stat.AS => Stats.AS,
                Character.Stat.CD => Stats.CD,
                Character.Stat.MS => Stats.MS,
                Character.Stat.CritChance => Stats.CritChance,
                Character.Stat.CritDamage => Stats.CritDamage,
                Character.Stat.DmgRate => Stats.DmgRate,
                Character.Stat.AdaptiveForce => Stats.AF,
                Character.Stat.LifeSteal => Stats.LifeSteal,
                Character.Stat.Range => Stats.Range,
                Character.Stat.DodgeChance => Stats.DodgeChance,
                Character.Stat.BallisticSpeed => Stats.BallisticSpeed,
                Character.Stat.HitEffectChance => Stats.HitEffectChance,
                Character.Stat.Knockback => Stats.Knockback,
                Character.Stat.Duration => Stats.Duration,
                Character.Stat.Luck => Stats.Luck,
                Character.Stat.Greed => Stats.Greed,
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }

        public static string Key(this Weapon.Stat stat)
        {
            return stat switch
            {
                Weapon.Stat.AD => "AD",
                Weapon.Stat.AP => "AP",
                Weapon.Stat.AS => "AS",
                Weapon.Stat.CD => "CD",
                Weapon.Stat.CritChance => Stats.CritChance,
                Weapon.Stat.CritDamage => Stats.CritDamage,

                Weapon.Stat.Range => "Range",
                Weapon.Stat.Scale => "Scale",
                Weapon.Stat.Duration => Stats.Duration,
                Weapon.Stat.Count => "Count",
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }
        
        public static string Key(this Ball.Stat stat)
        {
            return stat switch
            {
                Ball.Stat.HitDamage => "HitDamage",
                Ball.Stat.EffectDamage => "EffectDamage",
                Ball.Stat.HitDamageRate => "HitDamageRate",
                Ball.Stat.EffectDamageRate => "EffectDamageRate",
                Ball.Stat.AS => Stats.AS,
                Ball.Stat.BallisticSpeed => Stats.BallisticSpeed,
                Ball.Stat.CritChance => Stats.CritChance,
                Ball.Stat.CritDamage => Stats.CritDamage,
                Ball.Stat.DmgRate => Stats.DmgRate,
                Ball.Stat.Duration => Stats.Duration,
                Ball.Stat.HealthMax => Stats.HealthMax,
                Ball.Stat.HitEffectChance => Stats.HitEffectChance,
                Ball.Stat.Knockback => Stats.Knockback,
                Ball.Stat.LifeSteal => Stats.LifeSteal,
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }
    }
}