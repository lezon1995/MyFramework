namespace MoreMountains;

public class GainBlockAction : AGameAction
    , IArgs<ACreature, int>
    , IArgs<ACreature, ACreature, int>
{
    const float DUR = 0.1F;
    int blockAmount;

    public void onCreate(ACreature _target, int amount)
    {
        target = _target;
        blockAmount = amount;
        // actionType = ActionType.BLOCK;
        duration = DUR;
    }

    public void onCreate(ACreature _target, ACreature _source, int amount)
    {
        target = _target;
        source = _source;
        // actionType = ActionType.BLOCK;
        duration = DUR;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        blockAmount = 0;
    }


    public override void update(float dt)
    {
        if (!target.isDying && target.IsAlive() && duration.unstarted)
        {
            // effectManager.effectList.add(new FlashAtkImgEffect(target.hb.cX, target.hb.cY, AttackEffect.SHIELD));
            target.block.addBlock(blockAmount);
        }

        tickDuration(dt);
    }
}