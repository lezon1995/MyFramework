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
    }
}