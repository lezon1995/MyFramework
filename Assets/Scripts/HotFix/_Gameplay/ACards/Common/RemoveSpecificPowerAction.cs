namespace MarbleHero
{
    public class RemoveSpecificPowerAction : AGameAction
    {
        string powerToRemove;
        APower powerInstance;
        static float DURATION = 0.1F;

        public RemoveSpecificPowerAction(ACreature target, ACreature source, string powerToRemove)
        {
            setValues(target, source, amount);
            actionType = ActionType.DEBUFF;
            duration = DURATION;
            this.powerToRemove = powerToRemove;
        }

        public RemoveSpecificPowerAction(ACreature target, ACreature source, APower powerInstance)
        {
            setValues(target, source, amount);
            actionType = ActionType.DEBUFF;
            duration = DURATION;
            this.powerInstance = powerInstance;
        }

        public override void update(float dt)
        {
            if (duration == DURATION)
            {
                if (target.isDeadOrEscaped())
                {
                    isDone = true;
                    return;
                }

                APower removeMe = null;
                if (powerToRemove != null)
                {
                    removeMe = target.getPower(powerToRemove);
                }
                else if (powerInstance != null && target.powers.Contains(powerInstance))
                {
                    removeMe = powerInstance;
                }

                if (removeMe != null)
                {
                    // ADungeon.effectList.Add(new PowerExpireTextEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, removeMe.name, removeMe.region128));
                    removeMe.onRemove();
                    target.powers.Remove(removeMe);
                    ADungeon.onModifyPower();
                    // foreach (var o in player.orbs)
                    // o.updateDescription();
                }
                else
                {
                    duration = 0.0F;
                }
            }

            tickDuration(dt);
        }
    }
}