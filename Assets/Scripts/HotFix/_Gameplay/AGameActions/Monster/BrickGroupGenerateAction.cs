namespace MarbleHero;

public class BrickGroupGenerateAction : AGameAction, IGameActionArgs<AMonster, BrickGroup>
{
    const float GAP = 0.05F;
    ACreature creature;
    BrickGroup brickGroup;
    bool lastOne;

    public void onCreate(AMonster monster, BrickGroup group)
    {
        duration = GAP;
        creature = monster;
        brickGroup = group;
        lastOne = false;
        group.buildBrickTemplates(GameActionManager.turn);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        creature = null;
        brickGroup = null;
        lastOne = false;
    }

    public override void update(float dt)
    {
        if (duration.unstarted)
        {
            if (brickGroup.tryTakeOne(out var t, out var remain))
            {
                brickGroup.createOne(t);
                if (remain == 0)
                {
                    lastOne = true;
                    isDone = true;
                }
            }
        }

        tickDuration(dt);
        if (isDone && !lastOne)
        {
            duration.reset();
            isDone = false;
        }
    }
}