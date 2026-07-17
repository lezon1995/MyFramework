using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class BrickHealth : Health
    {
        Brick brick;

        public override void Initialization()
        {
            base.Initialization();
            brick = Character as Brick;
        }

        protected override void OnEnable()
        {
            if (ResetHealthOnEnable)
                InitializeCurrentHealth(RefreshHealthBarType.Resurrect);

            if (IsDead())
                DoResurrect();

            DamageEnabled();
        }

        public override void ResetHealthToMaxHealth()
        {
            SetHealth(maximumHealth, RefreshHealthBarType.Resurrect);
        }

        public override void RefreshHealthBar(bool show)
        {
            brick.brickRenderer.refreshHealthByBorn((int)CurrentHealth, (int)maximumHealth);
        }

        public override void RefreshHealthBarByDamage()
        {
            brick.brickRenderer.refreshHealthByDamage((int)CurrentHealth, (int)maximumHealth);
        }

        public override void RefreshHealthBarByHeal()
        {
            brick.brickRenderer.refreshHealthByHealing((int)CurrentHealth, (int)maximumHealth);
        }


        public override void Damage(ref Dmg dmg, GameObject instigator, Character source = null, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null)
        {
            if (dmg.hasSkillEffect())
                brick.brickRenderer.playFxSkillHit(dmg.Direction);
            
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

            instigator.TryGetComponent(out Ball ball);

            if (dmg.hasAttackEffect())
            {
                foreach (var p in ball.powers)
                    p.onBeforeHandleHitDamage(ball, brick, ref dmg);
            }

            if (dmg.hasSkillEffect())
            {
                foreach (var p in ball.powers)
                    p.onBeforeHandleSkillDamage(ball, brick, ref dmg);
            }

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

            if (dmg.TriggerEffect)
            {
                //触发本次伤害所造成的攻击特效/技能特效
                if (dmg.hasAttackEffect())
                {
                    var e = new DoHitEffect(ball, brick);
                    ball.Event.trigger(e);
                    ball.getPlayer().Event.trigger(e);
                }

                if (dmg.hasSkillEffect())
                {
                    var e = new DoSkillEffect(ball, brick);
                    ball.Event.trigger(e);
                    ball.getPlayer().Event.trigger(e);
                }

                Event.trigger(new OnHit());
            }

            foreach (var p in brick.powers)
                p.onBeforeApplyDamage(brick, ball, ref dmg);

            if (dmg.IsCrit)
            {
                if (dmg.hasAttackEffect())
                    ball.onCritHit(brick);

                if (dmg.hasSkillEffect())
                    ball.onCritSkill(brick);
            }

            Event.trigger(new OnHit());

            if (dmg.DamageDealt > 0)
            {
                // we decrease the character's health by the damage
                float preHealth = CurrentHealth;
                SetHealth(CurrentHealth - dmg.DamageDealt, RefreshHealthBarType.ReceiveDamage);
                LastDamage = dmg.DamageDealt;
                LastDamageType = dmg.ActualType;
                LastDamageDirection = direction;

                // we prevent the character from colliding with Projectiles, Player and Enemies
                if (invincibleTime > 0)
                {
                    DamageDisabled();
                    _coroutineTimeElapsed = 0F;
                    _coroutineState = CoroutineState.DamageEnabled;
                    _invincibleTime = invincibleTime;
                }

                // we trigger a damage taken event
                MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, dmg.DamageDealt, preHealth);

                //造成伤害后处理Source吸血，触发DoDmg
                if (source && !dmg.Self)
                {
                    if (dmg.Effect == Dmg.Effects.Attack)
                    {
                        if (source.Stats && source.Stats.TryGetStat(Stats.LS, out var lifeSteal))
                        {
                            var healing = lifeSteal.Value * dmg.DamageDealt;
                            source.Health.ReceiveHealth(Heal.Fixed(healing), source: source);
                        }
                    }

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
                    var e = new DoDmgBrick(brick, dmg);
                    source.Event.trigger(e);

                    //造成伤害后，触发OnDmg
                    Event.trigger(new OnDmg(source, dmg));

                    brick.brickRenderer.playFxDamage(dmg.Direction);
                }


                //检测是否死亡
                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;

                    var isLethal = Kill();
                    if (isLethal)
                    {
                        if (dmg.TriggerEffect)
                        {
                            if (dmg.hasAttackEffect())
                            {
                                var e = new DoAttackKillEffect(ball, brick, instigator);
                                ball.Event.trigger(e);
                                ball.getPlayer().Event.trigger(e);
                                ball.onHitKill(brick);
                            }

                            if (dmg.hasSkillEffect())
                            {
                                var e = new DoKillBrick(ball, brick, instigator);
                                ball.Event.trigger(e);
                                ball.getPlayer().Event.trigger(e);
                                ball.onSkillKill(brick);
                            }
                        }
                    }

                    if (source && isLethal && !dmg.Self)
                        source.Health.Event.trigger(new DoKill(Character, instigator));

                    dmg.IsLethal = isLethal;
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
            
            foreach (var p in brick.powers)
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

            if (heal.IsValid())
            {
                if (source)
                    source.Event.trigger(new DoHeal(this, heal));

                Event.trigger(new OnHeal(source, heal));

                brick.brickRenderer.playFxHeal();
            }
        }

        public override bool Kill()
        {
            if (ImmuneToDamage)
                return false;

            if (Character)
            {
                // we set its dead state to true
                Character.conditionState.ChangeState(Character.Conditions.Dead);
                Character.Reset();
            }

            SetHealth(0, RefreshHealthBarType.Killed);
            
            foreach (var p in brick.powers)
                p.onDeath();
            
            foreach (var r in player.relics)
                r.onMonsterDeath(brick);

            {
                var e = new OnBrickDeath(brick);
                e.trigger(this);
                e.trigger();

                brick.SetColliderEnabled(false);

                brick.brickRenderer.playFxDead();
                brick.brickRenderer.setHealthBarActive(false);
            }


            // we prevent further damage
            DamageDisabled();

            DeathMMFeedbacks.Play(transform.position);

            // we make it ignore the collisions from now on
            if (DisableCollisionsOnDeath)
            {
                if (_collider2D)
                    _collider2D.enabled = false;

                // if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
                if (_controller)
                    _controller.CollisionsOff();

                if (DisableChildCollisionsOnDeath)
                {
                    foreach (var c in GetComponentsInChildren<Collider2D>())
                        c.enabled = false;
                }
            }

            if (ChangeLayerOnDeath)
            {
                var layer = LayerOnDeath.LayerIndex;
                gameObject.layer = layer;
                if (ChangeLayersRecursivelyOnDeath)
                {
                    transform.ChangeLayersRecursively(layer);
                }
            }

            Event.trigger(new OnDeath());
            MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Death);

            if (DisableControllerOnDeath && _controller)
                _controller.enabled = false;

            if (DisableControllerOnDeath && _characterController)
                _characterController.enabled = false;

            if (DisableModelOnDeath && Model)
                Model.SetActive(false);

            if (DelayBeforeDestruction > 0f)
            {
                _coroutineTimeElapsed = 0F;
                _coroutineState = CoroutineState.DestroyObject;
            }
            else
                DestroyObject();

            return true;
        }

        protected override void DestroyObject()
        {
            var e = new OnBrickDeathTotally(brick);
            e.trigger(brick);

            base.DestroyObject();
        }
    }
}