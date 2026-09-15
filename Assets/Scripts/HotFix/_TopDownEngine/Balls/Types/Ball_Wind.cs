using UnityEngine;

namespace MoreMountains
{
    public class Ball_Wind : Ball
    {
        public override BallType BallType => BallType.Wind;
        
        protected override void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.BALL_HIT_PASS_THROUGH);
        }
    
        protected override void playHitBrickVfx(Brick brick, Vector2 normal)
        {
            fx.play(FxDefine.BALL_HIT_BRICK, curPos);
        }

    }
}