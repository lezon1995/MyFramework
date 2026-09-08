namespace MoreMountains
{
    public class ObstacleBrick : Brick
    {
        protected override bool registerToVolumeManager => false;
        
        public override void setBrickDef(BrickDef d)
        {
            def = d;
        }
    }
}