using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 球撞击砖块斜边暴击
/// </summary>
public class BallHypotenuseHitCritPower : BallPower
{
    static Vector2 up = new(0, 1);
    static Vector2 down = new(0, -1);
    static Vector2 left = new(-1, 0);
    static Vector2 right = new(1, 0);

    public override void onBeforeHandleHitDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
        var normal = dmg.HitNormal;
        if (normal == up)
            return;
        if (normal == down)
            return;
        if (normal == left)
            return;
        if (normal == right)
            return;

        dmg.Crit();
    }
}