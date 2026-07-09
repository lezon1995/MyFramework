using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
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

        /// the current reference movement speed
        public virtual float MovementSpeed { get; set; }

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
        private float _movementSpeedMultiplier;

        /// the multiplier to apply to the horizontal movement
        public float MovementSpeedMultiplier
        {
            get => Mathf.Min(_movementSpeedMultiplier, MovementSpeedMaxMultiplier);
            set => _movementSpeedMultiplier = value;
        }

        /// the multiplier to apply to the horizontal movement, applied by contextual elements (movement zones, etc)
        public Stack<float> ContextSpeedStack = new Stack<float>();

        public virtual float ContextSpeedMultiplier => ContextSpeedStack.Count > 0 ? ContextSpeedStack.Peek() : 1;

        [Header("Walk Feedback")]
        [Tooltip("the particles to trigger while walking")]
        public ParticleSystem[] WalkParticles;

        [Header("Touch The Ground Feedback")]
        [Tooltip("the particles to trigger when touching the ground")]
        public ParticleSystem[] TouchTheGroundParticles;

        [Tooltip("the sfx to trigger when touching the ground")]
        public AudioClip[] TouchTheGroundSfx;

        protected float _movementSpeed;
        protected float _horizontalMovement;
        protected float _verticalMovement;
        protected Vector3 _movementVector;
        protected Vector2 _currentInput;
        protected Vector2 _normalizedInput;
        protected Vector2 _lerpedInput;
        protected float _acceleration;
        protected bool _walkParticlesPlaying;

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
            ResetAbility();
        }

        protected override void OnBindStats()
        {
            base.OnBindStats();

            var moveSpeed = _character.Stats.GetStat(Character.Stat.MS.Key());
            WalkSpeedModifier = (ref float raw) => { raw = moveSpeed.Value; };
        }

        /// <summary>
        /// Resets character movement states and speeds
        /// </summary>
        public override void ResetAbility()
        {
            base.ResetAbility();
            MovementSpeed = walkSpeed;
            ContextSpeedStack.Clear();

            if (_movement != null && _movement.Not(Character.Motions.FallingDownHole))
            {
                _movement.ChangeState(Character.Motions.Idle);
            }

            MovementSpeedMultiplier = 1f;
            MovementForbidden = false;

            foreach (ParticleSystem system in TouchTheGroundParticles)
            {
                if (system) system.Stop();
            }

            foreach (ParticleSystem system in WalkParticles)
            {
                if (system) system.Stop();
            }
        }

        /// <summary>
        /// The second of the 3 passes you can have in your ability. Think of it as Update()
        /// </summary>
        /// <param name="dt"></param>
        public override void OnUpdate(float dt)
        {
            HandleFrozen();

            if (AbilityUnauthorized || _condition.Not(Character.Conditions.Normal))
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

            if (InputAuthorized)
            {
                _horizontalMovement = _horizontalInput;
                _verticalMovement = _verticalInput;
            }
            else
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
            }
        }

        /// <summary>
        /// Sets the horizontal move value.
        /// </summary>
        /// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
        public virtual void SetMovement(Vector2 value)
        {
            _horizontalMovement = value.x;
            _verticalMovement = value.y;
        }

        /// <summary>
        /// Sets the horizontal part of the movement
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetHorizontalMovement(float value)
        {
            _horizontalMovement = value;
        }

        /// <summary>
        /// Sets the vertical part of the movement
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetVerticalMovement(float value)
        {
            _verticalMovement = value;
        }

        /// <summary>
        /// Applies a movement multiplier for the specified duration
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        public virtual void ApplyMovementMultiplier(float movementMultiplier, float duration)
        {
            Timing.RunCoroutine(ApplyMovementMultiplierCo(movementMultiplier, duration));
        }

        /// <summary>
        /// A coroutine used to apply a movement multiplier for a certain duration only
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual IEnumerator<float> ApplyMovementMultiplierCo(float movementMultiplier, float duration)
        {
            if (_characterMovement == null)
                yield break;

            SetContextSpeedMultiplier(movementMultiplier);
            yield return Timing.WaitForSeconds(duration);
            ResetContextSpeedMultiplier();
        }

        /// <summary>
        /// Stacks a new context speed multiplier
        /// </summary>
        /// <param name="newMovementSpeedMultiplier"></param>
        public virtual void SetContextSpeedMultiplier(float newMovementSpeedMultiplier)
        {
            ContextSpeedStack.Push(newMovementSpeedMultiplier);
        }

        /// <summary>
        /// Revers the context speed multiplier to its previous value
        /// </summary>
        public virtual void ResetContextSpeedMultiplier()
        {
            if (ContextSpeedStack.Count <= 0)
                return;

            ContextSpeedStack.Pop();
        }

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
                    _verticalMovement = 0f;
                    break;
                case Movements.Strict2DirectionsVertical:
                    _horizontalMovement = 0f;
                    break;
                case Movements.Strict4Directions:
                    if (Mathf.Abs(_horizontalMovement) > Mathf.Abs(_verticalMovement))
                        _verticalMovement = 0f;
                    else
                        _horizontalMovement = 0f;

                    break;
                case Movements.Strict8Directions:
                    _verticalMovement = Mathf.Round(_verticalMovement);
                    _horizontalMovement = Mathf.Round(_horizontalMovement);
                    break;
            }
        }

        /// <summary>
        /// Called at Update(), handles horizontal movement
        /// </summary>
        protected virtual void HandleMovement()
        {
            // if we're not walking anymore, we stop our walking sound
            if (_movement.Not(Character.Motions.Walking) && _startFeedbackIsPlaying)
                StopStartFeedbacks();

            // if we're not walking anymore, we stop our walking sound
            if (_movement.Not(Character.Motions.Walking) && _abilityInProgressSfx)
                StopAbilityUsedSfx();

            if (_movement.Is(Character.Motions.Walking) && _abilityInProgressSfx == null)
                PlayAbilityUsedSfx();

            // if movement is prevented, or if the character is dead/frozen/can't move, we exit and do nothing
            if (AbilityUnauthorized || _condition.Not(Character.Conditions.Normal))
                return;

            CheckJustGotGrounded();

            if (MovementForbidden)
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
            }

            // if the character is not grounded, but currently idle or walking, we change its state to Falling
            if (!_controller.Grounded
                && _condition.Is(Character.Conditions.Normal)
                && _movement.Is(Character.Motions.Walking, Character.Motions.Idle))
            {
                _movement.ChangeState(Character.Motions.Falling);
            }

            if (_controller.Grounded && _movement.Is(Character.Motions.Falling))
            {
                _movement.ChangeState(Character.Motions.Idle);
            }

            if (_controller.Grounded
                && _controller.CurrentMovement.magnitude > IdleThreshold
                && _movement.Is(Character.Motions.Idle))
            {
                _movement.ChangeState(Character.Motions.Walking);
                PlayAbilityStartSfx();
                PlayAbilityUsedSfx();
                PlayAbilityStartFeedbacks();
            }

            // if we're walking and not moving anymore, we go back to the Idle state
            if (_movement.Is(Character.Motions.Walking)
                && _controller.CurrentMovement.magnitude <= IdleThreshold)
            {
                _movement.ChangeState(Character.Motions.Idle);
                PlayAbilityStopSfx();
                PlayAbilityStopFeedbacks();
            }

            if (ShouldSetMovement)
            {
                SetMovement();
            }
        }

        /// <summary>
        /// Describes what happens when the character is in the frozen state
        /// </summary>
        protected virtual void HandleFrozen()
        {
            if (AbilityUnauthorized)
                return;

            if (_condition.Is(Character.Conditions.Frozen))
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
                SetMovement();
            }
        }

        /// <summary>
        /// Moves the controller
        /// </summary>
        protected virtual void SetMovement()
        {
            _movementVector = Vector3.zero;
            _currentInput = new(_horizontalMovement, _verticalMovement);
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

            _movementVector = new Vector3(_lerpedInput.x, 0f, _lerpedInput.y);

            // var moveSpeed = MovementSpeed;
            var moveSpeed = walkSpeed;
            if (InterpolateMovementSpeed)
                _movementSpeed = Mathf.Lerp(_movementSpeed, moveSpeed * ContextSpeedMultiplier * MovementSpeedMultiplier, interpolationSpeed * dt);
            else
                _movementSpeed = moveSpeed * MovementSpeedMultiplier * ContextSpeedMultiplier;

            _movementVector *= _movementSpeed;

            if (_movementVector.magnitude > moveSpeed * ContextSpeedMultiplier * MovementSpeedMultiplier)
            {
                _movementVector = Vector3.ClampMagnitude(_movementVector, moveSpeed);
            }

            if (_currentInput.magnitude <= IdleThreshold && _controller.CurrentMovement.magnitude < IdleThreshold)
            {
                _movementVector = Vector3.zero;
            }

            _movementVector /= 100F;
            _controller.SetMovement(_movementVector);
        }

        /// <summary>
        /// Every frame, checks if we just hit the ground, and if yes, changes the state and triggers a particle effect
        /// </summary>
        protected virtual void CheckJustGotGrounded()
        {
            // if the character just got grounded
            if (_controller.JustGotGrounded)
            {
                _movement.ChangeState(Character.Motions.Idle);
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
            MovementSpeed = walkSpeed;
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
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, _movement.Is(Character.Motions.Walking), _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, _movement.Is(Character.Motions.Idle), _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
        }
    }
}