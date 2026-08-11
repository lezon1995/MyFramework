using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Add this class to a character so it can use weapons
    /// Note that this component will trigger animations (if their parameter is present in the Animator), based on 
    /// the current weapon's Animations
    /// Animator parameters : defined from the Weapon's inspector
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/CharacterHandleWeapon")]
    public class CharacterHandleWeapon : CharacterAbility
    {
        [Header("Weapon")]
        [Tooltip("the initial weapon owned by the character")]
        public Weapon InitialWeapon;

        [Tooltip("if this is set to true, the character can pick up PickableWeapons")]
        public bool CanPickupWeapons = true;

        [Header("Feedbacks")]
        [Tooltip("a feedback that gets triggered at the character level everytime the weapon is used")]
        public MMFeedbacks WeaponUseFeedback;

        [Header("Binding")]
        [Tooltip("the position the weapon will be attached to. If left blank, will be this.transform.")]
        public Transform WeaponAttachment;

        [Tooltip("the position from which projectiles will be spawned (can be safely left empty)")]
        public Transform ProjectileSpawn;

        [Tooltip("if this is true this animator will be automatically bound to the weapon")]
        public bool AutomaticallyBindAnimator = true;

        [Tooltip("the ID of the AmmoDisplay this ability should update")]
        public int AmmoDisplayID;

        [Tooltip("if this is true, IK will be automatically setup if possible")]
        public bool AutoIK = true;

        [Header("Input")]
        [Tooltip("if this is true you won't have to release your fire button to auto reload")]
        public bool ContinuousPress;

        [Tooltip("whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)")]
        public bool GettingHitInterruptsAttack;

        [Tooltip("whether or not pushing the secondary axis above its threshold should cause the weapon to shoot")]
        public bool UseSecondaryAxisThresholdToShoot;

        [Tooltip("if this is true, the ForcedWeaponAimControl mode will be applied to all weapons equipped by this character")]
        public bool ForceWeaponAimControl;

        [Tooltip("if ForceWeaponAimControl is true, the AimControls mode to apply to all weapons equipped by this character")]
        [MMCondition("ForceWeaponAimControl", true)]
        public WeaponAim.AimControls ForcedWeaponAimControl = WeaponAim.AimControls.PrimaryMovement;

        [Tooltip("if this is true, the character will continuously fire its weapon")]
        public bool ForceAlwaysShoot;

        [Header("Buffering")]
        [Tooltip("whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them")]
        public bool BufferInput;

        [MMCondition(nameof(BufferInput), true)]
        [Tooltip("if this is true, every new input will prolong the buffer")]
        public bool NewInputExtendsBuffer;

        [MMCondition(nameof(BufferInput), true)]
        [Tooltip("the maximum duration for the buffer, in seconds")]
        public float MaximumBufferDuration = 0.25f;
        
        [ShowInInspector]
        [Tooltip("the weapon currently equipped by the Character")]
        public Weapon CurrentWeapon { get; set; }

        /// the ID / index of this CharacterHandleWeapon. This will be used to determine what handle weapon ability should equip a weapon.
        /// If you create more Handle Weapon abilities, make sure to override and increment this  
        public virtual int HandleWeaponID => 1;

        /// an animator to update when the weapon is used
        public Animator CharacterAnimator { get; set; }

        /// the weapon's weapon aim component, if it has one
        public WeaponAim WeaponAim => _weaponAim;

        /// a delegate you can hook to, to be notified of weapon changes
        public event Action OnWeaponChange;

        protected WeaponAim _weaponAim;
        protected WeaponIK _weaponIK;
        protected float _bufferEndsAt;
        protected bool _buffering;
        protected const string _weaponEquippedAnimationParameterName = "WeaponEquipped";
        protected const string _weaponEquippedIDAnimationParameterName = "WeaponEquippedID";
        protected int _weaponEquippedAnimationParameter;
        protected int _weaponEquippedIDAnimationParameter;
        protected List<WeaponModel> _weaponModels = new();

        protected override void Initialization()
        {
            base.Initialization();
            Setup();
        }

        /// <summary>
        /// Grabs various components and inits stuff
        /// </summary>
        public virtual void Setup()
        {
            this.TryGetComponentInParent(out _character);

            _weaponModels.Clear();
            var weaponModels = _character.GetComponentsInChildren<WeaponModel>();
            _weaponModels.AddRange(weaponModels);

            CharacterAnimator = _animator;

            if (WeaponAttachment == null)
                WeaponAttachment = transform;

            if (_animator && AutoIK)
                _weaponIK = _animator.GetComponent<WeaponIK>();

            if (InitialWeapon == null)
                return;

            if (CurrentWeapon && CurrentWeapon.name == InitialWeapon.name)
                return;

            ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName);
        }
        
        public override void SetAbilityPermitted(bool abilityPermitted)
        {
            base.SetAbilityPermitted(abilityPermitted);
            if (WeaponAttachment)
            {
                WeaponAttachment.gameObject.SetActive(abilityPermitted);
            }
        }

        /// <summary>
        /// Every frame we check if it's needed to update the ammo display
        /// </summary>
        /// <param name="dt"></param>
        public override void OnUpdate(float dt)
        {
            HandleCharacterState();
            HandleFeedbacks();
            HandleBuffer();
        }

        /// <summary>
        /// Checks character state and stops shooting if not in normal state
        /// </summary>
        protected virtual void HandleCharacterState()
        {
            if (_conditionState.Not(Character.Conditions.Normal))
            {
                ShootStop();
            }
        }

        /// <summary>
        /// Triggers the weapon used feedback if needed
        /// </summary>
        protected virtual void HandleFeedbacks()
        {
            if (CurrentWeapon == null)
                return;

            if (CurrentWeapon.State.Is(Weapon.States.Use))
            {
                WeaponUseFeedback.Play();
            }
        }

        /// <summary>
        /// Gets input and triggers methods based on what's been pressed
        /// </summary>
        protected override void HandleInput()
        {
            if (AbilityUnauthorized)
                return;

            if (_conditionState.Not(Character.Conditions.Normal))
                return;

            if (CurrentWeapon == null)
                return;

            bool authorized = true;
            if (CurrentWeapon)
                authorized = CurrentWeapon.InputAuthorized;

            if (ForceAlwaysShoot)
                ShootStart();

            var input = _inputManager;

            if (authorized && (input.ShootButton.IsDown() || input.ShootAxis.IsDown()))
            {
                ShootStart();
            }

            bool buttonPressed = input.ShootButton.IsPressed() || input.ShootAxis.IsPressed();

            if (authorized && ContinuousPress && CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto && buttonPressed)
            {
                ShootStart();
            }

            if (authorized && ContinuousPress && CurrentWeapon.IsAutoComboWeapon && buttonPressed)
            {
                ShootStart();
            }

            if (input.ReloadButton != null && input.ReloadButton.IsDown())
            {
                Reload();
            }

            if (authorized && (input.ShootButton.IsUp() || input.ShootAxis.IsUp()))
            {
                ShootStop();
                CurrentWeapon.WeaponInputReleased();
            }

            if (CurrentWeapon.State.Is(Weapon.States.DelayBetweenUses)
                && input.ShootAxis.IsOff() && input.ShootButton.IsOff()
                && !(UseSecondaryAxisThresholdToShoot && input.SecondaryMovement.magnitude > input.Threshold.magnitude))
            {
                CurrentWeapon.WeaponInputStop();
            }

            if (authorized && UseSecondaryAxisThresholdToShoot && input.SecondaryMovement.magnitude > input.Threshold.magnitude)
            {
                ShootStart();
            }
        }

        /// <summary>
        /// Triggers an attack if the weapon is idle and an input has been buffered
        /// </summary>
        protected virtual void HandleBuffer()
        {
            if (CurrentWeapon == null)
                return;

            // if we are currently buffering an input and if the weapon is now idle
            if (_buffering && CurrentWeapon.State.Is(Weapon.States.Idle))
            {
                // and if our buffer is still valid, we trigger an attack
                if (Time.time < _bufferEndsAt)
                {
                    ShootStart();
                }
                else
                {
                    _buffering = false;
                }
            }
        }

        /// <summary>
        /// Causes the character to start shooting
        /// </summary>
        public virtual void ShootStart()
        {
            // if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
            if (AbilityUnauthorized)
                return;

            if (CurrentWeapon == null)
                return;

            if (_conditionState.Not(Character.Conditions.Normal))
                return;

            //  if we've decided to buffer input, and if the weapon is in use right now
            if (BufferInput && CurrentWeapon.State.Not(Weapon.States.Idle))
            {
                // if we're not already buffering, or if each new input extends the buffer, we turn our buffering state to true
                ExtendBuffer();
            }
            
            PlayAbilityStartFeedbacks();
            CurrentWeapon.WeaponInputStart();
        }

        /// <summary>
        /// Extends the duration of the buffer if needed
        /// </summary>
        protected virtual void ExtendBuffer()
        {
            if (!_buffering || NewInputExtendsBuffer)
            {
                _buffering = true;
                _bufferEndsAt = Time.time + MaximumBufferDuration;
            }
        }

        /// <summary>
        /// Causes the character to stop shooting
        /// </summary>
        public virtual void ShootStop()
        {
            // if the Shoot action is enabled in the permissions, we continue, if not we do nothing
            if (AbilityUnauthorized || CurrentWeapon == null)
                return;

            switch (CurrentWeapon.State.CurrentState)
            {
                case Weapon.States.Idle:
                case Weapon.States.Reloading:
                case Weapon.States.ReloadStart:
                case Weapon.States.ReloadStop:
                case Weapon.States.DelayBeforeUse when !CurrentWeapon.DelayBeforeUseReleaseInterruption:
                case Weapon.States.DelayBetweenUses when !CurrentWeapon.TimeBetweenUsesReleaseInterruption:
                case Weapon.States.Use:
                    return;
                case Weapon.States.Start:
                case Weapon.States.Stop:
                case Weapon.States.ReloadNeeded:
                case Weapon.States.Interrupted:
                default:
                    ForceStop();
                    break;
            }
        }

        /// <summary>
        /// Forces the weapon to stop 
        /// </summary>
        public virtual void ForceStop()
        {
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
            if (CurrentWeapon)
            {
                CurrentWeapon.TurnWeaponOff();
            }
        }

        /// <summary>
        /// Reloads the weapon
        /// </summary>
        public virtual void Reload()
        {
            if (CurrentWeapon)
            {
                CurrentWeapon.InitiateReloadWeapon();
            }
        }

        /// <summary>
        /// Changes the character's current weapon to the one passed as a parameter
        /// </summary>
        /// <param name="newWeapon">The new weapon.</param>
        public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
        {
            // if the character already has a weapon, we make it stop shooting
            if (CurrentWeapon)
            {
                CurrentWeapon.TurnWeaponOff();
                if (!combo)
                {
                    ShootStop();

                    if (_weaponAim)
                        _weaponAim.RemoveReticle();

                    if (_character.Animator)
                    {
                        foreach (var parameter in _character.Animator.parameters)
                        {
                            if (parameter.name == CurrentWeapon.EquippedAnimationParameter)
                            {
                                MMAnimatorExtensions.UpdateAnimatorBool(_animator, CurrentWeapon.EquippedAnimationParameter, false);
                            }
                        }
                    }

                    Destroy(CurrentWeapon.gameObject);
                }
            }

            if (newWeapon)
            {
                InstantiateWeapon(newWeapon, weaponID, combo);
            }
            else
            {
                CurrentWeapon = null;
                HandleWeaponModel(null, null);
            }

            OnWeaponChange?.Invoke();
        }

        /// <summary>
        /// Instantiates the specified weapon
        /// </summary>
        /// <param name="newWeapon"></param>
        /// <param name="weaponID"></param>
        /// <param name="combo"></param>
        protected virtual void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
        {
            if (!combo)
            {
                CurrentWeapon = Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);
            }

            CurrentWeapon.name = newWeapon.name;
            CurrentWeapon.transform.parent = WeaponAttachment.transform;
            CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;
            CurrentWeapon.SetOwner(_character, this);
            CurrentWeapon.WeaponID = weaponID;
            CurrentWeapon.FlipWeapon();
            _weaponAim = CurrentWeapon.GetComponent<WeaponAim>();

            HandleWeaponAim();

            // we handle (optional) inverse kinematics (IK) 
            HandleWeaponIK();

            // we handle the weapon model
            HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);

            // we turn off the gun's emitters.
            CurrentWeapon.Initialization();
            CurrentWeapon.InitializeComboWeapons();
            CurrentWeapon.InitializeAnimatorParameters();
            InitializeAnimatorParameters();
        }

        /// <summary>
        /// Applies aim if possible
        /// </summary>
        protected virtual void HandleWeaponAim()
        {
            if (_weaponAim && _weaponAim.enabled)
            {
                if (ForceWeaponAimControl)
                {
                    _weaponAim.AimControl = ForcedWeaponAimControl;
                }

                _weaponAim.ApplyAim();
            }
        }

        /// <summary>
        /// Sets IK handles if needed
        /// </summary>
        protected virtual void HandleWeaponIK()
        {
            if (_weaponIK)
            {
                _weaponIK.SetHandles(CurrentWeapon.LeftHandHandle, CurrentWeapon.RightHandHandle);
            }

            if (CurrentWeapon.TryGetComponent<ProjectileWeapon>(out var weapon))
            {
                weapon.SetProjectileSpawnTransform(ProjectileSpawn);
            }
        }

        protected virtual void HandleWeaponModel(Weapon newWeapon, string weaponID, bool combo = false, Weapon weapon = null)
        {
            if (_weaponModels == null)
                return;

            bool handlesSet = false;

            foreach (WeaponModel model in _weaponModels)
            {
                if (model.Owner == this)
                {
                    model.Hide();
                    if (model.UseIK && !handlesSet)
                    {
                        _weaponIK.SetHandles(null, null);
                    }
                }

                if (model.WeaponID == weaponID)
                {
                    model.Show(this);
                    if (model.UseIK)
                    {
                        _weaponIK.SetHandles(model.LeftHandHandle, model.RightHandHandle);
                        handlesSet = true;
                    }

                    if (weapon)
                    {
                        if (model.BindFeedbacks)
                        {
                            weapon.WeaponStartMMFeedback = model.WeaponStartMMFeedback;
                            weapon.WeaponUsedMMFeedback = model.WeaponUsedMMFeedback;
                            weapon.WeaponStopMMFeedback = model.WeaponStopMMFeedback;
                            weapon.WeaponReloadMMFeedback = model.WeaponReloadMMFeedback;
                            weapon.WeaponReloadNeededMMFeedback = model.WeaponReloadNeededMMFeedback;
                        }

                        if (model.AddAnimator)
                        {
                            weapon.Animators.Add(model.TargetAnimator);
                        }

                        if (model.OverrideWeaponUseTransform)
                        {
                            weapon.WeaponUseTransform = model.WeaponUseTransform;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flips the current weapon if needed
        /// </summary>
        public override void Flip()
        {
        }

        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        protected override void InitializeAnimatorParameters()
        {
            if (CurrentWeapon == null)
                return;

            RegisterAnimatorParameter(_weaponEquippedAnimationParameterName, AnimatorControllerParameterType.Bool, out _weaponEquippedAnimationParameter);
            RegisterAnimatorParameter(_weaponEquippedIDAnimationParameterName, AnimatorControllerParameterType.Int, out _weaponEquippedIDAnimationParameter);
        }

        /// <summary>
        /// Override this to send parameters to the character's animator. This is called once per cycle, by the Character
        /// class, after Early, normal and Late process().
        /// </summary>
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _weaponEquippedAnimationParameter, (CurrentWeapon != null), _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
            if (CurrentWeapon == null)
            {
                MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, -1, _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
            }
            else
            {
                MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, CurrentWeapon.WeaponAnimationID, _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
            }
        }

        public override void onEvent(OnHit e)
        {
            base.onEvent(e);
            if (GettingHitInterruptsAttack && CurrentWeapon)
            {
                CurrentWeapon.Interrupt();
            }
        }

        public override void onEvent(OnDeath e)
        {
            base.onEvent(e);
            ShootStop();
            if (CurrentWeapon)
            {
                ChangeWeapon(null, null);
            }
        }

        public override void onEvent(OnRevive e)
        {
            base.onEvent(e);
            Setup();
        }
    }
}