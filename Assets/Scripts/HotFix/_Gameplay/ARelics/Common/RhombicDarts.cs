using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 撞击砖块的斜边时必暴击
/// </summary>
public class RhombicDarts : ARelic
{
    public static string ID = "RhombicDarts";

    static Vector2 up = new(0, 1);
    static Vector2 down = new(0, -1);
    static Vector2 left = new(-1, 0);
    static Vector2 right = new(1, 0);

    public RhombicDarts() : base(ID, "RhombicDarts.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit, ref Dmg dmg)
    {
        if (normal == up)
            return;
        if (normal == down)
            return;
        if (normal == left)
            return;
        if (normal == right)
            return;

        dmg.setCrit();
    }

    public override ARelic makeCopy() => new RhombicDarts();
}