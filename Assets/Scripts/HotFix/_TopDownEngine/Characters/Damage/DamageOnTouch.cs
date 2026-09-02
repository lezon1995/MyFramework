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

        public bool AutoBindStats = true;

        [MMInspectorGroup("Targets")]
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

        [Tooltip("how to determine the damage direction passed to the Health damage method, usually you'll use velocity for moving damage areas (projectiles) and owner position for melee weapons")]
        public DamageDirections DamageDirectionMode;

        [Header("Knockback")]
        [Tooltip("the type of knockback to apply when causing damage")]
        public KnockbackStyles DamageCausedKnockbackType;

        [Tooltip("The direction to apply the knockback ")]
        public KnockbackDirections DamageCausedKnockbackDirection;

        [Tooltip("The force to apply to the object that gets damaged - this force will be rotated based on your knockback direction mode. So for example in 3D if you want to be pushed back the opposite direction, focus on the z component, with a force of 0,0,20 for example")]
        public Vector3 DamageKnockbackForce = new(2, 0, 0);

        public Vector3 LethalDamageKnockbackForce = new(1, 0, 0);

        [Header("Invincibility")]
        [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
        public float InvincibilityDuration;

        [MMInspectorGroup("Damage Taken")]
        [Tooltip("The Health component on which to apply damage taken. If left empty, will attempt to grab one on this object.")]
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
        protected Vector3 _lastPosition, _lastDamagePosition, _velocity, _damageDirection;
        protected float _startTime;
        protected Health _colliderHealth;
        protected TopDownController _selfController;
        protected TopDownController _colliderController;
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

            if (AutoBindStats)
            {
                BindStats();
            }
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

            TryGetComponent(out _selfController);
            TryGetComponent(out _boxCollider2D);
            TryGetComponent(out _circleCollider2D);
            _lastDamagePosition = transform.position;
        }

        protected virtual void BindStats()
        {
            DmgGetter = () =>
            {
                if (Source && Source.GetStat(Character.Stat.AD, out var stat))
                {
                    return Dmg.AD((int)stat.Value);
                }

                return Dmg;
            };
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

        public virtual void SetOwner(Character owner)
        {
            Owner = owner.gameObject;
            Source = owner;
            if (BuffOnTouch)
            {
                BuffOnTouch.SetOwner(owner.gameObject);
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
            _colliderController = null;

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
                if (!health.TryGetComponent(out _colliderController))
                {
                    health.TryGetComponentInParent(out _colliderController);
                }

                HitDamageableFeedback.Play(transform.position);
                HitDamageableEvent?.Invoke(_colliderHealth);

                // we apply the damage to the thing we've collided with
                var dmg = DmgGetter();
                DetermineDamageDirection();
                _colliderHealth.Damage(ref dmg, gameObject, Source, InvincibilityDuration, _damageDirection);
                ApplyKnockback(_colliderHealth, _colliderController, dmg);
            }
            else
            {
                switch (resistDamageType)
                {
                    case ResistDamageType.None:
                        break;
                    case ResistDamageType.Invincible:
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
            var selfDamage = DamageTakenEveryTime + DamageTakenDamageable;
            if (selfDamage > 0 && !_colliderHealth.PreventTakeSelfDamage)
            {
                SelfDamage(selfDamage);
            }
        }

        #region Knockback

        /// <summary>
        /// Applies knockback if needed
        /// </summary>
        protected virtual void ApplyKnockback(Health colliderHealth, TopDownController controller, Dmg damage)
        {
            if (DamageCausedKnockbackType != KnockbackStyles.AddForce)
                return;

            Vector3 force;
            if (damage.IsLethal)
                force = LethalDamageKnockbackForce;
            else
                force = DamageKnockbackForce;

            var knockbackForce = force * colliderHealth.KnockbackForceMultiplier;
            ApplyKnockback2D(ref knockbackForce);

            colliderHealth.ApplyKnockback(knockbackForce, damage);
        }

        /// <summary>
        /// Applies knockback if we're in a 2D context
        /// </summary>
        protected virtual void ApplyKnockback2D(ref Vector3 force)
        {
            switch (DamageCausedKnockbackDirection)
            {
                case KnockbackDirections.BasedOnSpeed:
                    var totalVelocity = _colliderController.IntentVelocity + _velocity;
                    force = Vector3.RotateTowards(force, totalVelocity.normalized, 10f, 0f);
                    break;
                case KnockbackDirections.BasedOnOwnerPosition:
                    var relativePosition = _colliderController.transform.position - Owner.transform.position;
                    force = Vector3.RotateTowards(force, relativePosition.normalized, 10f, 0f);
                    break;
                case KnockbackDirections.BasedOnDirection:
                    var direction = transform.position - _positionLastFrame;
                    force = direction * force.magnitude;
                    break;
                case KnockbackDirections.BasedOnScriptDirection:
                    force = _knockbackScriptDirection * force.magnitude;
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
            if (_selfController && _colliderController)
            {
                Vector3 totalVelocity = _colliderController.IntentVelocity + _velocity;
                Vector3 knockbackForce = Vector3.RotateTowards(DamageTakenKnockbackForce, totalVelocity.normalized, 10f, 0f);

                if (DamageTakenKnockbackType == KnockbackStyles.AddForce)
                {
                    _selfController.AddForce(knockbackForce);
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
        public int MetaType;
        public bool IsCrit;
        public float CritDamage;
        public Stat DmgRate;
        public bool Self;
        public int DamageRaw;
        public int DamageDealt;
        public Vector3 Direction;
        public bool TriggerEffect;
        public bool IsLethal;
        public Vector2 HitNormal;
        public Mixed Mix;
        public int Hash => hash;
        int hash;

        public static Dmg AD(int value) => new(value, Types.AD, false);
        public static Dmg AP(int value) => new(value, Types.AP, false);
        public static Dmg True(int value) => new(value, Types.True, false);
        public static Dmg Adaptive(int value) => new(value, Types.Adaptive, false);

        public Dmg(int value, Types type, bool isCrit, float critDamage = 2F)
        {
            Effect = Effects.Attack;
            Value = value;
            Type = type;
            ActualType = type;
            Algo = Algos.Fixed;
            MetaType = 0;
            IsCrit = isCrit;
            CritDamage = critDamage;
            DmgRate = 1F;
            DamageRaw = 0;
            DamageDealt = 0;
            Direction = Vector3.up;
            Self = false;
            Mix = default;
            TriggerEffect = true;
            HitNormal = Vector2.up;
            IsLethal = false;
            hash = 0;
        }

        public Dmg(int value, Types type, Algos algo)
        {
            Effect = Effects.Attack;
            Value = value;
            Type = type;
            ActualType = type;
            Algo = algo;
            MetaType = 0;
            IsCrit = false;
            CritDamage = 2F;
            DmgRate = 1F;
            DamageRaw = 0;
            DamageDealt = 0;
            Direction = Vector3.up;
            Self = false;
            Mix = default;
            TriggerEffect = true;
            HitNormal = Vector2.up;
            IsLethal = false;
            hash = 0;
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
            CritDamage = 2F;
            return this;
        }

        public Dmg Crit(float critDamage)
        {
            IsCrit = true;
            CritDamage = critDamage;
            return this;
        }

        public Dmg SetValue(int value)
        {
            Value = value;
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

        public Dmg setMixed(float pctAD, float pctAP, float pctTrue = 0F)
        {
            Mix = new()
            {
                On = true,
                PctAD = pctAD,
                PctAP = pctAP,
                PctTrue = pctTrue,
            };
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

        public Dmg SetCritDamage(float critDamage)
        {
            CritDamage = critDamage;
            return this;
        }

        public Dmg addDmgRate(float delta)
        {
            DmgRate.increase(delta);
            return this;
        }

        public void SetMetaType(int type)
        {
            MetaType = type;
        }

        public void SetHash(int hash)
        {
            this.hash = hash;
        }

        public bool equalsWith(Dmg dmg)
        {
            if (hash != 0 && dmg.hash != 0)
            {
                if (hash == dmg.hash)
                {
                    return true;
                }
            }

            return false;
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