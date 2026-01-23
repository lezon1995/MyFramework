namespace MarbleHero
{
    public class AddExpAction : AGameAction
    {
        public override void update(float dt)
        {
            player.addExp(GameActionManager.turnExp);
            isDone = true;
        }
    }
}