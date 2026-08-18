namespace MoreMountains
{
    public class Ball_RockSplash : Ball
    {
        public override BallType BallType => BallType.RockSplash;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<RockSplashEffect>().with(this, e.brick);
        }
    }
}