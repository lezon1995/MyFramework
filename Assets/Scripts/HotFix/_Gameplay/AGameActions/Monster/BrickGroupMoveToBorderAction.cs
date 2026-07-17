using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains;

public class BrickGroupMoveToBorderAction : AGameAction, IArgs<Border>
{
    const float DURATION = 0.2F;
    const int NUMBER = 1000;

    List<Brick> bricks = new();
    Dictionary<int, List<(Brick b, Vector2 startPos, Vector2 targetPos)>> brickPositions = new();

    MyCurve curve;

    public void onCreate(Border border)
    {
        duration = DURATION;
        bricks.Clear();
        foreach (var (key, poolList) in brickPositions)
        {
            ListPool<(Brick b, Vector2 startPos, Vector2 targetPos)>.Release(poolList);
        }

        brickPositions.Clear();
        room.getAllBricks(ref bricks);
        switch (border)
        {
            case BorderTop top:
                MoveToTop(top);
                break;
            case BorderBot bot:
                MoveToBot(bot);
                break;
            case BorderLeft left:
                MoveToLeft(left);
                break;
            case BorderRight right:
                MoveToRight(right);
                break;
        }

        curve = mKeyFrameManager.getKeyFrame(KEY_CURVE.SINE_IN_OUT);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        curve = null;
        bricks.Clear();
        foreach (var (key, poolList) in brickPositions)
        {
            ListPool<(Brick b, Vector2 startPos, Vector2 targetPos)>.Release(poolList);
        }

        brickPositions.Clear();
    }

    public override void update(float dt)
    {
        tickDuration(dt);

        foreach (var (key, targetPositions) in brickPositions)
        {
            foreach (var (b, startPos, targetPos) in targetPositions)
            {
                var f = curve.evaluate(duration.pct);
                var curY = lerp(startPos, targetPos, f);
                b.setWorldPosition(curY);
            }
        }
    }


    void MoveToLeft(BorderLeft left)
    {
        using var a = new ListScope<Brick>(out var list);
        var groupBy = bricks.GroupBy(brick => (int)(brick.getWorldPosition().y * NUMBER));
        var layout = brickManager.brickLayout;

        foreach (var grouping in groupBy)
        {
            list.setRange(grouping);
            list.Sort((b1, b2) => b1.getWorldPosition().x.CompareTo(b2.getWorldPosition().x));
            ListPool<(Brick b, Vector2 startPos, Vector2 targetPos)>.Get(out var targetPositions);
            for (var i = 0; i < list.Count; i++)
            {
                var b = list[i];
                float targetX;
                if (i == 0)
                {
                    targetX = left.getWorldPosition().x + layout.padding.x + b.getRect().width * 0.5F;
                }
                else
                    targetX = targetPositions[i - 1].targetPos.x + list[i - 1].getRect().width * 0.5F + layout.spacing.x + b.getRect().width * 0.5F;

                var startPos = b.getWorldPosition();
                var targetPos = startPos;
                targetPos.x = targetX;
                targetPositions.add((b, startPos, targetPos));
            }

            brickPositions[grouping.Key] = targetPositions;
        }
    }


    void MoveToRight(BorderRight right)
    {
        var layout = brickManager.brickLayout;
        using var a = new ListScope<Brick>(out var list);
        var groupBy = bricks.GroupBy(brick => (int)(brick.getWorldPosition().y * NUMBER));
        foreach (var grouping in groupBy)
        {
            list.setRange(grouping);
            list.Sort((b1, b2) => b2.getWorldPosition().x.CompareTo(b1.getWorldPosition().x));
            ListPool<(Brick b, Vector2 startPos, Vector2 targetPos)>.Get(out var targetPositions);
            for (var i = 0; i < list.Count; i++)
            {
                var b = list[i];
                float targetX;
                if (i == 0)
                    targetX = right.getWorldPosition().x - layout.padding.x - b.getRect().width * 0.5F;
                else
                    targetX = targetPositions[i - 1].targetPos.x - list[i - 1].getRect().width * 0.5F - layout.spacing.x - b.getRect().width * 0.5F;

                var startPos = b.getWorldPosition();
                var targetPos = startPos;
                targetPos.x = targetX;
                targetPositions.add((b, startPos, targetPos));
            }

            brickPositions[grouping.Key] = targetPositions;
        }
    }

    void MoveToTop(BorderTop top)
    {
        var layout = brickManager.brickLayout;
        using var a = new ListScope<Brick>(out var list);
        var groupBy = bricks.GroupBy(brick => (int)(brick.getWorldPosition().x * NUMBER));
        foreach (var grouping in groupBy)
        {
            list.setRange(grouping);
            list.Sort((b1, b2) => b2.getWorldPosition().y.CompareTo(b1.getWorldPosition().y));
            ListPool<(Brick b, Vector2 startPos, Vector2 targetPos)>.Get(out var targetPositions);
            for (var i = 0; i < list.Count; i++)
            {
                var b = list[i];
                float targetY;
                if (i == 0)
                    targetY = top.getWorldPosition().y - layout.padding.y - b.getRect().height * 0.5F;
                else
                    targetY = targetPositions[i - 1].targetPos.y - list[i - 1].getRect().height * 0.5F - layout.spacing.y - b.getRect().height * 0.5F;

                var startPos = b.getWorldPosition();
                var targetPos = startPos;
                targetPos.y = targetY;
                targetPositions.add((b, startPos, targetPos));
            }

            brickPositions[grouping.Key] = targetPositions;
        }
    }

    void MoveToBot(BorderBot bot)
    {
        var layout = brickManager.brickLayout;
        using var a = new ListScope<Brick>(out var list);
        var groupBy = bricks.GroupBy(brick => (int)(brick.getWorldPosition().x * NUMBER));
        foreach (var grouping in groupBy)
        {
            list.setRange(grouping);
            list.Sort((b1, b2) => b1.getWorldPosition().y.CompareTo(b2.getWorldPosition().y));
            ListPool<(Brick b, Vector2 startPos, Vector2 targetPos)>.Get(out var targetPositions);
            for (var i = 0; i < list.Count; i++)
            {
                var b = list[i];
                float targetY;
                if (i == 0)
                    targetY = bot.getWorldPosition().y + layout.getCellSize().y + layout.padding.y + b.getRect().height * 0.5F;
                else
                    targetY = targetPositions[i - 1].targetPos.y + list[i - 1].getRect().height * 0.5F + layout.spacing.y + b.getRect().height * 0.5F;

                var startPos = b.getWorldPosition();
                var targetPos = startPos;
                targetPos.y = targetY;
                targetPositions.add((b, startPos, targetPos));
            }

            brickPositions[grouping.Key] = targetPositions;
        }
    }
}