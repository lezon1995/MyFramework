namespace MarbleHero
{
    public class RemoveSpecificPowerAction : AGameAction
    {
        string powerToRemove;
        APower powerInstance;
        static float DURATION = 0.1F;

        public RemoveSpecificPowerAction(ACreature target, ACreature source, string powerToRemove)
        {
            duration = DURATION;
            this.powerToRemove = powerToRemove;
        }

        public RemoveSpecificPowerAction(ACreature target, ACreature source, APower powerInstance)
        {
            duration = DURATION;
            this.powerInstance = powerInstance;
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