namespace MarbleHero;

public class CreaturePower : APower
{
    public virtual int onPlayerGainedBlock(float blockAmount)
    {
        return floor(blockAmount);
    }

    public virtual int onPlayerGainedBlock(int blockAmount)
    {
        return blockAmount;
    }

    public virtual int onMonsterGainedBlock(float blockAmount)
    {
        return floor(blockAmount);
    }

    public virtual int onMonsterGainedBlock(int blockAmount)
    {
        return blockAmount;
    }
}