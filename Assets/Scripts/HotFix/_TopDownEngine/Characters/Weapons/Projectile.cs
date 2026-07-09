using System.Collections.Generic;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Projectile class to be used along with projectile weapons
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Projectile")]
    public class Projectile : MMPoolableObject
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

        [Tooltip("if FaceMovement is true, the projectile's vector specified below will be aligned to the movement vector, usually you'll want to go with Forward in 3D, Right in 2D")]
        [ShowIf(nameof(FaceMovement))]
        public MovementVectors MovementVector = MovementVectors.Forward;

        public UpdateModes UpdateMode = UpdateModes.FixedUpdate;

        [Tooltip("the speed of the object (relative to the level's speed), 米/秒")]
        public UnitLength Speed;

        [Tooltip("the acceleration of the object over time. Starts accelerating on enable.")]
        public float Acceleration;

        [Tooltip("the current direction of the object")]
        public Vector3 Direction = Vector3.left;

        [ReadOnly, ShowInInspector]
        public Vector3 CurDirection { get; set; }

        [Tooltip("if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.")]
        public bool DirectionCanBeChangedBySpawner = true;

        [Tooltip("the flip factor to apply if and when the projectile is mirrored")]
        public Vector3 FlipValue = new Vector3(-1, 1, 1);

        [Tooltip("set this to true if your projectile's model (or sprite) is facing right, false otherwise")]
        public bool ProjectileIsFacingRight = true;

        [Header("Spawn")]
        [MMInformation("Here you can define an initial delay (in seconds) during which this object won't take or cause damage. This delay starts when the object gets enabled. You can also define whether the projectiles should damage their owner (think rockets and the likes) or not")]
        [Tooltip("the initial delay during which the projectile can't be destroyed")]
        public float InitialInvulnerabilityDuration;

        [Tooltip("should the projectile damage its owner?")]
        public bool DamageOwner;

        public DamageOnTouch TargetDamageOnTouch => _damageOnTouch;
        public Weapon SourceWeapon => _weapon;
        public GameObject Owner => _owner;

        protected Weapon _weapon;
        protected GameObject _owner;
        protected Transform _target;
        protected Health _targetHealth;
        protected Vector3 _movement;
        protected float _initialSpeed;
        protected SpriteRenderer _spriteRenderer;
        protected DamageOnTouch _damageOnTouch;
        protected Collider _collider;
        protected Collider2D _collider2D;
        protected Rigidbody _rigidBody;
        protected Rigidbody2D _rigidBody2D;
        protected bool _facingRightInitially;
        protected bool _initialFlipX;
        protected Vector3 _startPosition;
        protected Vector3 _initialLocalScale;
        protected Vector3 _initialDirection;
        protected bool _shouldMove = true;
        protected Health _health;
        protected bool _spawnerIsFacingRight;

        CoroutineHandle coroutineInvulnerability;

        /// <summary>
        /// On awake, we store the initial speed of the object 
        /// </summary>
        protected virtual void Awake()
        {
            _facingRightInitially = ProjectileIsFacingRight;
            _initialSpeed = Speed;
            if (TryGetComponent(out _health))
            {
                _health.Event.addListener(this);
            }

            TryGetComponent(out _collider);
            TryGetComponent(out _collider2D);
            if (TryGetComponent(out _spriteRenderer))
            {
                _initialFlipX = _spriteRenderer.flipX;
            }

            TryGetComponent(out _damageOnTouch);
            TryGetComponent(out _rigidBody);
            TryGetComponent(out _rigidBody2D);

            _initialLocalScale = transform.localScale;
        }

        /// <summary>
        /// Handles the projectile's initial invincibility
        /// </summary>
        /// <returns>The invulnerability.</returns>
        protected virtual IEnumerator<float> InitialInvulnerability()
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

            yield return Timing.WaitForSeconds(InitialInvulnerabilityDuration);
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

            if (_collider) _collider.enabled = true;
            if (_collider2D) _collider2D.enabled = true;
            InUse = true;
        }

        /// <summary>
        /// On FixedUpdate(), we move the object based on the level's speed and the object's speed, and apply acceleration
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                var dt = Time.fixedDeltaTime;
                Tick(dt);
            }
        }

        public void Tick(float dt)
        {
            if (_shouldMove)
            {
                Movement(dt);

                if (FaceMovement)
                {
                    FaceMovementDirection(CurDirection);
                }
            }
        }

        /// <summary>
        /// Handles the projectile's movement, every frame
        /// </summary>
        public virtual void Movement(float dt)
        {
            _movement = Direction * (Speed * dt);
            CurDirection = Direction;
            //transform.Translate(_movement,Space.World);
            if (_rigidBody)
                _rigidBody.MovePosition(transform.position + _movement);

            if (_rigidBody2D)
                _rigidBody2D.MovePosition(transform.position + _movement);

            // We apply the acceleration to increase the speed
            Speed += Acceleration * dt;
        }

        /// <summary>
        /// Sets the projectile's direction.
        /// </summary>
        /// <param name="spawnerIsFacingRight">If set to <c>true</c> spawner is facing right.</param>
        public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
        {
            _spawnerIsFacingRight = spawnerIsFacingRight;

            if (DirectionCanBeChangedBySpawner)
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

        void FaceMovementDirection(Vector3 newDirection)
        {
            switch (MovementVector)
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
                _targetHealth = target.GetComponent<Health>();
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
        public virtual void SetDamage(float baseDamage)
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
        public virtual void SetOwner(GameObject newOwner)
        {
            _owner = newOwner;
            if (TryGetComponent<DamageOnTouch>(out var damageOnTouch))
            {
                damageOnTouch.SetOwner(newOwner);
                if (!DamageOwner)
                {
                    damageOnTouch.ClearIgnore();
                    damageOnTouch.AddIgnore(newOwner);
                }
            }
        }

        /// <summary>
        /// On death, disables colliders and prevents movement
        /// </summary>
        public virtual void StopAt()
        {
            if (_collider) _collider.enabled = false;
            if (_collider2D) _collider2D.enabled = false;
            _shouldMove = false;
            InUse = false;
        }

        /// <summary>
        /// On death, we stop our projectile
        /// </summary>
        public virtual void onEvent(OnDeath e)
        {
            StopAt();
        }

        /// <summary>
        /// On enable, we trigger a short invulnerability
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Initialization();

            Timing.KillCoroutines(ref coroutineInvulnerability);

            if (InitialInvulnerabilityDuration > 0)
                coroutineInvulnerability = Timing.RunCoroutine(InitialInvulnerability());

            _startPosition = transform.position;
        }

        protected virtual void OnDestroy()
        {
            if (_health)
                _health.Event.removeListener(this);
        }
    }
}