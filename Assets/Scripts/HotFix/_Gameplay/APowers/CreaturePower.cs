namespace MoreMountains;

public class CreaturePower : APower, IArgs<ACreature>
{
    protected ACreature owner;
    
    public void onCreate(ACreature creature)
    {
        owner = creature;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        owner = null;
    }
    
    public virtual int onMonsterGainedBlock(int blockAmount)
    {
        return blockAmount;
    }
}