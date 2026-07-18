using MoreMountains.Tools;

namespace MoreMountains
{
    public abstract class EnemyDecision : AIDecision
    {
        protected Brick brick;

        protected override void Awake()
        {
            base.Awake();
        }
        
        public override void Initialization()
        {
            base.Initialization();
            this.TryGetComponentInParent(out brick);
        }
    }
}