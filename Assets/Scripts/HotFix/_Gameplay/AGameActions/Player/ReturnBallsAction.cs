using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero
{
    public class ReturnBallsAction : AGameAction, IArgs<Vector3>
    {
        const float DURATION = 0.25F;

        Vector3 targetPosition;
        MyCurve curve;
        Dictionary<int, Vector3> ballStartPos = new();

        public override void onCreate()
        {
            base.onCreate();
            duration = DURATION;
            curve = mKeyFrameManager.getKeyFrame(KEY_CURVE.CUBIC_OUT);
            ballStartPos.Clear();
            foreach (var ball in player.activeBalls)
            {
                ball.setEnabled(false);
                ballStartPos[ball.instanceID] = ball.getWorldPosition();
            }
        }
        
        public void onCreate(Vector3 nextPosition)
        {
            targetPosition = nextPosition;
        }

        public override void destroy()
        {
            duration = 0F;
            curve = null;
            ballStartPos.Clear();
            targetPosition = default;
            base.destroy();
        }

        public override void update(float dt)
        {
            tickDuration(dt);

            var t = curve.evaluate(duration.pct);
            foreach (var ball in player.activeBalls)
            {
                var startPos = ballStartPos[ball.instanceID];
                var curPos = lerp(startPos, targetPosition, t);
                ball.setWorldPosition(curPos);
            }

            if (isDone)
            {
                foreach (var ball in player.activeBalls)
                {
                    ball.setWorldPosition(targetPosition);
                    ballManager.releaseBall(ball);
                }

                player.activeBalls.Clear();
                player.setOriginalShootPositionX(targetPosition.x);
            }
        }
    }
}