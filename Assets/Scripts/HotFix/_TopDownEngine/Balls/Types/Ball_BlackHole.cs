namespace MoreMountains
{
    public class Ball_BlackHole : Ball
    {
        public override BallType BallType => BallType.BlackHole;
        
        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            effectManager.addLogic<BlackHoleEffect>().with(this, e.brick);
        }
    }
}