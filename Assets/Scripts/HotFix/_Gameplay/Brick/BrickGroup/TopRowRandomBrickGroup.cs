namespace MarbleHero;

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
        var topRowGrids = brickManager.brickLayout.getTopRowGrids();

        using var _ = ListScope<int>.get(out var selectIndexes);
        randomSelect(topRowGrids.count(), count, selectIndexes);
        foreach (var index in selectIndexes)
        {
            var rect = topRowGrids.get(index);
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