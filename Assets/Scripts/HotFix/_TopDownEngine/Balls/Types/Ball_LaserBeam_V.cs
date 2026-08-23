namespace MoreMountains
{
    public class Ball_LaserBeam_V : Ball
    {
        protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LaserBeam_V.prefab";

        public override BallType BallType => BallType.LaserBeam_V;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<LaserBeamEffect>().with(path, this, e.brick, 0F);
        }
    }
}