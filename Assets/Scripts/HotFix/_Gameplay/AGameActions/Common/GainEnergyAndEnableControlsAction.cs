namespace MarbleHero
{
    public class GainEnergyAndEnableControlsAction : AGameAction, IGameActionArgs<int>
    {
        int energyGain;
        
        public void onCreate(int amount)
        {
            energyGain = amount;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            energyGain = 0;
        }

        public override void update(float dt)
        {
            if (isFloatEqual(duration, DEFAULT_DURATION))
            {
                // player.gainEnergy(energyGain);
                actionManager.updateEnergyGain(energyGain);

                foreach (var r in player.relics)
                    r.onEnergyRecharge();

                foreach (var p in player.powers)
                    p.onEnergyRecharge();

                actionManager.turnHasEnded = false;
            }

            tickDuration(dt);
        }
    }
}