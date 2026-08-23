namespace MoreMountains
{
    public class Ball_Wind : Ball
    {
        public override BallType BallType => BallType.Wind;
        
        protected override void playHitBrickSfx()
        {
            sound.play(SoundDefine.BALL_HIT_PASS_THROUGH);
        }
    
        protected override void playHitBrickVfx()
        {
            fx.play(FxDefine.BALL_HIT_BRICK, curPos);
        }

    }
}