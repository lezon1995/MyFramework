namespace MoreMountains
{
    public partial class Ball
    {
        protected virtual void playHitBrickSfx()
        {
            sound.play(SoundDefine.BALL_HIT_BRICK_COMMON);
        }
    
        protected virtual void playHitBrickVfx()
        {
            fx.play(FxDefine.BALL_HIT_BRICK, curPos);
        }

        protected virtual void playHitBorderSfx()
        {
            sound.play(SoundDefine.BALL_HIT_BORDER_COMMON);
        }

        protected virtual void playHitObstacleSfx()
        {
            sound.play(SoundDefine.BALL_HIT_BORDER_COMMON);
        }
    }
}