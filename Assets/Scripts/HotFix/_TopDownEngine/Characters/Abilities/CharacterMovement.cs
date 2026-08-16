using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Add this ability to a Character to have it handle ground movement (walk, and potentially run, crawl, etc) in x and z direction for 3D, x and y for 2D
    /// Animator parameters : Speed (float), Walking (bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/CharacterMovement")]
    public class CharacterMovement : CharacterAbility
    {
        /// the possible rotation modes for the character
        public enum Movements
        {
            Free,
            Strict2DirectionsHorizontal,
            Strict2DirectionsVertical,
            Strict4Directions,
            Strict8Directions
        }

        /// if this is true, movement will be forbidden (as well as flip)
        public virtual bool MovementForbidden { get; set; }

        [Header("Direction")]
        [Tooltip("whether the character can move freely, in 2D only, in 4 or 8 cardinal directions")]
        public Movements Movement = Movements.Free;

        [Header("Settings")]
        [Tooltip("whether or not movement input is authorized at that time")]
        public bool InputAuthorized = true;

        [Tooltip("whether or not input should be analog")]
        public bool AnalogInput;

        [Tooltip("whether or not input should be set from another script")]
        public bool ScriptDrivenInput;

        [Header("Speed")]
        [Tooltip("the speed of the character when it's walking")]
        public float WalkSpeed = 6f;

        public ValueModifier WalkSpeedModifier { get; set; }

        public float walkSpeed
        {
            get
            {
                var speed = WalkSpeed;
                return WalkSpeedModifier.SafeInvoke(ref speed);
            }
        }

        [Tooltip("whether or not this component should set the controller's movement")]
        public bool ShouldSetMovement = true;

        [Tooltip("the speed threshold after which the character is not considered idle anymore")]
        public float IdleThreshold = 0.05f;

        [Header("Acceleration")]
        [Tooltip("the acceleration to apply to the current speed / 0f : no acceleration, instant full speed")]
        public float Acceleration = 10f;

        [Tooltip("the deceleration to apply to the current speed / 0f : no deceleration, instant stop")]
        public float Deceleration = 10f;

        [Tooltip("whether or not to interpolate movement speed")]
        public bool InterpolateMovementSpeed;

        public virtual float MovementSpeedMaxMultiplier { get; set; } = float.MaxValue;
        float _movementSpeedMultiplier = 1;

        /// the multiplier to apply to the horizontal movement
        public float MovementSpeedMultiplier
        {
            get => Mathf.Min(_movementSpeedMultiplier, MovementSpeedMaxMultiplier);
            set => _movementSpeedMultiplier = value;
        }

        [Header("Walk Feedback")]
        [Tooltip("the particles to trigger while walking")]
        public ParticleSystem[] WalkParticles;

        [Header("Touch The Ground Feedback")]
        [Tooltip("the particles to trigger when touching the ground")]
        public ParticleSystem[] TouchTheGroundParticles;

        [Tooltip("the sfx to trigger when touching the ground")]
        public AudioClip[] TouchTheGroundSfx;

        public float _movementSpeed;
        public Vector2 _movement;
        public Vector3 _movementVector;
        public Vector2 _currentInput;
        public Vector2 _normalizedInput;
        public Vector2 _lerpedInput;
        public float _acceleration;
        public bool _walkParticlesPlaying;

        protected const string _speedAnimationParameterName = "Speed";
        protected const string _walkingAnimationParameterName = "Walking";
        protected const string _idleAnimationParameterName = "Idle";
        protected int _speedAnimationParameter;
        protected int _walkingAnimationParameter;
        protected int _idleAnimationParameter;

        /// <summary>
        /// On Initialization, we set our movement speed to WalkSpeed.
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
        }

        protected override void OnBindStats()
        {
            base.OnBindStats();

            var moveSpeed = _character.GetStat(Character.Stat.MS);
            WalkSpeedModifier = (ref float raw) => { raw = moveSpeed.Value; };
        }

        /// <summary>
        /// Resets character movement states and speeds
        /// </summary>
        public override void ResetAbility()
        {
            _motionState?.ChangeState(Character.Motions.Idle);

            MovementSpeedMultiplier = 1f;
            MovementForbidden = false;

            foreach (var system in TouchTheGroundParticles)
                if (system) system.Stop();

            foreach (var system in WalkParticles)
                if (system) system.Stop();
        }

        public override void OnUpdate(float dt)
        {
            HandleFrozen();

            if (AbilityUnauthorized || _conditionState.Not(Character.Conditions.Normal))
            {
                if (AbilityAuthorized)
                {
                    StopAbilityUsedSfx();
                }

                return;
            }

            HandleDirection();
            HandleMovement();
            Feedbacks();
        }

        /// <summary>
        /// Called at the very start of the ability's cycle, and intended to be overridden, looks for input and calls
        /// methods if conditions are met
        /// </summary>
        protected override void HandleInput()
        {
            if (ScriptDrivenInput)
                return;

            _movement = InputAuthorized ? _curInput : Vector2.zero;
        }

        public virtual void SetMovement(Vector2 value) => _movement = value;
        public virtual void SetHorizontalMovement(float value) => _movement.x = value;
        public virtual void SetVerticalMovement(float value) => _movement.y = value;

        /// <summary>
        /// Modifies player input to account for the selected movement mode
        /// </summary>
        protected virtual void HandleDirection()
        {
            switch (Movement)
            {
                case Movements.Free:
                    // do nothing
                    break;
                case Movements.Strict2DirectionsHorizontal:
                    _movement.y = 0f;
                    break;
                case Movements.Strict2DirectionsVertical:
                    _movement.x = 0;
                    break;
                case Movements.Strict4Directions:
                    if (Mathf.Abs(_movement.x) > Mathf.Abs(_movement.y))
                        _movement.y = 0f;
                    else
                        _movement.x = 0f;

                    break;
                case Movements.Strict8Directions:
                    _movement.x = Mathf.Round(_movement.x);
                    _movement.y = Mathf.Round(_movement.y);
                    break;
            }
        }

        protected virtual void HandleMovement()
        {
            // if we're not walking anymore, we stop our walking sound
            if (_motionState.Not(Character.Motions.Walking) && _startFeedbackIsPlaying)
                StopStartFeedbacks();

            // if we're not walking anymore, we stop our walking sound
            if (_motionState.Not(Character.Motions.Walking) && _abilityInProgressSfx)
                StopAbilityUsedSfx();

            if (_motionState.Is(Character.Motions.Walking) && _abilityInProgressSfx == null)
                PlayAbilityUsedSfx();

            // if movement is prevented, or if the character is dead/frozen/can't move, we exit and do nothing
            if (AbilityUnauthorized || _conditionState.Not(Character.Conditions.Normal))
                return;

            CheckJustGotGrounded();

            if (MovementForbidden)
            {
                _movement = Vector2.zero;
            }

            if (_controller.Grounded && _controller.CurrentMovement.magnitude > IdleThreshold && _motionState.Is(Character.Motions.Idle))
            {
                _motionState.ChangeState(Character.Motions.Walking);
                PlayAbilityStartSfx();
                PlayAbilityUsedSfx();
                PlayAbilityStartFeedbacks();
            }

            // if we're walking and not moving anymore, we go back to the Idle state
            if (_motionState.Is(Character.Motions.Walking) && _controller.CurrentMovement.magnitude <= IdleThreshold)
            {
                _motionState.ChangeState(Character.Motions.Idle);
                PlayAbilityStopSfx();
                PlayAbilityStopFeedbacks();
            }

            if (ShouldSetMovement)
            {
                ApplyMovement();
            }
        }

        /// <summary>
        /// Describes what happens when the character is in the frozen state
        /// </summary>
        protected virtual void HandleFrozen()
        {
            if (AbilityUnauthorized)
                return;

            if (_conditionState.Is(Character.Conditions.Frozen))
            {
                _movement = Vector2.zero;
                ApplyMovement();
            }
        }

        /// <summary>
        /// Moves the controller
        /// </summary>
        protected virtual void ApplyMovement()
        {
            _currentInput = _movement;
            _normalizedInput = _currentInput.normalized;

            float interpolationSpeed = 1f;

            var dt = Time.deltaTime;
            if (Acceleration == 0 || Deceleration == 0)
            {
                _lerpedInput = AnalogInput ? _currentInput : _normalizedInput;
            }
            else
            {
                if (_normalizedInput.magnitude == 0)
                {
                    _acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * dt);
                    _lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _acceleration, dt * Deceleration);
                    interpolationSpeed = Deceleration;
                }
                else
                {
                    _acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * dt);
                    _lerpedInput = AnalogInput ? Vector2.ClampMagnitude(_currentInput, _acceleration) : Vector2.ClampMagnitude(_normalizedInput, _acceleration);
                    interpolationSpeed = Acceleration;
                }
            }

            Vector3 curMovement = new(_lerpedInput.x, 0f, _lerpedInput.y);

            // var moveSpeed = MovementSpeed;
            var moveSpeed = walkSpeed;
            if (InterpolateMovementSpeed)
                _movementSpeed = Mathf.Lerp(_movementSpeed, moveSpeed * MovementSpeedMultiplier, interpolationSpeed * dt);
            else
                _movementSpeed = moveSpeed * MovementSpeedMultiplier;

            _movementSpeed /= 100F;
            curMovement *= _movementSpeed;

            if (curMovement.magnitude > moveSpeed * MovementSpeedMultiplier)
            {
                curMovement = Vector3.ClampMagnitude(curMovement, moveSpeed);
            }

            if (_currentInput.magnitude <= IdleThreshold && _controller.CurrentMovement.magnitude < IdleThreshold)
            {
                curMovement = Vector3.zero;
            }

            _controller.SetMovement(curMovement);
            _movementVector = curMovement;
        }

        /// <summary>
        /// Every frame, checks if we just hit the ground, and if yes, changes the state and triggers a particle effect
        /// </summary>
        protected virtual void CheckJustGotGrounded()
        {
            // if the character just got grounded
            if (_controller.JustGotGrounded)
            {
                _motionState.ChangeState(Character.Motions.Idle);
            }
        }

        /// <summary>
        /// Plays particles when walking, and particles and sounds when landing
        /// </summary>
        protected virtual void Feedbacks()
        {
            if (_controller.Grounded)
            {
                if (_controller.CurrentMovement.magnitude > IdleThreshold)
                {
                    foreach (ParticleSystem system in WalkParticles)
                    {
                        if (!_walkParticlesPlaying && system)
                        {
                            system.Play();
                        }

                        _walkParticlesPlaying = true;
                    }
                }
                else
                {
                    foreach (ParticleSystem system in WalkParticles)
                    {
                        if (_walkParticlesPlaying && system)
                        {
                            system.Stop();
                            _walkParticlesPlaying = false;
                        }
                    }
                }
            }
            else
            {
                foreach (ParticleSystem system in WalkParticles)
                {
                    if (_walkParticlesPlaying && system)
                    {
                        system.Stop();
                        _walkParticlesPlaying = false;
                    }
                }
            }

            if (_controller.JustGotGrounded)
            {
                foreach (ParticleSystem system in TouchTheGroundParticles)
                {
                    if (system)
                    {
                        system.Clear();
                        system.Play();
                    }
                }

                foreach (AudioClip clip in TouchTheGroundSfx)
                {
                    MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Sfx, transform.position);
                }
            }
        }

        /// <summary>
        /// Resets this character's speed
        /// </summary>
        public virtual void ResetSpeed()
        {
            if (_controller)
                _controller.Reset();
        }

        /// <summary>
        /// On Respawn, resets the speed
        /// </summary>
        public override void onEvent(OnRevive e)
        {
            ResetSpeed();
            MovementForbidden = false;
        }

        public override void onEvent(OnDeath e)
        {
            base.onEvent(e);
            DisableWalkParticles();
        }

        /// <summary>
        /// Disables all walk particle systems that may be playing
        /// </summary>
        protected virtual void DisableWalkParticles()
        {
            if (WalkParticles.Length > 0)
            {
                foreach (ParticleSystem walkParticle in WalkParticles)
                {
                    if (walkParticle)
                    {
                        walkParticle.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// On disable we make sure to turn off anything that could still be playing
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            DisableWalkParticles();
            PlayAbilityStopSfx();
            PlayAbilityStopFeedbacks();
            StopAbilityUsedSfx();
        }

        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_speedAnimationParameterName, AnimatorControllerParameterType.Float, out _speedAnimationParameter);
            RegisterAnimatorParameter(_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
            RegisterAnimatorParameter(_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
        }

        /// <summary>
        /// Sends the current speed and the current value of the Walking state to the animator
        /// </summary>
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, Mathf.Abs(_controller.CurrentMovement.magnitude), _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, _motionState.Is(Character.Motions.Walking), _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, _motionState.Is(Character.Motions.Idle), _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
        }
    }
}