using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 搅拌机
/// 球每次反弹球暴击率提高5%
/// </summary>
public class Blender : ARelic
{
    public static string ID = "Blender";

    public Blender() : base(ID, "Blender.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        ball.GetStat(Ball.Stat.CritChance, out var stat);
        stat.AddFlat(0.05F);
    }

    public override ARelic makeCopy() => new Blender();
}