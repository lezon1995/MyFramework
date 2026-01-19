namespace MarbleHero;

public class BallPower : APower, IArgs<Ball>
{
    protected Ball owner;
    
    public void onCreate(Ball ball)
    {
        owner = ball;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        owner = null;
    }

    public virtual void onGainPower(Ball ball)
    {
    }

    public virtual void onLosePower(Ball ball)
    {
    }
    
    public virtual void onBeforeHandleHitDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
    }

    public virtual void onBeforeHandleSkillDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
    }

    public virtual void onHitBrick(Brick brick)
    {
    }
}