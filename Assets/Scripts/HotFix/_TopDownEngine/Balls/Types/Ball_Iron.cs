namespace MoreMountains
{
    public class Ball_Iron : Ball
    {
        public override BallType BallType => BallType.Iron;
        
        protected override void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.IRON_HIT);
        }
    }
}