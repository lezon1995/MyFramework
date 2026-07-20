using UnityEngine;

namespace MoreMountains
{
    public class Ball_LightningStrike : Ball
    {
        public override BallType BallType => BallType.LightningStrike;

        protected override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            effectManager.addLogic<LightningStrikeEffect>().with(this, brick);
            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }
    }
}