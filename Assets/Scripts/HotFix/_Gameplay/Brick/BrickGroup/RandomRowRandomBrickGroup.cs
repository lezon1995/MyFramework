using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 在随机行的空位置生成砖块
/// </summary>
public class RandomRowRandomBrickGroup : BrickGroup
{
    protected override int getBrickAverageCount(int turnCount)
    {
        var cols = brickManager.brickLayout.getCols();
        var avg = cols / 2;
        return avg;
    }

    public override void buildBrickTemplates(int turnCount)
    {
        templates.Clear();
        var health = turnCount;
        int count = getBrickCount(turnCount);
        var rows = brickManager.brickLayout.getRows();
        using var _ = new ListScope2T<Rect, int>(out var grids, out var selectIndexes);

        int maxTry = 10;
        while (grids.Count == 0)
        {
            var randomRow = randomInt(0, rows - 1);
            brickManager.brickLayout.getGridsAtRow(ref grids, randomRow);

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
            templates.add(new(rect, health));
        }
    }

    public override void createBricks(int turnCount)
    {
        buildBrickTemplates(turnCount);
        foreach (var t in templates)
            createOne(t);
    }
}