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
                metaHandleWeapon.CurrentWeapon.SetAimTarget(e.brick.transform);
                metaHandleWeapon.ShootStart();
            }
        }
    }
}