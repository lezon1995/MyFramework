using UnityEngine;

namespace MoreMountains;

public class BallPower : APower, IArgs<Ball>
{
    protected Ball _ball;

    public void onCreate(Ball ball)
    {
        _ball = ball;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        _ball = null;
    }

    public virtual void onGainPower(Ball ball) { }
    public virtual void onLosePower(Ball ball) { }
    public virtual void onBeforeHandleImpactDamage(Ball ball, Brick brick, ref Dmg dmg) { }
    public virtual void onBeforeHandleSkillDamage(Ball ball, Brick brick, ref Dmg dmg) { }
    public virtual void onImpactHitBrick(Brick brick, Vector2 hitNormal) { }
    public virtual void onHitBorder(Border border) { }
    public virtual void onHitObstacle(Obstacle obstacle) { }
    public virtual void onHitPlayer(APlayer p) { }
}