namespace MarbleHero;

public class CreateBreakGroupAction : AGameAction
{
    static float GAP = 0.2F;
    ACreature creature;

    public CreateBreakGroupAction(AMonster monster)
    {
        duration = GAP;
        creature = monster;
    }

    public override void update(float dt)
    {
        tickDuration(dt);
        if (isDone)
        {
            gameplayManager.createBrickGroup(GameActionManager.turn);
        }
    }
}