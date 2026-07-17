using MoreMountains.Tools;

namespace MoreMountains
{
    public abstract class EnemyAction : AIAction
    {
        protected Brick _character;
        protected TopDownController _controller => _character.Controller;
        protected CharacterMovement _movement => _character.Movement;

        protected override void Awake()
        {
            base.Awake();
            this.TryGetComponentInParent(out _character);
        }
    }
}