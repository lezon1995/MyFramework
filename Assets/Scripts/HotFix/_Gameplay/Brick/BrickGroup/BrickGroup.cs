using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public abstract class BrickGroup : ClassObject, IEvent<OnBrickDeath>
{
    public struct Template
    {
        public Rect rect;
        public int health;

        public Template(Rect _rect, int _health)
        {
            rect = _rect;
            health = _health;
        }
    }

    public List<Brick> bricks = new();
    protected Action<BrickGroup> onBricksClear;
    protected List<Template> templates = new();

    protected BrickManager brickManager;
    protected LevelManager levelManager;

    public override void resetProperty()
    {
        base.resetProperty();
        bricks.Clear();
        templates.Clear();
        onBricksClear = null;
        brickManager = null;
        levelManager = null;
        per1 = per2 = per3 = per4 = per5 = 0;
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

    public abstract void buildBrickTemplates(int turnCount);
    public abstract void createBricks(int turnCount);

    public bool tryTakeOne(out Template t, out int remain)
    {
        if (templates.Count > 0)
        {
            t = templates.removeAt(0);
            remain = templates.Count;
            return true;
        }

        t = default;
        remain = 0;
        return false;
    }

    public void createOne(Template t)
    {
        var brick = brickManager.acquireBrick(t.rect.center, t.rect.size, t.health);
        brick.eventRouter.addListener(this);
        addBrick(brick);
    }

    public void setOnBricksClear(Action<BrickGroup> action) => onBricksClear = action;
    public void setBrickManager(BrickManager manager) => brickManager = manager;
    public void setLevelManager(LevelManager manager) => levelManager = manager;

    public void onEvent(OnBrickDeath e)
    {
        e.brick.eventRouter.removeListener(this);
        removeBrick(e.brick);
        if (bricks.isEmpty())
        {
            onBricksClear?.Invoke(this);
        }
    }

    int per1, per2, per3, per4, per5;

    protected abstract int getBrickAverageCount(int turnCount);

    /// <summary>
    /// Difficulty adjustment for each block according to the number of turns
    /// </summary>
    protected virtual int getBrickCount(int turnCount)
    {
        int n = randomInt(0, 99);
        int count = 0;

        var mid = getBrickAverageCount(turnCount);

        switch (turnCount)
        {
            case >= 0 and <= 10:
            {
                per1 = 20;
                per2 = 50;
                per3 = 100;

                if (n <= per1)
                    count = mid - 1;
                else if (n > per1 && n <= per2)
                    count = mid;
                else if (n > per2 && n <= per3)
                    count = mid + 1;

                break;
            }
            case > 10 and <= 20:
            {
                per1 = 5;
                per2 = 35;
                per3 = 75;
                per4 = 100;

                if (n <= per1)
                    count = mid - 1;
                else if (n > per1 && n <= per2)
                    count = mid;
                else if (n > per2 && n <= per3)
                    count = mid + 1;
                else if (n > per3 && n <= per4)
                    count = mid + 2;

                break;
            }
            case > 20 and <= 30:
            {
                per1 = 25;
                per2 = 60;
                per3 = 85;
                per4 = 100;

                if (n <= per1)
                    count = mid;
                else if (n > per1 && n <= per2)
                    count = mid + 1;
                else if (n > per2 && n <= per3)
                    count = mid + 2;
                else if (n > per3 && n <= per4)
                    count = mid + 3;

                break;
            }
            case > 30 and <= 40:
            {
                per1 = 10;
                per2 = 40;
                per3 = 75;
                per4 = 100;

                if (n <= per1)
                    count = mid;
                else if (n > per1 && n <= per2)
                    count = mid + 1;
                else if (n > per2 && n <= per3)
                    count = mid + 2;
                else if (n > per3 && n <= per4)
                    count = mid + 3;

                break;
            }
            case > 40 and <= 50:
            {
                per1 = 5;
                per2 = 35;
                per3 = 75;
                per4 = 100;

                if (n <= per1)
                    count = mid;
                else if (n > per1 && n <= per2)
                    count = mid + 1;
                else if (n > per2 && n <= per3)
                    count = mid + 2;
                else if (n > per3 && n <= per4)
                    count = mid + 3;

                break;
            }
            case > 50 and <= 60:
            {
                per1 = 30;
                per2 = 70;
                per3 = 100;

                if (n <= per1)
                    count = mid + 1;
                else if (n > per1 && n <= per2)
                    count = mid + 2;
                else if (n > per2 && n <= per3)
                    count = mid + 3;

                break;
            }
            case > 60 and <= 70:
            {
                per1 = 25;
                per2 = 65;
                per3 = 100;

                if (n <= per1)
                    count = mid + 1;
                else if (n > per1 && n <= per2)
                    count = mid + 2;
                else if (n > per2 && n <= per3)
                    count = mid + 3;

                break;
            }
            case > 70 and <= 80:
            {
                per1 = 20;
                per2 = 60;
                per3 = 100;

                if (n <= per1)
                    count = mid + 1;
                else if (n > per1 && n <= per2)
                    count = mid + 2;
                else if (n > per2 && n <= per3)
                    count = mid + 3;

                break;
            }
            case > 80 and <= 90:
            {
                per1 = 20;
                per2 = 60;
                per3 = 100;

                if (n <= per1)
                    count = mid + 1;
                else if (n > per1 && n <= per2)
                    count = mid + 2;
                else if (n > per2 && n <= per3)
                    count = mid + 3;

                break;
            }
            case > 90 and <= 100:
            {
                per1 = 15;
                per2 = 55;
                per3 = 100;

                if (n <= per1)
                    count = mid + 1;
                else if (n > per1 && n <= per2)
                    count = mid + 2;
                else if (n > per2 && n <= per3)
                    count = mid + 3;

                break;
            }
            case > 100 and <= 200:
            {
                per1 = 40;

                if (n <= per1)
                    count = mid + 2;
                else
                    count = mid + 3;

                break;
            }
            case > 200 and <= 300:
            {
                per1 = 30;

                if (n <= per1)
                    count = mid + 2;
                else
                    count = mid + 3;

                break;
            }
            default:
                count = mid + 3;
                break;
        }

        return count;
    }
}