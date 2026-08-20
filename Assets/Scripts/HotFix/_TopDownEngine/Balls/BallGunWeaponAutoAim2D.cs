using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// The 2D version of the WeaponAutoAim, meant to be used on objects equipped with a WeaponAim2D.
    /// It'll detect targets within the defined radius, pick the closest, and force the WeaponAim component to aim at them if a target is found
    /// </summary>
    [RequireComponent(typeof(WeaponAim2D))]
    public class BallGunWeaponAutoAim2D : WeaponAutoAim2D
    {
        public override float scanRadius
        {
            get
            {
                float range = 0F;
                var weaponRange = _weapon?.GetStat(Weapon.Stat.Range);
                if (weaponRange)
                    range += weaponRange.Value;
                
                var characterRange = _weapon?.Owner?.GetStat(Character.Stat.Range);
                if (characterRange)
                    range += characterRange.Value;
                
                if (range == 0F)
                    range = ScanRadius;
                
                return range;
            }
        }

        protected override void Initialization()
        {
            base.Initialization();
        }
    }
}