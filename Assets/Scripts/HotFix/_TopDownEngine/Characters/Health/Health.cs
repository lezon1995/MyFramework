using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    public struct Heal
    {
        public int Value;
        public Algos Algo;
        public int Healing;

        public static Heal Fixed(int value) => new(value, Algos.Fixed);
        public static Heal CurPct(int value) => new(value, Algos.CurPct);
        public static Heal LostPct(int value) => new(value, Algos.LostPct);
        public static Heal AllPct(int value) => new(value, Algos.AllPct);

        public Heal(int value) : this()
        {
            Value = value;
            Algo = Algos.Fixed;
            Healing = value;
        }

        public Heal(int value, Algos algo)
        {
            Value = value;
            Algo = algo;
            Healing = value;
        }

        public bool IsValid()
        {
            return Healing > 0F;
        }

        public void SetHealing(int value)
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
        BornInvincible,
        Invincible,
        ImmuneToDamage,
        Dead,
        Disabled,
    }

    public enum DodgeDamageType
    {
        None,
        Chance,
        Dash,
    }

    public enum RefreshHealthBarType
    {
        Immediately,
        ReceiveDamage,
        ReceiveHealing,
        Killed,
        Born,
        Resurrect,
    }

    /// <summary>
    /// This class manages the health of an object, pilots its potential health bar, handles what happens when it takes damage,
    /// and what happens when it dies.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/Health")]
    public class Health : TopDownMonoBehaviour, IEventRouter, IReusable
    {
        [MMInspectorGroup("Bindings")]
        [Tooltip("the model to disable (if set so)")]
        public GameObject Model;

        [MMInspectorGroup("Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("the current health of the character")]
        public int CurrentHealth { get; set; }

        public float HealthPct => (float)CurrentHealth / maximumHealth;

        [ShowInInspector, ReadOnly]
        [Tooltip("If this is true, this object can't take damage at this time")]
        public bool Invincible { get; set; }

        public bool DashInvincible { get; set; }

        public bool IsDeadTotally { get; set; }

        [MMInspectorGroup("Health")]
        public int InitialHealth = 10;

        public bool InitialHealthDrivenByMaximumHealth;

        public int MaximumHealth = 10;

        public ValueModifier MaximumHealthModifier { get; set; }

        public int maximumHealth
        {
            get
            {
                var maxHealth = (float)MaximumHealth;
                return (int)MaximumHealthModifier.SafeInvoke(ref maxHealth);
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


        [Tooltip("基础闪避率")]
        public float BaseDodgeChance;

        public ValueModifier DodgeChance_Modifier { get; set; }

        public float DodgeChance
        {
            get
            {
                var value = BaseDodgeChance;
                return DodgeChance_Modifier.SafeInvoke(ref value);
            }
        }

        [MMInspectorGroup("Damage")]
        [MMInformation("Here you can specify an effect and a sound FX to instantiate when the object gets damaged, and also how long the object should flicker when hit (only works for sprites).")]
        [Tooltip("whether or not this Health object can be damaged")]
        public bool ImmuneToDamage;

        public float DamageCheckInterval;

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

        [MMInspectorGroup("Animator")]
        [Tooltip("the target animator to pass a Death animation parameter to. The Health component will try to auto bind this if left empty")]
        public Animator TargetAnimator;

        /// if this is true, animator logs for the associated animator will be turned off to avoid potential spam
        [Tooltip("if this is true, animator logs for the associated animator will be turned off to avoid potential spam")]
        public bool DisableAnimatorLogs = true;

        public float LastDamage { get; set; }
        public Dmg.Types LastDamageType { get; set; }
        public Vector3 LastDamageDirection { get; set; }
        public Character Character;
        public TopDownController Controller => Character?.Controller;
        public bool hasCharacter { get; set; }
        public bool Initialized => _initialized;
        public IEventRouter Event => this;
        public Action<int, int> onHealthChanged;

        protected Renderer _renderer;
        protected CharacterMovement _characterMovement;
        protected TopDownController _controller;

        protected Collider2D _collider2D;
        protected bool _initialized;
        protected Color _initialColor;
        protected int _initialLayer;
        protected MaterialPropertyBlock _propertyBlock;

        protected float _timeElapsed;

        protected enum CoroutineState
        {
            None,
            DamageEnabled,
            DestroyObject
        }

        protected CoroutineState _coroutineState;
        protected float _coroutineTimeElapsed;
        protected float _invincibleTime;

        #region Initialization

        /// <summary>
        /// On Awake, we initialize our health
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
            InitializeCurrentHealth(RefreshHealthBarType.Born);
        }

        /// <summary>
        /// On Start, we grab our animator
        /// </summary>
        protected virtual void Start()
        {
            GrabAnimator();
            BindStats();
        }

        protected void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
            if (IsDeadTotally)
                return;

            var dt = Time.fixedDeltaTime;
            UpdateCoroutineState(dt);

            if (IsDead())
                return;

            UpdateHealthRegen(dt);
        }

        protected virtual void UpdateHealthRegen(float dt)
        {
            if (healthRegen > 0)
            {
                _timeElapsed += dt;
                if (_timeElapsed >= 1F)
                {
                    _timeElapsed = 0F;
                    ReceiveHealth(Heal.Fixed((int)healthRegen), source: Character);
                }
            }
        }

        protected void UpdateCoroutineState(float dt)
        {
            switch (_coroutineState)
            {
                case CoroutineState.DamageEnabled:
                    _coroutineTimeElapsed += dt;
                    if (_coroutineTimeElapsed > _invincibleTime)
                    {
                        _coroutineTimeElapsed = 0F;
                        DamageEnabled();
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

        /// <summary>
        /// Grabs useful components, enables damage and gets the initial color
        /// </summary>
        public virtual void Initialization()
        {
            hasCharacter = this.TryGetComponentInParent(out Character);

            if (Model)
                Model.SetActive(true);

            this.TryGetComponentInParent(out _renderer);

            if (Character)
            {
                Character.FindAbility(out _characterMovement);
                var model = Character.Model;
                if (model)
                {
                    if (model.GetComponentInChildren<Renderer>())
                    {
                        _renderer = model.GetComponentInChildren<Renderer>();
                    }
                }
            }

            if (_renderer)
            {
                if (UseMaterialPropertyBlocks && _propertyBlock == null)
                {
                    _propertyBlock = new();
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

            this.TryGetComponentInParent(out _controller);
            this.TryGetComponentInParent(out _collider2D);

            DamageMMFeedbacks.Initialize(gameObject);
            DeathMMFeedbacks.Initialize(gameObject);

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
                    TryGetComponent(out TargetAnimator);
                }
            }
            else
            {
                TryGetComponent(out TargetAnimator);
            }
        }

        protected virtual void BindStats()
        {
            if (Character && Character.Stats)
            {
                var hp = Character.GetStat(Character.Stat.HealthMax);
                hp.Event.Add((pre, now) => RefreshHealthBar(true));
                MaximumHealthModifier = (ref float raw) => { raw = hp.Value; };

                var hpRegen = Character.GetStat(Character.Stat.HealthRegen);
                HealthRegenModifier = (ref float raw) => { raw = hpRegen.Value; };

                var ar = Character.GetStat(Character.Stat.AR);
                AR_Modifier = (ref float raw) => { raw = ar.Value; };

                var mr = Character.GetStat(Character.Stat.MR);
                MR_Modifier = (ref float raw) => { raw = mr.Value; };

                var dodgeChance = Character.GetStat(Character.Stat.DodgeChance);
                DodgeChance_Modifier = (ref float raw) => { raw = dodgeChance.Value; };

                InitializeCurrentHealth(RefreshHealthBarType.Born);
            }
        }

        /// <summary>
        /// Initializes health to either initial or current values
        /// </summary>
        public virtual void InitializeCurrentHealth(RefreshHealthBarType type)
        {
            var initialHealth = InitialHealthDrivenByMaximumHealth ? maximumHealth : InitialHealth;
            SetHealth((int)initialHealth, type);
        }

        /// <summary>
        /// When the object is enabled (on respawn for example), we restore its initial health levels
        /// </summary>
        protected virtual void OnEnable()
        {
            if (ResetHealthOnEnable)
                InitializeCurrentHealth(RefreshHealthBarType.Resurrect);

            if (TargetAnimator)
                TargetAnimator.SetTrigger("Idle");

            if (IsDead())
                DoResurrect();

            DamageEnabled();

            if (Model)
                Model.SetActive(true);
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

            if (ImmuneToDamage)
            {
                type = ResistDamageType.ImmuneToDamage;
                return false;
            }

            if (Invincible)
            {
                type = ResistDamageType.Invincible;
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

        public virtual bool CanDodgeDamageThisFrame(out DodgeDamageType type)
        {
            if (DodgeChance > 0 && randomHit(DodgeChance))
            {
                type = DodgeDamageType.Chance;
                return true;
            }

            type = DodgeDamageType.None;
            return false;
        }

        /// <summary>
        /// Determines whether knockback should be applied
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldApplyKnockback(Dmg damage)
        {
            if (ImmuneToKnockbackIfZeroDamage && ComputeDamageOutput(ref damage, null))
                return false;

            if (Invincible)
                return false;

            return CanGetKnockback();
        }

        public virtual void ApplyKnockback(Vector3 knockbackForce, Dmg damage)
        {
            if (ShouldApplyKnockback(damage))
            {
                ComputeKnockbackForce(ref knockbackForce);

                Controller.AddImpact(knockbackForce.normalized, knockbackForce.magnitude);
            }
        }


        public bool IsDead()
        {
            return CurrentHealth <= 0;
        }

        public bool IsAlive()
        {
            return CurrentHealth > 0;
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
        public virtual void Damage(ref Dmg dmg, GameObject instigator, Character source = null, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null)
        {
            if (!CanTakeDamageThisFrame(out _))
                return;

            if (CanDodgeDamageThisFrame(out var dodgeType))
            {
                switch (dodgeType)
                {
                    case DodgeDamageType.Chance:
                        Character.Event.trigger(new DoChanceDodge());
                        break;
                    case DodgeDamageType.Dash:
                        Character.Event.trigger(new DoDashDodge());
                        break;
                }

                EnterInvincible(invincibleTime);
                return;
            }

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
                    }
                }
            }

            ComputeDamageOutput(ref dmg, source, calculator);

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

            Event.trigger(new OnHit());

            if (dmg.DamageDealt > 0)
            {
                // we decrease the character's health by the damage
                float preHealth = CurrentHealth;
                SetHealth(CurrentHealth - dmg.DamageDealt, RefreshHealthBarType.ReceiveDamage);
                LastDamage = dmg.DamageDealt;
                LastDamageType = dmg.ActualType;
                LastDamageDirection = direction;

                //造成伤害后处理Source吸血，触发DoDmg
                if (source && !dmg.Self)
                {
                    if (dmg.Effect == Dmg.Effects.Attack)
                    {
                        if (source.GetStat(Character.Stat.LifeSteal, out var lifeSteal))
                        {
                            var healing = lifeSteal.Value * dmg.DamageDealt;
                            source.Health.ReceiveHealth(Heal.Fixed((int)healing), source: source);
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
                if (!dmg.IsLethal)
                {
                    EnterInvincible(invincibleTime);
                }
            }
        }

        protected void EnterInvincible(float invincibleTime)
        {
            if (invincibleTime > 0)
            {
                DamageDisabled();
                _coroutineTimeElapsed = 0F;
                _coroutineState = CoroutineState.DamageEnabled;
                _invincibleTime = invincibleTime;
            }
        }

        public void InvincibleDuration(float duration)
        {
            if (duration > 0)
            {
                DamageDisabled();
                _coroutineTimeElapsed = 0F;
                _coroutineState = CoroutineState.DamageEnabled;
                _invincibleTime = duration;
            }
        }

        /// <summary>
        /// Returns the damage this health should take after processing potential resistances
        /// </summary>
        public virtual bool ComputeDamageOutput(ref Dmg dmg, Character source, IDmgCalculator calculator = null)
        {
            float actualDamage;
            if (Invincible)
                return false;

            if (ImmuneToDamage)
                return false;

            calculator ??= DmgCalculator.Default;

            float damage = dmg.Value;
            float totalDamage = damage;

            float rawBaseDamage = calculator.computeDamageAlgo(dmg.Algo, totalDamage, CurrentHealth, maximumHealth);
            float rawCritDamage = calculator.computeDamageCrit(dmg, rawBaseDamage);
            float rawFinalDamage = calculator.computeDamageRate(dmg, rawCritDamage);

            var physicResist = AR;
            var magicResist = MR;

            if (source)
            {
                if (source.GetStat(Character.Stat.AD_PT_Rate, out var physicResistPenetrationRate))
                {
                    physicResist *= (1 - physicResistPenetrationRate.Value);
                }

                if (source.GetStat(Character.Stat.AD_PT, out var physicResistPenetration))
                {
                    physicResist -= physicResistPenetration.Value;
                }

                if (source.GetStat(Character.Stat.AP_PT_Rate, out var magicResistPenetrationRate))
                {
                    magicResist *= (1 - magicResistPenetrationRate.Value);
                }

                if (source.GetStat(Character.Stat.AP_PT, out var magicResistPenetration))
                {
                    magicResist -= magicResistPenetration.Value;
                }
            }

            if (dmg.Mix.On)
            {
                dmg.Mix = calculator.computeDamageMix(dmg.Mix, rawFinalDamage, physicResist, magicResist);
                actualDamage = dmg.Mix.Sum();
            }
            else
            {
                actualDamage = calculator.computeDamageDefence(dmg.ActualType, rawFinalDamage, physicResist, magicResist);
            }

            dmg.SetDamageRaw((int)rawFinalDamage);
            dmg.SetDamageDealt((int)actualDamage);
            return actualDamage > 0;
        }

        protected virtual int ComputeHealAlgo(Heal.Algos algo, int value)
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

        protected virtual int ComputeHealRate(int value)
        {
            if (Character)
            {
                if (Character.GetStat(Character.Stat.HealRate, out var healRate))
                {
                    return (int)(value * (1 + healRate.Value));
                }
            }

            return value;
        }

        /// <summary>
        /// Determines a new knockback force by processing it through resistances
        /// </summary>
        /// <param name="knockbackForce"></param>
        /// <returns></returns>
        public virtual void ComputeKnockbackForce(ref Vector3 knockbackForce)
        {
        }

        /// <summary>
        /// Returns true if this Health can get knockbacked, false otherwise
        /// </summary>
        /// <returns></returns>
        public virtual bool CanGetKnockback()
        {
            if (ImmuneToKnockback)
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
                Character.conditionState.ChangeState(Character.Conditions.Dead);
                Character.Reset();
            }

            SetHealth(0, RefreshHealthBarType.Killed);

            DeathMMFeedbacks.Play(transform.position);

            if (TargetAnimator)
                TargetAnimator.SetTrigger("Death");

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

            return true;
        }

        /// <summary>
        /// Revive this object.
        /// </summary>
        public virtual void Resurrect()
        {
            if (!_initialized)
                return;

            DoResurrect();

            Initialization();
            InitializeCurrentHealth(RefreshHealthBarType.Resurrect);
            Event.trigger(new OnRevive());
        }

        protected virtual void DoResurrect()
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

            if (_controller)
            {
                _controller.enabled = true;
                _controller.CollisionsOn();
                _controller.Reset();
            }

            if (Character)
                Character.conditionState?.ChangeState(Character.Conditions.Normal);

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

            IsDeadTotally = true;
        }

        #region HealthManipulationAPIs

        /// <summary>
        /// Sets the current health to the specified new value, and updates the health bar
        /// </summary>
        public virtual void SetHealth(int curHealth, RefreshHealthBarType type = RefreshHealthBarType.Immediately)
        {
            CurrentHealth = curHealth;
            switch (type)
            {
                case RefreshHealthBarType.Immediately:
                    RefreshHealthBar(false);
                    break;
                case RefreshHealthBarType.ReceiveDamage:
                    RefreshHealthBarByDamage();
                    break;
                case RefreshHealthBarType.ReceiveHealing:
                    RefreshHealthBarByHeal();
                    break;
                case RefreshHealthBarType.Killed:
                    RefreshHealthBar(false);
                    break;
                case RefreshHealthBarType.Born:
                    RefreshHealthBar(false);
                    break;
                case RefreshHealthBarType.Resurrect:
                    RefreshHealthBar(false);
                    break;
            }

            onHealthChanged?.Invoke(CurrentHealth, maximumHealth);
        }

        public virtual void SetHealth(int curHealth, int maxHealth, RefreshHealthBarType type = RefreshHealthBarType.Immediately)
        {
            CurrentHealth = curHealth;
            MaximumHealth = maxHealth;
            switch (type)
            {
                case RefreshHealthBarType.Immediately:
                    RefreshHealthBar(false);
                    break;
                case RefreshHealthBarType.ReceiveDamage:
                    RefreshHealthBarByDamage();
                    break;
                case RefreshHealthBarType.ReceiveHealing:
                    RefreshHealthBarByHeal();
                    break;
                case RefreshHealthBarType.Killed:
                    RefreshHealthBar(false);
                    break;
                case RefreshHealthBarType.Born:
                    RefreshHealthBar(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            onHealthChanged?.Invoke(CurrentHealth, maximumHealth);
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
            healing = ComputeHealRate(healing);
            if (healing <= 0F)
                return;

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
            }
        }

        /// <summary>
        /// Resets the character's health to its max value
        /// </summary>
        public virtual void ResetHealthToMaxHealth()
        {
            SetHealth(maximumHealth, RefreshHealthBarType.Resurrect);
        }

        public virtual void RefreshHealthBar(bool show)
        {
        }

        public virtual void RefreshHealthBarByDamage()
        {
        }

        public virtual void RefreshHealthBarByHeal()
        {
        }

        #endregion

        #region DamageDisablingAPIs

        /// <summary>
        /// Prevents the character from taking any damage
        /// </summary>
        public virtual void DamageDisabled()
        {
            Invincible = true;
        }

        /// <summary>
        /// Allows the character to take damage
        /// </summary>
        public virtual void DamageEnabled()
        {
            Invincible = false;
        }

        /// <summary>
        /// Prevents the character from taking any damage
        /// </summary>
        public virtual void SetDashInvincible(bool value)
        {
            DashInvincible = value;
        }

        #endregion

        public bool inUse { get; set; }

        public virtual void onAcquire()
        {
            inUse = true;
            IsDeadTotally = false;
        }

        public virtual void onRelease()
        {
            inUse = false;
        }
    }
}