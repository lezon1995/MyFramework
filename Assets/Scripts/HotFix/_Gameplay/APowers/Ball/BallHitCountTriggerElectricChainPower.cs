using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 球撞击{0}次触发{1}次Effect
/// </summary>
public class BallHitCountTriggerElectricChainPower : BallPower, IArgs<int>
{
    Countdown countdown;

    public void onCreate(int c)
    {
        countdown = c;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        countdown = default;
    }

    public override void onHitBrick(Brick brick, Vector2 normal)
    {
        if (countdown.update())
        {
            countdown.reset();
            effectManager.addLogic<ElectricChainEffect>().with(owner, brick, 2);
        }
    }
}