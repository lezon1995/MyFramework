namespace MoreMountains;

public class CreaturePower : APower, IArgs<ACreature>
{
    protected ACreature owner;
    
    public void onCreate(ACreature ball)
    {
        owner = ball;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        owner = null;
    }

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