namespace MoreMountains;

public abstract class ABlock
{
    public int currentBlock;

    public void setBlock(int blockAmount)
    {
        currentBlock = blockAmount;
    }

    public abstract void addBlock(int blockAmount);
    public abstract void  loseBlock();
    public abstract void  loseBlock(int amount);
    public abstract void decrementBlock(ref int damageAmount);
    public abstract void brokeBlock();
}