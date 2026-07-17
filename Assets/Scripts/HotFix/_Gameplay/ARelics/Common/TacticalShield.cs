namespace MoreMountains;

/// <summary>
/// 战术盾牌
/// 每摧毁1个砖块，玩家获得2点护盾
/// </summary>
public class TacticalShield : ARelic
{
    public static string ID = "TacticalShield";

    public TacticalShield() : base(ID, "TacticalShield.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }
    
    public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    {
        actionManager.addToTop<GainBlockAction>().with(p, 2);
    }

    public override ARelic makeCopy() => new TacticalShield();
}