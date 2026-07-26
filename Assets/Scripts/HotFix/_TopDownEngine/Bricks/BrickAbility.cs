namespace MoreMountains
{
    public abstract class BrickAbility : CharacterAbility
    {
        protected Brick _brick;

        protected override void Initialization()
        {
            base.Initialization();
            _brick = _character as Brick;
        }
    }
}