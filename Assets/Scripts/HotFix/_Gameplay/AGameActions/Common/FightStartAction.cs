namespace MoreMountains
{
    public class FightStartAction : AGameAction
    {
        public override void update(float dt)
        {
            room.isFightStarted = true;
            room.onCombatFightStart();
            isDone = true;
        }
    }
}