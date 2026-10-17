using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 拨浪鼓
/// 球每次反弹暴击率提高5%
/// </summary>
public class Rattle : ARelic
{
    public static string ID = "Rattle";

    public Rattle() : base(ID, "Rattle.png", RelicTier.COMMON, LandingSound.SOLID) { }

    float critChancePerBounce;
    string modKey;

    protected override void onRelicDefSet(RelicDef d)
    {
        critChancePerBounce = d.Args[0];
        modKey = $"{ID}_{GetHashCode()}";
    }

    public override void onBallImpactHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    {

    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        ball.GetStat(Ball.Stat.CritChance, out var stat);
        if (stat.BonusFlat.GetMod(modKey, out var mod))
        {
            mod.Value += critChancePerBounce;
        }
        else
        {
            stat.BonusFlat.AddFlat(critChancePerBounce, modKey);
        }
    }

    public override ARelic makeCopy() => new Rattle();
}