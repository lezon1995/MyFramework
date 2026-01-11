using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 当球回到底边界时有概率再次反弹而不是回收
/// </summary>
public class BurlapBag : ARelic
{
    public static string ID = "BurlapBag";

    public BurlapBag() : base(ID, "BurlapBag.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallHitBorderBot(APlayer p, Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn)
    {
        forceReturn = false;
        ball.reflectBounce(normal);
    }

    public override ARelic makeCopy() => new BurlapBag();
}