using UnityEngine;

namespace MoreMountains;

public class BrickPower : CreaturePower, IArgs<Brick>
{
    protected new Brick owner;
    
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

    public override void onGainPower(ACreature c) => onGainPower(c as Brick);
    public override void onLosePower(ACreature c) => onLosePower(c as  Brick);

    protected virtual void onGainPower(Brick brick)
    {
    }

    protected virtual void onLosePower(Brick brick)
    {
    }
}