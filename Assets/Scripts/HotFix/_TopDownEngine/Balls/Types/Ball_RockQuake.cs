using UnityEngine;

namespace MoreMountains
{
    public class Ball_RockQuake : Ball
    {
        public override BallType BallType => BallType.RockQuake;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<RockQuakeEffect>().with(this, e.brick);
        }
    }
}