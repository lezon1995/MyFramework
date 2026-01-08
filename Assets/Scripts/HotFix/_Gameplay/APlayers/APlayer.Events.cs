namespace MarbleHero;

public partial class APlayer : IEventRouter
    , IEvent<DoAttackEffect>
    , IEvent<DoAbilityEffect>
    , IEvent<DoAttackKillEffect>
{
    public IEventRouter eventRouter => this;

    protected void addListeners() => eventRouter.addAllListener(this);
    protected void removeListeners() => eventRouter.removeAllListener(this);

    public void onEvent(DoAttackEffect e)
    {
        for (var i = 0; i < buffs.Count; i++)
        {
            var b = buffs[i];
            if (b is IDoAttackEffect effect)
            {
                effect.onDoAttack(this, e.ball, e.brick);
            }
        }
    }

    public void onEvent(DoAbilityEffect e)
    {
        for (var i = 0; i < buffs.Count; i++)
        {
            var b = buffs[i];
            if (b is IDoAbilityEffect effect)
            {
                effect.onDoAbility(this, e.ball, e.brick);
            }
        }
    }

    public void onEvent(DoAttackKillEffect e)
    {
        for (var i = 0; i < buffs.Count; i++)
        {
            var b = buffs[i];
            if (b is IDoAttackKillEffect effect)
            {
                effect.onDoAttackKill(this, e.ball, e.brick);
            }
        }
    }
}