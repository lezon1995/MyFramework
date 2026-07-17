using MoreMountains.Tools;

namespace MoreMountains
{
    public partial class APlayer : IEvent<DoHitEffect>
        , IEvent<DoSkillEffect>
        , IEvent<DoAttackKillEffect>
    {
        protected void addListeners() => Event.addAllListeners(this);
        protected void removeListeners() => Event.removeAllListeners(this);

        public void onEvent(DoHitEffect e)
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

        public void onEvent(DoSkillEffect e)
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
}