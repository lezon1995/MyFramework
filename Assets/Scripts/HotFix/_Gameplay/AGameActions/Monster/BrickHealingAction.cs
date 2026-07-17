using System.Collections.Generic;

namespace MoreMountains;

public class BrickHealingAction : AGameAction, IArgs<int, int>
{
    const float GAP = 0.05F;
    List<Brick> bricks = new();
    bool lastOne;
    int healingAmount;

    public void onCreate(int count, int amount)
    {
        duration = GAP;
        bricks.Clear();
        room.getAllBricks(ref bricks);

        using var _ = new ListScope<Brick>(out var selectedBricks);
        bricks.randomTake(count, ref selectedBricks);
        bricks.setRange(selectedBricks);
        healingAmount = amount;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        lastOne = false;
        bricks.Clear();
        healingAmount = 0;
    }

    public override void update(float dt)
    {
        if (duration.unstarted)
        {
            if (bricks.tryTakeOne(out var brick, out var remain))
            {
                effectManager.addLogic<BrickHealingEffect>().with(brick, healingAmount);
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