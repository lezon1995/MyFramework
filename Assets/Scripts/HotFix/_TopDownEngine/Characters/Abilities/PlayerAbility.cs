namespace MoreMountains
{
    public abstract class PlayerAbility : CharacterAbility
    {
        protected APlayer _player;

        protected override void Initialization()
        {
            base.Initialization();
            _player = _character as APlayer;
        }
    }
}