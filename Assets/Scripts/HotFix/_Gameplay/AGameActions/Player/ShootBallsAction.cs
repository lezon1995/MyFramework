using UnityEngine;

namespace MarbleHero
{
    public class ShootBallsAction : AGameAction, IGameActionArgs<Vector3, Vector3>
    {
        const float GAP = 0.05F;

        Vector3 shootPos, shootDir;
        bool lastOne;

        public override void onCreate()
        {
            base.onCreate();
            duration = GAP;
            player.ballCount = player.ballMaxCount;
            player.isFirstBallReturn = false;
        }

        public void onCreate(Vector3 pos, Vector3 dir)
        {
            shootPos = pos;
            shootDir = dir;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            shootPos = default;
            shootDir = default;
            lastOne = false;
        }

        public override void update(float dt)
        {
            if (duration.unstarted)
            {
                if (player.ballCount > 0)
                {
                    var ball = ballManager.acquireBall(shootPos, 0.14F, shootDir, 8F);
                    if (player.ballCount == player.ballMaxCount)
                    {
                        // ball.isFirst = true;
                    }

                    // CtrUI.instance.SetBallCount(ballCount);
                    player.activeBalls.Add(ball);
                    player.ballCount--;
                    if (player.ballCount == 0)
                    {
                        lastOne = true;
                        isDone = true;
                    }
                    
                    player.applyOnShootBallRelics(ball);
                }
            }

            tickDuration(dt);
            if (isDone)
            {
                if (!lastOne)
                {
                    duration.reset();
                    isDone = false;
                }
                else
                {
                    player.isEndingTurn = true;
                }
            }
        }
    }
}