using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// the possible ways to add knockback : noKnockback, which won't do anything, set force, or add force
    public enum KnockbackStyles
    {
        None,
        AddForce
    }

    /// the possible knockback directions
    public enum KnockbackDirections
    {
        BasedOnOwnerPosition,
        BasedOnSpeed,
        BasedOnDirection,
        BasedOnScriptDirection
    }

    /// <summary>
    /// Add this component to an object, and it will cause damage to objects that collide with it. 
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Damage/DamageOnTouch")]
    public class DamageOnTouch : MMMonoBehaviour
    {
        [Flags]
        public enum TriggerMask
        {
            IgnoreAll = 0,
            OnTriggerEnter = 1 << 0,
            OnTriggerStay = 1 << 1,
            OnTriggerEnter2D = 1 << 6,
            OnTriggerStay2D = 1 << 7,

            All_3D = OnTriggerEnter | OnTriggerStay,
            All_2D = OnTriggerEnter2D | OnTriggerStay2D,
            All = All_3D | All_2D
        }

        /// the possible ways to determine damage directions
        public enum DamageDirections
        {
            BasedOnOwnerPosition,
            BasedOnVelocity,
            BasedOnScriptDirection
        }

        public const TriggerMask AllowedTrigger = TriggerMask.OnTriggerEnter | TriggerMask.OnTriggerEnter2D;

        [MMInspectorGroup("Targets")]
        [MMInformation("This component will make your object cause damage to objects that collide with it. Here you can define what layers will be affected by the damage (for a standard enemy, choose Player), how much damage to give, and how much force should be applied to the object that gets the damage on hit. You can also specify how long the post-hit invincibility should last (in seconds).")]
        [Tooltip("the layers that will be damaged by this object")]
        public LayerMask TargetLayerMask;

        [ShowInInspector, ReadOnly]
        [Tooltip("the owner of the DamageOnTouch zone")]
        public GameObject Owner { get; set; }

        public Character Source { get; set; }

        [Tooltip("Defines on what triggers the damage should be applied, by default on enter and stay (both 2D and 3D) but this field will let you exclude triggers if needed")]
        public TriggerMask TriggerFilter = AllowedTrigger;

        public Dmg Dmg;
        public Func<Dmg> DmgGetter { get; set; }

        [Tooltip("a list of typed damage definitions that will be applied on top of the base damage")]
        public List<TypedDamage> TypedDamages;

        [Tooltip("how to determine the damage direction passed to the Health damage method, usually you'll use velocity for moving damage areas (projectiles) and owner position for melee weapons")]
        public DamageDirections DamageDirectionMode = DamageDirections.BasedOnVelocity;

        [Header("Knockback")]
        [Tooltip("the type of knockback to apply when causing damage")]
        public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;

        [Tooltip("The direction to apply the knockback ")]
        public KnockbackDirections DamageCausedKnockbackDirection = KnockbackDirections.BasedOnOwnerPosition;

        [Tooltip("The force to apply to the object that gets damaged - this force will be rotated based on your knockback direction mode. So for example in 3D if you want to be pushed back the opposite direction, focus on the z component, with a force of 0,0,20 for example")]
        public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 10);

        [Header("Invincibility")]
        [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
        public float InvincibilityDuration;

        [MMInspectorGroup("Damage Taken")]
        [MMInformation("After having applied the damage to whatever it collided with, you can have this object hurt itself. " +
                       "A bullet will explode after hitting a wall for example. Here you can define how much damage it'll take every time it hits something, " +
                       "or only when hitting something that's damageable, or non damageable. Note that this object will need a Health component too for this to be useful.")]
        [Tooltip("The Health component on which to apply damage taken. If left empty, will attempt to grab one on this object.")]
        public Health DamageTakenHealth;

        [Tooltip("The amount of damage taken every time, whether what we collide with is damageable or not")]
        public float DamageTakenEveryTime;

        [Tooltip("The amount of damage taken when colliding with a damageable object")]
        public float DamageTakenDamageable;

        [Tooltip("The amount of damage taken when colliding with something that is not damageable")]
        public float DamageTakenNonDamageable;

        [Tooltip("the type of knockback to apply when taking damage")]
        public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.None;

        [Tooltip("The force to apply to the object that gets damaged")]
        public Vector3 DamageTakenKnockbackForce = Vector3.zero;

        [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
        public float DamageTakenInvincibilityDuration;

        [MMInspectorGroup("Buff On Touch")]
        public BuffOnTouch BuffOnTouch;

        [MMInspectorGroup("Feedbacks")]
        public MMFeedbacks HitDamageableFeedback;
        public MMFeedbacks HitNonDamageableFeedback;
        public MMFeedbacks HitAnythingFeedback;

        public UnityEvent<Health> HitDamageableEvent;
        public UnityEvent<GameObject> HitNonDamageableEvent;
        public UnityEvent<GameObject> HitAnythingEvent;

        // storage		
        protected Vector3 _lastPosition, _lastDamagePosition, _velocity, _knockbackForce, _damageDirection;
        protected float _startTime;
        protected Health _colliderHealth;
        protected TopDownController _topDownController;
        protected TopDownController _colliderTopDownController;
        protected List<GameObject> _ignoreList = new();
        protected Vector3 _knockbackForceApplied;
        protected CircleCollider2D _circleCollider2D;
        protected BoxCollider2D _boxCollider2D;
        protected SphereCollider _sphereCollider;
        protected BoxCollider _boxCollider;
        protected Color _gizmosColor;
        protected Vector3 _gizmoSize;
        protected Vector3 _gizmoOffset;
        protected Transform _gizmoTransform;
        protected bool _twoD;
        protected bool _initializedFeedbacks;
        protected Vector3 _positionLastFrame;
        protected Vector3 _knockbackScriptDirection;
        protected Vector3 _relativePosition;
        protected Vector3 _damageScriptDirection;

        #region Initialization

        /// <summary>
        /// On Awake we initialize our damage on touch area
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
        }

        /// <summary>
        /// OnEnable we set the start time to the current timestamp
        /// </summary>
        protected virtual void OnEnable()
        {
            _startTime = Time.time;
            _lastPosition = transform.position;
            _lastDamagePosition = _lastPosition;
        }

        /// <summary>
        /// Initializes ignore list, feedbacks, colliders and grabs components
        /// </summary>
        public virtual void Initialization()
        {
            GrabComponents();
            InitializeGizmos();
            InitializeColliders();
            InitializeFeedbacks();
        }

        /// <summary>
        /// Stores components
        /// </summary>
        protected virtual void GrabComponents()
        {
            Owner = gameObject;
            if (Owner.TryGetComponent<Stats>(out var stats))
            {
                DmgGetter = () => Dmg.AD(stats.GetStat(Character.Stat.AD.Key()).Value);
            }
            else
            {
                DmgGetter = () => Dmg;
            }

            if (DamageTakenHealth == null)
                DamageTakenHealth = GetComponent<Health>();

            if (BuffOnTouch == null)
                BuffOnTouch = GetComponent<BuffOnTouch>();

            _topDownController = GetComponent<TopDownController>();
            _boxCollider = GetComponent<BoxCollider>();
            _sphereCollider = GetComponent<SphereCollider>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _circleCollider2D = GetComponent<CircleCollider2D>();
            _lastDamagePosition = transform.position;
        }

        /// <summary>
        /// Initializes colliders, setting them as trigger if needed
        /// </summary>
        protected virtual void InitializeColliders()
        {
            _twoD = _boxCollider2D || _circleCollider2D;
            if (_boxCollider2D)
            {
                SetGizmoOffset(_boxCollider2D.offset);
                _boxCollider2D.isTrigger = true;
            }

            if (_boxCollider)
            {
                SetGizmoOffset(_boxCollider.center);
                _boxCollider.isTrigger = true;
            }

            if (_sphereCollider)
            {
                SetGizmoOffset(_sphereCollider.center);
                _sphereCollider.isTrigger = true;
            }

            if (_circleCollider2D)
            {
                SetGizmoOffset(_circleCollider2D.offset);
                _circleCollider2D.isTrigger = true;
            }
        }

        /// <summary>
        /// Initializes feedbacks
        /// </summary>
        public virtual void InitializeFeedbacks()
        {
            if (_initializedFeedbacks)
                return;

            HitDamageableFeedback.Initialize(gameObject);
            HitNonDamageableFeedback.Initialize(gameObject);
            HitAnythingFeedback.Initialize(gameObject);
            _initializedFeedbacks = true;
        }

        /// <summary>
        /// On disable we clear our ignore list
        /// </summary>
        protected virtual void OnDisable()
        {
            ClearIgnore();
        }

        /// <summary>
        /// On validate we ensure our inspector is in sync
        /// </summary>
        protected virtual void OnValidate()
        {
            TriggerFilter &= AllowedTrigger;
        }

        #endregion

        #region Gizmos

        /// <summary>
        /// Initializes gizmo colors & settings
        /// </summary>
        protected virtual void InitializeGizmos()
        {
            _gizmosColor = Color.red;
            _gizmosColor.a = 0.25f;
        }

        /// <summary>
        /// A public method letting you (re)define gizmo size
        /// </summary>
        /// <param name="newGizmoSize"></param>
        public virtual void SetGizmoSize(Vector3 newGizmoSize)
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _boxCollider = GetComponent<BoxCollider>();
            _sphereCollider = GetComponent<SphereCollider>();
            _circleCollider2D = GetComponent<CircleCollider2D>();
            _gizmoSize = newGizmoSize;
        }

        public void SetEnabled(bool active)
        {
            if (_boxCollider2D)
                _boxCollider2D.enabled = active;
            if (_boxCollider)
                _boxCollider.enabled = active;
            if (_sphereCollider)
                _sphereCollider.enabled = active;
            if (_circleCollider2D)
                _circleCollider2D.enabled = active;
        }

        /// <summary>
        /// A public method letting you specify a gizmo offset
        /// </summary>
        /// <param name="newOffset"></param>
        public virtual void SetGizmoOffset(Vector3 newOffset)
        {
            _gizmoOffset = newOffset;
        }

        /// <summary>
        /// draws a cube or sphere around the damage area
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = _gizmosColor;

            if (_boxCollider2D)
            {
                if (_boxCollider2D.enabled)
                    MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, false);
                else
                    MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, true);
            }

            if (_circleCollider2D)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                if (_circleCollider2D.enabled)
                    Gizmos.DrawSphere((Vector2)_gizmoOffset, _circleCollider2D.radius);
                else
                    Gizmos.DrawWireSphere((Vector2)_gizmoOffset, _circleCollider2D.radius);
            }

            if (_boxCollider)
            {
                if (_boxCollider.enabled)
                    MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider.size, false);
                else
                    MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider.size, true);
            }

            if (_sphereCollider)
            {
                if (_sphereCollider.enabled)
                    Gizmos.DrawSphere(transform.position, _sphereCollider.radius);
                else
                    Gizmos.DrawWireSphere(transform.position, _sphereCollider.radius);
            }
        }

        #endregion

        #region PublicAPIs

        public virtual void SetOwner(GameObject owner)
        {
            Owner = owner;
            Source = owner.GetComponent<Character>();
            if (BuffOnTouch)
            {
                BuffOnTouch.SetOwner(owner);
            }
        }

        public virtual void SetDmg(Dmg dmg)
        {
            Dmg = dmg;
        }

        /// <summary>
        /// When knockback is in script direction mode, lets you specify the direction of the knockback
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetKnockbackScriptDirection(Vector3 newDirection)
        {
            _knockbackScriptDirection = newDirection;
        }

        /// <summary>
        /// When damage direction is in script mode, lets you specify the direction of damage
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetDamageScriptDirection(Vector3 newDirection)
        {
            _damageDirection = newDirection;
            _damageScriptDirection = newDirection;
        }

        public virtual void AddIgnore(GameObject go) => _ignoreList.Add(go);
        public virtual void RemoveIgnore(GameObject go) => _ignoreList.Remove(go);
        public virtual void ClearIgnore() => _ignoreList.Clear();

        #endregion

        #region Loop

        /// <summary>
        /// During last update, we store the position and velocity of the object
        /// </summary>
        protected virtual void Update()
        {
            ComputeVelocity();
        }

        /// <summary>
        /// On Late Update we store our position
        /// </summary>
        protected void LateUpdate()
        {
            _positionLastFrame = transform.position;
        }

        /// <summary>
        /// Computes the velocity based on the object's last position
        /// </summary>
        protected virtual void ComputeVelocity()
        {
            var dt = Time.deltaTime;
            if (dt != 0F)
            {
                _velocity = (_lastPosition - transform.position) / dt;

                if (Vector3.Distance(_lastDamagePosition, transform.position) > 0.5f)
                {
                    _lastDamagePosition = transform.position;
                }

                _lastPosition = transform.position;
            }
        }

        /// <summary>
        /// Determine the damage direction to pass to the Health Damage method
        /// </summary>
        protected virtual void DetermineDamageDirection()
        {
            switch (DamageDirectionMode)
            {
                case DamageDirections.BasedOnOwnerPosition:
                    var direction = _colliderHealth.transform.position - Owner.transform.position;
                    if (_twoD)
                        direction.z = 0;

                    _damageDirection = direction;
                    break;
                case DamageDirections.BasedOnVelocity:
                    _damageDirection = transform.position - _lastDamagePosition;
                    break;
                case DamageDirections.BasedOnScriptDirection:
                    _damageDirection = _damageScriptDirection;
                    break;
            }

            _damageDirection = _damageDirection.normalized;
        }

        #endregion

        #region CollisionDetection

        /// <summary>
        /// When a collision with the player is triggered, we give damage to the player and knock it back
        /// </summary>
        /// <param name="collider">what's colliding with the object.</param>
        public virtual void OnTriggerStay2D(Collider2D collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerStay2D))
                return;
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// On trigger enter 2D, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerEnter2D))
                return;
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// On trigger stay, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerStay(Collider collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerStay))
                return;
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// On trigger enter, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerEnter))
                return;
            Colliding(collider.gameObject);
        }

        #endregion

        public void ForceColliding(GameObject target)
        {
            Colliding(target);
        }

        /// <summary>
        /// When colliding, we apply the appropriate damage
        /// </summary>
        /// <param name="target"></param>
        protected virtual void Colliding(GameObject target)
        {
            if (!EvaluateAvailability(target))
                return;

            // cache reset 
            _colliderTopDownController = null;

            // if what we're colliding with is damageable
            if (target.TryGetComponent(out _colliderHealth))
            {
                OnCollideWithDamageable(_colliderHealth);

                if (_colliderHealth.CurrentHealth > 0)
                {
                    if (BuffOnTouch && BuffOnTouch.DriveByDamageOnTouch)
                    {
                        BuffOnTouch.Colliding(target);
                    }
                }
            }
            else // if what we're colliding with can't be damaged
            {
                OnCollideWithNonDamageable();
                HitNonDamageableEvent?.Invoke(target);
            }


            OnAnyCollision(target);
            HitAnythingEvent?.Invoke(target);
            HitAnythingFeedback.Play(transform.position);
        }

        /// <summary>
        /// Checks whether damage should be applied this frame
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual bool EvaluateAvailability(GameObject target)
        {
            // if we're inactive, we do nothing
            if (!isActiveAndEnabled)
                return false;

            // if the object we're colliding with is part of our ignore list, we do nothing and exit
            if (_ignoreList.Contains(target))
                return false;

            // if what we're colliding with isn't part of the target layers, we do nothing and exit
            if (!MMLayers.LayerInLayerMask(target.layer, TargetLayerMask))
                return false;

            // if we're on our first frame, we don't apply damage
            if (Time.time == 0f)
                return false;

            return true;
        }

        /// <summary>
        /// Describes what happens when colliding with a damageable object
        /// </summary>
        /// <param name="health">Health.</param>
        protected virtual void OnCollideWithDamageable(Health health)
        {
            if (health.CanTakeDamageThisFrame(out var resistDamageType))
            {
                // if what we're colliding with is a TopDownController, we apply a knockback force
                if (!health.TryGetComponent(out _colliderTopDownController))
                {
                    _colliderTopDownController = health.GetComponentInParent<TopDownController>();
                }

                HitDamageableFeedback.Play(transform.position);
                HitDamageableEvent?.Invoke(_colliderHealth);

                // we apply the damage to the thing we've collided with
                ApplyKnockback(DmgGetter(), TypedDamages);
                DetermineDamageDirection();
                _colliderHealth.Damage(DmgGetter(), gameObject, Source, InvincibilityDuration, _damageDirection, TypedDamages);
            }
            else
            {
                switch (resistDamageType)
                {
                    case ResistDamageType.None:
                        break;
                    case ResistDamageType.Invulnerable:
                        break;
                    case ResistDamageType.DashInvincible:
                        health.Character.Event.trigger(new DoDashDodge());
                        break;
                    case ResistDamageType.ImmuneToDamage:
                        break;
                    case ResistDamageType.Dead:
                        break;
                    case ResistDamageType.Disabled:
                        break;
                }
            }

            // we apply self damage
            if (DamageTakenEveryTime + DamageTakenDamageable > 0 && !_colliderHealth.PreventTakeSelfDamage)
            {
                SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
            }
        }

        #region Knockback

        /// <summary>
        /// Applies knockback if needed
        /// </summary>
        protected virtual void ApplyKnockback(Dmg damage, List<TypedDamage> typedDamages)
        {
            if (ShouldApplyKnockback(damage, typedDamages))
            {
                _knockbackForce = DamageCausedKnockbackForce * _colliderHealth.KnockbackForceMultiplier;
                _knockbackForce = _colliderHealth.ComputeKnockbackForce(_knockbackForce, typedDamages);

                if (_twoD)
                    ApplyKnockback2D();
                else
                    ApplyKnockback3D();

                if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
                {
                    _colliderTopDownController.Impact(_knockbackForce.normalized, _knockbackForce.magnitude);
                }
            }
        }

        /// <summary>
        /// Determines whether knockback should be applied
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldApplyKnockback(Dmg damage, List<TypedDamage> typedDamages)
        {
            if (_colliderHealth.ImmuneToKnockbackIfZeroDamage && !_colliderHealth.ComputeDamageOutput(ref damage, out _, out _, typedDamages))
                return false;

            if (!_colliderTopDownController)
                return false;

            if (DamageCausedKnockbackForce == Vector3.zero)
                return false;

            if (_colliderHealth.Invulnerable)
                return false;

            return _colliderHealth.CanGetKnockback(typedDamages);
        }

        /// <summary>
        /// Applies knockback if we're in a 2D context
        /// </summary>
        protected virtual void ApplyKnockback2D()
        {
            switch (DamageCausedKnockbackDirection)
            {
                case KnockbackDirections.BasedOnSpeed:
                    var totalVelocity = _colliderTopDownController.Speed + _velocity;
                    _knockbackForce = Vector3.RotateTowards(_knockbackForce, totalVelocity.normalized, 10f, 0f);
                    break;
                case KnockbackDirections.BasedOnOwnerPosition:
                    _relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
                    _knockbackForce = Vector3.RotateTowards(_knockbackForce, _relativePosition.normalized, 10f, 0f);
                    break;
                case KnockbackDirections.BasedOnDirection:
                    var direction = transform.position - _positionLastFrame;
                    _knockbackForce = direction * _knockbackForce.magnitude;
                    break;
                case KnockbackDirections.BasedOnScriptDirection:
                    _knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
                    break;
            }
        }

        /// <summary>
        /// Applies knockback if we're in a 3D context
        /// </summary>
        protected virtual void ApplyKnockback3D()
        {
            switch (DamageCausedKnockbackDirection)
            {
                case KnockbackDirections.BasedOnSpeed:
                    var totalVelocity = _colliderTopDownController.Speed + _velocity;
                    _knockbackForce = _knockbackForce * totalVelocity.magnitude;
                    break;
                case KnockbackDirections.BasedOnOwnerPosition:
                    _relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
                    _knockbackForce = Quaternion.LookRotation(_relativePosition) * _knockbackForce;
                    break;
                case KnockbackDirections.BasedOnDirection:
                    var direction = transform.position - _positionLastFrame;
                    _knockbackForce = direction * _knockbackForce.magnitude;
                    break;
                case KnockbackDirections.BasedOnScriptDirection:
                    _knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
                    break;
            }
        }

        #endregion


        /// <summary>
        /// Describes what happens when colliding with a non-damageable object
        /// </summary>
        protected virtual void OnCollideWithNonDamageable()
        {
            float selfDamage = DamageTakenEveryTime + DamageTakenNonDamageable;
            if (selfDamage > 0)
            {
                SelfDamage(selfDamage);
            }

            HitNonDamageableFeedback.Play(transform.position);
        }

        /// <summary>
        /// Describes what could happen when colliding with anything
        /// </summary>
        protected virtual void OnAnyCollision(GameObject other)
        {
        }

        /// <summary>
        /// Applies damage to itself
        /// </summary>
        /// <param name="damage">Damage.</param>
        protected virtual void SelfDamage(float damage)
        {
            if (DamageTakenHealth)
            {
                _damageDirection = Vector3.up;
                DamageTakenHealth.Damage(Dmg.True(damage).SetSelf(), gameObject, Source, DamageTakenInvincibilityDuration, _damageDirection);
            }

            // if what we're colliding with is a TopDownController, we apply a knockback force
            if (_topDownController && _colliderTopDownController)
            {
                Vector3 totalVelocity = _colliderTopDownController.Speed + _velocity;
                Vector3 knockbackForce = Vector3.RotateTowards(DamageTakenKnockbackForce, totalVelocity.normalized, 10f, 0f);

                if (DamageTakenKnockbackType == KnockbackStyles.AddForce)
                {
                    _topDownController.AddForce(knockbackForce);
                }
            }
        }
    }

    [Serializable]
    public struct Dmg
    {
        public Effects Effect;
        public float Value;
        public Types Type;
        public Types ActualType { get; set; }
        public Algos Algo;
        public bool IsCrit;
        public float CritRate;
        public float DmgRate;
        public bool Self { get; set; }
        public float DamageRaw { get; set; }
        public float DamageDealt { get; set; }
        public Vector3 Direction { get; set; }

        public Mixed Mix { get; set; }

        public static Dmg AD(float value) => new(value, Types.AD, false);
        public static Dmg AP(float value) => new(value, Types.AP, false);
        public static Dmg True(float value) => new(value, Types.True, false);
        public static Dmg Adaptive(float value) => new(value, Types.Adaptive, false);

        public Dmg(float value, Types type, bool isCrit)
        {
            Effect = Effects.Attack;
            Value = value;
            Type = type;
            ActualType = type;
            Algo = Algos.Fixed;
            IsCrit = isCrit;
            CritRate = 2F;
            DmgRate = 1F;
            DamageRaw = 0F;
            DamageDealt = 0F;
            Direction = Vector3.zero;
            Self = false;
            Mix = default;
        }

        public Dmg(float value, Types type, Algos algo)
        {
            Effect = Effects.Attack;
            Value = value;
            Type = type;
            ActualType = type;
            Algo = algo;
            IsCrit = false;
            CritRate = 2F;
            DmgRate = 1F;
            DamageRaw = 0F;
            DamageDealt = 0F;
            Direction = Vector3.zero;
            Self = false;
            Mix = default;
        }

        public bool IsAdaptive() => Type == Types.Adaptive;

        public Dmg Fixed()
        {
            Algo = Algos.Fixed;
            return this;
        }

        public Dmg CurPct()
        {
            Algo = Algos.CurPct;
            return this;
        }

        public Dmg LostPct()
        {
            Algo = Algos.LostPct;
            return this;
        }

        public Dmg AllPct()
        {
            Algo = Algos.AllPct;
            return this;
        }

        public Dmg Crit()
        {
            IsCrit = true;
            CritRate = 2F;
            return this;
        }

        public Dmg Crit(float critDamage)
        {
            IsCrit = true;
            CritRate = critDamage;
            return this;
        }

        public Dmg SetEffect(Effects effect)
        {
            Effect = effect;
            return this;
        }

        public Dmg SetDamageRaw(float damage)
        {
            DamageRaw = damage;
            return this;
        }

        public Dmg SetDamageDealt(float damage)
        {
            DamageDealt = damage;
            return this;
        }

        public Dmg SetDirection(Vector3 direction)
        {
            Direction = direction;
            return this;
        }

        public Dmg SetActualType(Types type)
        {
            ActualType = type;
            return this;
        }

        public Dmg SetDmgRate(float rate)
        {
            DmgRate = rate;
            return this;
        }

        public Dmg SetSelf()
        {
            Self = true;
            return this;
        }

        public enum Types
        {
            AD,
            AP,
            True,
            Adaptive,
        }

        public enum Algos
        {
            Fixed,
            CurPct,
            LostPct,
            AllPct,
        }

        public enum Effects
        {
            Attack,
            Ability,
        }

        [Serializable]
        public struct Mixed
        {
            public bool On;
            public float PctAD;
            public float PctAP;
            public float PctTrue;

            public bool Off => !On;
            public float DamageDealtAD { get; set; }
            public float DamageDealtAP { get; set; }
            public float DamageDealtTrue { get; set; }

            public float Sum()
            {
                return DamageDealtAD + DamageDealtAP + DamageDealtTrue;
            }
        }
    }
}