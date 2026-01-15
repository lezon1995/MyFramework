using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 粗糙的天花板
/// 球撞击上边界后如果反弹，会优先朝着最近的砖块撞去。
/// </summary>
public class RoughCelling : ARelic
{
    public static string ID = "RoughCelling";

    public RoughCelling() : base(ID, "RoughCelling.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallHitBorderTop(APlayer p, Ball ball, BorderTop border, ref Vector2 normal)
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

    public override ARelic makeCopy() => new RoughCelling();
}