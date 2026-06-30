namespace MarbleHero
{
    public class RemoveSpecificPlayerPowerAction : AGameAction
    {
        string powerToRemove;
        CreaturePower powerInstance;
        static float DURATION = 0.1F;

        public RemoveSpecificPlayerPowerAction(ACreature target, ACreature source, string powerToRemove)
        {
            duration = DURATION;
            this.powerToRemove = powerToRemove;
        }

        public RemoveSpecificPlayerPowerAction(ACreature target, ACreature source, CreaturePower powerInstance)
        {
            duration = DURATION;
            this.powerInstance = powerInstance;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            powerToRemove = null;
            powerInstance = null;
        }

        public override void update(float dt)
        {
            if (duration.unstarted)
            {
                if (target.isDeadOrEscaped())
                {
                    isDone = true;
                    return;
                }

                CreaturePower removeMe = null;
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