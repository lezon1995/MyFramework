using System.Collections.Generic;

namespace MarbleHero;

public partial class Player : IEvent<DoAttackEffect>
{
    protected void addListeners() => this.addListener<DoAttackEffect>();
    protected void removeListeners() => this.removeAllListener();

    List<Buff> buffs = new();

    public void onEvent(DoAttackEffect e)
    {
        for (var i = 0; i < buffs.Count; i++)
        {
            var b = buffs[i];
            if (b is IDoAttackEffect effect)
            {
                effect.onTrigger(this, e.ball, e.brick);
            }
        }
    }
}