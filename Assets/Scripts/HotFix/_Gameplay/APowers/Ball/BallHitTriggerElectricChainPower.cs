namespace MarbleHero;

/// <summary>
/// 球撞击X次触发ElectricChain
/// </summary>
public class BallHitTriggerElectricChainPower : BallPower, ITriggerAction<Brick>
{
    public override void onGainPower(Ball ball)
    {
        var trigger = CLASS<CounterTrigger>();
        trigger.setGap(4);
        trigger.setTriggerAction(this);
        ball.counters.hitBrick.addTrigger(trigger);
    }

    public override void onLosePower(Ball ball)
    {
    }

    public void trigger(Brick b)
    {
        effectManager.addLogic<ElectricChain>().with(owner, b, 3);
    }
}