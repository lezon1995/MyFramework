using UnityEngine;

namespace MoreMountains
{
    public class Ball_LightningStrike : Ball
    {
        public override BallType BallType => BallType.LightningStrike;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<LightningStrikeEffect>().with(this, e.brick);
        }
        
        protected override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }
    }
}