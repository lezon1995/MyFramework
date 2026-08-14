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
    }
}