using System.Collections.Generic;

namespace MarbleHero;

public class BrickGroupMoveDownAction : AGameAction, IGameActionArgs<AMonster>
{
    const float DURATION = 0.2F;

    List<Brick> bricks = new();
    List<(float, float)> bricksY = new();
    List<int> bricksRow = new();

    ACreature creature;
    MyCurve curve;

    public void onCreate(AMonster monster)
    {
        duration = DURATION;
        creature = monster;
        bricks.Clear();
        bricksY.Clear();
        bricksRow.Clear();
        room.getAllBricks(ref bricks);

        var brickGrid = brickManager.brickLayout;
        for (var i = 0; i < bricks.Count; i++)
        {
            var brick = bricks[i];
            var curPosY = brick.getWorldPosition().y;
            var curRow = brickGrid.getRowAtPosY(curPosY);
            var nextRow = curRow - 1;
            var nextPosY = brickGrid.getPosYAtRow(nextRow);
            bricksY.add((curPosY, nextPosY));
            bricksRow.add(nextRow);
        }

        curve = mKeyFrameManager.getKeyFrame(KEY_CURVE.SINE_IN_OUT);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        creature = null;
        curve = null;
        bricks.Clear();
        bricksY.Clear();
        bricksRow.Clear();
    }

    public override void update(float dt)
    {
        tickDuration(dt);

        for (var i = 0; i < bricks.Count; i++)
        {
            var brick = bricks[i];
            var (startY, endY) = bricksY[i];
            var f = curve.evaluate(duration.pct);
            var curY = lerp(startY, endY, f);
            brick.setWorldPositionY(curY);
        }

        if (isDone)
        {
            for (var i = 0; i < bricks.Count; i++)
            {
                var brick = bricks[i];
                var endRow = bricksRow[i];
                var (startY, endY) = bricksY[i];
                brick.setWorldPositionY(endY);
                if (endRow < 0)
                    brick.kill();
                else
                    brick.refreshRect();
            }
        }
    }
}