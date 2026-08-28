namespace MoreMountains
{
    public class Ball_LaserBeamCrossed : MergedBall
    {
        protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LaserBeam_Crossed.prefab";

        public override BallType BallType => BallType.LaserBeam_Crossed;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            effectManager.addLogic<CrossedLaserBeamEffect>().with(path, this, e.brick);
        }
    }
}