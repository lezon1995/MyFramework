using UnityEngine;

namespace MoreMountains
{
    public partial class Ball
    {
        protected virtual void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.BALL_HIT_BRICK_COMMON);
        }
    
        protected virtual void playHitBrickVfx(Brick brick, Vector2 normal)
        {
            fx.play(FxDefine.BALL_HIT_BRICK, curPos);
        }

        public virtual void playHitBorderSfx()
        {
            // sound.play(SoundDefine.BALL_HIT_BRICK_COMMON);
        }

        public virtual void playHitObstacleSfx()
        {
            // sound.play(SoundDefine.BALL_HIT_BRICK_COMMON);
        }
    }
}