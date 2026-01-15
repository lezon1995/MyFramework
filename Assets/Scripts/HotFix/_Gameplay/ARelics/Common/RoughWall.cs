using Drawing;
using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 粗糙的墙壁
/// 球撞击左右边界后如果反弹，会优先朝着最近的砖块撞去。
/// </summary>
public class RoughWall : ARelic
{
    public static string ID = "RoughWall";

    public RoughWall() : base(ID, "RoughWall.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallHitBorderLeft(APlayer p, Ball ball, BorderLeft border, ref Vector2 normal)
    {
        onBallHitSideBorder(ball, ref normal);
    }

    public override void onBallHitBorderRight(APlayer p, Ball ball, BorderRight border, ref Vector2 normal)
    {
        onBallHitSideBorder(ball, ref normal);
    }
    
    static Vector2 computeNormal(Vector2 incident, Vector2 reflected)
    {
        Vector2 n = (incident - reflected).normalized;

        // 保证法线朝向正确
        if (Vector2.Dot(incident, n) > 0)
            n = -n;

        return n;
    }

    static void onBallHitSideBorder(Ball ball, ref Vector2 normal)
    {
        var list = brickManager.getActiveBrickList();
        float minDistance = float.MaxValue;
        Brick minDistBrick = null;
        Vector2 minDisDirection = Vector2.zero;
        var ballPos = ball.getWorldPosition();
        foreach (var brick in list)
        {
            var brickPos = brick.getWorldPosition();
            var dir = brickPos - ballPos;
            var d = dir.sqrMagnitude;
            if (d < minDistance)
            {
                minDistance = d;
                minDistBrick = brick;
                minDisDirection = dir.normalized;
            }
        }

        if (minDistBrick)
        {
            normal = computeNormal(ball.getDirection(), minDisDirection);
        }
    }

    public override ARelic makeCopy() => new RoughWall();
}