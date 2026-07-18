using MoreMountains.Tools;

namespace MoreMountains
{
    public abstract class EnemyAction : AIAction
    {
        protected Brick brick;
        protected TopDownController _controller => brick.Controller;
        protected CharacterMovement _movement => brick.Movement;

        protected override void Awake()
        {
            base.Awake();
            this.TryGetComponentInParent(out brick);
        }
    }
}