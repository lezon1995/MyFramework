using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 球的撞击造成爆炸的能力
/// </summary>
public class BallImpactExplodePower : BallPower, IArgs<float>
{
    public override void resetProperty()
    {
        base.resetProperty();
        timer = 0;
    }

    Timer timer;

    public void onCreate(float p1)
    {
        timer = p1;
    }

    public override void onFixedUpdate(float dt)
    {
        timer.update(dt);
    }

    public override void onImpactHitBrick(Brick brick, Vector2 hitNormal)
    {
        if (timer)
            effectManager.addLogic<BallImpactShotExplodeEffect>().with(_ball, brick, _ball.Direction);
    }
}