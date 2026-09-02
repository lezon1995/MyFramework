using UnityEngine;

namespace MoreMountains
{
    public class Ball_ChainSaw : Ball
    {
        public override BallType BallType => BallType.ChainSaw;

        Countdown countdown;
        
        protected override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            countdown = 5;
            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }
        
        protected override void OnFixedUpdateOverlappingBrick(Brick brick)
        {
            if (countdown.update())
            {
                countdown = 5;
                var hitDmg = getHitDmg(brick, lastHitNormal);
                DamageOnTouch.Colliding(brick, hitDmg);
                fx.play(FxDefine.CLAW_FLASH, brick.getWorldPosition());
                sound.play(SoundDefine.CLAW_HIT);
            }
        }
        
        protected override void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.CLAW_HIT);
        }
    
        protected override void playHitBrickVfx(Brick brick)
        {
            fx.play(FxDefine.CLAW_FLASH, brick.getWorldPosition());
        }

    }
}