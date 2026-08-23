namespace MoreMountains
{
    public class Ball_LaserBeam : Ball
    {
        protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LaserBeam.prefab";

        public override BallType BallType => BallType.LaserBeam;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            effectManager.addLogic<LaserBeamEffect>().with(path, this, e.brick, randomFloat(0F, 360F));
        }
    }
}