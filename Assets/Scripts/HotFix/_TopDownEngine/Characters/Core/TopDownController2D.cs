using System;
using UnityEngine;

namespace MoreMountains
{
    public class TopDownController2D : TopDownController
    {
        [Header("体积参数")]
        [Tooltip("碰撞半径（当作圆形碰撞体）")]
        public float Radius = 0.5f;

        [Tooltip("质量，影响碰撞时谁推谁动")]
        [Range(0.1f, 10f)]
        public float Mass = 1f;

        [Tooltip("最大可重叠程度（0-1），0表示完全不能重叠，0.5表示可以重叠一半")]
        [Range(0f, 1f)]
        public float MaxOverlapRatio = 0.3f;

        [Tooltip("推力权重，当两个物体互相推挤时，优先级")]
        [Range(0f, 10f)]
        public float PushForceWeight = 1f;

        [Tooltip("移动速度倍率")]
        [Range(0.1f, 5f)]
        public float SpeedMultiplier = 1f;

        [Header("击退参数")]
        [Tooltip("击退抗性，0表示完全不受击退，1表示正常受击退")]
        [Range(0f, 1f)]
        public float KnockbackResistance;

        [Tooltip("击退被其他怪物分担的比率（0-1）")]
        [Range(0f, 1f)]
        public float KnockbackSpreadRatio = 0.5f;

        [Header("阻力参数")]
        [Tooltip("位置修正速度，越大越快分开重叠的物体")]
        [Range(0f, 50f)]
        public float SeparationForce = 10f;

        [Tooltip("速度阻力，用于平滑移动")]
        [Range(0f, 1f)]
        public float VelocityDamping = 0.9f;

        [Header("调试")]
        [Tooltip("显示碰撞范围")]
        public bool ShowGizmos = true;

        [Tooltip("调试文字颜色")]
        public Color GizmosColor = new(0, 1, 0, 0.3f);

        // 运行时数据
        [NonSerialized] public Vector2 Position;
        [NonSerialized] public Vector2 ExternalForce;
        [NonSerialized] public bool IsRegistered;
        public float MaxOverlapDistance => Radius * 2f * MaxOverlapRatio;// 计算实际可重叠的最大距离
        public float EffectiveRadius => Radius * (1f - MaxOverlapRatio);// 计算有效半径（考虑最大重叠）
        public float CollisionMass => Mass * PushForceWeight;// 碰撞质量（考虑推力权重）

        public override Vector3 MovingPlatformSpeed
        {
            get
            {
                // if (_movingPlatform)
                    // return _movingPlatform.CurrentSpeed;

                return Vector3.zero;
            }
        }

        public Vector2 ColliderSize
        {
            get
            {
                if (_boxCollider) return _boxCollider.size;
                if (_circleCollider) return Vector2.one * _circleCollider.radius;
                return Vector2.zero;
            }
            set
            {
                if (_boxCollider) _boxCollider.size = value;
                if (_circleCollider) _circleCollider.radius = value.x;
            }
        }

        public Vector2 ColliderOffset
        {
            get
            {
                if (_boxCollider) return _boxCollider.offset;
                if (_circleCollider) return _circleCollider.offset;
                return Vector2.zero;
            }
            set
            {
                if (_boxCollider) _boxCollider.offset = value;
                if (_circleCollider) _circleCollider.offset = value;
            }
        }

        public BoxCollider2D boxCollider => _boxCollider;

        protected BoxCollider2D _boxCollider;
        protected CircleCollider2D _circleCollider;
        protected Vector2 _originalColliderSize;
        protected Vector3 _originalColliderCenter;

        protected RaycastHit2D _raycastUp,_raycastDown, _raycastLeft, _raycastRight;

