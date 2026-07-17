using UnityEngine;

namespace MoreMountains;

public class BrickPower : APower, IArgs<Brick>
{
    protected Brick owner;
    
    public void onCreate(Brick ball)
    {
        owner = ball;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        owner = null;
    }
    
    public virtual void onBeforeApplyDamage(Brick brick, Ball ball, ref Dmg dmg)
    {
    }

    public virtual void onKnockbackReceived(Brick brick, Vector2 direction, float force)
    {
    }
    
    public virtual void onGainPower(Brick brick)
    {
    }

    public virtual void onLosePower(Brick brick)
    {
    }
}