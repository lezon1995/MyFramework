using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class Ball_Creation : Ball, IEvent<OnBrickDeathTotally>
    {
        public override BallType BallType => BallType.Creation;

        public BrickDef ObstacleBrickDef;
        Chance chance;

        public override void onAcquire()
        {
            base.onAcquire();
            chance = 0.25F;
        }

        public override void onRelease()
        {
            base.onRelease();
        }

        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);
        }

        protected override bool OnCollidingWithBrick(Brick brick, Vector2 normal, Ball ball)
        {
            if (brickManager.brickDamageTimers.tryGetValue(brick, out var damageTimer))
            {
                damageTimer.Item2.reset();
            }
            else
            {
                brickManager.brickDamageTimers.add(brick, (this, 3F));
                brick.Event.addListener<OnBrickDeathTotally>(this);
            }

            return base.OnCollidingWithBrick(brick, normal, ball);
        }

        public void onEvent(OnBrickDeathTotally e)
        {
            e.brick.Event.removeListener<OnBrickDeathTotally>(this);

            if (e.brick is not ObstacleBrick)
            {
                if (chance.check())
                {
                    brickManager.acquireBrick(ObstacleBrickDef, e.brick.getWorldPosition());
                }
            }
        }
    }
}