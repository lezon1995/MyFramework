namespace MarbleHero
{
    public class GainEnergyAndEnableControlsAction : AGameAction
    {
        int energyGain;

        public GainEnergyAndEnableControlsAction(int amount)
        {
            energyGain = amount;
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