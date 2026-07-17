namespace MoreMountains;

public class BrickAttackPower : BrickPower, IArgs<Brick, int>
{
    BrickBlock block = new();

    public override void resetProperty()
    {
        base.resetProperty();
        block.reset();
    }

    public void onCreate(Brick brick, int blockAmount)
    {
        block.setBrick(brick);
        block.setBlock(blockAmount);
    }

    public override void onBeforeApplyDamage(Brick brick, Ball ball, ref Dmg dmg)
    {
        block.decrementBlock(ref dmg.DamageDealt);
        if (block.currentBlock > 0)
        {
            dmg.setTriggerEffect(false);
        }
    }
}