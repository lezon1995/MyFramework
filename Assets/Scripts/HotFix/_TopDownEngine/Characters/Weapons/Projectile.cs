using System.Collections.Generic;
using Drawing;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Projectile class to be used along with projectile weapons
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Projectile")]
    public class Projectile : MainActorBehaviour
        , IEvent<OnDeath>
    {
        public enum UpdateModes
        {
            FixedUpdate,
            ManualUpdate,
        }

        public enum MovementVectors
        {
            Forward,
            Right,
            Up
        }

        [Header("Movement")]
        [Tooltip("if true, the projectile will rotate at initialization towards its rotation")]
        public bool FaceDirection = true;

        [Tooltip("if true, the projectile will rotate towards movement")]
        public bool FaceMovement;

        public bool ManuallyColliding;

        [Tooltip("if FaceMovement is true, the projectile's vector specified below will be aligned to the movement vector, usually you'll want to go with Forward in 3D, Right in 2D")]
        [ShowIf(nameof(FaceMovement))]
        public MovementVectors movementVector = MovementVectors.Right;

        [Tooltip("the speed of the object (relative to the level's speed), 米/秒")]
        public UnitLength Speed;

        public ValueModifier SpeedModifier { get; set; }

        public float moveSpeed
        {
            get
            {
                float speed = Speed;
                return SpeedModifier.SafeInvoke(ref speed);
            }
        }

        [Tooltip("the acceleration of the object over time. Starts accelerating on enable.")]
        public float Acceleration;

        [Tooltip("the current direction of the object")]
        public Vector3 Direction;

        [Tooltip("the flip factor to apply if and when the projectile is mirrored")]
        public Vector3 FlipValue = new(-1, 1, 1);

        [Tooltip("set this to true if your projectile's model (or sprite) is facing right, false otherwise")]
        public bool ProjectileIsFacingRight = true;

        [Header("Spawn")]
        [MMInformation("Here you can define an initial delay (in seconds) during which this object won't take or cause damage. This delay starts when the object gets enabled. You can also define whether the projectiles should damage their owner (think rockets and the likes) or not")]
        [Tooltip("the initial delay during which the projectile can't be destroyed")]
        public float InitialInvincibleDuration;

        [Tooltip("should the projectile damage its owner?")]
        public bool DamageOwner;

        public DamageOnTouch DamageOnTouch => _damageOnTouch;
        public Weapon SourceWeapon => _weapon;
        public Character Owner => _owner;
        public Stats Stats => _stats;
        public Buffable Buffable => _buffable;
        public Health Health => _health;
        public TrailRenderer Trail => _trailRenderer;

        protected Weapon _weapon;
        protected Character _owner;
        protected Transform _target;
        protected Health _targetHealth;
        protected Vector3 _movement;
        protected float _initialSpeed;
        protected SpriteRenderer _spriteRenderer;
        protected DamageOnTouch _damageOnTouch;
        protected Collider2D _collider2D;
        protected bool _hasCollider2D;
        protected Rigidbody2D _rigidBody2D;
        protected bool _hasRigidBody2D;
        protected bool _facingRightInitially;
        protected bool _initialFlipX;
        protected Vector3 _startPosition;
        protected Vector3 _initialLocalScale;
        protected Vector3 _initialDirection;
        public Vector3 prePos, curPos, correctPos;
        protected RaycastHit2D willPassingThroughHit;
        protected bool willPassingThroughThisFrame;
        protected bool _shouldMove = true;
        protected Health _health;
        protected Stats _stats;
        protected bool _hasStats;
        protected Buffable _buffable;
        protected TrailRenderer _trailRenderer;
        protected bool _spawnerIsFacingRight;

        CoroutineHandle coroutineInvincible;

        /// <summary>
        /// On awake, we store the initial speed of the object 
        /// </summary>
        protected override void OnAwake()
        {
            base.OnAwake();
            _facingRightInitially = ProjectileIsFacingRight;
            _initialSpeed = Speed;
            if (TryGetComponent(out _health))
            {
                _health.Event.addListener(this);
            }

            _hasCollider2D = TryGetComponent(out _collider2D);
            if (TryGetComponent(out _spriteRenderer))
            {
                _initialFlipX = _spriteRenderer.flipX;
            }

            TryGetComponent(out _damageOnTouch);
            _hasStats = TryGetComponent(out _stats);
            TryGetComponent(out _buffable);
            _hasRigidBody2D = TryGetComponent(out _rigidBody2D);
            this.TryGetComponentInChildren(out _trailRenderer);
            _initialLocalScale = transform.localScale;
            OnStatsSet();
        }

        protected virtual void OnStatsSet()
        {
        }

        /// <summary>
        /// Handles the projectile's initial invincibility
        /// </summary>
        protected virtual IEnumerator<float> InitialInvincible()
        {
            var damageOnTouch = _damageOnTouch;
            if (damageOnTouch == null)
                yield break;

            if (_weapon == null)
                yield break;

            damageOnTouch.ClearIgnore();
            if (_weapon.Owner)
            {
                damageOnTouch.AddIgnore(_weapon.Owner.gameObject);
            }

            yield return Timing.WaitForSeconds(InitialInvincibleDuration);
            if (DamageOwner)
            {
                damageOnTouch.RemoveIgnore(_weapon.Owner.gameObject);
            }
        }

        /// <summary>
        /// Initializes the projectile
        /// </summary>
        protected virtual void Initialization()
        {
            Speed = _initialSpeed;
            ProjectileIsFacingRight = _facingRightInitially;
            if (_spriteRenderer)
            {
                _spriteRenderer.flipX = _initialFlipX;
            }

            transform.localScale = _initialLocalScale;
            _shouldMove = true;
            _damageOnTouch?.InitializeFeedbacks();

            if (_hasCollider2D)
                _collider2D.enabled = true;

            inUse = true;
        }

        /// <summary>
        /// On FixedUpdate(), we move the object based on the level's speed and the object's speed, and apply acceleration
        /// </summary>
        protected override void FixedUpdate()
        {
            if (mNeedFixedUpdate)
            {
                var dt = Time.fixedDeltaTime;
                OnFixedUpdate(dt);
            }
        }

        public override void OnFixedUpdate(float dt)
        {
            if (_shouldMove)
            {
                Movement(dt);

                if (FaceMovement)
                {
                    FaceMovementDirection(Direction);
                }
            }
        }

        protected virtual void CollidingManually(GameObject hitObject, Vector2 hitNormal, Vector2 hitPoint)
        {
        }

        /// <summary>
        /// Handles the projectile's movement, every frame
        /// </summary>
        public virtual void Movement(float dt)
        {
            _movement = Direction * (moveSpeed * dt);

            if (_hasRigidBody2D)
            {
                prePos = transform.position;
                _rigidBody2D.MovePosition(transform.position + _movement);
            }

            // We apply the acceleration to increase the speed
            Speed += Acceleration * dt;
        }

        public virtual void MovementTo(Vector3 pos)
        {
            if (_hasRigidBody2D)
            {
                prePos = transform.position;
                _rigidBody2D.MovePosition(pos);
            }
        }

        /// <summary>
        /// Sets the projectile's direction.
        /// </summary>
        /// <param name="spawnerIsFacingRight">If set to <c>true</c> spawner is facing right.</param>
        public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
        {
            _spawnerIsFacingRight = spawnerIsFacingRight;

            Direction = newDirection;

            if (ProjectileIsFacingRight != spawnerIsFacingRight)
                Flip();

            if (FaceDirection)
                transform.rotation = newRotation;

            _damageOnTouch?.SetKnockbackScriptDirection(newDirection);

            if (FaceMovement)
            {
                FaceMovementDirection(newDirection);
            }
        }

        protected virtual void FaceMovementDirection(Vector3 newDirection)
        {
            switch (movementVector)
            {
                case MovementVectors.Forward:
                    transform.forward = newDirection;
                    break;
                case MovementVectors.Right:
                    transform.right = newDirection;
                    break;
                case MovementVectors.Up:
                    transform.up = newDirection;
                    break;
            }
        }

        /// <summary>
        /// Flip the projectile
        /// </summary>
        protected virtual void Flip()
        {
            if (_spriteRenderer)
                _spriteRenderer.flipX = !_spriteRenderer.flipX;
            else
                transform.localScale = Vector3.Scale(transform.localScale, FlipValue);
        }

        /// <summary>
        /// Flip the projectile
        /// </summary>
        protected virtual void Flip(bool state)
        {
            if (_spriteRenderer)
                _spriteRenderer.flipX = state;
            else
                transform.localScale = Vector3.Scale(transform.localScale, FlipValue);
        }

        /// <summary>
        /// Sets the projectile's parent weapon.
        /// </summary>
        /// <param name="newWeapon">New weapon.</param>
        public virtual void SetWeapon(Weapon newWeapon)
        {
            _weapon = newWeapon;
            if (_weapon && _weapon.Stats)
            {
            }
        }

        /// <summary>
        /// Sets the projectile's attacking target
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetTarget(Transform target)
        {
            if (target)
            {
                _target = target;
                target.TryGetComponent(out _targetHealth);
            }
            else
            {
                _target = null;
                _targetHealth = null;
            }
        }

        /// <summary>
        /// Sets the damage caused by the projectile's DamageOnTouch to the specified value
        /// </summary>
        /// <param name="baseDamage"></param>
        public virtual void SetDamage(int baseDamage)
        {
            if (_damageOnTouch)
                _damageOnTouch.Dmg.Value = baseDamage;
        }

        public virtual void SetDamage(Dmg dmg)
        {
            if (_damageOnTouch)
                _damageOnTouch.SetDmg(dmg);
        }

        /// <summary>
        /// Sets the projectile's owner.
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(Character newOwner)
        {
            _owner = newOwner;
            if (TryGetComponent<DamageOnTouch>(out var damageOnTouch))
            {
                damageOnTouch.SetOwner(newOwner);
                if (!DamageOwner)
                {
                    damageOnTouch.ClearIgnore();
                    damageOnTouch.AddIgnore(newOwner.gameObject);
                }
            }
        }

        /// <summary>
        /// On death, disables colliders and prevents movement
        /// </summary>
        public virtual void StopAt()
        {
            if (_hasCollider2D)
                _collider2D.enabled = false;

            _shouldMove = false;
            inUse = false;
        }

        public void TryClearTrails()
        {
            if (_trailRenderer)
                _trailRenderer.Clear();
        }

        /// <summary>
        /// On death, we stop our projectile
        /// </summary>
        public virtual void onEvent(OnDeath e)
        {
            StopAt();
        }

        /// <summary>
        /// On enable, we trigger a short invincible
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Initialization();

            Timing.KillCoroutines(ref coroutineInvincible);

            if (InitialInvincibleDuration > 0)
                coroutineInvincible = Timing.RunCoroutine(InitialInvincible());

            _startPosition = transform.position;
        }

        protected override void OnDestroy()
        {
            if (_health)
                _health.Event.removeListener(this);

            base.OnDestroy();
        }
    }
}