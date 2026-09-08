namespace MoreMountains
{
    public class Ball_Bomb : Ball
    {
        public override BallType BallType => BallType.Bomb;
        
        Countdown countdown;

        public override void onAcquire()
        {
            base.onAcquire();
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            effectManager.addLogic<BombEffect>().with(this, e.brick);
        }
    }
}