namespace MoreMountains
{
    public class Ball_FissionMini : Ball
    {
        public override BallType BallType => BallType.FissionMini;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
        }
    }
}