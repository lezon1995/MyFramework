using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 在最顶行的空位置生成砖块
/// </summary>
public class TopRowRandomBrickGroup : BrickGroup
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
        using var _ = new ListScope2T<Rect, int>(out var grids, out var selectIndexes);
        grids.setRange(brickManager.brickLayout.getTop2RowGrids());
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
            var randomDef = brickManager.GetRandomDef(new(1, 1));
            templates.add(new(rect.center, randomDef, health));
        }
    }

    public override void createBricks(int turnCount)
    {
        buildBrickTemplates(turnCount);
        foreach (var t in templates)
            createOne(t);
    }
}