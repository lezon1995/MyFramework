using UnityEngine;

namespace MoreMountains
{
    public class Ball_ElectricityStrike : Ball
    {
        public override BallType BallType => BallType.ElectricityStrike;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<ElectricityStrikeEffect>().with(this, e.brick, 1);
        }
        
        protected override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }
    }
}