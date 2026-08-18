namespace MoreMountains
{
    /// <summary>
    /// 水滩球
    /// </summary>
    public class Ball_WaterSpot : Ball
    {
        public override BallType BallType => BallType.WaterSpot;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            effectManager.addLogic<WaterSpotEffect>().with(this, e.brick);
        }
    }
}