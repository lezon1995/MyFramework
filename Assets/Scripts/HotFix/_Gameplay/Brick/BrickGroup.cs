using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace MarbleHero;

public abstract class BrickGroup : ClassObject, IEvent<OnBrickDeath>
{
    protected List<Brick> bricks = new();
    protected Action<BrickGroup> onBricksClear;

    public override void resetProperty()
    {
        base.resetProperty();
        bricks.Clear();
        onBricksClear = null;
    }

    public override void onCreate()
    {
        base.onCreate();
    }

    public override void destroy()
    {
        base.destroy();
    }

    protected void addBrick(Brick brick)
    {
        bricks.Add(brick);
    }

    protected void removeBrick(Brick brick)
    {
        bricks.Remove(brick);
    }

    public abstract void createBricks(int turnCount);

    public virtual void doNextTurnMove(float duration)
    {
        var brickGrid = brickManager.brickGrid;
        for (var i = 0; i < bricks.Count; i++)
        {
            var brick = bricks[i];
            var curRow = brickGrid.getRowAtPosY(brick.getWorldPosition().y);
            var nextRow = curRow - 1;
            var nextPosY = brickGrid.getPosYAtRow(nextRow);
            var tween = Tween.PositionY(brick.getTransform(), endValue: nextPosY, duration: duration, ease: Ease.InOutSine);
            if (nextRow < 0)
            {
                tween.OnComplete(brick, b =>
                {
                    b.kill();
                });
            }
        }
    }

    public void setOnBricksClear(Action<BrickGroup> action)
    {
        onBricksClear = action;
    }

    public void onEvent(OnBrickDeath e)
    {
        e.brick.eventRouter.removeListener(this);
        removeBrick(e.brick);
        if (bricks.isEmpty())
        {
            onBricksClear?.Invoke(this);
        }
    }
}

public class RandomTopRowBrickGroup : BrickGroup
{
    List<int> selectIndexes = new();

    public override void createBricks(int turnCount)
    {
        var health = turnCount;
        int count = getBrickCount(turnCount);
        var topRowGrids = brickManager.brickGrid.getTopRowGrids();
        randomSelect(topRowGrids.count(), count, selectIndexes);
        foreach (var index in selectIndexes)
        {
            var rect = topRowGrids.get(index);
            var brick = brickManager.createBrick<NormalBrick>("Brick", rect.center, rect.size, health);
            brick.eventRouter.addListener(this);
            addBrick(brick);
        }
    }


    int per1, per2, per3, per4, per5;

    /// <summary>
    /// Difficulty adjustment for each block according to the number of turns
    /// </summary>
    int getBrickCount(int turnCount)
    {
        int n = randomInt(0, 99);
        int count = 0;

        switch (turnCount)
        {
            case >= 0 and <= 10:
            {
                per1 = 20;
                per2 = 50;
                per3 = 100;

                if (n <= per1)
                    count = 2;
                else if (n > per1 && n <= per2)
                    count = 3;
                else if (n > per2 && n <= per3)
                    count = 4;

                break;
            }
            case > 10 and <= 20:
            {
                per1 = 5;
                per2 = 35;
                per3 = 75;
                per4 = 100;

                if (n <= per1)
                    count = 2;
                else if (n > per1 && n <= per2)
                    count = 3;
                else if (n > per2 && n <= per3)
                    count = 4;
                else if (n > per3 && n <= per4)
                    count = 5;

                break;
            }
            case > 20 and <= 30:
            {
                per1 = 25;
                per2 = 60;
                per3 = 85;
                per4 = 100;

                if (n <= per1)
                    count = 3;
                else if (n > per1 && n <= per2)
                    count = 4;
                else if (n > per2 && n <= per3)
                    count = 5;
                else if (n > per3 && n <= per4)
                    count = 6;

                break;
            }
            case > 30 and <= 40:
            {
                per1 = 10;
                per2 = 40;
                per3 = 75;
                per4 = 100;

                if (n <= per1)
                    count = 3;
                else if (n > per1 && n <= per2)
                    count = 4;
                else if (n > per2 && n <= per3)
                    count = 5;
                else if (n > per3 && n <= per4)
                    count = 6;

                break;
            }
            case > 40 and <= 50:
            {
                per1 = 5;
                per2 = 35;
                per3 = 75;
                per4 = 100;

                if (n <= per1)
                    count = 3;
                else if (n > per1 && n <= per2)
                    count = 4;
                else if (n > per2 && n <= per3)
                    count = 5;
                else if (n > per3 && n <= per4)
                    count = 6;

                break;
            }
            case > 50 and <= 60:
            {
                per1 = 30;
                per2 = 70;
                per3 = 100;

                if (n <= per1)
                    count = 4;
                else if (n > per1 && n <= per2)
                    count = 5;
                else if (n > per2 && n <= per3)
                    count = 6;

                break;
            }
            case > 60 and <= 70:
            {
                per1 = 25;
                per2 = 65;
                per3 = 100;

                if (n <= per1)
                    count = 4;
                else if (n > per1 && n <= per2)
                    count = 5;
                else if (n > per2 && n <= per3)
                    count = 6;

                break;
            }
            case > 70 and <= 80:
            {
                per1 = 20;
                per2 = 60;
                per3 = 100;

                if (n <= per1)
                    count = 4;
                else if (n > per1 && n <= per2)
                    count = 5;
                else if (n > per2 && n <= per3)
                    count = 6;

                break;
            }
            case > 80 and <= 90:
            {
                per1 = 20;
                per2 = 60;
                per3 = 100;

                if (n <= per1)
                    count = 4;
                else if (n > per1 && n <= per2)
                    count = 5;
                else if (n > per2 && n <= per3)
                    count = 6;

                break;
            }
            case > 90 and <= 100:
            {
                per1 = 15;
                per2 = 55;
                per3 = 100;

                if (n <= per1)
                    count = 4;
                else if (n > per1 && n <= per2)
                    count = 5;
                else if (n > per2 && n <= per3)
                    count = 6;

                break;
            }
            case > 100 and <= 200:
            {
                per1 = 40;

                if (n <= per1)
                    count = 5;
                else
                    count = 6;

                break;
            }
            case > 200 and <= 300:
            {
                per1 = 30;

                if (n <= per1)
                    count = 5;
                else
                    count = 6;

                break;
            }
            default:
                count = 6;
                break;
        }

        return count;
    }
}

public class RandomAnyEmptyBrickGroup : BrickGroup
{
    List<int> selectIndexes = new();

    public override void createBricks(int turnCount)
    {
        var health = turnCount;
        int count = 2;
        using var _ = new ListScope<Rect>(out var emptyGrids);
        var allGrids = brickManager.brickGrid.getAllGrids();
        emptyGrids.setRange(allGrids);
        var allBricks = brickManager.getBricks();

        foreach (var brick in allBricks.Values)
        {
            for (var i = emptyGrids.Count - 1; i >= 0; i--)
            {
                if (emptyGrids[i].Contains(brick.getWorldPosition()))
                {
                    emptyGrids.removeAt(i);
                    break;
                }
            }
        }

        randomSelect(emptyGrids.count(), count, selectIndexes);
        foreach (var index in selectIndexes)
        {
            var rect = allGrids.get(index);
            var brick = brickManager.createBrick<NormalBrick>("Brick", rect.center, rect.size, health);
            brick.eventRouter.addListener(this);
            addBrick(brick);
        }
    }
}