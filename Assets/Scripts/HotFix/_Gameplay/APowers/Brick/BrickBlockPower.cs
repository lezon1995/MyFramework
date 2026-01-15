namespace MarbleHero;

public class BrickBlockPower : BrickPower, IArgs<Brick, int>
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
        brick.brickRenderer.playFxGainBlock();
        brick.brickRenderer.refreshBlockAmount(blockAmount);
    }

    public override void onBeforeApplyDamage(Brick brick, Ball ball, ref Dmg dmg)
    {
        block.decrementBlock(ref dmg.damageDealt);
        brick.brickRenderer.refreshBlockAmount(block.currentBlock);
        if (block.currentBlock > 0)
        {
            dmg.setTriggerEffect(false);
        }
        else
        {
            brick.brickRenderer.playFxLoseBlock();
        }
    }
}