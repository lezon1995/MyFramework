namespace MoreMountains
{
    public class EnemyBulletWeapon : ProjectileWeapon
    {
        protected override int Damage
        {
            get
            {
                var damage = (float)BaseDamage;
                return (int)damage;
                // return (int)DamageModifier.SafeInvoke(ref damage);
            }
        }
        
        protected override Dmg Dmg => new(Damage, Dmg.Types.AP, IsCritThisFrame, CritDamageThisFrame);

        protected override void OnOwnerStatsSet()
        {
            var characterAS = Owner.GetStat(Character.Stat.AS);
            var weaponAS = GetStat(Stat.AS);
            //Weapon的DelayBeforeUseF = (1 + Character.AS + Weapon.AS) * Weapon.DelayBeforeUseF
            // DelayBeforeUseModifier = (ref float raw) =>
            // {
            //     float totalAS = 0F;
            //     if (characterAS)
            //         totalAS += characterAS.Value;
            //     if (weaponAS)
            //         totalAS += weaponAS.Value;
            //
            //     var baseWindupTime = DelayBeforeUsePct / characterAS.Initial;
            //     var currentAttackTotalTime = 1 / totalAS;
            //
            //     var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
            //     raw = windupTime;
            // };

            //Weapon的TimeBetweenUsesF = (1 + Character.AS + Weapon.AS) * Weapon.TimeBetweenUsesF
            // TimeBetweenUsesModifier = (ref float raw) =>
            // {
            //     float totalAS = 0F;
            //     if (characterAS)
            //         totalAS += characterAS.Value;
            //     if (weaponAS)
            //         totalAS += weaponAS.Value;
            //
            //     float baseWindupTime = 0F;
            //     if (characterAS.Initial > 0)
            //         baseWindupTime = DelayBeforeUsePct / characterAS.Initial;
            //
            //     float currentAttackTotalTime = 0F;
            //     if (totalAS > 0)
            //         currentAttackTotalTime = 1 / totalAS;
            //
            //     var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
            //     raw = currentAttackTotalTime - windupTime;
            // };

            var characterAP = Owner.GetStat(Character.Stat.AP);
            var weaponAP = GetStat(Stat.AP);
            //Weapon的Damage = (Character.AD + Weapon.AD) * Weapon.AD_Coeff
            DamageModifier = (ref float raw) =>
            {
                float v1 = 0F, v2 = 0F;

                if (characterAP)
                    v1 = characterAP.Value;

                if (weaponAP)
                    v2 = weaponAP.Value;

                raw = v1 + v2;
            };
        }
    }
}