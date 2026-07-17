using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 捕鱼网
/// 从左、右、上、三种边界反弹回底边界的球会反弹而不是回收。
/// </summary>
public class FishingNets : ARelic
{
    public static string ID = "FishingNets";

    public FishingNets() : base(ID, "FishingNets.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallHitBorderBot(APlayer p, Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn)
    {
        if (ball.lastHittable is Border)
        {
            forceReturn = false;
        }
        else
        {
            forceReturn = true;
        }
    }

    public override ARelic makeCopy() => new FishingNets();
}