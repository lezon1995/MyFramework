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
            var inDir = ball.getDirection();
            var outDir = minDisDirection;
            normal = (-inDir + outDir) / 2F;
        }
    }

    public override ARelic makeCopy() => new RoughWall();
}