        protected override void Awake()
        {
            base.Awake();
            TryGetComponent(out _boxCollider);
            TryGetComponent(out _circleCollider);
            _originalColliderSize = ColliderSize;
            _originalColliderCenter = ColliderOffset;
            
            Position = transform.position;
            Velocity = Vector2.zero;
            ExternalForce = Vector2.zero;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            VolumeManager.Instance.Register(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            VolumeManager.Instance.Unregister(this);
        }

        /// <summary>
        /// Determines whether this character is grounded
        /// </summary>
        protected override void CheckIfGrounded()
        {
            Grounded = true;
            JustGotGrounded = !_groundedLastFrame && Grounded;
            _groundedLastFrame = Grounded;
        }

        protected override void Update()
        {
            base.Update();
            Position = transform.position;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            var dt = Time.deltaTime;
            ApplyVelocity(dt);
        }
        
        /// <summary>
        /// 施加速度到位置
        /// </summary>
        protected virtual void ApplyVelocity(float dt)
        {
            Position += (Vector2)Velocity * dt;
            transform.position = Position;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (IsPlayer)
            {
                Vector2 targetVel = CurrentMovement;
                Velocity = Vector2.Lerp(Velocity, targetVel, Time.fixedDeltaTime * 10f);
                Position += (Vector2)Velocity * Time.fixedDeltaTime;
                transform.position = Position;
            }
            else
            {
                Position += (Vector2)Velocity * Time.fixedDeltaTime;
                transform.position = Position;
            }
        }

        /// <summary>
        /// Another way to add a force of the specified force and direction
        /// </summary>
        public override void AddImpact(Vector3 direction, float force)
        {
            if (force <= 0) 
                return;

            float reducedForce = force * (1f - KnockbackResistance);
            if (reducedForce > 0)
            {
                Velocity += direction.normalized * reducedForce;
            }
        }

        /// <summary>
        /// Adds a force of the specified vector
        /// </summary>
        public override void AddForce(Vector3 force)
        {
            ExternalForce += (Vector2)force;
        }

        /// <summary>
        /// Sets the current movement
        /// </summary>
        /// <param name="movement"></param>
        public override void SetMovement(Vector3 movement)
        {
            movement.y = movement.z;
            movement.z = 0;
            CurrentMovement = movement;
        }

        public override void MovePosition(Vector3 newPosition)
        {
            Position = newPosition;
        }

        public override void SetPosition(Vector3 newPosition)
        {
            Position = newPosition;
        }

        /// <summary>
        /// Resizes the collider to the new size set in parameters
        /// </summary>
        /// <param name="newHeight">New size.</param>
        /// <param name="translateCenter"></param>
        public override void ResizeColliderHeight(float newHeight, bool translateCenter = false)
        {
            float newYOffset = _originalColliderCenter.y - (_originalColliderSize.y - newHeight) / 2;
            Vector2 newSize = ColliderSize;
            newSize.y = newHeight;
            ColliderSize = newSize;
            ColliderOffset = newYOffset * Vector3.up;
        }

        /// <summary>
        /// Returns the collider to its initial size
        /// </summary>
        public override void ResetColliderSize()
        {
            ColliderSize = _originalColliderSize;
            ColliderOffset = _originalColliderCenter;
        }

        /// <summary>
        /// Determines the controller's current direction
        /// </summary>
        protected override void DetermineDirection()
        {
            if (CurrentMovement != Vector3.zero)
            {
                CurrentDirection = CurrentMovement.normalized;
            }
        }

        /// <summary>
        /// Sets a moving platform to this controller
        /// </summary>
        /// <param name="platform"></param>
        // public virtual void SetMovingPlatform(MovingPlatform2D platform)
        // {
        //     _movingPlatform = platform;
        // }

        /// <summary>
        /// Sets this rigidbody as kinematic
        /// </summary>
        /// <param name="state"></param>
        public override void SetKinematic(bool state)
        {
        }

        /// <summary>
        /// Enables the collider
        /// </summary>
        public override void CollisionsOn()
        {
            if (_boxCollider) _boxCollider.enabled = true;
            if (_circleCollider) _circleCollider.enabled = true;
        }

        /// <summary>
        /// Disables the collider
        /// </summary>
        public override void CollisionsOff()
        {
            if (_boxCollider) _boxCollider.enabled = false;
            if (_circleCollider) _circleCollider.enabled = false;
        }

        /// <summary>
        /// On reset, we reset our rb's velocity
        /// </summary>
        public override void Reset()
        {
            base.Reset();
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            if (!ShowGizmos)
                return;

            Gizmos.color = GizmosColor;
            Gizmos.DrawWireSphere(transform.position, Radius);

            Color overlapColor = new Color(1, 0, 0, 0.2f);
            Gizmos.color = overlapColor;
            Gizmos.DrawWireSphere(transform.position, Radius * (1f - MaxOverlapRatio));
        }
    }
}