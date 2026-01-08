namespace MarbleHero
{
    public class AddScoreAction : AGameAction
    {
        public override void update(float dt)
        {
            player.addExp(GameActionManager.turnScore);
            isDone = true;
        }
    }
}