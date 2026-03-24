namespace MarbleHero
{
    public class GainExpAction : AGameAction
    {
        public override void update(float dt)
        {
            player.gainExp(GameActionManager.turnExp);
            isDone = true;
        }
    }
}