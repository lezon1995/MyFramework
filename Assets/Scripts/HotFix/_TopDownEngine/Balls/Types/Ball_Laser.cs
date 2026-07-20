using UnityEngine;

namespace MoreMountains
{
    public class Ball_Laser : Ball
    {
        public override BallType BallType => BallType.Laser;

        protected override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            effectManager.addLogic<LaserBeamEffect>().with(this, brick, randomFloat(0F, 360F));
            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }
    }
}