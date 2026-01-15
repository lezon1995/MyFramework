using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 粗糙的瓦片
/// 球撞击上边界后如果反弹，会优先朝着最近的砖块撞去。
/// </summary>
public class RoughTiles : ARelic
{
    public static string ID = "RoughTiles";

    public RoughTiles() : base(ID, "RoughTiles.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallHitBorderBot(APlayer p, Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn)
    {
        forceReturn = false;
        ball.counters.hit.count();
        ball.reflectBounce(normal);
    }

    public override ARelic makeCopy() => new RoughTiles();
}