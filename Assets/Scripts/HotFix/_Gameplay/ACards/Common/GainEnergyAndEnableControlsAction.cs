namespace MarbleHero
{
    public class GainEnergyAndEnableControlsAction : AGameAction
    {
        int energyGain;

        public GainEnergyAndEnableControlsAction(int amount)
        {
            setValues(player, player, 0);
            energyGain = amount;
        }

        public override void update(float dt)
        {
            if (duration == DEFAULT_DURATION)
            {
                player.gainEnergy(energyGain);
                actionManager.updateEnergyGain(energyGain);

                foreach (var c in player.hand.group)
                    c.triggerOnGainEnergy(energyGain, false);

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