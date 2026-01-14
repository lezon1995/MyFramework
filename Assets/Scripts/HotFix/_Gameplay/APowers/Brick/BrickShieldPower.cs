namespace MarbleHero;

public class BrickShieldPower : BrickPower, IArgs<float>
{
    float shield;
    
    public void onCreate(float p1)
    {
        shield = p1;
    }
    
    public override void onBeforeApplyDamage(Brick brick, Ball ball, ref Dmg dmg)
    {
    }
}