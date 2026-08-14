using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 奶昔
/// 球每次反弹球速提高5%
/// </summary>
public class MilkShake : ARelic
{
    public static string ID = "MilkShake";

    public MilkShake() : base(ID, "MilkShake.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        ball.GetStat(Ball.Stat.BallisticSpeed, out var stat);
        stat.AddPct(0.05F);
    }

    public override ARelic makeCopy() => new MilkShake();
}