namespace MarbleHero;

/// <summary>
/// 不稳定电池
/// 每次撞击砖块有30%概率对随机1个其他砖块造成连锁电流攻击。
/// 【连锁】【闪电】【撞击概率】
/// </summary>
public class UnstableBattery : ARelic
{
    public static string ID = "UnstableBattery";

    public UnstableBattery() : base(ID, "UnstableBattery.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onShootBall(Ball ball)
    {
        ball.addPower<BallHitChanceTriggerElectricChainPower>().with(0.3F, true);
    }

    public override ARelic makeCopy() => new UnstableBattery();
}