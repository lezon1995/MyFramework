using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 在随机列的空位置生成砖块
/// </summary>
public class RandomColRandomBrickGroup : BrickGroup
{
    protected override int getBrickAverageCount(int turnCount)
    {
        var rows = brickManager.brickLayout.getRows();
        var avg = rows / 2;
        return avg;
    }

    public override void buildBrickTemplates(int turnCount)
    {
        templates.Clear();
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
            templates.add(new(rect.center, new(1, 1), health));
        }
    }

    public override void createBricks(int turnCount)
    {
        buildBrickTemplates(turnCount);
        foreach (var t in templates)
            createOne(t);
    }
}