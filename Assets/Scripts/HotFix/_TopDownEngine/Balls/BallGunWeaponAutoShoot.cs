namespace MoreMountains
{
    public class BallGunWeaponAutoShoot : WeaponAutoShoot
    {
        protected override bool CanAutoShoot()
        {
            if (OnlyAutoShootIfOwnerIsIdle)
            {
                if (_weapon.Owner.motionState.Not(Character.Motions.Idle))
                    return false;
            }

            return true;
        }

        protected override void HandleAutoShoot()
        {
            if (!CanAutoShoot())
                return;

            _weapon.WeaponInputStart();
        }
    }
}