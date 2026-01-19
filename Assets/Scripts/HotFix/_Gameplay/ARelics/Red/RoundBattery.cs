namespace MarbleHero;

/// <summary>
/// 圆形电池
/// 球每第4次撞击砖块对随机2个其他砖块造成连锁电流攻击。
/// 【连锁】【闪电】【撞击概率】
/// </summary>
public class RoundBattery : ARelic
{
    public static string ID = "RoundBattery";

    public RoundBattery() : base(ID, "RoundBattery.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onShootBall(Ball ball)
    {
        ball.addPower<BallHitCountTriggerElectricChainPower>().with(4);
    }

    public override ARelic makeCopy() => new RoundBattery();
}