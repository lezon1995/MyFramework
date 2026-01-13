using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 拨浪鼓
/// 球每次撞击砖块而反弹暴击率提高10%
/// </summary>
public class Rattle : ARelic
{
    public static string ID = "Rattle";

    public Rattle() : base(ID, "Rattle.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        if (fromBrick)
        {
            ball.crit.increase(0.1F);
        }
    }

    public override ARelic makeCopy() => new Rattle();
}