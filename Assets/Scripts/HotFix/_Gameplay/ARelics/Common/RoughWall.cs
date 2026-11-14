using Drawing;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 粗糙的墙壁
/// 球撞击左右边界后如果反弹，会优先朝着最近的砖块撞去。
/// </summary>
public class RoughWall : ARelic
{
    public static string ID = "RoughWall";

    public RoughWall() : base(ID, "RoughWall.png", RelicTier.COMMON, LandingSound.SOLID) { }

    public override void onBallHitBorder(APlayer p, Ball ball, Border border, ref Vector2 normal)
    {
        onBallHitSideBorder(ball, ref normal);
    }

    public override void onBallHitObstacle(APlayer p, Ball ball, Obstacle obstacle, ref Vector2 normal)
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
        var originNormal = normal;
        var list = brickManager.getActiveBrickList();
        float minDistance = float.MaxValue;
        Brick minDistBrick = null;
        Vector2 minDisDirection = Vector2.zero;
        var ballPos = ball.getWorldPosition();
        foreach (var brick in list)
        {
            var brickPos = (Vector3)brick.getCenterPosition();
            var dir = brickPos - ballPos;
            var dot = Vector2.Dot(originNormal, dir);
            var d = dir.sqrMagnitude;
            if (d < minDistance && dot > 0)
            {
                if (!Physics2D.CircleCast(ballPos, ball.Radius, dir.normalized, dir.magnitude, LayerManager.Obstacles_Mask))
                {
                    minDistance = d;
                    minDistBrick = brick;
                    minDisDirection = dir.normalized;
                }
            }
        }

        if (minDistBrick)
        {
            normal = computeNormal(ball.getDirection(), minDisDirection);
        }
    }

    public override ARelic makeCopy() => new RoughWall();
}