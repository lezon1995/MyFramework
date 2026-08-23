using UnityEngine;

namespace MoreMountains
{
    public class Ball_Fission : Ball
    {
        public override BallType BallType => BallType.Fission;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            var direction = -Direction;
            if (randomHit(0.25F))
            {
                var ball = Player.BallManagement.Instance.acquireBall(BallType.FissionMini, curPos, direction);
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
                
                fx.play(FxDefine.BALL_FISSION, curPos);
            }
        }
    }
}