using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public struct Heal
    {
        public float Value;
        public Algos Algo;
        public float Healing;

        public static Heal Fixed(float value) => new(value, Algos.Fixed);
        public static Heal CurPct(float value) => new(value, Algos.CurPct);
        public static Heal LostPct(float value) => new(value, Algos.LostPct);
        public static Heal AllPct(float value) => new(value, Algos.AllPct);

        public Heal(float value, Algos algo)
        {
            Value = value;
            Algo = algo;
            Healing = value;
        }

        public bool IsValid()
        {
            return Healing > 0F;
        }

        public void SetHealing(float value)
        {
            Healing = value;
        }

        public enum Algos
        {
            Fixed,
            CurPct,
            LostPct,
            AllPct,
        }
    }

    public enum ResistDamageType
    {
        None,
        Invulnerable,
        DashInvincible,
        ImmuneToDamage,
        Dead,
        Disabled,
    }

    /// <summary>
    /// This class manages the health of an object, pilots its potential health bar, handles what happens when it takes damage,
    /// and what happens when it dies.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/Health")]
    public class Health : TopDownMonoBehaviour, IEventRouter
    {
        [MMInspectorGroup("Bindings")]
        [Tooltip("the model to disable (if set so)")]
        public GameObject Model;

        [MMInspectorGroup("Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("the current health of the character")]
        public float CurrentHealth { get; set; }

        public float HealthPct => CurrentHealth / maximumHealth;

        [ShowInInspector, ReadOnly]
        [Tooltip("If this is true, this object can't take damage at this time")]
        public bool Invulnerable { get; set; }

        public bool DashInvincible { get; set; }

        [MMInspectorGroup("Health")]
        public float InitialHealth = 10;

        public bool InitialHealthDrivenByMaximumHealth;

        public float MaximumHealth = 10;

        public ValueModifier MaximumHealthModifier { get; set; }

        public float maximumHealth
        {
            get
            {
                var maxHealth = MaximumHealth;
                return MaximumHealthModifier.SafeInvoke(ref maxHealth);
            }
        }

        [Tooltip("if this is true, health values will be reset everytime this character is enabled (usually at the start of a scene)")]
        public bool ResetHealthOnEnable = true;

        [MMInspectorGroup("Health Regen")]
        [Tooltip("the base amount of health regen (receive health X per 1 second)")]
        public float BaseHealthRegen;

        public ValueModifier HealthRegenModifier { get; set; }

        public float healthRegen
        {
            get
            {
                var value = BaseHealthRegen;
                return HealthRegenModifier.SafeInvoke(ref value);
            }
        }

        [MMInspectorGroup("Defence")]
        [Tooltip("基础护甲（AD防御）")]
        public float BaseAR;

        public ValueModifier AR_Modifier { get; set; }

        public float AR
        {
            get
            {
                var value = BaseAR;
                return AR_Modifier.SafeInvoke(ref value);
            }
        }

        [Tooltip("基础魔抗（AP防御）")]
        public float BaseMR;

        public ValueModifier MR_Modifier { get; set; }

        public float MR
        {
            get
            {
                var value = BaseMR;
                return MR_Modifier.SafeInvoke(ref value);
            }
        }

        [MMInspectorGroup("Damage")]
        [MMInformation("Here you can specify an effect and a sound FX to instantiate when the object gets damaged, and also how long the object should flicker when hit (only works for sprites).")]
        [Tooltip("whether or not this Health object can be damaged")]
        public bool ImmuneToDamage;

        [Tooltip("the feedback to play when getting damage")]
        public MMFeedbacks DamageMMFeedbacks;

        [Tooltip("if this is true, the damage value will be passed to the MMFeedbacks as its Intensity parameter, letting you trigger more intense feedbacks as damage increases")]
        public bool FeedbackIsProportionalToDamage;

        [Tooltip("if you set this to true, other objects damaging this one won't take any self damage")]
        public bool PreventTakeSelfDamage;

        [MMInspectorGroup("Knockback")]
        [Tooltip("whether or not this object is immune to damage knockback")]
        public bool ImmuneToKnockback;

        [Tooltip("whether or not this object is immune to damage knockback if the damage received is zero")]
        public bool ImmuneToKnockbackIfZeroDamage;

        [Tooltip("a multiplier applied to the incoming knockback forces. 0 will cancel all knockback, 0.5 will cut it in half, 1 will have no effect, 2 will double the knockback force, etc")]
        public float KnockbackForceMultiplier = 1f;

        [MMInspectorGroup("Death")]
        [MMInformation("Here you can set an effect to instantiate when the object dies, a force to apply to it (topdown controller required), how many points to add to the game score, and where the character should respawn (for non-player characters only).")]
        [Tooltip("whether or not this object should get destroyed on death")]
        public bool DestroyOnDeath = true;

        [Tooltip("the time (in seconds) before the character is destroyed or disabled")]
        public float DelayBeforeDestruction;

        [Tooltip("if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)")]
        public bool RespawnAtInitialLocation;

        [Tooltip("if this is true, the controller will be disabled on death")]
        public bool DisableControllerOnDeath = true;

        [Tooltip("if this is true, the model will be disabled instantly on death (if a model has been set)")]
        public bool DisableModelOnDeath = true;

        [Tooltip("if this is true, collisions will be turned off when the character dies")]
        public bool DisableCollisionsOnDeath = true;

        [Tooltip("if this is true, collisions will also be turned off on child colliders when the character dies")]
        public bool DisableChildCollisionsOnDeath;

        [Tooltip("whether or not this object should change layer on death")]
        public bool ChangeLayerOnDeath;

        [Tooltip("whether or not this object should change layer on death")]
        public bool ChangeLayersRecursivelyOnDeath;

        [Tooltip("the layer we should move this character to on death")]
        public MMLayer LayerOnDeath;

        [Tooltip("the feedback to play when dying")]
        public MMFeedbacks DeathMMFeedbacks;

        [Tooltip("if this is true, color will be reset on revive")]
        public bool ResetColorOnRevive = true;

        [Tooltip("the name of the property on your renderer's shader that defines its color")]
        [MMCondition("ResetColorOnRevive", true)]
        public string ColorMaterialPropertyName = "_Color";

        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")]
        public bool UseMaterialPropertyBlocks;

        [MMInspectorGroup("Damage Resistance")]
        [Tooltip("a DamageResistanceProcessor this Health will use to process damage when it's received")]
        public DamageResistanceProcessor TargetDamageResistanceProcessor;

        [MMInspectorGroup("Animator")]
        [Tooltip("the target animator to pass a Death animation parameter to. The Health component will try to auto bind this if left empty")]
        public Animator TargetAnimator;

        /// if this is true, animator logs for the associated animator will be turned off to avoid potential spam
        [Tooltip("if this is true, animator logs for the associated animator will be turned off to avoid potential spam")]
        public bool DisableAnimatorLogs = true;

        public float LastDamage { get; set; }
        public Dmg.Types LastDamageType { get; set; }
        public Vector3 LastDamageDirection { get; set; }
        public Character Character { get; set; }
        public bool Initialized => _initialized;
        public IEventRouter Event => this;
        public IEventRouter eventRouter => this;

        protected Vector3 _initialPosition;
        protected Renderer _renderer;
        protected CharacterMovement _characterMovement;
        protected TopDownController _controller;

        protected MMHealthBar _healthBar;
        protected Collider2D _collider2D;
        protected CharacterController _characterController;
        protected bool _initialized;
        protected Color _initialColor;
        protected int _initialLayer;
        protected MaterialPropertyBlock _propertyBlock;

        float _timeElapsed;

        enum CoroutineState
        {
            None,
            DamageEnabled,
            DestroyObject
        }

        CoroutineState _coroutineState;
        float _coroutineTimeElapsed;
        float _invincibleTime;

        #region Initialization

        /// <summary>
        /// On Awake, we initialize our health
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
            InitializeCurrentHealth();
        }

        /// <summary>
        /// On Start we grab our animator
        /// </summary>
        protected virtual void Start()
        {
            GrabAnimator();
            BindStats();
        }

        void Update()
        {
            var dt = Time.deltaTime;
            switch (_coroutineState)
            {
                case CoroutineState.DamageEnabled:
                    _coroutineTimeElapsed += dt;
                    if (_coroutineTimeElapsed > _invincibleTime)
                    {
                        _coroutineTimeElapsed = 0F;
                        Invulnerable = false;
                        _coroutineState = CoroutineState.None;
                    }

                    break;
                case CoroutineState.DestroyObject:
                    _coroutineTimeElapsed += dt;
                    if (_coroutineTimeElapsed > DelayBeforeDestruction)
                    {
                        _coroutineTimeElapsed = 0F;
                        DestroyObject();
                        _coroutineState = CoroutineState.None;
                    }

                    break;
            }
        }

        void FixedUpdate()
        {
            if (IsDead())
                return;

            var dt = Time.fixedDeltaTime;
            _timeElapsed += dt;
            if (_timeElapsed >= 0.5F)
            {
                _timeElapsed = 0F;
                if (healthRegen == 0)
                    return;

                ReceiveHealth(Heal.Fixed(healthRegen), source: Character);
            }
        }

        /// <summary>
        /// Grabs useful components, enables damage and gets the initial color
        /// </summary>
        public virtual void Initialization()
        {
            Character = GetComponentInParent<Character>();

            if (Model)
            {
                Model.SetActive(true);
            }

            if (GetComponentInParent<Renderer>())
            {
                _renderer = GetComponentInParent<Renderer>();
            }

            if (Character)
            {
                _characterMovement = Character.FindAbility<CharacterMovement>();
                if (Character.Model)
                {
                    if (Character.Model.GetComponentInChildren<Renderer>())
                    {
                        _renderer = Character.Model.GetComponentInChildren<Renderer>();
                    }
                }
            }

            if (_renderer)
            {
                if (UseMaterialPropertyBlocks && _propertyBlock == null)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }

                if (ResetColorOnRevive)
                {
                    if (UseMaterialPropertyBlocks)
                    {
                        if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
                        {
                            _initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
                        }
                    }
                    else
                    {
                        if (_renderer.material.HasProperty(ColorMaterialPropertyName))
                        {
                            _initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
                        }
                    }
                }
            }

            _initialLayer = gameObject.layer;

            _healthBar = GetComponentInParent<MMHealthBar>();
            _controller = GetComponentInParent<TopDownController>();
            _characterController = GetComponentInParent<CharacterController>();
            _collider2D = GetComponentInParent<Collider2D>();

            DamageMMFeedbacks.Initialize(gameObject);
            DeathMMFeedbacks.Initialize(gameObject);

            StoreInitialPosition();
            _initialized = true;
            _timeElapsed = 0F;

            DamageEnabled();
        }

        /// <summary>
        /// Grabs the target animator
        /// </summary>
        protected virtual void GrabAnimator()
        {
            if (TargetAnimator == null)
            {
                BindAnimator();
            }

            if (TargetAnimator && DisableAnimatorLogs)
            {
                TargetAnimator.logWarnings = false;
            }
        }

        /// <summary>
        /// Finds and binds an animator if possible
        /// </summary>
        protected virtual void BindAnimator()
        {
            if (Character)
            {
                if (Character.CharacterAnimator)
                {
                    TargetAnimator = Character.CharacterAnimator;
                }
                else
                {
                    TargetAnimator = GetComponent<Animator>();
                }
            }
            else
            {
                TargetAnimator = GetComponent<Animator>();
            }
        }

        protected virtual void BindStats()
        {
            if (Character && Character.Stats)
            {
                var hp = Character.Stats.GetStat(Character.Stat.HealthMax.Key());
                hp.Event.Add((pre, now) => UpdateHealthBar(true));
                MaximumHealthModifier = (ref float raw) =>
                {
                    raw = hp.Value;
                };

                var hpRegen = Character.Stats.GetStat(Character.Stat.HealthRegen.Key());
                HealthRegenModifier = (ref float raw) =>
                {
                    raw = hpRegen.Value;
                };

                var ar = Character.Stats.GetStat(Character.Stat.AR.Key());
                AR_Modifier = (ref float raw) =>
                {
                    raw = ar.Value;
                };

                var mr = Character.Stats.GetStat(Character.Stat.MR.Key());
                MR_Modifier = (ref float raw) =>
                {
                    raw = mr.Value;
                };

                InitializeCurrentHealth();
            }
        }

        /// <summary>
        /// Stores the initial position for further use
        /// </summary>
        public virtual void StoreInitialPosition()
        {
            _initialPosition = transform.position;
        }

        /// <summary>
        /// Initializes health to either initial or current values
        /// </summary>
        public virtual void InitializeCurrentHealth()
        {
            var initialHealth = InitialHealthDrivenByMaximumHealth ? maximumHealth : InitialHealth;
            SetHealth(initialHealth);
        }

        /// <summary>
        /// When the object is enabled (on respawn for example), we restore its initial health levels
        /// </summary>
        protected virtual void OnEnable()
        {
            if (ResetHealthOnEnable)
                InitializeCurrentHealth();

            if (TargetAnimator)
                TargetAnimator.SetTrigger("Idle");

            if (IsDead())
                DoRevive();

            DamageEnabled();
        }

        /// <summary>
        /// On Disable, we prevent any delayed destruction from running
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        #endregion

        /// <summary>
        /// Returns true if this Health component can be damaged this frame, and false otherwise
        /// </summary>
        /// <returns></returns>
        public virtual bool CanTakeDamageThisFrame(out ResistDamageType type)
        {
            if (!enabled)
            {
                type = ResistDamageType.Disabled;
                return false;
            }

            // if the object is invulnerable, we do nothing and exit
            if (Invulnerable)
            {
                type = ResistDamageType.Invulnerable;
                return false;
            }

            if (ImmuneToDamage)
            {
                type = ResistDamageType.ImmuneToDamage;
                return false;
            }

            // if we're already below zero, we do nothing and exit
            if (CurrentHealth <= 0 && InitialHealth != 0)
            {
                type = ResistDamageType.Dead;
                return false;
            }

            type = ResistDamageType.None;
            return true;
        }

        public bool IsDead()
        {
            return CurrentHealth <= 0 && InitialHealth != 0;
        }

        /// <summary>
        /// Called when the object takes damage
        /// </summary>
        /// <param name="dmg">The amount of health points that will get lost.</param>
        /// <param name="instigator">The object that caused the damage.</param>
        /// <param name="source">This damage is caused by</param>
        /// <param name="invincibleTime">The duration of the short invincibility following the hit.</param>
        /// <param name="direction">The direction of damage.</param>
        /// <param name="typedDamages"></param>
        /// <returns>is lethal damage</returns>
        public virtual void Damage(Dmg dmg, GameObject instigator, Character source = null, float invincibleTime = 0F, Vector3 direction = default, List<TypedDamage> typedDamages = null)
        {
            if (!CanTakeDamageThisFrame(out _))
                return;

            //应用Source的DmgRate
            {
                if (source)
                {
                    var stats = source.Stats;
                    if (stats)
                    {
                        //决定当前伤害的实际伤害类型
                        if (dmg.IsAdaptive())
                        {
                            var bonusAD = stats.GetStat(Stats.AD).BonusValue;
                            var bonusAP = stats.GetStat(Stats.AP).BonusValue;
                            dmg.SetActualType(bonusAD >= bonusAP ? Dmg.Types.AD : Dmg.Types.AP);
                        }

                        //应用Source的DmgRate
                        var rate = stats.GetStat(Stats.DmgRate).Value;
                        dmg.SetDmgRate(rate);
                    }
                }
            }

            ComputeDamageOutput(ref dmg, out var damageDealt, out var damageRaw, typedDamages, true);

            //设置此次dmg实际造成的伤害，并通知伤害飘字显示
            {
                dmg.SetDamageRaw(damageRaw);
                dmg.SetDamageDealt(damageDealt);
                dmg.SetDirection(direction);

                if ((int)dmg.DamageDealt > 0)
                {
                    new DmgTextEvent(dmg, transform).trigger();
                }
            }

            //触发本次伤害所造成的攻击特效/技能特效
            {
                if (source && !dmg.Self)
                {
                    switch (dmg.Effect)
                    {
                        case Dmg.Effects.Attack:
                            source.Event.trigger(new DoAttackEffect(Character));
                            break;
                        case Dmg.Effects.Ability:
                            source.Event.trigger(new DoAbilityEffect(Character));
                            break;
                    }
                }
            }

            // we decrease the character's health by the damage
            float preHealth = CurrentHealth;
            SetHealth(CurrentHealth - damageDealt);
            LastDamage = damageDealt;
            LastDamageType = dmg.ActualType;
            LastDamageDirection = direction;

            Event.trigger(new OnHit());

            // we prevent the character from colliding with Projectiles, Player and Enemies
            if (invincibleTime > 0)
            {
                DamageDisabled();
                _coroutineTimeElapsed = 0F;
                _coroutineState = CoroutineState.DamageEnabled;
                _invincibleTime = invincibleTime;
                // Timing.RunCoroutine(DamageEnabled(invincibleTime));
            }

            // we trigger a damage taken event
            MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damageDealt, preHealth, typedDamages);

            //造成伤害后处理Source吸血，触发DoDmg
            {
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
            }

            //造成伤害后，触发OnDmg
            {
                if (Character && !dmg.Self)
                    Event.trigger(new OnDmg(source, dmg));
            }

            // we update our animator
            if (TargetAnimator)
                TargetAnimator.SetTrigger("Damage");

            // we play our feedback
            if (FeedbackIsProportionalToDamage)
                DamageMMFeedbacks.Play(transform.position, damageDealt);
            else
                DamageMMFeedbacks.Play(transform.position);

            // we update the health bar
            UpdateHealthBar(true);

            // we process any condition state change
            ComputeCharacterConditionStateChanges(typedDamages);
            ComputeCharacterMovementMultipliers(typedDamages);

            //检测是否死亡
            {
                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;
                    var isLethal = Kill();
                    if (source && isLethal && !dmg.Self)
                        source.Health.Event.trigger(new DoKill(Character, instigator));
                }
            }
        }

        /// <summary>
        /// Returns the damage this health should take after processing potential resistances
        /// </summary>
        public virtual bool ComputeDamageOutput(ref Dmg dmg, out float actualDamage, out float rawFinalDamage, List<TypedDamage> typedDamages = null, bool damageApplied = false)
        {
            actualDamage = 0F;
            rawFinalDamage = 0F;
            if (Invulnerable)
                return false;

            if (ImmuneToDamage)
                return false;

            float damage = dmg.Value;
            float totalDamage = 0F;
            // we process our damage through our potential resistances
            if (TargetDamageResistanceProcessor)
            {
                if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                {
                    totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);
                }
            }
            else
            {
                totalDamage = damage;
                if (typedDamages != null)
                {
                    foreach (var typedDamage in typedDamages)
                    {
                        totalDamage += typedDamage.DamageCaused;
                    }
                }
            }

            float rawBaseDamage = ComputeDamageAlgo(dmg.Algo, totalDamage);
            float rawCritDamage = ComputeDamageCrit(dmg, rawBaseDamage);
            rawFinalDamage = ComputeDamageRate(dmg, rawCritDamage);

            if (dmg.Mix.On)
            {
                dmg.Mix = ComputeDamageMix(dmg.Mix, rawFinalDamage);
                actualDamage = dmg.Mix.Sum();
            }
            else
            {
                actualDamage = ComputeDamageDefence(dmg.ActualType, rawFinalDamage);
            }

            return actualDamage > 0;
        }

        protected virtual float ComputeDamageAlgo(Dmg.Algos algo, float value)
        {
            return algo switch
            {
                Dmg.Algos.Fixed => value,
                Dmg.Algos.CurPct => CurrentHealth * value,
                Dmg.Algos.LostPct => (maximumHealth - CurrentHealth) * value,
                Dmg.Algos.AllPct => maximumHealth * value,
                _ => value
            };
        }

        protected virtual float ComputeHealAlgo(Heal.Algos algo, float value)
        {
            return algo switch
            {
                Heal.Algos.Fixed => value,
                Heal.Algos.CurPct => CurrentHealth * value,
                Heal.Algos.LostPct => (maximumHealth - CurrentHealth) * value,
                Heal.Algos.AllPct => maximumHealth * value,
                _ => value
            };
        }

        protected virtual float ComputeDamageCrit(Dmg dmg, float damage)
        {
            return dmg.IsCrit switch
            {
                true => damage * dmg.CritRate,
                false => damage,
            };
        }

        protected virtual float ComputeDamageRate(Dmg dmg, float damage)
        {
            return damage * dmg.DmgRate;
        }

        protected virtual Dmg.Mixed ComputeDamageMix(Dmg.Mixed mix, float damage)
        {
            var dmgAD = mix.PctAD * damage;
            if (dmgAD > 0)
                mix.DamageDealtAD = ComputeDamageDefence(Dmg.Types.AD, dmgAD);

            var dmgAP = mix.PctAP * damage;
            if (dmgAP > 0)
                mix.DamageDealtAP = ComputeDamageDefence(Dmg.Types.AP, dmgAP);

            var dmgTrue = mix.PctTrue * damage;
            if (dmgTrue > 0)
                mix.DamageDealtTrue = ComputeDamageDefence(Dmg.Types.True, dmgTrue);

            return mix;
        }

        protected virtual float ComputeDamageDefence(Dmg.Types type, float damage)
        {
            return type switch
            {
                Dmg.Types.AD => damage / (AR / 100 + 1),
                Dmg.Types.AP => damage / (MR / 100 + 1),
                Dmg.Types.True => damage,
                _ => damage
            };
        }

        /// <summary>
        /// Goes through resistances and applies condition state changes if needed
        /// </summary>
        /// <param name="typedDamages"></param>
        protected virtual void ComputeCharacterConditionStateChanges(List<TypedDamage> typedDamages)
        {
            if (typedDamages == null || Character == null)
                return;

            foreach (TypedDamage typedDamage in typedDamages)
            {
                if (typedDamage.ForceCharacterCondition)
                {
                    if (TargetDamageResistanceProcessor)
                    {
                        if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                        {
                            if (TargetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage.AssociatedDamageType))
                            {
                                continue;
                            }
                        }
                    }

                    Character.ChangeCharacterConditionTemporarily(typedDamage.ForcedCondition, typedDamage.ForcedConditionDuration, typedDamage.ResetControllerForces, typedDamage.DisableGravity);
                }
            }
        }

        /// <summary>
        /// Goes through the resistance list and applies movement multipliers if needed
        /// </summary>
        /// <param name="typedDamages"></param>
        protected virtual void ComputeCharacterMovementMultipliers(List<TypedDamage> typedDamages)
        {
            if (typedDamages == null)
                return;

            if (Character == null)
                return;

            foreach (TypedDamage typedDamage in typedDamages)
            {
                if (typedDamage.ApplyMovementMultiplier)
                {
                    if (TargetDamageResistanceProcessor)
                    {
                        if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                        {
                            if (TargetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage.AssociatedDamageType))
                            {
                                continue;
                            }
                        }
                    }

                    _characterMovement?.ApplyMovementMultiplier(typedDamage.MovementMultiplier, typedDamage.MovementMultiplierDuration);
                }
            }
        }

        /// <summary>
        /// Determines a new knockback force by processing it through resistances
        /// </summary>
        /// <param name="knockbackForce"></param>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual Vector3 ComputeKnockbackForce(Vector3 knockbackForce, List<TypedDamage> typedDamages = null)
        {
            return TargetDamageResistanceProcessor == null ? knockbackForce : TargetDamageResistanceProcessor.ProcessKnockbackForce(knockbackForce, typedDamages);
        }

        /// <summary>
        /// Returns true if this Health can get knockbacked, false otherwise
        /// </summary>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual bool CanGetKnockback(List<TypedDamage> typedDamages)
        {
            if (ImmuneToKnockback)
                return false;

            if (!TargetDamageResistanceProcessor)
                return true;

            if (!TargetDamageResistanceProcessor.isActiveAndEnabled)
                return true;

            if (TargetDamageResistanceProcessor.CheckPreventKnockback(typedDamages))
                return false;

            return true;
        }

        /// <summary>
        /// Kills the character, instantiates death effects, handles points, etc
        /// </summary>
        public virtual bool Kill()
        {
            if (ImmuneToDamage)
                return false;

            if (Character)
            {
                // we set its dead state to true
                Character.ConditionState.ChangeState(Character.Conditions.Dead);
                Character.Reset();
            }

            SetHealth(0);

            // we prevent further damage
            DamageDisabled();

            DeathMMFeedbacks.Play(transform.position);

            if (TargetAnimator)
                TargetAnimator.SetTrigger("Death");

            // we make it ignore the collisions from now on
            if (DisableCollisionsOnDeath)
            {
                if (_collider2D) _collider2D.enabled = false;

                // if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
                if (_controller) _controller.CollisionsOff();

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
                // Timing.RunCoroutine(CoDestroyObject(DelayBeforeDestruction), gameObject);
            }
            else
                DestroyObject();

            return true;
        }

        /// <summary>
        /// Revive this object.
        /// </summary>
        public virtual void Revive()
        {
            if (!_initialized)
                return;

            DoRevive();

            Initialization();
            InitializeCurrentHealth();
            Event.trigger(new OnRevive());
            MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Revive);
        }

        void DoRevive()
        {
            if (DisableChildCollisionsOnDeath)
            {
                if (_collider2D) _collider2D.enabled = true;

                foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
                    c.enabled = true;
            }

            if (ChangeLayerOnDeath)
            {
                gameObject.layer = _initialLayer;
                if (ChangeLayersRecursivelyOnDeath)
                {
                    transform.ChangeLayersRecursively(_initialLayer);
                }
            }

            if (_characterController)
                _characterController.enabled = true;

            if (_controller)
            {
                _controller.enabled = true;
                _controller.CollisionsOn();
                _controller.Reset();
            }

            if (Character)
                Character.ConditionState?.ChangeState(Character.Conditions.Normal);

            if (ResetColorOnRevive && _renderer)
            {
                if (UseMaterialPropertyBlocks)
                {
                    _renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
                    _renderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    _renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
                }
            }

            if (RespawnAtInitialLocation)
                transform.position = _initialPosition;

            if (_healthBar)
                _healthBar.Initialization();
        }

        IEnumerator<float> CoDestroyObject(float duration)
        {
            yield return Timing.WaitForSeconds(duration);
            DestroyObject();
        }

        /// <summary>
        /// Destroys the object, or tries to, depending on the character's settings
        /// </summary>
        protected virtual void DestroyObject()
        {
            if (DestroyOnDeath)
            {
                if (Character)
                    Character.gameObject.SetActive(false);
                else
                    gameObject.SetActive(false);
            }
        }

        #region HealthManipulationAPIs

        /// <summary>
        /// Sets the current health to the specified new value, and updates the health bar
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetHealth(float newValue)
        {
            CurrentHealth = newValue;
            UpdateHealthBar(false);
        }

        /// <summary>
        /// Called when the character gets health (from a stimpack for example)
        /// </summary>
        /// <param name="heal">The health the character gets.</param>
        /// <param name="source"></param>
        /// <param name="instigator">The thing that gives the character health.</param>
        public virtual void ReceiveHealth(Heal heal, GameObject instigator = null, Character source = null)
        {
            //阵亡后无法再回血
            if (CurrentHealth <= 0F)
                return;

            var healing = ComputeHealAlgo(heal.Algo, heal.Value);
            if (healing <= 0F)
                return;

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

            SetHealth(newHealth);
            UpdateHealthBar(true);

            if (heal.IsValid())
            {
                if (source)
                    source.Event.trigger(new DoHeal(this, heal));

                Event.trigger(new OnHeal(source, heal));
            }
        }

        /// <summary>
        /// Resets the character's health to its max value
        /// </summary>
        public virtual void ResetHealthToMaxHealth()
        {
            SetHealth(maximumHealth);
        }

        /// <summary>
        /// Forces a refresh of the character's health bar
        /// </summary>
        public virtual void UpdateHealthBar(bool show)
        {
            if (_healthBar)
                _healthBar.UpdateBar(CurrentHealth, 0f, maximumHealth, show);
        }

        #endregion

        #region DamageDisablingAPIs

        /// <summary>
        /// Prevents the character from taking any damage
        /// </summary>
        public virtual void DamageDisabled()
        {
            Invulnerable = true;
        }

        /// <summary>
        /// Allows the character to take damage
        /// </summary>
        public virtual void DamageEnabled()
        {
            Invulnerable = false;
        }

        /// <summary>
        /// Prevents the character from taking any damage
        /// </summary>
        public virtual void SetDashInvincible(bool value)
        {
            DashInvincible = value;
        }

        /// <summary>
        /// makes the character able to take damage again after the specified delay
        /// </summary>
        /// <returns>The layer collision.</returns>
        IEnumerator<float> DamageEnabled(float delay)
        {
            yield return Timing.WaitForSeconds(delay);
            Invulnerable = false;
        }

        #endregion
    }
}