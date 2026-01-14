namespace MarbleHero;

public class BallPower : APower
{
    public virtual void onBeforeHandleHitDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
    }

    public virtual void onBeforeHandleSkillDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
    }
}