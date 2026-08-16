using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    public abstract partial class Weapon
    {
        public enum Stat
        {
            AD, //Attack Damage
            AP, //Ability Power
            AS, //Attack Speed
            CD, //Cooldown
            CritChance, //Crit Chance
            CritDamage, //Crit Damage

            Range,
            Scale,
            Duration,
            Count,
        }

        /// the possible use modes for the trigger
        /// semi auto : the Player needs to release the trigger to fire again,
        /// auto : the Player can hold the trigger to fire repeatedly
        public enum TriggerModes
        {
            SemiAuto,
            Auto
        }

        /// the possible states the weapon can be in
        public enum States
        {
            Idle,
            Start,
            DelayBeforeUse, //Windup phase
            Use,
            DelayBetweenUses, //Cooldown phase
            Stop,
            ReloadNeeded,
            ReloadStart,
            Reloading,
            ReloadStop,
            Interrupted
        }

        public enum Types
        {
            None,
            BasicAttack, //普通攻击
        }

        public UniStats.Stat GetStat(Stat key)
        {
            if (Stats == null)
                TryGetComponent(out Stats);

            return Stats == null ? null : Stats.GetStat(key.Key());
        }

        public bool GetStat(Stat key, out UniStats.Stat stat)
        {
            if (Stats == null)
            {
                stat = null;
                return false;
            }

            return Stats.GetStat(key.Key(), out stat);
        }
    }

    /// <summary>
    /// This base class, meant to be extended (see ProjectileWeapon.cs for an example of that) handles rate of fire (rate of use actually), and ammo reloading
    /// </summary>
    [SelectionBase]
    public abstract partial class Weapon : MMMonoBehaviour, IStatsGetter<Weapon.Stat>
    {
        [MMInspectorGroup("ID")]
        [Tooltip("the name of the weapon, only used for debugging")]
        public string WeaponName;

        public Types Type;
        public bool IsBasicAttack => Type == Types.BasicAttack;

        [ShowInInspector, ReadOnly]
        [Tooltip("whether or not the weapon is currently active")]
        public bool WeaponCurrentlyActive { get; set; } = true;

        [MMInspectorGroup("Use")]
        [Tooltip("if this is true, this weapon will be able to read input (usually via the CharacterHandleWeapon ability), otherwise player input will be disabled")]
        public bool InputAuthorized = true;

        [Tooltip("是否要求有瞄准目标才能射击")]
        public bool RequireAimTarget;

        [Tooltip("is this weapon on semi or full auto ?")]
        public TriggerModes TriggerMode = TriggerModes.Auto;

        [Tooltip("the delay before use, that will be applied for every shot")]
        public float DelayBeforeUse;

        public float DelayBeforeUsePct;
        public float DelayBeforeUseMultiplier = 1;

        public ValueModifier DelayBeforeUseModifier { get; set; }

        //武器使用前摇频率（次/秒）
        protected float delayBeforeUseF => 1 / delayBeforeUse;

        //武器使用前摇时长（秒）
        protected float delayBeforeUse
        {
            get
            {
                var value = DelayBeforeUse;
                return DelayBeforeUseModifier.SafeInvoke(ref value);
            }
        }

        [Tooltip("whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)")]
        public bool DelayBeforeUseReleaseInterruption = true;

        [Tooltip("the time (in seconds) between two shots")]
        public float TimeBetweenUses = 1f;

        public ValueModifier TimeBetweenUsesModifier { get; set; }

        //武器使用后摇频率（次/秒）
        protected float timeBetweenUsesF => 1 / timeBetweenUses;

        //武器使用后摇时长（秒）
        protected float timeBetweenUses
        {
            get
            {
                var value = TimeBetweenUses;
                return TimeBetweenUsesModifier.SafeInvoke(ref value);
            }
        }

        [Tooltip("whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)")]
        public bool TimeBetweenUsesReleaseInterruption = true;

        [Header("Burst Mode")]
        [Tooltip("if this is true, the weapon will activate repeatedly for every shoot request")]
        public bool UseBurstMode;

        [Tooltip("the amount of 'shots' in a burst sequence")]
        public int BurstLength = 3;

        [Tooltip("the time between shots in a burst sequence (in seconds)")]
        public float BurstTimeBetweenShots = 0.1f;

        [MMInspectorGroup("Magazine")]
        [Tooltip("whether the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool")]
        public bool MagazineBased;

        [Tooltip("the size of the magazine")]
        public int MagazineSize = 30;

        [Tooltip("if this is true, pressing the fire button when a reload is needed will reload the weapon. Otherwise you'll need to press the reload button")]
        public bool AutoReload;

        [Tooltip("if this is true, reload will automatically happen right after the last bullet is shot, without the need for input")]
        public bool NoInputReload;

        [Tooltip("the time it takes to reload the weapon")]
        public float ReloadTime = 2f;

        [Tooltip("the amount of ammo consumed everytime the weapon fires")]
        public int AmmoConsumedPerShot = 1;

        [Tooltip("if this is set to true, the weapon will auto destroy when there's no ammo left")]
        public bool AutoDestroyWhenEmpty;

        [Tooltip("the delay (in seconds) before weapon destruction if empty")]
        public float AutoDestroyWhenEmptyDelay = 1f;

        [Tooltip("if this is true, the weapon won't try and reload if the ammo is empty, when using WeaponAmmo")]
        public bool PreventReloadIfAmmoEmpty;

        [ShowInInspector, ReadOnly]
        [Tooltip("the current amount of ammo loaded inside the weapon")]
        public int CurrentAmmoLoaded { get; set; }

        [MMInspectorGroup("Position")]
        [Tooltip("an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.")]
        public Vector3 WeaponAttachmentOffset = Vector3.zero;

        [Tooltip("should that weapon be flipped when the character flips?")]
        public bool FlipWeaponOnCharacterFlip = true;

        [Tooltip("the FlipValue will be used to multiply the model's transform's localScale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs")]
        public Vector3 RightFacingFlipValue = new(1, 1, 1);

        [Tooltip("the FlipValue will be used to multiply the model's transform's localScale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs")]
        public Vector3 LeftFacingFlipValue = new(-1, 1, 1);

        [Tooltip("a transform to use as the spawn point for weapon use (if null, only offset will be considered, otherwise the transform without offset)")]
        public Transform WeaponUseTransform;

        [Tooltip("if this is true, the weapon will flip to match the character's orientation")]
        public bool WeaponShouldFlip = true;

        [MMInspectorGroup("IK")]
        [Tooltip("the transform to which the character's left hand should be attached to")]
        public Transform LeftHandHandle;

        [Tooltip("the transform to which the character's right hand should be attached to")]
        public Transform RightHandHandle;

        [MMInspectorGroup("Movement")]
        [Tooltip("if this is true, a multiplier will be applied to movement while the weapon is active")]
        public bool ModifyMovementWhileAttacking;

        public bool ModifyMovementWhileDelayBeforeUse;

        [Tooltip("the multiplier to apply to movement while attacking")]
        public float MovementMultiplier;

        [Tooltip("if this is true all movement will be prevented (even flip) while the weapon is active")]
        public bool PreventAllMovementWhileInUse;

        [Tooltip("if this is true all aim will be prevented while the weapon is active")]
        public bool PreventAllAimWhileInUse;

        [MMInspectorGroup("Recoil")]
        [Tooltip("the force to apply to push the character back when shooting - positive values will push the character back, negative values will launch it forward, turning that recoil into a thrust")]
        public float RecoilForce;

        [MMInspectorGroup("Animation")]
        [Tooltip("the other animators (other than the Character's) that you want to update every time this weapon gets used")]
        public List<Animator> Animators;

        [Tooltip("If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.")]
        public bool PerformAnimatorSanityChecks;

        [Tooltip("if this is true, the weapon's animator(s) will mirror the animation parameter of the owner character (that way your weapon's animator will be able to 'know' if the character is walking, jumping, etc)")]
        public bool MirrorCharacterAnimatorParameters;

        [MMInspectorGroup("Animation Parameters Names")]
        [Tooltip("the ID of the weapon to pass to the animator")]
        public int WeaponAnimationID;

        [Tooltip("the name of the weapon's idle animation parameter : this will be true all the time except when the weapon is being used")]
        public string IdleAnimationParameter;

        [Tooltip("the name of the weapon's start animation parameter : true at the frame where the weapon starts being used")]
        public string StartAnimationParameter;

        [Tooltip("the name of the weapon's delay before use animation parameter : true when the weapon has been activated but hasn't been used yet")]
        public string DelayBeforeUseAnimationParameter;

        [Tooltip("the name of the weapon's single use animation parameter : true at each frame the weapon activates (shoots)")]
        public string SingleUseAnimationParameter;

        [Tooltip("the name of the weapon's in use animation parameter : true at each frame the weapon has started firing but hasn't stopped yet")]
        public string UseAnimationParameter;

        [Tooltip("the name of the weapon's delay between each use animation parameter : true when the weapon is in use")]
        public string DelayBetweenUsesAnimationParameter;

        [Tooltip("the name of the weapon stop animation parameter : true after a shot and before the next one or the weapon's stop ")]
        public string StopAnimationParameter;

        [Tooltip("the name of the weapon reload start animation parameter")]
        public string ReloadStartAnimationParameter;

        [Tooltip("the name of the weapon reload animation parameter")]
        public string ReloadAnimationParameter;

        [Tooltip("the name of the weapon reload end animation parameter")]
        public string ReloadStopAnimationParameter;

        [Tooltip("the name of the weapon's angle animation parameter")]
        public string WeaponAngleAnimationParameter;

        [Tooltip("the name of the weapon's angle animation parameter, adjusted so it's always relative to the direction the character is currently facing")]
        public string WeaponAngleRelativeAnimationParameter;

        [Tooltip("the name of the parameter to send to true as long as this weapon is equipped, used or not. While all the other parameters defined here are updated by the Weapon class itself, and passed to the weapon and character, this one will be updated by CharacterHandleWeapon only.")]
        public string EquippedAnimationParameter;

        [MMInspectorGroup("Feedbacks")]
        [Tooltip("the feedback to play when the weapon starts being used")]
        public MMFeedbacks WeaponStartMMFeedback;

        [Tooltip("the feedback to play while the weapon is in use")]
        public MMFeedbacks WeaponUsedMMFeedback;

        [Tooltip("if set, this feedback will be used randomly instead of WeaponUsedMMFeedback")]
        public MMFeedbacks WeaponUsedMMFeedbackAlt;

        [Tooltip("the feedback to play when the weapon stops being used")]
        public MMFeedbacks WeaponStopMMFeedback;

        [Tooltip("the feedback to play when the weapon gets reloaded")]
        public MMFeedbacks WeaponReloadMMFeedback;

        [Tooltip("the feedback to play when the weapon gets reloaded")]
        public MMFeedbacks WeaponReloadNeededMMFeedback;

        [Tooltip("the feedback to play when the weapon can't reload as there's no more ammo available. You'll need PreventReloadIfAmmoEmpty to be true for this to work")]
        public MMFeedbacks WeaponReloadImpossibleMMFeedback;

        [MMInspectorGroup("Settings")]
        [Tooltip("If this is true, the weapon will initialize itself on start, otherwise it'll have to be init manually, usually by the CharacterHandleWeapon class")]
        public bool InitializeOnStart;

        [Tooltip("whether or not this weapon can be interrupted")]
        public bool Interruptable;

        [MMInspectorGroup("Stats")]
        [Tooltip("the Stats script associated to this Weapon, will be grabbed automatically if left empty")]
        public Stats Stats;

        public bool IsCritThisFrame { get; set; }
        public float CritDamageThisFrame { get; set; }
        public int BaseDamage = 5;
        public Dmg.Types BaseDamageType;

        protected int Damage
        {
            get
            {
                var damage = (float)BaseDamage;
                return (int)DamageModifier.SafeInvoke(ref damage);
            }
        }

        protected Dmg.Types DamageType => BaseDamageType;
        protected Dmg Dmg => new(Damage, DamageType, IsCritThisFrame, CritDamageThisFrame);

        public ValueModifier DamageModifier { get; set; }

        /// the name of the inventory item corresponding to this weapon. Automatically set (if needed) by InventoryEngineWeapon
        public string WeaponID { get; set; }

        public Character Owner { get; private set; }
        public Stats OwnerStats { get; private set; }
        public CharacterHandleWeapon HandleWeapon { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("if true, the weapon is flipped right now")]
        public bool Flipped { get; set; }

        /// the WeaponAmmo component optionally associated to this weapon
        public WeaponAmmo WeaponAmmo;

        public MMStateMachine<States> State = new();

        protected SpriteRenderer _spriteRenderer;
        protected WeaponAim _weaponAim;
        protected float _movementMultiplierStorage = 1f;

        public float MovementMultiplierStorage
        {
            get => _movementMultiplierStorage;
            set => _movementMultiplierStorage = value;
        }

        public bool IsComboWeapon { get; set; }
        public bool IsAutoComboWeapon { get; set; }
        public Transform AimTarget => _aimTarget;

        protected Animator _ownerAnimator;
        protected WeaponPreventShooting _weaponPreventShooting;
        protected Timer _delayBeforeUseTimer;
        protected Timer _delayBetweenUsesTimer;
        protected Timer _reloadingTimer;
        protected bool _triggerReleased;
        protected bool _reloading;
        protected ComboWeapon _comboWeapon;
        protected TopDownController _controller;
        protected CharacterMovement _characterMovement;
        protected Vector3 _weaponOffset;
        protected Vector3 _weaponAttachmentOffset;
        protected Transform _weaponAttachment;
        protected Transform _aimTarget;
        protected List<HashSet<int>> _animatorParameters;
        protected HashSet<int> _ownerAnimatorParameters;
        protected List<int> weaponList = new();

        protected const string _aliveAnimationParameterName = "Alive";
        protected int _idleAnimationParameter;
        protected int _startAnimationParameter;
        protected int _delayBeforeUseAnimationParameter;
        protected int _singleUseAnimationParameter;
        protected int _useAnimationParameter;
        protected int _delayBetweenUsesAnimationParameter;
        protected int _stopAnimationParameter;
        protected int _reloadStartAnimationParameter;
        protected int _reloadAnimationParameter;
        protected int _reloadStopAnimationParameter;
        protected int _weaponAngleAnimationParameter;
        protected int _weaponAngleRelativeAnimationParameter;
        protected int _aliveAnimationParameter;
        protected int _comboInProgressAnimationParameter;
        protected int _equippedAnimationParameter;
        protected float _lastShootRequestAt = -float.MaxValue;
        protected float _lastTurnWeaponOnAt = -float.MaxValue;
        protected bool _movementSpeedMultiplierSet;

        /// <summary>
        /// On start, we initialize our weapon
        /// </summary>
        protected virtual void Start()
        {
            if (InitializeOnStart)
            {
                Initialization();
            }
        }

        /// <summary>
        /// Initialize this weapon.
        /// </summary>
        public virtual void Initialization()
        {
            Flipped = false;
            TryGetComponent(out _spriteRenderer);
            TryGetComponent(out _comboWeapon);
            TryGetComponent(out _weaponPreventShooting);

            State.Initialize(gameObject, true, OnStateChange);
            State.ChangeState(States.Idle);
            TryGetComponent(out WeaponAmmo);
            _animatorParameters = new();
            TryGetComponent(out _weaponAim);
            InitializeAnimatorParameters();

            if (WeaponAmmo == null)
                CurrentAmmoLoaded = MagazineSize;

            if (Stats == null)
                TryGetComponent(out Stats);

            InitializeFeedbacks();
        }

        protected virtual void InitializeFeedbacks()
        {
            WeaponStartMMFeedback.Initialize(gameObject);
            WeaponUsedMMFeedback.Initialize(gameObject);
            WeaponUsedMMFeedbackAlt.Initialize(gameObject);
            WeaponStopMMFeedback.Initialize(gameObject);
            WeaponReloadNeededMMFeedback.Initialize(gameObject);
            WeaponReloadMMFeedback.Initialize(gameObject);
        }

        /// <summary>
        /// Initializes the combo weapon, if it's one
        /// </summary>
        public virtual void InitializeComboWeapons()
        {
            IsComboWeapon = false;
            IsAutoComboWeapon = false;
            if (_comboWeapon)
            {
                IsComboWeapon = true;
                IsAutoComboWeapon = _comboWeapon.InputMode == ComboWeapon.InputModes.Auto;
                _comboWeapon.Initialization();
            }
        }

        /// <summary>
        /// Sets the weapon's owner
        /// </summary>
        /// <param name="owner">New owner.</param>
        public virtual void SetOwner(Character owner, CharacterHandleWeapon handleWeapon = null)
        {
            Owner = owner;
            OwnerStats = owner.Stats;
            if (Owner)
            {
                HandleWeapon = handleWeapon;
                Owner.FindAbility(out _characterMovement);
                _controller = Owner.Controller;

                if (HandleWeapon && HandleWeapon.AutomaticallyBindAnimator)
                {
                    if (HandleWeapon.CharacterAnimator)
                        _ownerAnimator = HandleWeapon.CharacterAnimator;

                    if (_ownerAnimator == null)
                        _ownerAnimator = HandleWeapon.Character.CharacterAnimator;

                    if (_ownerAnimator == null)
                        _ownerAnimator = HandleWeapon.GetComponentInParent<Animator>();
                }
            }

            if (OwnerStats)
            {
                OnOwnerStatsSet();
            }
        }

        public void SetAimTarget(Transform target)
        {
            _aimTarget = target;
        }

        protected virtual void OnOwnerStatsSet()
        {
            var characterAS = Owner.GetStat(Character.Stat.AS);
            var weaponAS = GetStat(Stat.AS);
            //Weapon的DelayBeforeUseF = (1 + Character.AS + Weapon.AS) * Weapon.DelayBeforeUseF
            DelayBeforeUseModifier = (ref float raw) =>
            {
                float totalAS = 0F;
                if (characterAS)
                    totalAS += characterAS.Value;
                if (weaponAS)
                    totalAS += weaponAS.Value;

                var baseWindupTime = DelayBeforeUsePct / characterAS.Initial;
                var currentAttackTotalTime = 1 / totalAS;

                var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
                raw = windupTime;
            };

            //Weapon的TimeBetweenUsesF = (1 + Character.AS + Weapon.AS) * Weapon.TimeBetweenUsesF
            TimeBetweenUsesModifier = (ref float raw) =>
            {
                float totalAS = 0F;
                if (characterAS)
                    totalAS += characterAS.Value;
                if (weaponAS)
                    totalAS += weaponAS.Value;

                float baseWindupTime = 0F;
                if (characterAS.Initial > 0)
                    baseWindupTime = DelayBeforeUsePct / characterAS.Initial;

                float currentAttackTotalTime = 0F;
                if (totalAS > 0)
                    currentAttackTotalTime = 1 / totalAS;

                var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
                raw = currentAttackTotalTime - windupTime;
            };

            var characterAD = Owner.GetStat(Character.Stat.AD);
            var weaponAD = GetStat(Stat.AD);
            //Weapon的Damage = (Character.AD + Weapon.AD) * Weapon.AD_Coeff
            DamageModifier = (ref float raw) =>
            {
                float v1 = 0F, v2 = 0F;

                if (characterAD)
                    v1 = characterAD.Value;

                if (weaponAD)
                    v2 = weaponAD.Value;

                raw = v1 + v2;
            };
        }

        /// <summary>
        /// Called by input, turns the weapon on
        /// </summary>
        public virtual void WeaponInputStart()
        {
            if (_reloading)
                return;

            if (RequireAimTarget && _aimTarget == null)
                return;

            if (State.Is(States.Idle))
            {
                _triggerReleased = false;
                TurnWeaponOn();
            }
        }

        /// <summary>
        /// Describes what happens when the weapon's input gets released
        /// </summary>
        public virtual void WeaponInputReleased()
        {
        }

        /// <summary>
        /// Describes what happens when the weapon starts
        /// </summary>
        public virtual void TurnWeaponOn()
        {
            var now = Time.time;
            if (!InputAuthorized && now - _lastTurnWeaponOnAt < timeBetweenUses)
                return;

            _lastTurnWeaponOnAt = now;

            TriggerWeaponStartFeedback();
            State.ChangeState(States.Start);
            if (_characterMovement && ModifyMovementWhileAttacking)
            {
                _movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
                _characterMovement.MovementSpeedMultiplier = MovementMultiplier;
                _movementSpeedMultiplierSet = true;
            }

            if (_comboWeapon)
                _comboWeapon.WeaponStarted(this);

            if (PreventAllMovementWhileInUse && _characterMovement && _controller)
            {
                _characterMovement.SetMovement(Vector2.zero);
                _characterMovement.MovementForbidden = true;
            }

            if (PreventAllAimWhileInUse && _weaponAim)
            {
                _weaponAim.AimControlActive = false;
            }
        }

        void Update()
        {
            OnUpdate(Time.deltaTime);
        }

        void FixedUpdate()
        {
            OnFixedUpdate(Time.fixedDeltaTime);
        }

        /// <summary>
        /// On LateUpdate, processes the weapon state
        /// </summary>
        void LateUpdate()
        {
            var dt = Time.deltaTime;
            OnLateUpdate(dt);
        }

        /// <summary>
        /// On Update, we check if the weapon is or should be used
        /// </summary>
        protected virtual void OnUpdate(float dt)
        {
            FlipWeapon();
            ApplyOffset();
        }

        protected virtual void OnFixedUpdate(float dt)
        {
        }

        protected virtual void OnLateUpdate(float dt)
        {
            ProcessWeaponState(dt);
        }

        /// <summary>
        /// Called every lastUpdate, processes the weapon's state machine
        /// </summary>
        public virtual bool ProcessWeaponState(float dt)
        {
            UpdateAnimator();
            return State.CurrentState switch
            {
                States.Idle => CaseWeaponIdle(),
                States.Start => CaseWeaponStart(),
                States.DelayBeforeUse => CaseWeaponDelayBeforeUse(dt),
                States.Use => CaseWeaponUse(),
                States.DelayBetweenUses => CaseWeaponDelayBetweenUses(dt),
                States.Stop => CaseWeaponStop(),
                States.ReloadNeeded => CaseWeaponReloadNeeded(),
                States.ReloadStart => CaseWeaponReloadStart(),
                States.Reloading => CaseWeaponReloading(dt),
                States.ReloadStop => CaseWeaponReloadStop(),
                States.Interrupted => CaseWeaponInterrupted(),
                _ => false
            };
        }


        protected void OnStateChange(States pre, States cur)
        {
            switch (pre)
            {
                case States.DelayBeforeUse:
                    ExitWeaponDelayBeforeUse(cur);
                    break;
            }

            switch (cur)
            {
                case States.DelayBeforeUse:
                    EnterWeaponDelayBeforeUse();
                    break;
            }
        }

        /// <summary>
        /// If the weapon is idle, we reset the movement multiplier
        /// </summary>
        public virtual bool CaseWeaponIdle()
        {
            _delayBeforeUseTimer.kill();
            _delayBetweenUsesTimer.kill();
            _reloadingTimer.kill();
            ResetMovementMultiplier();
            return true;
        }

        /// <summary>
        /// When the weapon starts we switch to a delay or shoot based on our weapon's settings
        /// </summary>
        public virtual bool CaseWeaponStart()
        {
            var delay = delayBeforeUse;
            if (delay > 0)
            {
                _delayBeforeUseTimer = delay;
                State.ChangeState(States.DelayBeforeUse);
            }
            else
            {
                DoShootRequest();
            }

            return true;
        }

        public virtual void ExitWeaponDelayBeforeUse(States toState)
        {
            if (_characterMovement && ModifyMovementWhileDelayBeforeUse)
            {
                _characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
                _movementMultiplierStorage = 1f;
            }

            if (Owner)
            {
                if (toState == States.Use)
                    Owner.Event.trigger(OnWindup.Finish(this));
                else
                    Owner.Event.trigger(OnWindup.Cancel(this));
            }
        }

        public virtual void EnterWeaponDelayBeforeUse()
        {
            if (_characterMovement && ModifyMovementWhileDelayBeforeUse)
            {
                _movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
                _characterMovement.MovementSpeedMultiplier = MovementMultiplier;
            }

            if (Owner)
                Owner.Event.trigger(OnWindup.Start(this, delayBeforeUse));
        }

        /// <summary>
        /// If we're in delay before use, we wait until our delay is passed and then request a shoot
        /// </summary>
        public virtual bool CaseWeaponDelayBeforeUse(float dt)
        {
            if (_delayBeforeUseTimer.update(dt, true))
                DoShootRequest();

            return true;
        }

        /// <summary>
        /// On weapon use we use our weapon then switch to delay between uses
        /// </summary>
        public virtual bool CaseWeaponUse()
        {
            DetermineWeaponCrit();
            WeaponUse();
            _delayBetweenUsesTimer = timeBetweenUses;
            State.ChangeState(States.DelayBetweenUses);
            return true;
        }

        /// <summary>
        /// When in delay between uses, we either turn our weapon off or make a shoot request
        /// </summary>
        public virtual bool CaseWeaponDelayBetweenUses(float dt)
        {
            if (_triggerReleased && TimeBetweenUsesReleaseInterruption)
            {
                TurnWeaponOff();
                return false;
            }

            if (_delayBetweenUsesTimer.update(dt, true))
                RestartOrTurnOff();

            return true;
        }

        void RestartOrTurnOff()
        {
            _delayBetweenUsesTimer.kill();
            if (TriggerMode == TriggerModes.Auto && !_triggerReleased)
                State.ChangeState(States.Start);
            else
                TurnWeaponOff();
        }

        public void ResetUseTimer()
        {
            switch (State.CurrentState)
            {
                case States.DelayBeforeUse:
                    CaseWeaponStart();
                    break;
                case States.DelayBetweenUses:
                    RestartOrTurnOff();
                    break;
            }
        }

        /// <summary>
        /// On weapon stop, we switch to idle
        /// </summary>
        public virtual bool CaseWeaponStop()
        {
            State.ChangeState(States.Idle);
            return true;
        }

        /// <summary>
        /// If a reload is needed, we mention it and switch to idle
        /// </summary>
        public virtual bool CaseWeaponReloadNeeded()
        {
            ReloadNeeded();
            ResetMovementMultiplier();
            State.ChangeState(States.Idle);
            return true;
        }

        /// <summary>
        /// on reload start, we reload the weapon and switch to reload
        /// </summary>
        public virtual bool CaseWeaponReloadStart()
        {
            ReloadWeapon();
            _reloadingTimer = ReloadTime;
            State.ChangeState(States.Reloading);
            return true;
        }

        /// <summary>
        /// on reload, we reset our movement multiplier, and switch to reload stop once our reload delay has passed
        /// </summary>
        public virtual bool CaseWeaponReloading(float dt)
        {
            ResetMovementMultiplier();
            if (_reloadingTimer.update(dt, true))
            {
                State.ChangeState(States.ReloadStop);
            }

            return true;
        }

        /// <summary>
        /// on reload stop, we switch to idle and load our ammo
        /// </summary>
        public virtual bool CaseWeaponReloadStop()
        {
            _reloading = false;
            State.ChangeState(States.Idle);
            if (WeaponAmmo == null)
                CurrentAmmoLoaded = MagazineSize;

            return true;
        }

        /// <summary>
        /// on weapon interrupted, we turn our weapon off and switch back to idle
        /// </summary>
        public virtual bool CaseWeaponInterrupted()
        {
            TurnWeaponOff();
            ResetMovementMultiplier();
            switch (State.CurrentState)
            {
                case States.Reloading:
                case States.ReloadStart:
                case States.ReloadStop:
                    return false;
            }

            State.ChangeState(States.Idle);
            return true;
        }

        /// <summary>
        /// Call this method to interrupt the weapon
        /// </summary>
        public virtual void Interrupt()
        {
            switch (State.CurrentState)
            {
                case States.Reloading:
                case States.ReloadStart:
                case States.ReloadStop:
                    return;
            }

            if (Interruptable)
            {
                State.ChangeState(States.Interrupted);
            }
        }

        protected void DoShootRequest()
        {
            var now = Time.time;
            if (now - _lastShootRequestAt < timeBetweenUses)
                return;

            var remainingShots = UseBurstMode ? BurstLength : 1;
            switch (remainingShots)
            {
                case 1:
                    ShootRequest();
                    _lastShootRequestAt = now;
                    break;
                case > 1:
                    ShootRequest();
                    _lastShootRequestAt = now;

                    remainingShots--;
                    Timing.RunCoroutine(ShootRequestCo(remainingShots));
                    break;
            }
        }

        /// <summary>
        /// Determines whether the weapon can fire
        /// </summary>
        /// <param name="i"></param>
        protected IEnumerator<float> ShootRequestCo(int remainingShots)
        {
            var interval = UseBurstMode ? BurstTimeBetweenShots : 1;
            for (int i = remainingShots; i > 0; i--)
            {
                yield return Timing.WaitForSeconds(interval);

                ShootRequest();
                _lastShootRequestAt = Time.time;
            }
        }

        public virtual void ShootRequest()
        {
            // if we have a weapon ammo component,
            // we determine if we have enough ammunition to shoot
            if (_reloading)
                return;

            if (RequireAimTarget && _aimTarget == null)
                return;

            if (_weaponPreventShooting && !_weaponPreventShooting.ShootingAllowed())
                return;

            if (MagazineBased)
            {
                if (WeaponAmmo)
                {
                    if (WeaponAmmo.EnoughAmmoToFire())
                    {
                        State.ChangeState(States.Use);
                    }
                    else
                    {
                        if (AutoReload)
                            InitiateReloadWeapon();
                        else
                            State.ChangeState(States.ReloadNeeded);
                    }
                }
                else
                {
                    if (CurrentAmmoLoaded > 0)
                    {
                        State.ChangeState(States.Use);
                        CurrentAmmoLoaded -= AmmoConsumedPerShot;
                    }
                    else
                    {
                        if (AutoReload)
                            InitiateReloadWeapon();
                        else
                            State.ChangeState(States.ReloadNeeded);
                    }
                }
            }
            else
            {
                if (WeaponAmmo)
                {
                    if (WeaponAmmo.EnoughAmmoToFire())
                        State.ChangeState(States.Use);
                    else
                        State.ChangeState(States.ReloadNeeded);
                }
                else
                {
                    State.ChangeState(States.Use);
                }
            }
        }

        /// <summary>
        /// When the weapon is used, plays the corresponding sound
        /// </summary>
        public virtual void WeaponUse()
        {
            ApplyRecoil();
            TriggerWeaponUsedFeedback();
        }

        public virtual void DetermineWeaponCrit()
        {
            var critChance = 0F;
            var characterCritChance = Owner.GetStat(Character.Stat.CritChance);
            if (characterCritChance != null)
                critChance += characterCritChance.Value;

            var weaponCritChance = GetStat(Stat.CritChance);
            if (weaponCritChance != null)
                critChance += weaponCritChance.Value;

            IsCritThisFrame = MMMaths.Chance(critChance);
            
            var critDamage = 0F;
            var weaponCritDamage = GetStat(Stat.CritDamage);
            if (weaponCritDamage != null)
                critDamage += weaponCritDamage.Value;
            
            var characterCritDamage = Owner.GetStat(Character.Stat.CritDamage);
            if (characterCritDamage != null)
                critDamage *= (1 + characterCritDamage.Value);

            CritDamageThisFrame = critDamage;
        }

        /// <summary>
        /// Applies recoil if necessary
        /// </summary>
        protected virtual void ApplyRecoil()
        {
            if (RecoilForce == 0F)
                return;

            if (_controller == null)
                return;

            if (Owner == null)
                return;

            var right = transform.right;
            _controller.AddImpact(Flipped ? right : -right, RecoilForce);
        }

        /// <summary>
        /// Called by input, turns the weapon off if in auto mode
        /// </summary>
        public virtual void WeaponInputStop()
        {
            if (_reloading)
                return;

            _triggerReleased = true;
            if (_characterMovement && ModifyMovementWhileAttacking)
            {
                _characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
                _movementMultiplierStorage = 1f;
            }
        }

        /// <summary>
        /// Turns the weapon off.
        /// </summary>
        public virtual void TurnWeaponOff()
        {
            if (State.Is(States.Idle, States.Stop))
                return;

            _triggerReleased = true;

            TriggerWeaponStopFeedback();
            State.ChangeState(States.Stop);
            ResetMovementMultiplier();

            _comboWeapon?.WeaponStopped(this);

            if (PreventAllMovementWhileInUse && _characterMovement)
            {
                _characterMovement.MovementForbidden = false;
            }

            if (PreventAllAimWhileInUse && _weaponAim)
            {
                _weaponAim.AimControlActive = true;
            }

            if (NoInputReload)
            {
                bool needToReload;
                if (WeaponAmmo)
                    needToReload = !WeaponAmmo.EnoughAmmoToFire();
                else
                    needToReload = CurrentAmmoLoaded <= 0;

                if (needToReload)
                {
                    InitiateReloadWeapon();
                }
            }
        }

        protected virtual void ResetMovementMultiplier()
        {
            if (_characterMovement && ModifyMovementWhileAttacking && _movementSpeedMultiplierSet)
            {
                _characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
                _movementMultiplierStorage = 1f;
                _movementSpeedMultiplierSet = false;
            }
        }

        /// <summary>
        /// Describes what happens when the weapon needs a reload
        /// </summary>
        public virtual void ReloadNeeded()
        {
            TriggerWeaponReloadNeededFeedback();
        }

        /// <summary>
        /// Initiates a reload
        /// </summary>
        public virtual void InitiateReloadWeapon()
        {
            if (PreventReloadIfAmmoEmpty && WeaponAmmo && WeaponAmmo.CurrentAmmoAvailable == 0)
            {
                WeaponReloadImpossibleMMFeedback.Play();
                return;
            }

            // if we're already reloading, we do nothing and exit
            if (_reloading || !MagazineBased)
                return;

            if (PreventAllMovementWhileInUse && _characterMovement)
            {
                _characterMovement.MovementForbidden = false;
            }

            if (PreventAllAimWhileInUse && _weaponAim)
            {
                _weaponAim.AimControlActive = true;
            }

            State.ChangeState(States.ReloadStart);
            _reloading = true;
        }

        /// <summary>
        /// Reloads the weapon
        /// </summary>
        protected virtual void ReloadWeapon()
        {
            if (MagazineBased)
            {
                TriggerWeaponReloadFeedback();
            }
        }

        /// <summary>
        /// Flips the weapon.
        /// </summary>
        public virtual void FlipWeapon()
        {
            if (!WeaponShouldFlip)
                return;

            if (Owner == null)
                return;

            if (Owner.Orientation2D == null)
                return;

            if (FlipWeaponOnCharacterFlip)
            {
                Flipped = !Owner.Orientation2D.IsFacingRight;

                if (_spriteRenderer)
                    _spriteRenderer.flipX = Flipped;
                else
                    transform.localScale = Flipped ? LeftFacingFlipValue : RightFacingFlipValue;
            }

            if (_comboWeapon)
                _comboWeapon.FlipUnusedWeapons();
        }

        /// <summary>
        /// Destroys the weapon
        /// </summary>
        /// <returns>The destruction.</returns>
        public virtual IEnumerator<float> WeaponDestruction()
        {
            yield return Timing.WaitForSeconds(AutoDestroyWhenEmptyDelay);
            // if we don't have ammo anymore, and need to destroy our weapon, we do it
            TurnWeaponOff();
            Destroy(gameObject);

            if (WeaponID != null)
            {
                // we remove it from the inventory
                // if (Owner.TryFindAbility<CharacterInventory>(out var inventory))
                // {
                //     inventory.WeaponInventory.Search(WeaponID, ref weaponList);
                //     if (weaponList.Count > 0)
                //     {
                //         inventory.WeaponInventory.DestroyItem(weaponList[0]);
                //     }
                // }
            }
        }

        /// <summary>
        /// Applies the offset specified in the inspector
        /// </summary>
        public virtual void ApplyOffset()
        {
            if (!WeaponCurrentlyActive)
                return;

            _weaponAttachmentOffset = WeaponAttachmentOffset;

            if (Owner == null)
                return;

            if (Owner.Orientation2D)
            {
                if (Flipped)
                    _weaponAttachmentOffset.x = -WeaponAttachmentOffset.x;

                // we apply the offset
                if (transform.parent)
                    transform.position = _weaponOffset = transform.parent.position + _weaponAttachmentOffset;
            }
            else
            {
                if (transform.parent)
                    transform.localPosition = _weaponOffset = _weaponAttachmentOffset;
            }
        }

        /// <summary>
        /// Plays the weapon's start sound
        /// </summary>
        protected virtual void TriggerWeaponStartFeedback()
        {
            WeaponStartMMFeedback.Play(transform.position);
        }

        /// <summary>
        /// Plays the weapon's used sound
        /// </summary>
        protected virtual void TriggerWeaponUsedFeedback()
        {
            if (WeaponUsedMMFeedbackAlt)
            {
                int random = MMMaths.RollADice(2);
                if (random > 1)
                {
                    WeaponUsedMMFeedbackAlt.Play(transform.position);
                }
                else
                {
                    WeaponUsedMMFeedback.Play(transform.position);
                }
            }
            else
            {
                WeaponUsedMMFeedback.Play(transform.position);
            }
        }

        /// <summary>
        /// Plays the weapon's stop sound
        /// </summary>
        protected virtual void TriggerWeaponStopFeedback()
        {
            WeaponStopMMFeedback.Play(transform.position);
        }

        /// <summary>
        /// Plays the weapon's reload needed sound
        /// </summary>
        protected virtual void TriggerWeaponReloadNeededFeedback()
        {
            WeaponReloadNeededMMFeedback.Play(transform.position);
        }

        /// <summary>
        /// Plays the weapon's reload sound
        /// </summary>
        protected virtual void TriggerWeaponReloadFeedback()
        {
            WeaponReloadMMFeedback.Play(transform.position);
        }

        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        public virtual void InitializeAnimatorParameters()
        {
            if (Animators.Count > 0)
            {
                for (int i = 0; i < Animators.Count; i++)
                {
                    _animatorParameters.Add(new HashSet<int>());
                    AddParametersToAnimator(Animators[i], _animatorParameters[i]);
                    if (!PerformAnimatorSanityChecks)
                    {
                        Animators[i].logWarnings = false;
                    }

                    if (MirrorCharacterAnimatorParameters)
                    {
                        MMAnimatorMirror mirror = Animators[i].gameObject.AddComponent<MMAnimatorMirror>();
                        mirror.SourceAnimator = _ownerAnimator;
                        mirror.TargetAnimator = Animators[i];
                        mirror.Initialization();
                    }
                }
            }

            if (_ownerAnimator)
            {
                _ownerAnimatorParameters = new HashSet<int>();
                AddParametersToAnimator(_ownerAnimator, _ownerAnimatorParameters);
                if (!PerformAnimatorSanityChecks)
                {
                    _ownerAnimator.logWarnings = false;
                }
            }
        }

        protected virtual void AddParametersToAnimator(Animator animator, HashSet<int> list)
        {
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, EquippedAnimationParameter, out _equippedAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleAnimationParameter, out _weaponAngleAnimationParameter, AnimatorControllerParameterType.Float, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleRelativeAnimationParameter, out _weaponAngleRelativeAnimationParameter, AnimatorControllerParameterType.Float, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, IdleAnimationParameter, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StartAnimationParameter, out _startAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBeforeUseAnimationParameter, out _delayBeforeUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBetweenUsesAnimationParameter, out _delayBetweenUsesAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StopAnimationParameter, out _stopAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStartAnimationParameter, out _reloadStartAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStopAnimationParameter, out _reloadStopAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadAnimationParameter, out _reloadAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, SingleUseAnimationParameter, out _singleUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, UseAnimationParameter, out _useAnimationParameter, AnimatorControllerParameterType.Bool, list);

            if (_comboWeapon)
            {
                MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, _comboWeapon.ComboInProgressAnimationParameter, out _comboInProgressAnimationParameter, AnimatorControllerParameterType.Bool, list);
            }
        }

        /// <summary>
        /// Override this to send parameters to the character's animator. This is called once per cycle, by the Character
        /// class, after Early, normal and Late process().
        /// </summary>
        public virtual void UpdateAnimator()
        {
            for (int i = 0; i < Animators.Count; i++)
                UpdateAnimator(Animators[i], _animatorParameters[i]);

            if (_ownerAnimator && State != null && _ownerAnimatorParameters != null)
                UpdateAnimator(_ownerAnimator, _ownerAnimatorParameters);
        }

        protected virtual void UpdateAnimator(Animator animator, HashSet<int> list)
        {
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _equippedAnimationParameter, true, list);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _idleAnimationParameter, State.Is(States.Idle), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _startAnimationParameter, State.Is(States.Start), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBeforeUseAnimationParameter, State.Is(States.DelayBeforeUse), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _useAnimationParameter, State.Is(States.DelayBeforeUse, States.Use, States.DelayBetweenUses), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _singleUseAnimationParameter, State.Is(States.Use), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBetweenUsesAnimationParameter, State.Is(States.DelayBetweenUses), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _stopAnimationParameter, State.Is(States.Stop), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStartAnimationParameter, State.Is(States.ReloadStart), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadAnimationParameter, State.Is(States.Reloading), list, PerformAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStopAnimationParameter, State.Is(States.ReloadStop), list, PerformAnimatorSanityChecks);

            if (Owner)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(animator, _aliveAnimationParameter, Owner.conditionState.Not(Character.Conditions.Dead), list, PerformAnimatorSanityChecks);
            }

            if (_weaponAim)
            {
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, _weaponAim.CurrentAngle, list, PerformAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, _weaponAim.CurrentAngleRelative, list, PerformAnimatorSanityChecks);
            }
            else
            {
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
            }

            if (_comboWeapon)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(animator, _comboInProgressAnimationParameter, _comboWeapon.ComboInProgress, list, PerformAnimatorSanityChecks);
            }
        }
    }
}