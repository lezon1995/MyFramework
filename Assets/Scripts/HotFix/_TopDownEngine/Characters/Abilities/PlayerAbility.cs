namespace MoreMountains
{
    public abstract class PlayerAbility : CharacterAbility
    {
        protected APlayer _player;
        public APlayer Player => _player;

        protected BallTestWeapon _testWeapon;

        protected override void Initialization()
        {
            base.Initialization();
            _player = _character as APlayer;
        }
    }
}