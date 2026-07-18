using MoreMountains.Feedbacks;
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
        
        
        
        public override void Damage(ref Dmg dmg, GameObject instigator, Character source = null, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null)
        {
            if (!CanTakeDamageThisFrame(out _))
                return;

            //应用Source的DmgRate
            if (source)
            {
                var stats = source.Stats;
                if (stats)
                {
                    //应用Source的DmgRate
                    var rate = stats.GetStat(Stats.DmgRate).Value;
                    dmg.SetDmgRate(rate);
                }
            }

            instigator.TryGetComponent(out Brick brick);

            ComputeDamageOutput(ref dmg, calculator);

            //设置此次dmg实际造成的伤害，并通知伤害飘字显示
            {
                dmg.SetDirection(direction);

                if (dmg.DamageDealt > 0)
                    new DmgTextEvent(dmg, transform).trigger();
            }

            //触发本次伤害所造成的攻击特效/技能特效
            if (dmg.TriggerEffect && source && !dmg.Self)
            {
                if (dmg.hasAttackEffect())
                {
                    var e = new DoAttackEffect(Character);
                    source.Event.trigger(e);
                }

                if (dmg.hasSkillEffect())
                {
                    var e = new DoAbilityEffect(Character);
                    source.Event.trigger(e);
                }
            }
            
            foreach (var p in player.powers)
                p.onBeforeApplyDamage(brick, ref dmg);
            
            Event.trigger(new OnHit());

            if (dmg.DamageDealt > 0)
            {
                // we decrease the character's health by the damage
                float preHealth = CurrentHealth;
                SetHealth(CurrentHealth - dmg.DamageDealt, RefreshHealthBarType.ReceiveDamage);
                LastDamage = dmg.DamageDealt;
                LastDamageType = dmg.ActualType;
                LastDamageDirection = direction;

                // we trigger a damage taken event
                MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, dmg.DamageDealt, preHealth);

                //造成伤害后处理Source吸血，触发DoDmg
                if (source && !dmg.Self)
                {
                    source.Health.Event.trigger(new DoDmg(Character, dmg));
                }

                //造成伤害后，触发OnDmg
                if (Character && !dmg.Self)
                    Event.trigger(new OnDmg(source, dmg));

                // we play our feedback
                if (FeedbackIsProportionalToDamage)
                    DamageMMFeedbacks.Play(transform.position, dmg.DamageDealt);
                else
                    DamageMMFeedbacks.Play(transform.position);

                {
                    var e = new DoDmgPlayer(player, dmg);
                    source.Event.trigger(e);

                    //造成伤害后，触发OnDmg
                    Event.trigger(new OnDmg(source, dmg));

                    player.playerRenderer.playFxDamage(dmg.Direction);
                }

                //检测是否死亡
                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;

                    var isLethal = Kill();
                    if (source && isLethal && !dmg.Self)
                        source.Health.Event.trigger(new DoKill(Character, instigator));

                    dmg.IsLethal = isLethal;
                }

                // we prevent the character from colliding with Projectiles, Player and Enemies
                if (invincibleTime > 0 && !dmg.IsLethal)
                {
                    DamageDisabled();
                    _coroutineTimeElapsed = 0F;
                    _coroutineState = CoroutineState.DamageEnabled;
                    _invincibleTime = invincibleTime;
                }
            }
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