using MoreMountains.Tools;

namespace MoreMountains
{
    public abstract class EnemyAction : AIAction
    {
        protected Brick _brick;
        protected TopDownController2D _controller2D => _brick.Controller2D;
        protected CharacterMovement _movement => _brick.Movement;

        protected override void Awake()
        {
            base.Awake();
            this.TryGetComponentInParent(out _brick);
        }
    }
}