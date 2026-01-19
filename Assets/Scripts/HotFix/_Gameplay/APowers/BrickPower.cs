namespace MarbleHero;

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
}