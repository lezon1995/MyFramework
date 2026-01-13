using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 破损的鼎
/// 球从正下方撞击砖块时可以穿透砖块
/// </summary>
public class BrokenTripod : ARelic
{
    public static string ID = "BrokenTripod";

    public BrokenTripod() : base(ID, "BrokenTripod.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit, ref Dmg dmg)
    {
        if (normal == Vector2.down)
        {
            triggerRegularHit = false;
            ball.counters.hit.count();
            ball.counters.hitBrick.count();
            ball.setDirection(ball.getDirection());
        }
    }

    public override ARelic makeCopy() => new BrokenTripod();
}