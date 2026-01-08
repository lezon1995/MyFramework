namespace MarbleHero;

public class CreateBreakGroupAction : AGameAction, IGameActionArgs<AMonster>
{
    static float GAP = 0.2F;
    ACreature creature;

    public void onCreate(AMonster monster)
    {
        duration = GAP;
        creature = monster;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        creature = null;
    }

    public override void update(float dt)
    {
        tickDuration(dt);
        if (isDone)
        {
            creature.createBrickGroup(GameActionManager.turn);
        }
    }
}