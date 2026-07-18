using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    public partial class Character
    {
        // the possible character types : player controller or AI (controlled by the computer)
        public enum Types
        {
            Player,
            AI
        }

        public enum Stat
        {
            HealthMax, //Health Point
            HealthRegen, //Health Point Regen(per 1s)
            ManaMax, //Mana Point
            ManaRegen, //Mana Point Regen(per 1s)
            AD, //Attack Damage
            AR, //Attack Damage Defence
            AD_PT, //Attack Damage Penetration Fixed
            AD_PT_Rate, //Attack Damage Penetration Rate
            AP, //Ability Power
            MR, //Ability Power Defence
            AP_PT, //Ability Power Penetration Fixed
            AP_PT_Rate, //Ability Power Penetration Rate
            AS, //Attack Speed
            CD, //Cooldown
            MS, //Move Speed
            CritChance, //Crit Chance
            CritDamage, //Crit Damage
            DmgRate, //DmgRate
            AF, //Adaptive Force
            LS, //Life Steal
            Range, //Attack Range
            DodgeChance, //Dodge Chance
            BallisticSpeed, //Ballistic Speed
        }

        /// The possible Movement States the character can be in. These usually correspond to their own class, 
        /// but it's not mandatory
        public enum Motions
        {
            Idle,
            Walking,
            Running,
            Dashing,
        }

        /// The possible character conditions
        public enum Conditions
        {
            Normal,
            Frozen,
            Paused,
            Dead,
            Stunned
        }

        /// the possible initial facing direction for your character
        public enum FacingDirections
        {
            West,
            East,
            North,
            South
        }

        [Flags]
        public enum ComponentFlags
        {
            Buffable = 1 << 0,
            Stats = 1 << 1,
        }
    }

    /// <summary>
    /// This class will pilot the TopDownController component of your character.
    /// This is where you'll implement all of your character's game rules, like jump, dash, shoot, stuff like that.
    /// Animator parameters : Grounded (bool), xSpeed (float), ySpeed (float), 
    /// CollidingLeft (bool), CollidingRight (bool), CollidingBelow (bool), CollidingAbove (bool), Idle (bool)
    /// Random : a random float between 0 and 1, updated every frame, useful to add variance to your state entry transitions for example
    /// RandomConstant : a random int (between 0 and 1000), generated at Start and that'll remain constant for the entire lifetime of this animator, useful to have different characters of the same type 
    /// </summary>
    [SelectionBase]
    [AddComponentMenu("TopDown Engine/Character/Core/Character")]
    public partial class Character : MainActorBehaviour
        , IEventRouter
        , IEvent<OnRevive>
        , IEvent<OnDeath>
        , IEvent<OnHit>
        , IEvent<OnHeal>
        , IEvent<OnDmg>
        , IEvent<DoDmg>
        , IEvent<OnCombat>
        , IEvent<OnWindup>
        , IEvent<DoKill>
        , IStatsGetter<Character.Stat>
    {
        public ComponentFlags Flags { get; set; }

        [TitleGroup("Base")] public Types CharacterType = Types.AI;

        [TitleGroup("Base")] public string PlayerID;

        [TitleGroup("Animator")] [Tooltip("the character animator, that this class and all abilities should update parameters on")]
        public Animator CharacterAnimator;

        [TitleGroup("Animator")] [Tooltip("Set this to false if you want to implement your own animation system")]
        public bool UseDefaultMecanim = true;

        [TitleGroup("Animator")] [Tooltip("If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.")]
        public bool RunAnimatorSanityChecks;

        [TitleGroup("Animator")] [Tooltip("if this is true, animator logs for the associated animator will be turned off to avoid potential spam")]
        public bool DisableAnimatorLogs = true;

        [TitleGroup("Bindings")] [Tooltip("the 'model' (can be any game object) used to manipulate the character. Ideally it's separated (and nested) from the collider/TopDown controller/abilities, to avoid messing with collisions.")]
        public GameObject Model;

        [TitleGroup("Bindings")] [Tooltip("the Health script associated to this Character, will be grabbed automatically if left empty")]
        public Health Health;

        [TitleGroup("Bindings")] [Tooltip("the Stats script associated to this Character, will be grabbed automatically if left empty")]
        public Stats Stats => stats;
        public Stats stats;

        [TitleGroup("Bindings")] [Tooltip("the Buffable script associated to this Character, will be grabbed automatically if left empty")]
        public Buffable Buffable;

        [TitleGroup("Bindings")] [Tooltip("the Exp script associated to this Character, will be grabbed automatically if left empty")]
        public Exp Exp;

        [TitleGroup("Bindings")] [Tooltip("A list of gameObjects (usually nested under the Character) under which to search for additional abilities")]
        public List<GameObject> AdditionalAbilityNodes;

        [TitleGroup("Bindings")] [Tooltip("The brain currently associated with this character, if it's an Advanced AI. By default the engine will pick the one on this object, but you can attach another one if you'd like")]
        public AIBrain CharacterBrain;

        public MMStateMachine<Motions> motionState;
        public MMStateMachine<Conditions> conditionState;

        public IEventRouter Event => this;
        public virtual InputManager Input { get; protected set; }
        public virtual Animator Animator { get; protected set; }
        public virtual HashSet<int> AnimatorParameters { get; protected set; }
        public CharacterOrientation2D Orientation2D;
        public CharacterMovement Movement;
        public virtual GameObject CameraTarget { get; protected set; }
        public virtual Vector3 CameraDirection { get; protected set; }

        public virtual TopDownController Controller
        {
            get
            {
                if (_controller == null)
                    TryGetComponent(out _controller);

                return _controller;
            }
        }

        public OnCombat Combat;
        public bool InCombat => Combat.IsOn;

        protected List<CharacterAbility> _characterAbilities = new();
        protected bool _abilitiesCachedOnce;
        protected TopDownController _controller;
        protected float _animatorRandomNumber;
        protected bool _spawnDirectionForced;

        protected const string _groundedAnimationParameterName = "Grounded";
        protected const string _aliveAnimationParameterName = "Alive";
        protected const string _currentSpeedAnimationParameterName = "CurrentSpeed";
        protected const string _xSpeedAnimationParameterName = "xSpeed";
        protected const string _ySpeedAnimationParameterName = "ySpeed";
        protected const string _zSpeedAnimationParameterName = "zSpeed";
        protected const string _xVelocityAnimationParameterName = "xVelocity";
        protected const string _yVelocityAnimationParameterName = "yVelocity";
        protected const string _zVelocityAnimationParameterName = "zVelocity";
        protected const string _idleAnimationParameterName = "Idle";
        protected const string _randomAnimationParameterName = "Random";
        protected const string _randomConstantAnimationParameterName = "RandomConstant";
        protected int _groundedAnimationParameter;
        protected int _aliveAnimationParameter;
        protected int _currentSpeedAnimationParameter;
        protected int _xSpeedAnimationParameter;
        protected int _ySpeedAnimationParameter;
        protected int _zSpeedAnimationParameter;
        protected int _xVelocityAnimationParameter;
        protected int _yVelocityAnimationParameter;
        protected int _zVelocityAnimationParameter;
        protected int _idleAnimationParameter;
        protected int _randomAnimationParameter;
        protected int _randomConstantAnimationParameter;
        protected bool _animatorInitialized;
        protected bool _onReviveRegistered;
        protected CoroutineHandle _conditionChangeCoroutine;
        protected Conditions _lastState;

        public Action<Character> onDeath { get; set; }
        public Action<Character, Character> onDoKill { get; set; }
        public Action<Character, Character, Dmg> onDoDmg { get; set; }
        public Action<Character, Character, Dmg> onTakeDmg { get; set; }
        public Action<Character, Character, Heal> onTakeHeal { get; set; }
        public Action<Character> onBasicAttackWinddown { get; set; }

        /// <summary>
        /// Initializes this instance of the character
        /// </summary>
        protected override void OnAwake()
        {
            base.OnAwake();
            Initialization();
        }

        /// <summary>
        /// Gets and stores input manager, camera and components
        /// </summary>
        protected virtual void Initialization()
        {
            // we initialize our state machines
            motionState = new(gameObject);
            conditionState = new(gameObject);

            // we get the current input manager
            SetInputManager();

            // we store our components for further use 
            TryGetComponent(out _controller);

            if (Health == null)
                TryGetComponent(out Health);

            if (Stats == null)
                TryGetComponent(out stats);

            if (Buffable == null)
                TryGetComponent(out Buffable);

            if (Buffable)
                Flags |= ComponentFlags.Buffable;

            if (Exp == null)
                TryGetComponent(out Exp);

            CacheAbilitiesAtInit();

            if (CharacterBrain == null)
                TryGetComponent(out CharacterBrain);

            if (CharacterBrain)
                CharacterBrain.SetOwner(gameObject);

            FindAbility(out Orientation2D);
            FindAbility(out Movement);

            AssignAnimator();

            if (CameraTarget == null)
                CameraTarget = new GameObject();

            CameraTarget.transform.SetParent(transform);
            CameraTarget.transform.localPosition = Vector3.zero;
            CameraTarget.name = "CameraTarget";

            Combat.Character = this;

            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.CharacterInitialized, this);
        }

        /// <summary>
        /// Caches abilities if necessary
        /// </summary>
        protected virtual void CacheAbilitiesAtInit()
        {
            if (_abilitiesCachedOnce)
                return;

            CacheAbilities();
        }

        /// <summary>
        /// Grabs abilities and caches them for further use
        /// Make sure you call this if you add abilities at runtime
        /// Ideally you'll want to avoid adding components at runtime, it's costly,
        /// and it's best to activate/disable components instead.
        /// But if you need to, call this method.
        /// </summary>
        public virtual void CacheAbilities()
        {
            // we grab all abilities at our level
            _characterAbilities.Clear();
            GetComponents(_characterAbilities);

            // if the user has specified more nodes
            var list = AdditionalAbilityNodes;
            if (list is { Count: > 0 })
            {
                // we add the ones from the nodes
                for (var i = 0; i < list.Count; i++)
                {
                    list[i].TryGetComponentsInChildren(ref _characterAbilities);
                }
            }

            _abilitiesCachedOnce = true;
        }

        /// <summary>
        /// Forces the (re)initialization of the character's abilities
        /// </summary>
        public virtual void ForceAbilitiesInitialization()
        {
            for (int i = 0; i < _characterAbilities.Count; i++)
            {
                _characterAbilities[i].ForceInitialization();
            }

            var list = AdditionalAbilityNodes;
            if (list is { Count: > 0 })
            {
                for (int i = 0; i < list.Count; i++)
                {
                    foreach (var ability in list[i].GetComponentsInChildren<CharacterAbility>())
                    {
                        ability.ForceInitialization();
                    }
                }
            }
        }

        public void AddAbility(CharacterAbility ability)
        {
            var list = AdditionalAbilityNodes;
            list.Add(ability.gameObject);

            ability.transform.SetParent(transform);
            ability.transform.localPosition = Vector3.zero;

            _characterAbilities.Add(ability);
            ability.ForceInitialization();
            ability.SetInputManager(Input);
        }

        public void RemoveAbility(CharacterAbility ability)
        {
            var list = AdditionalAbilityNodes;
            list.Remove(ability.gameObject);

            ability.transform.SetParent(transform);
            ability.transform.localPosition = Vector3.zero;

            _characterAbilities.Remove(ability);
        }

        /// <summary>
        /// A method to check whether a Character has a certain ability or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindAbility<T>() where T : CharacterAbility
        {
            CacheAbilitiesAtInit();

            foreach (var ability in _characterAbilities)
            {
                if (ability is T characterAbility)
                {
                    return characterAbility;
                }
            }

            return null;
        }

        public bool FindAbility<T>(out T result) where T : CharacterAbility
        {
            CacheAbilitiesAtInit();

            foreach (var ability in _characterAbilities)
            {
                if (ability is T characterAbility)
                {
                    result = characterAbility;
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// A method to check whether a Character has a certain ability or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public CharacterAbility FindAbility(string abilityName)
        {
            CacheAbilitiesAtInit();

            foreach (var ability in _characterAbilities)
            {
                if (ability.GetType().Name == abilityName)
                    return ability;
            }

            return null;
        }

        public bool FindAbility(string abilityName, out CharacterAbility foundAbility)
        {
            CacheAbilitiesAtInit();

            foreach (var ability in _characterAbilities)
            {
                if (ability.GetType().Name == abilityName)
                {
                    foundAbility = ability;
                    return true;
                }
            }

            foundAbility = null;
            return false;
        }

        /// <summary>
        /// A method to check whether a Character has a certain ability or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void FindAbilities<T>(ref List<T> resultList) where T : CharacterAbility
        {
            CacheAbilitiesAtInit();

            resultList.Clear();

            foreach (var ability in _characterAbilities)
            {
                if (ability is T characterAbility)
                {
                    resultList.Add(characterAbility);
                }
            }
        }

        /// <summary>
        /// Binds an animator to this character
        /// </summary>
        public virtual void AssignAnimator(bool forceAssignation = false)
        {
            if (_animatorInitialized && !forceAssignation)
                return;

            AnimatorParameters = new();

            Animator = CharacterAnimator ? CharacterAnimator : GetComponent<Animator>();

            if (Animator)
            {
                if (DisableAnimatorLogs)
                {
                    Animator.logWarnings = false;
                }

                InitializeAnimatorParameters();
            }

            _animatorInitialized = true;
        }

        /// <summary>
        /// Initializes the animator parameters.
        /// </summary>
        protected virtual void InitializeAnimatorParameters()
        {
            if (Animator == null)
                return;

            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _groundedAnimationParameterName, out _groundedAnimationParameter, AnimatorControllerParameterType.Bool, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _currentSpeedAnimationParameterName, out _currentSpeedAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _xSpeedAnimationParameterName, out _xSpeedAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _ySpeedAnimationParameterName, out _ySpeedAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _zSpeedAnimationParameterName, out _zSpeedAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _idleAnimationParameterName, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _aliveAnimationParameterName, out _aliveAnimationParameter, AnimatorControllerParameterType.Bool, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _randomAnimationParameterName, out _randomAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _randomConstantAnimationParameterName, out _randomConstantAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _xVelocityAnimationParameterName, out _xVelocityAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _yVelocityAnimationParameterName, out _yVelocityAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(Animator, _zVelocityAnimationParameterName, out _zVelocityAnimationParameter, AnimatorControllerParameterType.Float, AnimatorParameters);

            // we update our constant float animation parameter
            int randomConstant = Random.Range(0, 1000);
            MMAnimatorExtensions.UpdateAnimatorInteger(Animator, _randomConstantAnimationParameter, randomConstant, AnimatorParameters, RunAnimatorSanityChecks);
        }

        /// <summary>
        /// Gets (if it exists) the InputManager matching the Character's Player ID
        /// </summary>
        public virtual void SetInputManager()
        {
            switch (CharacterType)
            {
                case Types.AI:
                    Input = null;
                    break;
                case Types.Player:
                    // we get the corresponding input manager
                    if (!string.IsNullOrEmpty(PlayerID))
                    {
                        Input = null;
                        foreach (var input in FindObjectsByType<InputManager>(FindObjectsSortMode.None))
                        {
                            if (input.PlayerID == PlayerID)
                                Input = input;
                        }
                    }

                    break;
            }

            UpdateInputManagersInAbilities();
        }

        /// <summary>
        /// Sets a new input manager for this Character and all its abilities
        /// </summary>
        /// <param name="inputManager"></param>
        public virtual void SetInputManager(InputManager inputManager)
        {
            Input = inputManager;
            UpdateInputManagersInAbilities();
        }

        /// <summary>
        /// Updates the linked input manager for all abilities
        /// </summary>
        protected virtual void UpdateInputManagersInAbilities()
        {
            foreach (var ability in _characterAbilities)
            {
                ability.SetInputManager(Input);
            }
        }

        /// <summary>
        /// Resets the input for all abilities
        /// </summary>
        public virtual void ResetInput()
        {
            foreach (var ability in _characterAbilities)
            {
                ability.ResetInput();
            }
        }

        /// <summary>
        /// Sets the player ID
        /// </summary>
        /// <param name="newPlayerID">New player ID.</param>
        public virtual void SetPlayerID(string newPlayerID)
        {
            PlayerID = newPlayerID;
            SetInputManager();
        }

        /// <summary>
        /// We do this every frame. This is separate from Update for more flexibility.
        /// </summary>
        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);
            OnTickBefore(dt);
            OnTick(dt);
        }

        /// <summary>
        /// We do this every frame. This is separate from Update for more flexibility.
        /// </summary>
        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            // we process our abilities
            UpdateAbilitiesBefore();
            UpdateAbilities(dt);
            // UpdateAbilitiesAfter();

            // we send our various states to the animator.		 
            UpdateAnimators();
        }

        /// <summary>
        /// Calls all registered abilities' Early Process methods
        /// </summary>
        protected virtual void UpdateAbilitiesBefore()
        {
            foreach (var ability in _characterAbilities)
                if (ability.enabled && ability.AbilityInitialized)
                    ability.OnUpdateBefore();
        }

        /// <summary>
        /// Calls all registered abilities' Process methods
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void UpdateAbilities(float dt)
        {
            foreach (var ability in _characterAbilities)
                if (ability.enabled && ability.AbilityInitialized)
                    ability.OnUpdate(dt);
        }

        /// <summary>
        /// This is called at Update() and sets each of the animators parameters to their corresponding State values
        /// </summary>
        protected virtual void UpdateAnimators()
        {
            UpdateAnimationRandomNumber();

            if (UseDefaultMecanim && Animator)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(Animator, _groundedAnimationParameter, _controller.Grounded, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorBool(Animator, _aliveAnimationParameter, conditionState.Not(Conditions.Dead), AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _currentSpeedAnimationParameter, _controller.CurrentMovement.magnitude, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _xSpeedAnimationParameter, _controller.CurrentMovement.x, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _ySpeedAnimationParameter, _controller.CurrentMovement.y, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _zSpeedAnimationParameter, _controller.CurrentMovement.z, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorBool(Animator, _idleAnimationParameter, motionState.Is(Motions.Idle), AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _randomAnimationParameter, _animatorRandomNumber, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _xVelocityAnimationParameter, _controller.IntentVelocity.x, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _yVelocityAnimationParameter, _controller.IntentVelocity.y, AnimatorParameters, RunAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(Animator, _zVelocityAnimationParameter, _controller.IntentVelocity.z, AnimatorParameters, RunAnimatorSanityChecks);

                foreach (var ability in _characterAbilities)
                {
                    if (ability.enabled && ability.AbilityInitialized)
                    {
                        ability.UpdateAnimator();
                    }
                }
            }
        }


        void OnTickBefore(float dt)
        {
            CheckInCombatStatus(dt);
        }

        void OnTick(float dt)
        {
            // we process our abilities
            // TickAbilitiesBefore();
            TickAbilities(dt);
            TickBuffs(dt);
            // TickAbilitiesAfter();
        }

        protected virtual void TickAbilities(float dt)
        {
            foreach (var ability in _characterAbilities)
                if (ability.enabled && ability.AbilityInitialized)
                    ability.Tick(dt);
        }

        protected virtual void TickBuffs(float dt)
        {
            if ((Flags & ComponentFlags.Buffable) != 0)
            {
                Buffable.OnTick(dt);
            }
        }

        public virtual void SetColliderEnabled(bool enable)
        {
            if (enable)
                _controller.CollisionsOn();
            else
                _controller.CollisionsOff();
        }

        public virtual void RespawnAt(Vector3 spawnPosition, FacingDirections facingDirection = FacingDirections.South)
        {
            transform.position = spawnPosition;

            gameObject.SetActive(true);

            // we raise it from the dead (if it was dead)
            conditionState.ChangeState(Conditions.Normal);

            // we make it handle collisions again
            _controller.enabled = true;
            _controller.CollisionsOn();
            _controller.Reset();

            Reset();
            UnFreeze();

            if (Health)
            {
                Health.StoreInitialPosition();
                Health.ResetHealthToMaxHealth();
                Health.Resurrect();
            }

            if (CharacterBrain)
            {
                CharacterBrain.enabled = true;
                CharacterBrain.ResetBrain();
            }

            // facing direction
            if (FindAbility<CharacterOrientation2D>(out var orientation2D))
            {
                orientation2D.InitialFacingDirection = facingDirection;
                orientation2D.Face(facingDirection);
            }
        }

        /// <summary>
        /// Makes the player respawn at the location passed in parameters
        /// </summary>
        /// <param name="spawnPoint">The location of the respawn.</param>
        public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection)
        {
            RespawnAt(spawnPoint.position, facingDirection);
        }

        /// <summary>
        /// Calls flip on all abilities
        /// </summary>
        public virtual void FlipAllAbilities()
        {
            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled)
                {
                    ability.Flip();
                }
            }
        }

        /// <summary>
        /// Generates a random number to send to the animator
        /// </summary>
        protected virtual void UpdateAnimationRandomNumber()
        {
            _animatorRandomNumber = Random.Range(0f, 1f);
        }

        /// <summary>
        /// Stores the associated camera direction
        /// </summary>
        public virtual void SetCameraDirection(Vector3 direction)
        {
            CameraDirection = direction;
        }

        /// <summary>
        /// Freezes this character.
        /// </summary>
        public virtual void Freeze()
        {
            _controller.SetGravityActive(false);
            _controller.SetMovement(Vector2.zero);
            conditionState.ChangeState(Conditions.Frozen);
        }

        /// <summary>
        /// Unfreezes this character
        /// </summary>
        public virtual void UnFreeze()
        {
            if (conditionState.CurrentState == Conditions.Frozen)
            {
                _controller.SetGravityActive(true);
                conditionState.ChangeState(Conditions.Normal);
            }
        }

        /// <summary>
        /// Called to disable the player (at the end of a level for example. 
        /// It won't move and respond to input after this.
        /// </summary>
        public virtual void Disable()
        {
            enabled = false;
            _controller.enabled = false;
        }

        /// <summary>
        /// Called when the Character dies. 
        /// Calls every abilities' Reset() method, so you can restore settings to their original value if needed
        /// </summary>
        public virtual void Reset()
        {
            _spawnDirectionForced = false;

            foreach (var ability in _characterAbilities)
            {
                if (ability.enabled)
                {
                    ability.ResetAbility();
                }
            }
        }

        void CheckInCombatStatus(float dt)
        {
            Combat.Check(dt);
        }

        /// <summary>
        /// On revive, we force the spawn direction
        /// </summary>
        public virtual void onEvent(OnRevive e)
        {
            if (CharacterBrain)
            {
                CharacterBrain.enabled = true;
                CharacterBrain.ResetBrain();
            }
        }

        public virtual void onEvent(OnDeath e)
        {
            if (CharacterBrain)
            {
                CharacterBrain.TransitionToState("");
                CharacterBrain.enabled = false;
            }

            motionState.ChangeState(Motions.Idle);
            onDeath?.Invoke(this);
        }

        public virtual void onEvent(OnHit e)
        {
        }

        public virtual void onEvent(OnHeal e)
        {
            onTakeHeal?.Invoke(e.Source, this, e.Heal);
        }

        public virtual void onEvent(DoDmg e)
        {
            Combat.Turn(true);
            onDoDmg?.Invoke(this, e.Character, e.Dmg);
        }

        public virtual void onEvent(DoKill e)
        {
            onDoKill?.Invoke(this, e.Character);
        }

        public virtual void onEvent(OnDmg e)
        {
            Combat.Turn(true);
            onTakeDmg?.Invoke(e.Source, this, e.Dmg);
        }

        public virtual void onEvent(OnCombat e)
        {
            if (Buffable)
                Buffable.NotifyOnCombat(e.IsOn);
        }

        public virtual void onEvent(OnWindup e)
        {
            if (e.Weapon.IsBasicAttack)
            {
                switch (e.State)
                {
                    case OnWindup.States.Start:
                        OnBasicAttackWindup();
                        break;
                    case OnWindup.States.Finish:
                        OnBasicAttackWinddown();
                        break;
                    case OnWindup.States.Cancel:
                        OnBasicAttackWindupCanceled();
                        break;
                }
            }
        }

        protected virtual void OnBasicAttackWindup()
        {
        }

        protected virtual void OnBasicAttackWindupCanceled()
        {
        }

        protected virtual void OnBasicAttackWinddown()
        {
            onBasicAttackWinddown?.Invoke(this);
        }

        /// <summary>
        /// OnEnable, we register our OnRevive event
        /// </summary>
        protected override void OnEnable()
        {
            if (Health)
            {
                if (!_onReviveRegistered)
                {
                    Health.Event.addListener<OnRevive>(this);
                    _onReviveRegistered = true;
                }

                Health.Event.addListener<OnDeath>(this);
                Health.Event.addListener<OnHit>(this);
                Health.Event.addListener<OnHeal>(this);
                Health.Event.addListener<OnDmg>(this);
                Health.Event.addListener<DoDmg>(this);
                Health.Event.addListener<DoKill>(this);
            }

            Event.addListener<OnCombat>(this);
            Event.addListener<OnWindup>(this);
            
            base.OnEnable();
        }

        /// <summary>
        /// OnDisable, we unregister our OnRevive event
        /// </summary>
        protected override void OnDisable()
        {
            if (Health)
            {
                Health.Event.removeListener<OnDeath>(this);
                Health.Event.removeListener<OnHit>(this);
                Health.Event.removeListener<OnHeal>(this);
                Health.Event.removeListener<OnDmg>(this);
                Health.Event.removeListener<DoDmg>(this);
                Health.Event.removeListener<DoKill>(this);
            }

            Event.removeListener<OnCombat>(this);
            Event.removeListener<OnWindup>(this);
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            if (Stats)
                Stats.ClearStats();
            base.OnDestroy();
        }

        public UniStats.Stat GetStat(Stat key)
        {
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
}