using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// A class meant to be overridden that handles a character's ability. 
    /// </summary>
    public abstract class CharacterAbility : TopDownMonoBehaviour
        , IEvent<OnRevive>
        , IEvent<OnDeath>
        , IEvent<OnHit>
    {
        public bool InitializeOnAwake = true;

        public AudioClip AbilityStartSfx;
        public AudioClip AbilityInProgressSfx;
        public AudioClip AbilityStopSfx;

        public MMFeedbacks AbilityStartFeedbacks;
        public MMFeedbacks AbilityStopFeedbacks;

        [Header("Permission")]
        [Tooltip("if true, this ability can perform as usual, if not, it'll be ignored. You can use this to unlock abilities over time for example")]
        public bool AbilityPermitted = true;

        [Tooltip("an array containing all the blocking movement states. If the Character is in one of these states and tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while Idle or Swimming, for example.")]
        public Character.Motions[] BlockingMovementStates;

        [Tooltip("an array containing all the blocking condition states. If the Character is in one of these states and tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while dead, for example.")]
        public Character.Conditions[] BlockingConditionStates;

        [Tooltip("an array containing all the blocking weapon states. If one of the character's weapons is in one of these states and yet the character tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while attacking, for example.")]
        public Weapon.States[] BlockingWeaponStates;

        public bool AbilityUnauthorized => !AbilityAuthorized;

        public bool AbilityAuthorized
        {
            get
            {
                if (_character)
                {
                    var motions = BlockingMovementStates;
                    if (motions is { Length: > 0 })
                    {
                        for (int i = 0; i < motions.Length; i++)
                        {
                            if (motions[i] == _character.motionState.CurrentState)
                                return false;
                        }
                    }

                    var conditions = BlockingConditionStates;
                    if (conditions is { Length: > 0 })
                    {
                        for (int i = 0; i < conditions.Length; i++)
                        {
                            if (conditions[i] == _character.conditionState.CurrentState)
                                return false;
                        }
                    }

                    var states = BlockingWeaponStates;
                    if (states is { Length: > 0 })
                    {
                        for (int i = 0; i < states.Length; i++)
                        {
                            foreach (var handleWeapon in _handleWeaponList)
                            {
                                if (handleWeapon.CurrentWeapon && handleWeapon.CurrentWeapon.State.Is(states[i]))
                                    return false;
                            }
                        }
                    }
                }

                return AbilityPermitted;
            }
        }

        public bool AbilityInitialized => _abilityInitialized;
        public Character Character
        {
            get
            {
                if (_character == null)
                    this.TryGetComponentInParent(out _character);

                return _character;
            }
        }

        public event Action OnAbilityStart;
        public event Action OnAbilityStop;

        protected Character _character;
        protected Stats _stats;
        protected TopDownController _controller;
        protected TopDownController2D _controller2D;
        protected GameObject _model;
        protected Health _health;
        protected CharacterMovement _characterMovement;
        protected InputManager _inputManager;
        protected Animator _animator;
        protected SpriteRenderer _spriteRenderer;
        protected MMStateMachine<Character.Motions> _motionState => _character?.motionState;
        protected MMStateMachine<Character.Conditions> _conditionState => _character?.conditionState;
        protected AudioSource _abilityInProgressSfx;
        protected bool _abilityInitialized;
        protected Vector2 _curInput;
        protected bool _startFeedbackIsPlaying;
        protected List<CharacterHandleWeapon> _handleWeaponList = new();

        protected void Awake()
        {
        }

        protected void Start()
        {
            if (InitializeOnAwake)
            {
                Initialization();
            }
        }

        /// <summary>
        /// Gets and stores components for further use
        /// </summary>
        protected virtual void Initialization()
        {
            this.TryGetComponentInParent(out _character);
            this.TryGetComponentInParent(out _stats);
            this.TryGetComponentInParent(out _controller);
            this.TryGetComponentInParent(out _controller2D);
            this.TryGetComponentInParent(out _spriteRenderer);
            
            _character.FindAbility(out _characterMovement);

            _model = _character.Model;
            _health = _character.Health;
            _character.FindAbilities(ref _handleWeaponList);
            _inputManager = _character.Input;
            _abilityInitialized = true;

            BindAnimator();
            BindStats();
        }

        /// <summary>
        /// Call this any time you want to force this ability to initialize (again)
        /// </summary>
        public void ForceInitialization()
        {
            Initialization();
        }

        /// <summary>
        /// Binds the animator from the character and initializes the animator parameters
        /// </summary>
        protected virtual void BindAnimator()
        {
            if (_character.Animator == null)
            {
                _character.AssignAnimator();
            }

            _animator = _character.Animator;

            if (_animator)
            {
                InitializeAnimatorParameters();
            }
        }

        protected void BindStats()
        {
            if (_stats)
            {
                OnBindStats();
            }
        }

        protected virtual void OnBindStats()
        {
        }


        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        protected virtual void InitializeAnimatorParameters()
        {
        }

        /// <summary>
        /// Internal method to check if an input manager is present or not
        /// </summary>
        protected virtual void InternalHandleInput()
        {
            if (_inputManager)
            {
                var movement = _inputManager.PrimaryMovement;
                _curInput = movement;
                HandleInput();
            }
        }

        /// <summary>
        /// Called at the very start of the ability's cycle, and intended to be overridden, looks for input and calls methods if conditions are met
        /// </summary>
        protected virtual void HandleInput()
        {
        }

        /// <summary>
        /// Resets all input for this ability. Can be overridden for ability specific directives
        /// </summary>
        public void ResetInput()
        {
            _curInput = Vector2.zero;
        }

        /// <summary>
        /// The first of the 3 passes you can have in your ability. Think of it as EarlyUpdate() if it existed
        /// </summary>
        public void OnUpdateBefore()
        {
            InternalHandleInput();
        }

        /// <summary>
        /// The second of the 3 passes you can have in your ability. Think of it as Update()
        /// </summary>
        public virtual void OnUpdate(float dt)
        {
        }

        /// <summary>
        /// Override this to send parameters to the character's animator. This is called once per cycle, by the Character class, after Early, normal and Late process().
        /// </summary>
        public virtual void UpdateAnimator()
        {
        }

        public virtual void Tick(float dt)
        {
        }
        
        /// <summary>
        /// Changes the status of the ability's permission
        /// </summary>
        /// <param name="abilityPermitted">If set to <c>true</c> ability permitted.</param>
        public virtual void PermitAbility(bool abilityPermitted)
        {
            AbilityPermitted = abilityPermitted;
        }

        /// <summary>
        /// Override this to specify what should happen in this ability when the character flips
        /// </summary>
        public virtual void Flip()
        {
        }

        /// <summary>
        /// Override this to reset this ability's parameters. It'll be automatically called when the character gets killed, in anticipation for its respawn.
        /// </summary>
        public virtual void ResetAbility()
        {
        }

        /// <summary>
        /// Changes the reference to the input manager with the one set in parameters
        /// </summary>
        /// <param name="newInputManager"></param>
        public virtual void SetInputManager(InputManager newInputManager)
        {
            _inputManager = newInputManager;
        }

        /// <summary>
        /// Plays the ability start sound effect
        /// </summary>
        public virtual void PlayAbilityStartSfx()
        {
            if (AbilityStartSfx)
            {
                MMSoundManagerSoundPlayEvent.Trigger(AbilityStartSfx, MMSoundManager.MMSoundManagerTracks.Sfx, transform.position);
            }
        }

        /// <summary>
        /// Plays the ability used sound effect
        /// </summary>
        public virtual void PlayAbilityUsedSfx()
        {
            if (AbilityInProgressSfx && _abilityInProgressSfx == null)
            {
                _abilityInProgressSfx = MMSoundManagerSoundPlayEvent.Trigger(AbilityInProgressSfx, MMSoundManager.MMSoundManagerTracks.Sfx, transform.position, true);
            }
        }

        /// <summary>
        /// Stops the ability used sound effect
        /// </summary>
        public virtual void StopAbilityUsedSfx()
        {
            if (_abilityInProgressSfx)
            {
                MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Free, 0, _abilityInProgressSfx);
                _abilityInProgressSfx = null;
            }
        }

        /// <summary>
        /// Plays the ability stop sound effect
        /// </summary>
        public virtual void PlayAbilityStopSfx()
        {
            if (AbilityStopSfx)
            {
                MMSoundManagerSoundPlayEvent.Trigger(AbilityStopSfx, MMSoundManager.MMSoundManagerTracks.Sfx, transform.position);
            }
        }

        /// <summary>
        /// Plays the ability start sound effect
        /// </summary>
        public virtual void PlayAbilityStartFeedbacks()
        {
            AbilityStartFeedbacks.Play(transform.position);
            _startFeedbackIsPlaying = true;
            OnAbilityStart?.Invoke();
        }

        /// <summary>
        /// Stops the ability used sound effect
        /// </summary>
        public virtual void StopStartFeedbacks()
        {
            AbilityStartFeedbacks.Stop();
            _startFeedbackIsPlaying = false;
        }

        /// <summary>
        /// Plays the ability stop sound effect
        /// </summary>
        public virtual void PlayAbilityStopFeedbacks()
        {
            AbilityStopFeedbacks.Play();
            OnAbilityStop?.Invoke();
        }

        /// <summary>
        /// Registers a new animator parameter to the list
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterType">Parameter type.</param>
        protected virtual void RegisterAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType, out int parameter)
        {
            parameter = Animator.StringToHash(parameterName);

            if (_animator == null)
                return;

            if (_animator.MMHasParameterOfType(parameterName, parameterType))
            {
                if (_character)
                {
                    _character.AnimatorParameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// Override this to describe what should happen to this ability when the character respawns
        /// </summary>
        public virtual void onEvent(OnRevive e)
        {
        }

        /// <summary>
        /// Override this to describe what should happen to this ability when the character respawns
        /// </summary>
        public virtual void onEvent(OnDeath e)
        {
            StopAbilityUsedSfx();
            StopStartFeedbacks();
        }

        /// <summary>
        /// Override this to describe what should happen to this ability when the character takes a hit
        /// </summary>
        public virtual void onEvent(OnHit e)
        {
        }

        /// <summary>
        /// On enable, we bind our respawn delegate
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_health == null)
                _health = GetComponentInParent<Character>().Health;

            if (_health == null)
                _health = GetComponentInParent<Health>();

            if (_health)
            {
                _health.Event.addListener<OnRevive>(this);
                _health.Event.addListener<OnDeath>(this);
                _health.Event.addListener<OnHit>(this);
            }
        }

        /// <summary>
        /// On disable, we unbind our respawn delegate
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_health)
            {
                _health.Event.removeListener<OnRevive>(this);
                _health.Event.removeListener<OnDeath>(this);
                _health.Event.removeListener<OnHit>(this);
            }
        }
    }
}