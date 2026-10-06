namespace MoreMountains
{
    public class Ball_Missile : Ball
    {
        public override BallType BallType => BallType.Missile;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            if (Player.ballMetaHandleWeapons.TryGetValue(Slot, out var metaHandleWeapon))
            {
                if (metaHandleWeapon.CurrentWeapon is MissileProjectileWeapon weapon)
                {
                    weapon.SetStatsOwner(this);
                    weapon.SetAimTarget(e.brick.transform);
                }

                metaHandleWeapon.ShootStart();
            }
        }
    }
}