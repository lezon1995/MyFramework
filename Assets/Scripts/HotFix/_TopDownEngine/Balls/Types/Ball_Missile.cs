namespace MoreMountains
{
    public class Ball_Missile : Ball
    {
        public override BallType BallType => BallType.Missile;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            if (Player.metaHandleWeapons.TryGetValue(Slot, out var metaHandleWeapon))
            {
                if (metaHandleWeapon.CurrentWeapon is MissileProjectileWeapon weapon)
                {
                    weapon.SetBallOwner(this);
                    weapon.SetAimTarget(e.brick.transform);
                }

                metaHandleWeapon.ShootStart();
            }
        }
    }
}