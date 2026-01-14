namespace MarbleHero;

/// <summary>
/// 冲击锤
/// 球速每提高1%，伤害率提高2%。
/// </summary>
public class ImpactHammer : ARelic
{
    public static string ID = "ImpactHammer";

    public ImpactHammer() : base(ID, "ImpactHammer.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onShootBall(Ball ball)
    {
        ball.addPower<BallSpeedDmgPower>().with(0.01F, 0.02F);
    }

    public override ARelic makeCopy() => new ImpactHammer();
}