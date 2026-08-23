namespace MoreMountains
{
    public class Ball_Duplicate : Ball
    {
        public override BallType BallType => BallType.Duplicate;

        int hitCount;

        public override void onAcquire()
        {
            base.onAcquire();
            hitCount = 0;
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            var direction = -Direction;
            hitCount++;
            if (hitCount >= 2)
            {
                hitCount = 0;
                var ball = Player.BallManagement.Instance.acquireBall(BallType, curPos, direction);
                ball.setTeleportPosition(curPos);

                // we activate the object
                ball.setActive(true);

                ball.SetWeapon(_weapon);
                if (Owner)
                {
                    ball.SetOwner(Owner);
                    ball.SetPlayer(Player);
                    var dmg = _weapon.Dmg;
                    dmg.SetDmgRate(0.5F);
                    ball.SetDamage(dmg);
                }

                ball.setShootDirection(direction);
                ball.SetDirection(direction, transform.rotation);
                ball.setDuration(getDurationRemain());
                
                fx.play(FxDefine.BALL_DUPLICATE, curPos);
            }
        }
    }
}