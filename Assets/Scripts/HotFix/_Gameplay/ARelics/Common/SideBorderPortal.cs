namespace MarbleHero;

/// <summary>
/// 侧边传送门
/// 球碰到左右边界时不再反弹，而是传送至对方边界。
/// </summary>
public class SideBorderPortal : ARelic
{
    public static string ID = "SideBorderPortal";

    public SideBorderPortal() : base(ID, "SideBorderPortal.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onShootBall(Ball ball)
    {
        ball.setHorizontalBorderTeleportable(true);
    }

    public override ARelic makeCopy() => new SideBorderPortal();
}