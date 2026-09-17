using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(APlayer))]
    public class PlayerHealth : Health
    {
        APlayer _player => Character as APlayer;

        public override void Initialization()
        {
            base.Initialization();
        }

        protected override void InitializeShield()
        {
            if (Shield != null)
            {
                // 护盾变化时刷新血条
                Shield.OnShieldChanged += (prev, cur) =>
                {
                    if (cur < prev)
                    {
                        var curProgress = (CurrentHealth + Shield.CurrentShield) / (float)maximumHealth;
                        RefreshShieldBarByDamage(curProgress);
                    }
                    else
                    {
                        _player.playerRenderer.healthBar.barRenderer.RefreshHealthBarAndShieldBar();
                        _player.playerRenderer.healthBar.barUI.RefreshHealthBarAndShieldBar();
                    }
                };

                // 护盾被打破时触发特殊事件
                Shield.OnShieldDepleted += () => { Event.trigger(new OnShieldDepleted()); };
            }
        }

        public override void RefreshHealthBar(bool show)
        {
            _player.playerRenderer.refreshHealthByBorn((int)CurrentHealth, (int)maximumHealth);
        }

        public override void RefreshHealthBarByDamage()
        {
            _player.playerRenderer.refreshHealthByDamage((int)CurrentHealth, (int)maximumHealth);
        }

        public void RefreshShieldBarByDamage(float curProgress)
        {
            _player.playerRenderer.refreshShieldByDamage(curProgress);
        }

        public override void RefreshHealthBarByHeal()
        {
            _player.playerRenderer.refreshHealthByHealing((int)CurrentHealth, (int)maximumHealth);
        }

        float healthPerSecondAccumulated;
        float damagePerSecondAccumulated;
        float shieldRegenGainAccumulated;
        float shieldRegenLostAccumulated;

        protected override void UpdateHealthRegen(float dt)
        {
            var regen = healthRegen;
            var absRegen = regen.abs();
            if (regen > 0)
            {
                var healthEveryXSeconds = 11.25F / (1.25F + absRegen);
                if (healthEveryXSeconds >= 1)
                {
                    _timeElapsedForHealthRegen += dt;
                    if (_timeElapsedForHealthRegen >= healthEveryXSeconds)
                    {
                        _timeElapsedForHealthRegen -= healthEveryXSeconds;
                        ReceiveHealth(Heal.Fixed(1), source: Character);
                    }
                }
                else
                {
                    var healthPerSecond = absRegen / 11.25F + 1 / 9F;
                    healthPerSecondAccumulated += healthPerSecond * dt;
                    _timeElapsedForHealthRegen += dt;
                    if (_timeElapsedForHealthRegen >= 1F)
                    {
                        _timeElapsedForHealthRegen -= 1F;
                        var heal = (int)healthPerSecondAccumulated;
                        healthPerSecondAccumulated -= heal;
                        ReceiveHealth(Heal.Fixed(heal), source: Character);
                    }
                }
            }
            else if (regen < 0)
            {
                var damageEveryXSeconds = 11.25F / (1.25F + absRegen);
                if (damageEveryXSeconds >= 1)
                {
                    _timeElapsedForHealthRegen += dt;
                    if (_timeElapsedForHealthRegen >= damageEveryXSeconds)
                    {
                        _timeElapsedForHealthRegen -= damageEveryXSeconds;
                        var dmg = Dmg.True(1).setTriggerEffect(false);
                        Damage(ref dmg, gameObject, _player, 0, Vector3.up);
                    }
                }
                else
                {
                    var damagePerSecond = absRegen / 11.25F + 1 / 9F;
                    damagePerSecondAccumulated += damagePerSecond * dt;
                    _timeElapsedForHealthRegen += dt;
                    if (_timeElapsedForHealthRegen >= 1F)
                    {
                        _timeElapsedForHealthRegen -= 1F;
                        var damage = (int)damagePerSecondAccumulated;
                        damagePerSecondAccumulated -= damage;
                        var dmg = Dmg.True(damage).setTriggerEffect(false);
                        Damage(ref dmg, gameObject, _player, 0, Vector3.up);
                    }
                }
            }
        }

        /// <summary>
        /// 更新护盾回复（覆盖基类实现，使用累积方式）
        /// </summary>
        protected override void UpdateShieldRegen(float dt)
        {
            if (Shield is { BaseShieldRegen: > 0 })
            {
                var regen = Shield.BaseShieldRegen;
                var absRegen = regen.abs();
                var shieldGainEveryXSeconds = 11.25F / (1.25F + absRegen);
                if (shieldGainEveryXSeconds >= 1)
                {
                    _timeElapsedForShieldRegen += dt;
                    if (_timeElapsedForShieldRegen >= shieldGainEveryXSeconds)
                    {
                        _timeElapsedForShieldRegen -= shieldGainEveryXSeconds;
                        Shield.AddShield(1);
                    }
                }
                else
                {
                    var shieldPerSecond = absRegen / 11.25F + 1 / 9F;
                    shieldRegenGainAccumulated += shieldPerSecond * dt;
                    _timeElapsedForShieldRegen += dt;
                    if (_timeElapsedForShieldRegen >= 1F)
                    {
                        _timeElapsedForShieldRegen -= 1F;
                        var shield = (int)shieldRegenGainAccumulated;
                        shieldRegenGainAccumulated -= shield;
                        Shield.AddShield(shield);
                    }
                }
            }
            else if (Shield is { BaseShieldRegen: < 0 })
            {
                var regen = Shield.BaseShieldRegen;
                var absRegen = regen.abs();
                var shieldLostEveryXSeconds = 11.25F / (1.25F + absRegen);
                if (shieldLostEveryXSeconds >= 1)
                {
                    _timeElapsedForShieldRegen += dt;
                    if (_timeElapsedForShieldRegen >= shieldLostEveryXSeconds)
                    {
                        _timeElapsedForShieldRegen -= shieldLostEveryXSeconds;
                        Shield.RemoveShield(1);
                    }
                }
                else
                {
                    var shieldLostPerSecond = absRegen / 11.25F + 1 / 9F;
                    shieldRegenLostAccumulated += shieldLostPerSecond * dt;
                    _timeElapsedForShieldRegen += dt;
                    if (_timeElapsedForShieldRegen >= 1F)
                    {
                        _timeElapsedForShieldRegen -= 1F;
                        var shieldLost = (int)shieldRegenLostAccumulated;
                        shieldRegenLostAccumulated -= shieldLost;
                        Shield.RemoveShield(shieldLost);
                    }
                }
            }
        }

        public override void Damage(ref Dmg dmg, GameObject instigator, Character source = null, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null)
        {
            if (!CanTakeDamageThisFrame(out _))
                return;

            if (CanDodgeDamageThisFrame(out var dodgeType))
            {
                switch (dodgeType)
                {
                    case DodgeDamageType.Chance:
                        _player.Event.trigger(new DoChanceDodge());
                        break;
                    case DodgeDamageType.Dash:
                        _player.Event.trigger(new DoDashDodge());
                        break;
                }

                EnterInvincible(invincibleTime);
                return;
            }

            instigator.TryGetComponent(out Brick brick);

            ComputeDamageOutput(ref dmg, source, calculator);

            //设置此次dmg实际造成的伤害，并通知伤害飘字显示
            {
                dmg.SetDirection(direction);

                //由于下面有护盾值最终减免，所以伤害跳字不在这里触发
                // if (dmg.DamageDealt > 0)
                // new DmgTextEvent(dmg, transform).trigger();
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

            foreach (var p in _player.powers)
                p.onBeforeApplyDamage(brick, ref dmg);

            Event.trigger(new OnHit());

            if (dmg.DamageDealt > 0)
            {
                // =====================================================
                // MOBA 护盾系统：伤害优先作用于护盾
                // =====================================================
                int rawDamage = dmg.DamageDealt;
                int actualHealthDamage;

                if (Shield is { HasShield: true })
                {
                    // 有护盾时，伤害先被护盾吸收
                    actualHealthDamage = Shield.AbsorbDamage(rawDamage, out var shieldedDamage);

                    if (shieldedDamage > 0)
                    {
                        var shieldedDmg = dmg with
                        {
                            ActualType = Dmg.Types.AbsorbedByShield,
                            DamageDealt = shieldedDamage,
                        };

                        new DmgTextEvent(shieldedDmg, transform).trigger();
                    }

                    // 护盾完全被打破时触发特殊事件
                    if (!Shield.HasShield)
                    {
                        Event.trigger(new OnShieldBreak(Character, rawDamage - actualHealthDamage));
                    }
                }
                else
                {
                    // 无护盾时，全部伤害作用于生命值
                    actualHealthDamage = rawDamage;
                }

                // =====================================================
                // 应用实际生命值伤害
                // =====================================================
                if (actualHealthDamage > 0)
                {
                    dmg.SetDamageDealt(actualHealthDamage);
                    new DmgTextEvent(dmg, transform).trigger();

                    float preHealth = CurrentHealth;
                    SetHealth(CurrentHealth - actualHealthDamage, RefreshHealthBarType.ReceiveDamage);
                    LastDamage = actualHealthDamage;
                    LastDamageType = dmg.ActualType;
                    LastDamageDirection = direction;

                    //造成伤害后处理Source吸血，触发DoDmg
                    if (source && !dmg.Self)
                    {
                        source.Health.Event.trigger(new DoDmg(Character, dmg));
                    }

                    //造成伤害后，触发OnDmg
                    if (Character && !dmg.Self)
                        Event.trigger(new OnDmg(source, dmg));

                    //检测是否死亡
                    if (IsDead())
                    {
                        var isLethal = Kill();
                        if (source && isLethal && !dmg.Self)
                            source.Health.Event.trigger(new DoKill(Character, instigator));

                        dmg.IsLethal = isLethal;
                    }
                }
                else if (rawDamage > 0)
                {
                    // 伤害完全被护盾吸收时
                    LastDamage = rawDamage;
                    LastDamageType = dmg.ActualType;
                    LastDamageDirection = direction;
                }

                // we play our feedback (基于原始伤害值)
                if (FeedbackIsProportionalToDamage)
                    DamageMMFeedbacks.Play(transform.position, rawDamage);
                else
                    DamageMMFeedbacks.Play(transform.position);

                {
                    var e = new DoDmgPlayer(_player, dmg);
                    source.Event.trigger(e);

                    //造成伤害后，触发OnDmg
                    Event.trigger(new OnDmg(source, dmg));

                    _player.playerRenderer.playFxDamage(dmg.Direction);
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
            healing = ComputeHealRate(healing);
            if (healing <= 0F)
                return;

            foreach (var r in _player.relics)
                r.onPlayerHeal(ref healing);

            foreach (var p in _player.powers)
                p.onHeal(ref healing);

            int newHealth;
            int actualHealing;
            int maxHealth = maximumHealth;

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

            SetHealth((int)newHealth, RefreshHealthBarType.ReceiveHealing);

            if (CurrentHealth > maxHealth / 2F && _player.isBloodied)
            {
                _player.isBloodied = false;
                foreach (var relic in _player.relics)
                    relic.onExitBloodied();
            }

            if (heal.IsValid())
            {
                if (source)
                    source.Event.trigger(new DoHeal(this, heal));

                Event.trigger(new OnHeal(source, heal));
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

            // 死亡时清空护盾
            Shield?.ClearShield();

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

            Event.trigger(new OnDeath());

            if (DisableControllerOnDeath && _controller)
                _controller.enabled = false;

            if (DisableModelOnDeath && Model)
                Model.SetActive(false);

            if (DelayBeforeDestruction > 0f)
            {
                _coroutineTimeElapsed = 0F;
                _coroutineState = CoroutineState.DestroyObject;
            }
            else
                DestroyObject();

            _controller.IntentVelocity = Vector3.zero;
            _player.Controller2D.UnregisterToVolumeManager();
            return true;
        }
    }
}