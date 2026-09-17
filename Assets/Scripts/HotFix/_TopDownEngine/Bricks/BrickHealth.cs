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
                        brick.brickRenderer.healthBar.barRenderer.RefreshHealthBarAndShieldBar();
                    }
                };

                // 护盾被打破时触发特殊事件
                Shield.OnShieldDepleted += () => { Event.trigger(new OnShieldDepleted()); };
            }
        }

        protected override void OnEnable()
        {
            if (ResetHealthOnEnable)
            {
                InitializeCurrentHealth(RefreshHealthBarType.Resurrect);
            }

            if (IsDead())
                DoResurrect();

            DamageEnabled();
        }

        float healthPerSecondAccumulated;
        float damagePerSecondAccumulated;
        float shieldRegenAccumulated;

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
                        Damage(ref dmg, gameObject, player, 0, Vector3.up);
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
                        Damage(ref dmg, gameObject, player, 0, Vector3.up);
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
                var healthEveryXSeconds = 11.25F / (1.25F + absRegen);
                if (healthEveryXSeconds >= 1)
                {
                    _timeElapsedForHealthRegen += dt;
                    if (_timeElapsedForHealthRegen >= healthEveryXSeconds)
                    {
                        _timeElapsedForHealthRegen -= healthEveryXSeconds;
                        Shield.AddShield(1);
                    }
                }
                else
                {
                    var shieldPerSecond = absRegen / 11.25F + 1 / 9F;
                    shieldRegenAccumulated += shieldPerSecond * dt;
                    _timeElapsedForHealthRegen += dt;
                    if (_timeElapsedForHealthRegen >= 1F)
                    {
                        _timeElapsedForHealthRegen -= 1F;
                        var shield = (int)shieldRegenAccumulated;
                        shieldRegenAccumulated -= shield;
                        Shield.AddShield(shield);
                    }
                }
            }
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

        public void RefreshShieldBarByDamage(float curProgress)
        {
            brick.brickRenderer.refreshShieldByDamage(curProgress);
        }

        public override void RefreshHealthBarByHeal()
        {
            brick.brickRenderer.refreshHealthByHealing((int)CurrentHealth, (int)maximumHealth);
        }

        public override void Resurrect()
        {
            if (!_initialized)
                return;

            DoResurrect();

            Initialization();
            InitializeCurrentHealth(RefreshHealthBarType.Resurrect);
            Event.trigger(new OnRevive());
        }

        protected override void DoResurrect()
        {
            if (DisableChildCollisionsOnDeath)
            {
                _collider2D.enabled = true;
            }

            _controller.MovementDisabled = false;

            Character.conditionState?.ChangeState(Character.Conditions.Normal);
        }

        public override bool CanTakeDamageThisFrame(out ResistDamageType type)
        {
            if (brick.brickRenderer.isPlayingAnimationBorn())
            {
                type = ResistDamageType.BornInvincible;
                return false;
            }

            return base.CanTakeDamageThisFrame(out type);
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
                        brick.Event.trigger(new DoChanceDodge());
                        break;
                    case DodgeDamageType.Dash:
                        brick.Event.trigger(new DoDashDodge());
                        break;
                }

                return;
            }

            if (dmg.hasSkillEffect() || dmg.hasAttackEffect())
                brick.brickRenderer.playFxHit(dmg.HitNormal);

            if (instigator.TryGetComponent(out Ball ball))
            {
                DamageByBall(ref dmg, ball, source, invincibleTime, direction, calculator);
            }
            else
            {
                DamageByOther(ref dmg, instigator, source, invincibleTime, direction, calculator);
            }
        }

        public override void ComputeKnockbackForce(ref Vector3 knockbackForce)
        {
            if (brick.GetStat(Brick.Stat.KnockbackResistance, out var stat))
            {
                knockbackForce *= Mathf.Clamp01(1 - stat.Value);
            }
        }

        void DamageByBall(ref Dmg dmg, Ball ball, Character source, float invincibleTime, Vector3 direction, IDmgCalculator calculator)
        {
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

            ComputeDamageOutput(ref dmg, source, calculator);

            //设置此次dmg实际造成的伤害，并通知伤害飘字显示
            {
                dmg.SetDirection(direction);

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
                        if (dmg.Effect == Dmg.Effects.Attack)
                        {
                            var b1 = source.GetStat(Character.Stat.LifeSteal, out var lifeSteal1);
                            var b2 = ball.GetStat(Ball.Stat.LifeSteal, out var lifeSteal2);
                            if (source.Stats && b1 && b2)
                            {
                                var lifeSteal = lifeSteal1.Value + lifeSteal2.Value;
                                if (lifeSteal > 0 && randomHit(lifeSteal))
                                {
                                    source.Health.ReceiveHealth(Heal.Fixed(1), source: source);
                                }
                            }
                        }

                        source.Health.Event.trigger(new DoDmg(Character, dmg));
                    }

                    //造成伤害后，触发OnDmg
                    if (Character && !dmg.Self)
                        Event.trigger(new OnDmg(source, dmg));

                    //检测是否死亡
                    if (IsDead())
                    {
                        var isLethal = Kill();
                        if (isLethal)
                        {
                            if (dmg.TriggerEffect)
                            {
                                if (dmg.hasAttackEffect())
                                {
                                    var e = new DoAttackKillEffect(ball, brick, ball.gameObject);
                                    ball.Event.trigger(e);
                                    ball.getPlayer().Event.trigger(e);
                                    ball.onHitKill(brick);
                                }

                                if (dmg.hasSkillEffect())
                                {
                                    var e = new DoKillBrick(ball, brick, ball.gameObject);
                                    ball.Event.trigger(e);
                                    ball.getPlayer().Event.trigger(e);
                                    ball.onSkillKill(brick);
                                }
                            }
                        }

                        if (source && isLethal && !dmg.Self)
                            source.Health.Event.trigger(new DoKill(Character, ball.gameObject));

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
                    var e = new DoDmgBrick(brick, dmg);
                    source.Event.trigger(e);

                    //造成伤害后，触发OnDmg
                    Event.trigger(new OnDmg(source, dmg));

                    brick.brickRenderer.playFxDamage(dmg.Direction);
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

            if (dmg is { TriggerEffect: true, IsLethal: false or true })
            {
                //触发本次伤害所造成的攻击特效/技能特效
                if (dmg.hasAttackEffect())
                {
                    var b1 = ball.GetStat(Ball.Stat.HitEffectChance, out var stat1);
                    var b2 = ball.getPlayer().GetStat(Character.Stat.HitEffectChance, out var stat2);
                    if (b1 && b2)
                    {
                        var value = stat1.Value + stat2.Value;
                        var count = value.toInt();
                        var pct = value - count;
                        var e = new DoHitEffect(ball, brick, in dmg);

                        for (int i = 0; i < count; i++)
                        {
                            ball.Event.trigger(e);
                            ball.getPlayer().Event.trigger(e);
                        }

                        if (randomHit(pct))
                        {
                            ball.Event.trigger(e);
                            ball.getPlayer().Event.trigger(e);
                        }
                    }
                }

                if (dmg.hasSkillEffect())
                {
                    var e = new DoSkillEffect(ball, brick);
                    ball.Event.trigger(e);
                    ball.getPlayer().Event.trigger(e);
                }
            }
        }


        void DamageByOther(ref Dmg dmg, GameObject instigator, Character source, float invincibleTime, Vector3 direction, IDmgCalculator calculator)
        {
            // if (dmg.hasAttackEffect())
            // {
            //     foreach (var p in ball.powers)
            //         p.onBeforeHandleHitDamage(ball, brick, ref dmg);
            // }

            // if (dmg.hasSkillEffect())
            // {
            //     foreach (var p in ball.powers)
            //         p.onBeforeHandleSkillDamage(ball, brick, ref dmg);
            // }

            ComputeDamageOutput(ref dmg, source, calculator);

            //设置此次dmg实际造成的伤害，并通知伤害飘字显示
            {
                dmg.SetDirection(direction);

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

            if (dmg.TriggerEffect)
            {
                //触发本次伤害所造成的攻击特效/技能特效
                // if (dmg.hasAttackEffect())
                // {
                //     var e = new DoHitEffect(ball, brick, dmg.Direction);
                //     ball.Event.trigger(e);
                //     ball.getPlayer().Event.trigger(e);
                // }

                // if (dmg.hasSkillEffect())
                // {
                //     var e = new DoSkillEffect(ball, brick);
                //     ball.Event.trigger(e);
                //     ball.getPlayer().Event.trigger(e);
                // }
            }

            // foreach (var p in brick.powers)
            //     p.onBeforeApplyDamage(brick, ball, ref dmg);

            // if (dmg.IsCrit)
            // {
            //     if (dmg.hasAttackEffect())
            //         ball.onCritHit(brick);
            //
            //     if (dmg.hasSkillEffect())
            //         ball.onCritSkill(brick);
            // }

            Event.trigger(new OnHit());

            if (dmg.DamageDealt > 0)
            {
                // =====================================================
                // MOBA 护盾系统：伤害优先作用于护盾
                // =====================================================
                int rawDamage = dmg.DamageDealt;
                int actualHealthDamage = 0;

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
                        if (isLethal)
                        {
                            if (dmg.TriggerEffect)
                            {
                                // if (dmg.hasAttackEffect())
                                // {
                                //     var e = new DoAttackKillEffect(ball, brick, ball.gameObject);
                                //     ball.Event.trigger(e);
                                //     ball.getPlayer().Event.trigger(e);
                                //     ball.onHitKill(brick);
                                // }

                                // if (dmg.hasSkillEffect())
                                // {
                                //     var e = new DoKillBrick(ball, brick, ball.gameObject);
                                //     ball.Event.trigger(e);
                                //     ball.getPlayer().Event.trigger(e);
                                //     ball.onSkillKill(brick);
                                // }
                            }
                        }

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
                    var e = new DoDmgBrick(brick, dmg);
                    source.Event.trigger(e);

                    //造成伤害后，触发OnDmg
                    Event.trigger(new OnDmg(source, dmg));

                    brick.brickRenderer.playFxDamage(dmg.Direction);
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

            foreach (var p in brick.powers)
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

            if (heal.IsValid())
            {
                if (source)
                    source.Event.trigger(new DoHeal(this, heal));

                Event.trigger(new OnHeal(source, heal));

                brick.brickRenderer.playFxHeal();
            }
        }

        public override bool ShouldApplyKnockback(Dmg damage)
        {
            return base.ShouldApplyKnockback(damage);
        }

        public override bool ShouldApplyKnockback()
        {
            return base.ShouldApplyKnockback();
        }

        public override bool Kill()
        {
            brick.conditionState.ChangeState(Character.Conditions.Dead);
            brick.Reset();

            SetHealth(0, RefreshHealthBarType.Killed);

            // 死亡时清空护盾
            Shield?.ClearShield();

            foreach (var p in brick.powers)
                p.onDeath();

            foreach (var r in player.relics)
                r.onMonsterDeath(brick);

            {
                var e = new OnBrickDeath(brick);
                e.trigger(this);
                e.trigger(brick);
                e.trigger();

                brick.SetColliderEnabled(false);

                brick.brickRenderer.playFxDead();
                brick.brickRenderer.setHealthBarActive(false);
            }

            DeathMMFeedbacks.Play(transform.position);

            // we make it ignore the collisions from now on
            if (DisableCollisionsOnDeath)
            {
                _collider2D.enabled = false;

                // if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
                _controller.CollisionsOff();
            }

            Event.trigger(new OnDeath());

            if (DisableControllerOnDeath)
                _controller.enabled = false;

            _controller.MovementDisabled = true;

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
            brick.Controller2D.UnregisterToVolumeManager();
            return true;
        }

        protected override void DestroyObject()
        {
            brick.brickRenderer.ResetToIdle();

            var e = new OnBrickDeathTotally(brick);
            e.trigger(brick);

            base.DestroyObject();
        }
    }
}