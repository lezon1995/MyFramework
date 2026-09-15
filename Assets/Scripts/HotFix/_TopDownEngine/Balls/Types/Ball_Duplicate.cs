namespace MoreMountains
{
    public class Ball_Duplicate : Ball
    {
        public override BallType BallType => BallType.Duplicate;

        public float duplicatedBallDuration = 3F;
        public float duplicateChance = 0.20F;
        float curBallDuration => duplicatedBallDuration * _player.durationPct;
        float curDuplicateChance => duplicateChance + _player.triggerChance;

        public override void onAcquire()
        {
            base.onAcquire();
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            //复制出来的球不具备再次复制的能力
            if (isTemp)
                return;

            if (randomHit(curDuplicateChance))
            {
                var direction = -Direction;
                var ball = Player.BallManagement.Instance.acquireBall(BallType, curPos, direction, level, duration: curBallDuration);
                ball.setTeleportPosition(curPos);
                ball.setTemp(true);

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

                fx.play(FxDefine.BALL_DUPLICATE, curPos);
            }
        }
    }
}