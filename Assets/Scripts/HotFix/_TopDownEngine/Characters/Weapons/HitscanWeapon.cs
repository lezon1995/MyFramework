using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    public class HitscanWeapon : Weapon
    {
        /// the possible modes this weapon laser sight can run on, 3D by default
        public enum Modes
        {
            TwoD,
            ThreeD
        }

        [MMInspectorGroup("Hitscan Spawn")] [Tooltip("the offset position at which the projectile will spawn")]
        public Vector3 ProjectileSpawnOffset = Vector3.zero;

        [Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
        public Vector3 Spread = Vector3.zero;

        [Tooltip("whether or not the weapon should rotate to align with the spread angle")]
        public bool RotateWeaponOnSpread;

        [Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
        public bool RandomSpread = true;

        [ShowInInspector, ReadOnly]
        [Tooltip("the projectile's spawn position")]
        public Vector3 SpawnPosition { get; set; }

        [MMInspectorGroup("Hitscan")] [Tooltip("whether this hitscan should work in 2D or 3D")]
        public Modes Mode = Modes.ThreeD;

        [Tooltip("the layer(s) on which to hitscan ray should collide")]
        public LayerMask HitscanTargetLayers;

        [Tooltip("the maximum distance of this weapon, after that bullets will be considered lost")]
        public float HitscanMaxDistance = 100f;

        [Tooltip("the duration of the invincibility after a hit (to prevent instant death in the case of rapid fire)")]
        public float DamageCausedInvincibilityDuration = 0.2f;

        [MMInspectorGroup("Knockback")] [Tooltip("the type of knockback to apply when causing damage")]
        public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.None;

        [Tooltip("The force to apply to the object that gets damaged")]
        public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 10);

        [MMInspectorGroup("Hit Damageable")] [Tooltip("a MMFeedbacks to move to the position of the hit and to play when hitting something with a Health component")]
        public MMFeedbacks HitDamageable;

        [Tooltip("a particle system to move to the position of the hit and to play when hitting something with a Health component")]
        public ParticleSystem DamageableImpactParticles;

        [MMInspectorGroup("Hit Non Damageable")] [Tooltip("a MMFeedbacks to move to the position of the hit and to play when hitting something without a Health component")]
        public MMFeedbacks HitNonDamageable;

        [Tooltip("a particle system to move to the position of the hit and to play when hitting something without a Health component")]
        public ParticleSystem NonDamageableImpactParticles;

        protected Vector3 _flippedProjectileSpawnOffset;
        protected Vector3 _randomSpreadDirection;
        protected Transform _projectileSpawnTransform;

        protected Vector3 _destination;
        protected Vector3 _direction;
        protected GameObject _hitObject;
        protected Vector3 _hitPoint;
        protected Health _health;
        protected Vector3 _damageDirection;
        protected Vector3 _knockbackRelativePosition = Vector3.zero;
        protected TopDownController _knockbackTopDownController;

        protected RaycastHit _hit { get; set; }
        protected RaycastHit2D _hit2D { get; set; }
        protected Vector3 _origin { get; set; }

        [MMInspectorButton("TestShoot")] public bool TestShootButton;

        protected virtual void TestShoot()
        {
            if (State.Is(States.Idle))
            {
                WeaponInputStart();
            }
            else
            {
                WeaponInputStop();
            }
        }

        /// <summary>
        /// Initialize this weapon
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();

            if (FlipWeaponOnCharacterFlip)
            {
                _flippedProjectileSpawnOffset = ProjectileSpawnOffset;
                _flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
            }
        }

        /// <summary>
        /// Called everytime the weapon is used
        /// </summary>
        public override void WeaponUse()
        {
            base.WeaponUse();

            // DetermineSpawnPosition();
            // DetermineDirection();
            // SpawnProjectile(SpawnPosition);
            HandleDamage();
        }

        /// <summary>
        /// Determines the direction of the ray we have to cast
        /// </summary>
        protected virtual void DetermineDirection()
        {
            if (RandomSpread)
            {
                var x = Random.Range(-Spread.x, Spread.x);
                var y = Random.Range(-Spread.y, Spread.y);
                var z = Random.Range(-Spread.z, Spread.z);
                _randomSpreadDirection = new Vector3(x, y, z);
            }
            else
            {
                _randomSpreadDirection = Vector3.zero;
            }

            Quaternion spread = Quaternion.Euler(_randomSpreadDirection);
            _randomSpreadDirection = spread * transform.right * (Flipped ? -1 : 1);

            if (RotateWeaponOnSpread)
            {
                transform.rotation *= spread;
            }
        }

        /// <summary>
        /// Spawns a new object and positions/resizes it
        /// </summary>
        public virtual void SpawnProjectile(Vector3 spawnPosition, bool triggerObjectActivation = true)
        {
            _hitObject = null;

            switch (Mode)
            {
                case Modes.ThreeD:
                    _origin = SpawnPosition;
                    _hit = MMDebug.Raycast3D(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);

                    // if we've hit something, our destination is the raycast hit
                    if (_hit.transform)
                    {
                        _hitObject = _hit.collider.gameObject;
                        _hitPoint = _hit.point;
                    }
                    // otherwise we just draw our laser in front of our weapon 
                    else
                    {
                        _hitObject = null;
                    }

                    break;
                case Modes.TwoD:
                    //_direction = Flipped ? Vector3.left : Vector3.right;
                    // we cast a ray in front of the weapon to detect an obstacle
                    _origin = SpawnPosition;
                    _hit2D = MMDebug.RayCast(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
                    if (_hit2D)
                    {
                        _hitObject = _hit2D.collider.gameObject;
                        _hitPoint = _hit2D.point;
                    }
                    // otherwise we just draw our laser in front of our weapon 
                    else
                    {
                        _hitObject = null;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles damage and the associated feedbacks
        /// </summary>
        protected virtual void HandleDamage()
        {
            if (_aimTarget == null)
                return;

            _hitObject = _aimTarget.gameObject;

            if (_hitObject == null)
                return;

            if (_hitObject.TryGetComponent(out _health))
            {
                // hit damageable
                _damageDirection = (_hitObject.transform.position - transform.position).normalized;

                var dmg = Dmg;
                _health.Damage(ref dmg, gameObject, Owner, DamageCausedInvincibilityDuration, _damageDirection);

                if (HitDamageable)
                {
                    HitDamageable.transform.position = _hitPoint;
                    HitDamageable.transform.LookAt(transform);
                    HitDamageable.Play();
                }

                if (DamageableImpactParticles)
                {
                    DamageableImpactParticles.transform.position = _hitPoint;
                    DamageableImpactParticles.transform.LookAt(transform);
                    DamageableImpactParticles.Play();
                }

                ApplyKnockback();
            }
            else
            {
                // hit non damageable
                if (HitNonDamageable)
                {
                    HitNonDamageable.transform.position = _hitPoint;
                    HitNonDamageable.transform.LookAt(transform);
                    HitNonDamageable.Play();
                }

                if (NonDamageableImpactParticles)
                {
                    NonDamageableImpactParticles.transform.position = _hitPoint;
                    NonDamageableImpactParticles.transform.LookAt(transform);
                    NonDamageableImpactParticles.Play();
                }
            }
        }

        /// <summary>
        /// Applies knockback to the hit target if necessary
        /// </summary>
        protected virtual void ApplyKnockback()
        {
            if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
            {
                if (_hitObject.TryGetComponent(out _knockbackTopDownController))
                {
                    var knockbackForce = DamageCausedKnockbackForce * _health.KnockbackForceMultiplier;
                    _health.ComputeKnockbackForce(ref knockbackForce);
                    switch (Mode)
                    {
                        case Modes.ThreeD:
                            _knockbackRelativePosition = _hitPoint - Owner.transform.position;
                            knockbackForce = Quaternion.LookRotation(_knockbackRelativePosition) * knockbackForce;
                            break;
                        case Modes.TwoD:
                            _knockbackRelativePosition = _hitPoint - Owner.transform.position;
                            knockbackForce = Vector3.RotateTowards(knockbackForce, _knockbackRelativePosition.normalized, 10f, 0f);
                            break;
                    }

                    _knockbackTopDownController.AddImpact(knockbackForce.normalized, knockbackForce.magnitude);
                }
            }
        }

        /// <summary>
        /// Determines the spawn position based on the spawn offset and whether the weapon is flipped
        /// </summary>
        public virtual void DetermineSpawnPosition()
        {
            if (Flipped)
            {
                if (FlipWeaponOnCharacterFlip)
                {
                    SpawnPosition = transform.position - transform.rotation * _flippedProjectileSpawnOffset;
                }
                else
                {
                    SpawnPosition = transform.position - transform.rotation * ProjectileSpawnOffset;
                }
            }
            else
            {
                SpawnPosition = transform.position + transform.rotation * ProjectileSpawnOffset;
            }

            if (WeaponUseTransform)
            {
                SpawnPosition = WeaponUseTransform.position;
            }
        }

        /// <summary>
        /// When the weapon is selected, draws a circle at the spawn's position
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            DetermineSpawnPosition();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
        }
    }
}