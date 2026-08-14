using UnityEngine;

namespace MoreMountains
{
    public class Ball_Laser : Ball
    {
        public override BallType BallType => BallType.LaserBeam;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<LaserBeamEffect>().with(this, e.brick, randomFloat(0F, 360F));
        }
    }
}