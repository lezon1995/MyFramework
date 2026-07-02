namespace MarbleHero;

public partial class Ball
{
    protected virtual void playHitBrickSfx()
    {
        sound.play(SoundDefine.BALL_HIT_BRICK_COMMON);
    }

    protected virtual void playHitBorderSfx()
    {
        sound.play(SoundDefine.BALL_HIT_BORDER_COMMON);
    }
}