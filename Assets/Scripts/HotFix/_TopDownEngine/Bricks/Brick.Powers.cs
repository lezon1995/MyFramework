namespace MoreMountains
{
    public partial class Brick
    {
        public void addBlock(int amount)
        {
            if (!tryGetPower<BrickBlockPower>(out var power))
            {
                power = addPower<BrickBlockPower>();
                power.with(this, amount);
            }
            else
            {
                power.addBlockAmount(amount);
            }
        }

        public void removeBlock(int amount)
        {
            if (tryGetPower<BrickBlockPower>(out var power))
            {
                power.removeBlockAmount(amount);
            }
        }
    }
}