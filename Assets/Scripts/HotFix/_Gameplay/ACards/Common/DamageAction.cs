using UnityEngine;

namespace MarbleHero
{
    public class DamageAction : AGameAction
    {
        ACreature source, target;


        DamageInfo info;
        int damageAmount;
        int goldAmount;
        static float DURATION = 0.1F;
        static float POST_ATTACK_WAIT_DUR = 0.1F;
        bool skipWait;
        bool muteSfx;

        public DamageAction(ACreature target, DamageInfo info, AttackEffect effect)
        {
            this.info = info;
            setValues(target, info);
            actionType = ActionType.DAMAGE;
            attackEffect = effect;
            duration = DURATION;
        }

        public DamageAction(ACreature target, DamageInfo info, int stealGoldAmount) : this(target, info, AttackEffect.SLASH_DIAGONAL)
        {
            goldAmount = stealGoldAmount;
        }

        public DamageAction(ACreature target, DamageInfo info) : this(target, info, AttackEffect.NONE)
        {
        }

        public DamageAction(ACreature target, DamageInfo info, bool superFast) : this(target, info, AttackEffect.NONE)
        {
            skipWait = superFast;
        }

        public DamageAction(ACreature target, DamageInfo info, AttackEffect effect, bool superFast) : this(target, info, effect)
        {
            skipWait = superFast;
        }

        public DamageAction(ACreature target, DamageInfo info, AttackEffect effect, bool superFast, bool muteSfx) : this(target, info, effect, superFast)
        {
            this.muteSfx = muteSfx;
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
                if (goldAmount != 0)
                    stealGold();
            }

            tickDuration(dt);
            if (isDone)
            {
                switch (attackEffect)
                {
                    case AttackEffect.POISON:
                        // target.tint.color.set(Color.CHARTREUSE.cpy());
                        // target.tint.changeColor(Color.WHITE.cpy());
                        break;
                    case AttackEffect.FIRE:
                        // target.tint.color.set(Color.RED);
                        // target.tint.changeColor(Color.WHITE.cpy());
                        break;
                }

                target.damage(info);
                if (room.monsters.areMonstersBasicallyDead)
                    actionManager.clearPostCombatActions();

                if (!skipWait && !Settings.FAST_MODE)
                    addToTop(new WaitAction(POST_ATTACK_WAIT_DUR));
            }
        }

        void stealGold()
        {
            if (target.gold == 0)
                return;

            // sound.play("GOLD_JINGLE");
            if (target.gold < goldAmount)
                goldAmount = target.gold;
            target.gold -= goldAmount;
            for (int i = 0; i < goldAmount; i++)
            {
                if (source.isPlayer)
                {
                    // ADungeon.effectList.Add(new GainPennyEffect(target.hb.cX, target.hb.cY));
                }
                else
                {
                    // ADungeon.effectList.Add(new GainPennyEffect(source, target.hb.cX, target.hb.cY, source.hb.cX, source.hb.cY, false));
                }
            }
        }
    }
}