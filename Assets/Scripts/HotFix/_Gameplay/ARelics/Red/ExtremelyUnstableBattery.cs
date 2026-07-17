namespace MoreMountains;

/// <summary>
/// 极不稳定电池
/// 每次撞击有20%概率对随机1个其他砖块造成连锁电流攻击。
/// 【连锁】【闪电】【撞击概率】
/// </summary>
public class ExtremelyUnstableBattery : ARelic
{
    public static string ID = "ExtremelyUnstableBattery";

    public ExtremelyUnstableBattery() : base(ID, "ExtremelyUnstableBattery.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onShootBall(Ball ball)
    {
        ball.addPower<BallHitChanceTriggerElectricChainPower>().with(0.2F, false);
    }

    public override ARelic makeCopy() => new ExtremelyUnstableBattery();
}