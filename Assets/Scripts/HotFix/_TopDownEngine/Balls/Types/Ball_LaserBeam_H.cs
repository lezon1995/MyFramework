namespace MoreMountains
{
    public class Ball_LaserBeam_H : Ball
    {
        protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LaserBeam_H.prefab";
        
        public override BallType BallType => BallType.LaserBeam_H;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
            
            effectManager.addLogic<LaserBeamEffect>().with(path, this, e.brick, 90F);
        }
    }
}