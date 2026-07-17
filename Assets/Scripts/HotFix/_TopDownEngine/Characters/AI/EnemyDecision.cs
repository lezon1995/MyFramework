using MoreMountains.Tools;

namespace MoreMountains
{
    public abstract class EnemyDecision : AIDecision
    {
        protected Brick _character;

        protected override void Awake()
        {
            base.Awake();
        }
        
        public override void Initialization()
        {
            base.Initialization();
            this.TryGetComponentInParent(out _character);
        }
    }
}