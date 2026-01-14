using UnityEngine;

namespace MarbleHero
{
    public class DamageAction : AGameAction, IArgs<ACreature, DamageInfo>
    {
        DamageInfo damage;
        int damageAmount;
        static float DURATION = 0.1F;
        static float POST_ATTACK_WAIT_DUR = 0.1F;
        bool skipWait;
        bool muteSfx;
        
        public void onCreate(ACreature _target, DamageInfo info)
        {
            damage = info;
            duration = DURATION;
            source = info.owner;
            target = _target;
        }

        public override void update(float dt)
        {
            if (shouldCancelAction() && damage.type != DamageInfo.DamageType.THORNS)
            {
                isDone = true;
                return;
            }

            if (duration.unstarted)
            {
                Debug.Log($"{source.name} Cause {damageAmount} dmg to {target.name}");

                if (damage.type != DamageInfo.DamageType.THORNS && (damage.owner.isDying || damage.owner.halfDead))
                {
                    isDone = true;
                    return;
                }

                // ADungeon.effectList.Add(new FlashAtkImgEffect(target.hb.cX, target.hb.cY, attackEffect, muteSfx));
            }

            tickDuration(dt);
            if (isDone)
            {
                target.damage(damage);
                if (room.monsters.areMonstersBasicallyDead)
                    actionManager.clearPostCombatActions();

                if (!skipWait && !Settings.FAST_MODE)
                    actionManager.addToTop<WaitAction>().with(POST_ATTACK_WAIT_DUR);
            }
        }
    }
}