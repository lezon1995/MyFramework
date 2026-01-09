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
        using var _ = new ListScope2T<Rect, int>(out var emptyGrids, out var selectIndexes);
        var allGrids = brickManager.brickLayout.getAllGrids();
        emptyGrids.setRange(allGrids);

        for (var i = emptyGrids.Count - 1; i >= 0; i--)
        {
            if (brickManager.containsBrickAt(emptyGrids[i]))
            {
                emptyGrids.removeAt(i);
                break;
            }
        }

        randomSelect(emptyGrids.count(), count, selectIndexes);
        foreach (var index in selectIndexes)
        {
            var rect = allGrids.get(index);
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