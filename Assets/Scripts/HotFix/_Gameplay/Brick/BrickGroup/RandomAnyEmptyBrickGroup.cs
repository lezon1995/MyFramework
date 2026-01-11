using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 在随机空的位置生成砖块
/// </summary>
public class RandomAnyEmptyBrickGroup : BrickGroup
{
    protected override int getBrickAverageCount(int turnCount)
    {
        var rows = brickManager.brickLayout.getRows();
        var cols = brickManager.brickLayout.getCols();
        var avg = (rows + cols) / 2;
        return avg;
    }

    public override void buildBrickTemplates(int turnCount)
    {
        templates.Clear();
        var health = turnCount;
        int count = getBrickCount(turnCount);
        using var _ = new ListScope2T<Rect, int>(out var grids, out var selectIndexes);
        grids.setRange(brickManager.brickLayout.getAllGrids());

        for (var i = grids.Count - 1; i >= 0; i--)
        {
            if (brickManager.containsBrickAt(grids[i]))
            {
                grids.removeAt(i);
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