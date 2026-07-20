using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains
{
    public enum KnockbackStyles
    {
        None,
        AddForce
    }

    public enum KnockbackDirections
    {
        BasedOnOwnerPosition,
        BasedOnSpeed,
        BasedOnDirection,
        BasedOnScriptDirection
    }

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
            BasedOnVelocity,
            BasedOnOwnerPosition,
            BasedOnScriptDirection
        }

        public const TriggerMask AllowedTrigger = TriggerMask.OnTriggerEnter2D | TriggerMask.OnTriggerStay2D;

        [MMInspectorGroup("Targets")]
        public bool ManuallyColliding;
        
        [MMInspectorGroup("Targets")] [Tooltip("the layers that will be damaged by this object")]
        public LayerMask TargetLayerMask;

        [ShowInInspector, ReadOnly]
        [Tooltip("the owner of the DamageOnTouch zone")]
        public GameObject Owner { get; set; }

        public Character Source { get; set; }

        [Tooltip("Defines on what triggers the damage should be applied, by default on enter and stay (both 2D and 3D) but this field will let you exclude triggers if needed")]
        public TriggerMask TriggerFilter = AllowedTrigger;

        public Dmg Dmg;
        public Func<Dmg> DmgGetter { get; set; }

        [Tooltip("how to determine the damage direction passed to the Health damage method, usually you'll use velocity for moving damage areas (projectiles) and owner position for melee weapons")]
        public DamageDirections DamageDirectionMode;

        [Header("Knockback")] [Tooltip("the type of knockback to apply when causing damage")]
        public KnockbackStyles DamageCausedKnockbackType;

        [Tooltip("The direction to apply the knockback ")]
        public KnockbackDirections DamageCausedKnockbackDirection;

        [Tooltip("The force to apply to the object that gets damaged - this force will be rotated based on your knockback direction mode. So for example in 3D if you want to be pushed back the opposite direction, focus on the z component, with a force of 0,0,20 for example")]
        public Vector3 DamageKnockbackForce = new(2, 0, 0);
        
        public Vector3 LethalDamageKnockbackForce = new(1, 0, 0);

        [Header("Invincibility")] [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
        public float InvincibilityDuration;

        [MMInspectorGroup("Damage Taken")] [Tooltip("The Health component on which to apply damage taken. If left empty, will attempt to grab one on this object.")]
        public Health DamageTakenHealth;

        [Tooltip("The amount of damage taken every time, whether what we collide with is damageable or not")]
        public int DamageTakenEveryTime;

        [Tooltip("The amount of damage taken when colliding with a damageable object")]
        public int DamageTakenDamageable;

        [Tooltip("The amount of damage taken when colliding with something that is not damageable")]
        public int DamageTakenNonDamageable;

        [Tooltip("the type of knockback to apply when taking damage")]
        public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.None;

        [Tooltip("The force to apply to the object that gets damaged")]
        public Vector3 DamageTakenKnockbackForce = Vector3.zero;

        [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
        public float DamageTakenInvincibilityDuration;

        [MMInspectorGroup("Buff On Touch")] public BuffOnTouch BuffOnTouch;

        [MMInspectorGroup("Feedbacks")] public MMFeedbacks HitDamageableFeedback;
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
        protected Color _gizmosColor;
        protected Vector3 _gizmoSize;
        protected Vector3 _gizmoOffset;
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

            BindStats();
        }

        /// <summary>
        /// Stores components
        /// </summary>
        protected virtual void GrabComponents()
        {
            Owner = gameObject;
            if (DamageTakenHealth == null)
                TryGetComponent(out DamageTakenHealth);

            if (BuffOnTouch == null)
                TryGetComponent(out BuffOnTouch);

            TryGetComponent(out _topDownController);
            TryGetComponent(out _boxCollider2D);
            TryGetComponent(out _circleCollider2D);
            _lastDamagePosition = transform.position;
        }

        protected virtual void BindStats()
        {
            if (Owner.TryGetComponent<Stats>(out var stats))
            {
                DmgGetter = () => Dmg.AD((int)stats.GetStat(Character.Stat.AD.Key()).Value);
            }
            else
            {
                DmgGetter = () => Dmg;
            }
        }

        /// <summary>
        /// Initializes colliders, setting them as trigger if needed
        /// </summary>
        protected virtual void InitializeColliders()
        {
            if (_boxCollider2D)
            {
                SetGizmoOffset(_boxCollider2D.offset);
                _boxCollider2D.isTrigger = true;
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
            TryGetComponent(out _boxCollider2D);
            TryGetComponent(out _circleCollider2D);
            _gizmoSize = newGizmoSize;
        }

        public void SetEnabled(bool active)
        {
            if (_boxCollider2D)
                _boxCollider2D.enabled = active;
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
                    _lastDamagePosition = transform.position;

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
                    _damageDirection = _colliderHealth.transform.position - Owner.transform.position;
                    _damageDirection.z = 0;
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

        public virtual void OnTriggerStay2D(Collider2D c)
        {
            if (ManuallyColliding)
                return;
            
            if (0 == (TriggerFilter & TriggerMask.OnTriggerStay2D))
                return;

            Colliding(c.gameObject);
        }

        public virtual void OnTriggerEnter2D(Collider2D c)
        {
            if (ManuallyColliding)
                return;
            
            if (0 == (TriggerFilter & TriggerMask.OnTriggerEnter2D))
                return;

            Colliding(c.gameObject);
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
        protected virtual void OnCollideWithDamageable(Health health)
        {
            if (health.CanTakeDamageThisFrame(out var resistDamageType))
            {
                // if what we're colliding with is a TopDownController, we apply a knockback force
                if (!health.TryGetComponent(out _colliderTopDownController))
                {
                    health.TryGetComponentInParent(out _colliderTopDownController);
                }

                HitDamageableFeedback.Play(transform.position);
                HitDamageableEvent?.Invoke(_colliderHealth);

                // we apply the damage to the thing we've collided with
                var dmg = DmgGetter();
                DetermineDamageDirection();
                _colliderHealth.Damage(ref dmg, gameObject, Source, InvincibilityDuration, _damageDirection);
                ApplyKnockback(dmg);
            }
            else
            {
                switch (resistDamageType)
                {
                    case ResistDamageType.None:
                        break;
                    case ResistDamageType.Invincible:
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
        protected virtual void ApplyKnockback(Dmg damage)
        {
            if (ShouldApplyKnockback(damage))
            {
                Vector3 force;
                if (damage.IsLethal)
                    force = LethalDamageKnockbackForce;
                else
                    force = DamageKnockbackForce;

                _knockbackForce = force * _colliderHealth.KnockbackForceMultiplier;
                _knockbackForce = _colliderHealth.ComputeKnockbackForce(_knockbackForce);

                ApplyKnockback2D();

                if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
                {
                    _colliderTopDownController.AddImpact(_knockbackForce.normalized, _knockbackForce.magnitude);
                }
            }
        }

        /// <summary>
        /// Determines whether knockback should be applied
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldApplyKnockback(Dmg damage)
        {
            if (_colliderHealth.ImmuneToKnockbackIfZeroDamage && !_colliderHealth.ComputeDamageOutput(ref damage))
                return false;

            if (!_colliderTopDownController)
                return false;

            if (_colliderHealth.Invincible)
                return false;

            return _colliderHealth.CanGetKnockback();
        }

        /// <summary>
        /// Applies knockback if we're in a 2D context
        /// </summary>
        protected virtual void ApplyKnockback2D()
        {
            switch (DamageCausedKnockbackDirection)
            {
                case KnockbackDirections.BasedOnSpeed:
                    var totalVelocity = _colliderTopDownController.IntentVelocity + _velocity;
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

        #endregion


        /// <summary>
        /// Describes what happens when colliding with a non-damageable object
        /// </summary>
        protected virtual void OnCollideWithNonDamageable()
        {
            int selfDamage = DamageTakenEveryTime + DamageTakenNonDamageable;
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
        protected virtual void SelfDamage(int damage)
        {
            if (DamageTakenHealth)
            {
                _damageDirection = Vector3.up;
                var dmg = Dmg.True(damage).SetSelf();
                DamageTakenHealth.Damage(ref dmg, gameObject, Source, DamageTakenInvincibilityDuration, _damageDirection);
            }

            // if what we're colliding with is a TopDownController, we apply a knockback force
            if (_topDownController && _colliderTopDownController)
            {
                Vector3 totalVelocity = _colliderTopDownController.IntentVelocity + _velocity;
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
        public int Value;
        public Types Type;
        public Types ActualType;
        public Algos Algo;
        public bool IsCrit;
        public float CritRate;
        public Stat DmgRate;
        public bool Self;
        public int DamageRaw;
        public int DamageDealt;
        public Vector3 Direction;
        public bool TriggerEffect;
        public bool IsLethal;
        public Vector2 HitNormal;
        public Mixed Mix;

        public static Dmg AD(int value) => new(value, Types.AD, false);
        public static Dmg AP(int value) => new(value, Types.AP, false);
        public static Dmg True(int value) => new(value, Types.True, false);
        public static Dmg Adaptive(int value) => new(value, Types.Adaptive, false);

        public Dmg(int value, Types type, bool isCrit)
        {
            Effect = Effects.Attack;
            Value = value;
            Type = type;
            ActualType = type;
            Algo = Algos.Fixed;
            IsCrit = isCrit;
            CritRate = 2F;
            DmgRate = 1F;
            DamageRaw = 0;
            DamageDealt = 0;
            Direction = Vector3.zero;
            Self = false;
            Mix = default;
            TriggerEffect = true;
            HitNormal = Vector2.zero;
            IsLethal = false;
        }

        public Dmg(int value, Types type, Algos algo)
        {
            Effect = Effects.Attack;
            Value = value;
            Type = type;
            ActualType = type;
            Algo = algo;
            IsCrit = false;
            CritRate = 2F;
            DmgRate = 1F;
            DamageRaw = 0;
            DamageDealt = 0;
            Direction = Vector3.zero;
            Self = false;
            Mix = default;
            TriggerEffect = true;
            HitNormal = Vector2.zero;
            IsLethal = false;
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

        public bool hasAttackEffect()
        {
            return (Effect & Effects.Attack) != 0;
        }

        public Dmg setAttackEffect()
        {
            Effect = Effects.Attack;
            return this;
        }

        public bool hasSkillEffect()
        {
            return (Effect & Effects.Skill) != 0;
        }

        public Dmg setSkillEffect()
        {
            Effect = Effects.Skill;
            return this;
        }

        public Dmg addAttackEffect()
        {
            Effect |= Effects.Attack;
            return this;
        }

        public Dmg addSkillEffect()
        {
            Effect |= Effects.Skill;
            return this;
        }


        public Dmg setHitNormal(Vector2 normal)
        {
            HitNormal = normal;
            return this;
        }

        public Dmg setTriggerEffect(bool v)
        {
            TriggerEffect = v;
            return this;
        }

        public Dmg SetDamageRaw(int damage)
        {
            DamageRaw = damage;
            return this;
        }

        public Dmg SetDamageDealt(int damage)
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

        public Dmg addDmgRate(float delta)
        {
            DmgRate.increase(delta);
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

        [Flags]
        public enum Effects
        {
            Attack = 1 << 0,
            Skill = 1 << 1,
        }

        [Serializable]
        public struct Mixed
        {
            public bool On;
            public float PctAD;
            public float PctAP;
            public float PctTrue;

            public bool Off => !On;
            public int DamageDealtAD { get; set; }
            public int DamageDealtAP { get; set; }
            public int DamageDealtTrue { get; set; }

            public float Sum()
            {
                return DamageDealtAD + DamageDealtAP + DamageDealtTrue;
            }
        }
    }
}