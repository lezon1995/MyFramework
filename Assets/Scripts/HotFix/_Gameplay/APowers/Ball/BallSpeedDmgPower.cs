namespace MarbleHero;

/// <summary>
/// 球的撞击伤害随球的额外速度提升而提升
/// </summary>
public class BallSpeedDmgPower : BallPower, IArgs<float, float>
{
    float speedStandard;
    float dmgRateStandard;

    public override void resetProperty()
    {
        base.resetProperty();
        speedStandard = 0F;
        dmgRateStandard = 0F;
    }

    public void onCreate(float p1, float p2)
    {
        speedStandard = p1;
        dmgRateStandard = p2;
    }

    public override void destroy()
    {
        base.destroy();
    }

    public override void onBeforeHandleHitDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
        var share = ball.speed.extraMultiplier / speedStandard;
        var extraDmgRate = dmgRateStandard * share;
        dmg.addDmgRate(extraDmgRate);
    }
}