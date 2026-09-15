using UnityEngine;

namespace MoreMountains
{
    public class Ball_ChainSaw : Ball
    {
        public override BallType BallType => BallType.ChainSaw;

        Countdown countdown;
        
        public override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            countdown = 5;
            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }
        
        protected override void OnFixedUpdateOverlappingBrick(Brick brick, float dt)
        {
            if (countdown.update())
            {
                countdown = 5;
                CollidingWithBrick(brick, lastHitNormal);
                fx.play(FxDefine.CLAW_FLASH, brick.getWorldPosition());
                sound.play(SoundDefine.CLAW_HIT);
            }
        }
        
        protected override void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.CLAW_HIT);
        }
    
        protected override void playHitBrickVfx(Brick brick, Vector2 normal)
        {
            fx.play(FxDefine.CLAW_FLASH, brick.getWorldPosition());
        }

    }
}