using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
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
            foreach (var ball in player.BallManagement.Instance.getActiveBalls())
            {
                ball.setEnabled(false);
                ballStartPos[ball.instanceID] = ball.getWorldPosition();
            }
        }

        public override void resetProperty()
        {
            base.resetProperty();
            targetPosition = default;
            curve = null;
            ballStartPos.Clear();
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
            foreach (var ball in player.BallManagement.Instance.getActiveBalls())
            {
                var startPos = ballStartPos[ball.instanceID];
                var curPos = lerp(startPos, targetPosition, t);
                ball.setWorldPosition(curPos);
            }

            if (isDone)
            {
                using var _ = new ListScope<Ball>(out var list);
                list.AddRange(player.BallManagement.Instance.getActiveBalls());
                foreach (var ball in list)
                {
                    ball.setWorldPosition(targetPosition);
                    player.BallManagement.Instance.enqueueBallToShootQueue(ball);
                }

                player.setOriginalShootPositionX(targetPosition.x);
            }
        }
    }
}