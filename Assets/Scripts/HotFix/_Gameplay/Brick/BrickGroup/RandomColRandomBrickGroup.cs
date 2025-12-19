using UnityEngine;

namespace MarbleHero;

public class RandomColRandomBrickGroup : BrickGroup
{
    public override void createBricks(int turnCount)
    {
        var health = turnCount;
        int count = getBrickCount(turnCount);
        var cols = brickManager.brickLayout.getCols();
        using var _ = new ListScope2T<Rect, int>(out var grids, out var selectIndexes);

        int maxTry = 10;
        while (grids.Count == 0)
        {
            var randomCol = randomInt(0, cols - 1);
            brickManager.brickLayout.getGridsAtCol(ref grids, randomCol);

            for (var i = grids.Count - 1; i >= 0; i--)
            {
                if (brickManager.containsBrickAt(grids[i]))
                {
                    grids.removeAt(i);
                }
            }

            maxTry--;
            if (maxTry <= 0)
            {
                break;
            }
        }

        randomSelect(grids.count(), count, selectIndexes);
        foreach (var index in selectIndexes)
        {
            var rect = grids.get(index);
            var brick = brickManager.acquireBrick(rect.center, rect.size, health);
            brick.eventRouter.addListener(this);
            addBrick(brick);
        }
    }
}