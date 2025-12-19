namespace MarbleHero;

public class TopRowRandomBrickGroup : BrickGroup
{
    protected override int getBrickAverageCount(int turnCount)
    {
        var cols = brickManager.brickLayout.getCols();
        var avg = cols / 2;
        return avg;
    }

    public override void createBricks(int turnCount)
    {
        var health = turnCount;
        int count = getBrickCount(turnCount);
        var topRowGrids = brickManager.brickLayout.getTopRowGrids();

        using var _ = ListScope<int>.get(out var selectIndexes);
        randomSelect(topRowGrids.count(), count, selectIndexes);
        foreach (var index in selectIndexes)
        {
            var rect = topRowGrids.get(index);
            var brick = brickManager.acquireBrick(rect.center, rect.size, health);
            brick.eventRouter.addListener(this);
            addBrick(brick);
        }
    }
}
