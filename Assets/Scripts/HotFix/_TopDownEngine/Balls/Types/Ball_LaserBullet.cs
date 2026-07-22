using UnityEngine;

namespace MoreMountains
{
    public class Ball_LaserBullet : Ball
    {
        public override BallType BallType => BallType.LaserBullet;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<LaserBulletEffect>().with(this, e.brick, e.hitDir);
        }
        
        protected override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }
    }
}