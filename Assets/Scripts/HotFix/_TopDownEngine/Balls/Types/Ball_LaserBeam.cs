namespace MoreMountains
{
    public class Ball_LaserBeam : Ball
    {
        protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LaserBeam.prefab";

        public override BallType BallType => BallType.LaserBeam;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            var angle = randomFloat(0F, 360F);
            foreach (var p in powers)
            {
                if (p is ILaserBeamEffectAngleModifier m)
                {
                    m.modifyAngle(getWorldPosition(), ref angle);
                }
            }

            effectManager.addLogic<LaserBeamEffect>().with(path, this, angle);
        }
    }
}