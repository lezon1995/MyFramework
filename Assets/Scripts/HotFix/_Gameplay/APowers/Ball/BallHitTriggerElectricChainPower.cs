namespace MarbleHero;

/// <summary>
/// 球撞击{0}次触发{1}次Effect
/// </summary>
public class BallHitBrickCountEffectPower : BallPower
{
    public override void onGainPower(Ball ball)
    {
        var trigger = CLASS<CounterTrigger>();
        trigger.setGap(4);
    }

    public override void onLosePower(Ball ball)
    {
    }
    
    public override void onHitBrick(Brick brick)
    {
    }

    public void trigger(Brick b)
    {
        effectManager.addLogic<ElectricChainEffect>().with(owner, b, 3);
    }
}