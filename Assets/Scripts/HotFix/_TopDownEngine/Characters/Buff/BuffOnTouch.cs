using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this component to an object and it will cause buff to objects that collide with it. 
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Buff/BuffOnTouch")]
    public class BuffOnTouch : MonoBehaviour
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

        /// the possible ways to determine buff directions
        public enum BuffDirections
        {
            BasedOnOwnerPosition,
            BasedOnVelocity,
            BasedOnScriptDirection
        }

        public const TriggerMask AllowedTriggerCallbacks = TriggerMask.OnTriggerEnter | TriggerMask.OnTriggerStay | TriggerMask.OnTriggerEnter2D | TriggerMask.OnTriggerStay2D;

        [Title("Buffs")]
        public bool DriveByDamageOnTouch = true;

        public List<Buff.Data> Buffs = new();

        [Title("Targets")]
        public LayerMask TargetLayerMask;

        [ShowInInspector]
        public GameObject Owner { get; set; }

        [Tooltip("Defines on what triggers the buff should be applied, by default on enter and stay (both 2D and 3D) but this field will let you exclude triggers if needed")]
        public TriggerMask TriggerFilter = AllowedTriggerCallbacks;

        [Tooltip("how to determine the buff direction passed to the Health buff method, usually you'll use velocity for moving buff areas (projectiles) and owner position for melee weapons")]
        public BuffDirections BuffDirectionMode = BuffDirections.BasedOnVelocity;

        [Header("Buff over time")]
        [Tooltip("Whether or not this buff on touch zone should apply buff over time")]
        public bool RepeatBuffOverTime;

        [Tooltip("if in buff over time mode, the duration, in seconds, between two buffs")]
        [MMCondition(nameof(RepeatBuffOverTime), true)]
        public float IntervalBetweenRepeats = 1f;

        [Title("Feedbacks")]
        [Tooltip("the feedback to play when applying a Buff to target")]
        public MMFeedbacks ApplyTargetBuffFeedback;

        [Tooltip("the feedback to play when applying a Buff to owner")]
        public MMFeedbacks ApplyOwnerBuffFeedback;

        // storage		
        protected Vector3 _lastPosition, _lastBuffPosition, _buffDirection;
        protected float _startTime;
        protected Health _colliderHealth;
        protected Buffable _colliderBuffable;
        protected List<GameObject> _ignoredGameObjects = new();
        protected CircleCollider2D _circleCollider2D;
        protected BoxCollider2D _boxCollider2D;
        protected SphereCollider _sphereCollider;
        protected BoxCollider _boxCollider;
        protected Color _gizmosColor;
        protected Vector3 _gizmoSize;
        protected Vector3 _gizmoOffset;
        protected bool _twoD;
        protected bool _initializedFeedbacks;
        protected Vector3 _buffScriptDirection;

        protected Dictionary<GameObject, MMCooldown> _targetBuffTimer = new();

        #region Initialization

        /// <summary>
        /// On Awake we initialize our buff on touch area
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
            _lastBuffPosition = transform.position;
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
            TryGetComponent(out _boxCollider);
            TryGetComponent(out _sphereCollider);
            TryGetComponent(out _boxCollider2D);
            TryGetComponent(out _circleCollider2D);
            _lastBuffPosition = transform.position;
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
            if (_initializedFeedbacks) return;

            ApplyTargetBuffFeedback.Initialize(gameObject);
            ApplyOwnerBuffFeedback.Initialize(gameObject);
            _initializedFeedbacks = true;
        }

        /// <summary>
        /// On disable we clear our ignore list
        /// </summary>
        protected virtual void OnDisable()
        {
            ClearIgnoreList();
            ClearTargetBuffDict();
        }

        /// <summary>
        /// On validate we ensure our inspector is in sync
        /// </summary>
        protected virtual void OnValidate()
        {
            TriggerFilter &= AllowedTriggerCallbacks;
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
            TryGetComponent(out _boxCollider);
            TryGetComponent(out _sphereCollider);
            TryGetComponent(out _circleCollider2D);
            _gizmoSize = newGizmoSize;
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
        /// draws a cube or sphere around the buff area
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = _gizmosColor;

            if (_boxCollider2D)
            {
                if (_boxCollider2D.enabled)
                {
                    MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, false);
                }
                else
                {
                    MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, true);
                }
            }

            if (_circleCollider2D)
            {
                Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
                Gizmos.matrix = rotationMatrix;
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
        }

        /// <summary>
        /// When buff direction is in script mode, lets you specify the direction of buff
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetBuffScriptDirection(Vector3 newDirection)
        {
            _buffScriptDirection = newDirection;
            _buffDirection = newDirection;
        }

        /// <summary>
        /// Adds the gameobject set in parameters to the ignore list
        /// </summary>
        /// <param name="newIgnoredGameObject">New ignored game object.</param>
        public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
        {
            _ignoredGameObjects.Add(newIgnoredGameObject);
        }

        /// <summary>
        /// Removes the object set in parameters from the ignore list
        /// </summary>
        /// <param name="ignoredGameObject">Ignored game object.</param>
        public virtual void StopIgnoringObject(GameObject ignoredGameObject)
        {
            _ignoredGameObjects.Remove(ignoredGameObject);
        }

        /// <summary>
        /// Clears the ignore list.
        /// </summary>
        public virtual void ClearIgnoreList()
        {
            _ignoredGameObjects.Clear();
        }

        public virtual void ClearTargetBuffDict()
        {
            foreach (var (_, cooldown) in _targetBuffTimer)
            {
                MMCooldown.Return(cooldown);
            }

            _targetBuffTimer.Clear();
        }

        #endregion

        #region Loop

        /// <summary>
        /// During last update, we store the position and velocity of the object
        /// </summary>
        protected virtual void Update()
        {
            ComputeVelocity();
            CountTargetBuffTimer();
        }

        /// <summary>
        /// Computes the velocity based on the object's last position
        /// </summary>
        protected virtual void ComputeVelocity()
        {
            var dt = Time.deltaTime;
            if (dt != 0f)
            {
                if (Vector3.Distance(_lastBuffPosition, transform.position) > 0.5f)
                {
                    _lastBuffPosition = transform.position;
                }

                _lastPosition = transform.position;
            }
        }

        protected virtual void CountTargetBuffTimer()
        {
            if (_targetBuffTimer.Count == 0)
                return;

            var dt = Time.deltaTime;
            foreach (var (_, cooldown) in _targetBuffTimer)
            {
                cooldown.Update(dt);
            }
        }

        /// <summary>
        /// Determine the buff direction to pass to the Buffable ApplyBuff method
        /// </summary>
        protected virtual void DetermineBuffDirection()
        {
            switch (BuffDirectionMode)
            {
                case BuffDirections.BasedOnOwnerPosition:
                    if (Owner == null)
                        Owner = gameObject;

                    var direction = _colliderBuffable.transform.position - Owner.transform.position;
                    if (_twoD)
                        direction.z = 0;
                    _buffDirection = direction;

                    break;
                case BuffDirections.BasedOnVelocity:
                    _buffDirection = transform.position - _lastBuffPosition;
                    break;
                case BuffDirections.BasedOnScriptDirection:
                    _buffDirection = _buffScriptDirection;
                    break;
            }

            _buffDirection = _buffDirection.normalized;
        }

        #endregion

        #region CollisionDetection

        /// <summary>
        /// When a collision with the player is triggered, we give buff to the player and knock it back
        /// </summary>
        /// <param name="collider">what's colliding with the object.</param>
        public virtual void OnTriggerStay2D(Collider2D collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerStay2D)) return;
            if (DriveByDamageOnTouch) return;
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// On trigger enter 2D, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerEnter2D)) return;
            if (DriveByDamageOnTouch) return;
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// On trigger stay, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerStay(Collider collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerStay)) return;
            if (DriveByDamageOnTouch) return;
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// On trigger enter, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
        {
            if (0 == (TriggerFilter & TriggerMask.OnTriggerEnter)) return;
            if (DriveByDamageOnTouch) return;
            Colliding(collider.gameObject);
        }

        #endregion

        /// <summary>
        /// When colliding, we apply the appropriate buff
        /// </summary>
        /// <param name="collider"></param>
        public virtual void Colliding(GameObject collider)
        {
            if (!EvaluateAvailability(collider))
                return;

            if (!CheckBuffCanApply(collider))
                return;

            if (collider.TryGetComponent(out _colliderHealth))
            {
                if (_colliderHealth.CurrentHealth > 0)
                {
                    if (collider.TryGetComponent(out _colliderBuffable))
                    {
                        OnCollideWithBuffable();
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether or not buff should be applied this frame
        /// </summary>
        /// <param name="collider"></param>
        protected virtual bool EvaluateAvailability(GameObject collider)
        {
            // if we're inactive, we do nothing
            if (!isActiveAndEnabled)
                return false;

            // if the object we're colliding with is part of our ignore list, we do nothing and exit
            if (_ignoredGameObjects.Contains(collider))
                return false;

            // if what we're colliding with isn't part of the target layers, we do nothing and exit
            if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
                return false;

            // if we're on our first frame, we don't apply buff
            if (Time.time == 0f)
                return false;

            return true;
        }

        protected virtual bool CheckBuffCanApply(GameObject collider)
        {
            if (_targetBuffTimer.TryGetValue(collider, out var cooldown))
            {
                if (cooldown.NotReady())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Describes what happens when colliding with a buffable object
        /// </summary>
        protected virtual void OnCollideWithBuffable()
        {
            if (_colliderBuffable.CanTakeBuffThisFrame())
            {
                DetermineBuffDirection();

                for (var i = 0; i < Buffs.Count; i++)
                {
                    var data = Buffs[i];
                    switch (data.ApplyTo)
                    {
                        case Buff.Actors.Target:
                        {
                            ApplyTargetBuffFeedback.Play(transform.position);
                            var param = new Buff.Param(_buffDirection, data.Stack);
                            _colliderBuffable.ApplyBuff(data.Buff, Owner, param);
                        }
                            break;
                        case Buff.Actors.Source:
                            if (Owner && Owner.TryGetComponent<Buffable>(out var buffable))
                            {
                                ApplyOwnerBuffFeedback.Play(transform.position);
                                var param = new Buff.Param(_buffDirection, data.Stack);
                                buffable.ApplyBuff(data.Buff, Owner, param);
                            }

                            break;
                    }
                }

                if (RepeatBuffOverTime)
                    _targetBuffTimer[_colliderBuffable.gameObject] = MMCooldown.Get(IntervalBetweenRepeats);
                else
                    _targetBuffTimer[_colliderBuffable.gameObject] = MMCooldown.Get(float.MaxValue);
            }
        }
    }
}