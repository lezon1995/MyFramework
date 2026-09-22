using MoreMountains.Tools;

namespace MoreMountains
{
    public abstract class EnemyAction : AIAction , IEvent<OnDeath>
    {
        protected Brick _brick;
        protected TopDownController2D _controller2D => _brick.Controller2D;
        protected CharacterMovement _movement => _brick.Movement;
        protected override void Awake()
        {
            base.Awake();
            this.TryGetComponentInParent(out _brick);
        }
        
        public override void OnEnterState()
        {
            base.OnEnterState();
            _brick.Event.addListener<OnDeath>(this);
        }

        public override void OnExitState()
        {
            _brick.Event.removeListener<OnDeath>(this);
            base.OnExitState();
        }

        public virtual void onEvent(OnDeath e)
        {
        }
    }
}