using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(APlayer))]
    public class PlayerHealth : Health
    {
        APlayer player => Character as APlayer;

        public override void Initialization()
        {
            base.Initialization();
        }
        
        public override void RefreshHealthBar(bool show)
        {
            player.playerRenderer.refreshHealthByBorn((int)CurrentHealth, (int)maximumHealth);
        }

        public override void RefreshHealthBarByDamage()
        {
            player.playerRenderer.refreshHealthByDamage((int)CurrentHealth, (int)maximumHealth);
        }

        public override void RefreshHealthBarByHeal()
        {
            player.playerRenderer.refreshHealthByHealing((int)CurrentHealth, (int)maximumHealth);
        }

        public override void ReceiveHealth(Heal heal, GameObject instigator = null, Character source = null)
        {
            //阵亡后无法再回血
            if (CurrentHealth <= 0F)
                return;

            var healing = ComputeHealAlgo(heal.Algo, heal.Value);
            if (healing <= 0F)
                return;
            
            foreach (var r in player.relics)
                healing = r.onPlayerHeal((int)healing);
            
            foreach (var p in player.powers)
                healing = p.onHeal((int)healing);

            float newHealth;
            float actualHealing;
            float maxHealth = maximumHealth;

            if (CurrentHealth + healing <= maxHealth)
            {
                newHealth = CurrentHealth + healing;
                actualHealing = healing;
            }
            else
            {
                newHealth = maxHealth;
                actualHealing = maxHealth - CurrentHealth;
            }

            heal.SetHealing(actualHealing);
            if (Mathf.FloorToInt(actualHealing) > 0 /* && actualHealing / maxHealth > 0.01F*/)
            {
                new HealTextEvent(heal, transform).trigger();
            }

            SetHealth(newHealth, RefreshHealthBarType.ReceiveHealing);
            
            if (CurrentHealth > maxHealth / 2F && player.isBloodied)
            {
                player.isBloodied = false;
                foreach (var relic in player.relics)
                    relic.onNotBloodied();
            }

            if (heal.IsValid())
            {
                if (source)
                    source.Event.trigger(new DoHeal(this, heal));

                Event.trigger(new OnHeal(source, heal));
            }

        }
    }
}