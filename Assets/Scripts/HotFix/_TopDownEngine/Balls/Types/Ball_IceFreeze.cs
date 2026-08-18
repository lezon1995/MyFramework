namespace MoreMountains
{
    public class Ball_IceFreeze : Ball
    {
        public override BallType BallType => BallType.IceFreeze;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<IceFreezeEffect>().with(this, e.brick);
        }
    }
}