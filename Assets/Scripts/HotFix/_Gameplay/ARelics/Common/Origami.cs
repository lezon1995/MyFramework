using System.Collections.Generic;

namespace MarbleHero;

/// <summary>
/// 千纸鹤
/// 发射出去的球具有1次穿透能力
/// </summary>
public class Origami : ARelic
{
    public static string ID = "Origami";
    HashSet<Ball> effectingBalls = new();

    public Origami() : base(ID, "Origami.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
    }

    public override void onShootBall(Ball ball)
    {
        ball.setPenetrable(true);
        effectingBalls.add(ball);
    }
    
    public override void onBallEndOverlappingBrick(APlayer p, Ball ball, Brick brick, bool prematurely)
    {
        if (effectingBalls.Contains(ball))
        {
            ball.setPenetrable(false);
            effectingBalls.Remove(ball);
        }
    }

    public override void onFightingPhaseEnd(APlayer p)
    {
        effectingBalls.Clear();
    }

    public override ARelic makeCopy() => new Origami();
}