namespace MoreMountains;

/// <summary>
/// 菱形飞镖
/// 撞击砖块的斜边时必暴击
/// </summary>
public class RhombicDarts : ARelic
{
    public static string ID = "RhombicDarts";

    public RhombicDarts() : base(ID, "RhombicDarts.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onShootBall(Ball ball)
    {
        ball.addPower<BallHypotenuseHitCritPower>();
    }

    public override ARelic makeCopy() => new RhombicDarts();
}