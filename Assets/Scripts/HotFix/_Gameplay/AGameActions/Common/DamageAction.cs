using UnityEngine;

namespace MarbleHero
{
    public class DamageAction : AGameAction
    {
        DamageInfo info;
        int damageAmount;
        static float DURATION = 0.1F;
        static float POST_ATTACK_WAIT_DUR = 0.1F;
        bool skipWait;
        bool muteSfx;

        public DamageAction(ACreature target, DamageInfo info)
        {
            this.info = info;
            duration = DURATION;
        }

        public override void update(float dt)
        {
            if (shouldCancelAction() && info.type != DamageInfo.DamageType.THORNS)
            {
                isDone = true;
                return;
            }

            if (duration == DURATION)
            {
                Debug.Log($"{source.name} Cause {damageAmount} dmg to {target.name}");

                if (info.type != DamageInfo.DamageType.THORNS && (info.owner.isDying || info.owner.halfDead))
                {
                    isDone = true;
                    return;
                }

                // ADungeon.effectList.Add(new FlashAtkImgEffect(target.hb.cX, target.hb.cY, attackEffect, muteSfx));
            }

            tickDuration(dt);
            if (isDone)
            {
                target.damage(info);
                if (room.monsters.areMonstersBasicallyDead)
                    actionManager.clearPostCombatActions();

                if (!skipWait && !Settings.FAST_MODE)
                    addToTop(new WaitAction(POST_ATTACK_WAIT_DUR));
            }
        }
    }
}