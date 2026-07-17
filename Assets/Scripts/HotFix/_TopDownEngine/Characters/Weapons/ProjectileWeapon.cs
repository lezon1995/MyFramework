using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    /// <summary>
    /// A weapon class aimed specifically at allowing the creation of various projectile weapons, from shotgun to machine gun, via plasma gun or rocket launcher
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/ProjectileWeapon")]
    public class ProjectileWeapon : Weapon, IEvent<TopDownEngineEvent>
    {
        [MMInspectorGroup("Projectiles")] [Tooltip("the offset position at which the projectile will spawn")]
        public Vector3 ProjectileSpawnOffset;

        [Tooltip("in the absence of a character owner, the default direction of the projectiles")]
        public Vector3 DefaultProjectileDirection = Vector3.forward;

        [Tooltip("the number of projectiles to spawn per shot")]
        public int ProjectilesPerShot = 1;

        [Header("Spawn Transforms")] [Tooltip("a list of transforms that can be used a spawn points, instead of the ProjectileSpawnOffset. Will be ignored if left emtpy")]
        public List<Transform> SpawnTransforms = new();

        /// a list of modes the spawn transforms can operate on
        public enum SpawnTransformsModes
        {
            Random,
            Sequential
        }

        [Tooltip("the selected mode for spawn transforms. Sequential will go through the list sequentially, while Random will pick a random one every shot")]
        public SpawnTransformsModes SpawnTransformsMode = SpawnTransformsModes.Sequential;

        [Header("Spread")] [Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
        public Vector3 Spread;

        [Tooltip("whether or not the weapon should rotate to align with the spread angle")]
        public bool RotateWeaponOnSpread;

        [Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
        public bool RandomSpread = true;

        [ShowInInspector, ReadOnly]
        [Tooltip("the projectile's spawn position")]
        public Vector3 SpawnPosition { get; set; }

        [Tooltip("the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object")]
        public MMObjectPooler ObjectPooler;

        [Header("Spawn Feedbacks")] public List<MMFeedbacks> SpawnFeedbacks = new();

        protected Vector3 _flippedProjectileSpawnOffset;
        protected Vector3 _randomSpreadDirection;
        protected bool _poolInitialized;
        protected Transform _projectileSpawnTransform;
        protected int _spawnArrayIndex;

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
            _weaponAim = GetComponent<WeaponAim>();

            if (!_poolInitialized)
            {
                if (ObjectPooler == null)
                    ObjectPooler = GetComponent<MMObjectPooler>();

                if (ObjectPooler == null)
                {
                    Debug.LogWarning(name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
                    return;
                }

                if (FlipWeaponOnCharacterFlip)
                {
                    _flippedProjectileSpawnOffset = ProjectileSpawnOffset;
                    _flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
                }

                _poolInitialized = true;
            }
        }

        /// <summary>
        /// Called everytime the weapon is used
        /// </summary>
        public override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();

            for (int i = 0; i < ProjectilesPerShot; i++)
            {
                SpawnProjectile(SpawnPosition, i, ProjectilesPerShot);
                PlaySpawnFeedbacks();
            }
        }

        /// <summary>
        /// Spawns a new object and positions/resizes it
        /// </summary>
        public virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            // we get the next object in the pool and make sure it's not null
            var nextGameObject = ObjectPooler.GetPooledGameObject();

            // mandatory checks
            if (nextGameObject == null)
                return null;

            // we position the object
            nextGameObject.transform.position = spawnPosition;
            if (_projectileSpawnTransform)
            {
                nextGameObject.transform.position = _projectileSpawnTransform.position;
            }

            // we activate the object
            nextGameObject.SetActive(true);

            var success = nextGameObject.TryGetComponent<Projectile>(out var projectile);
            if (success)
            {
                projectile.SetWeapon(this);
                if (Owner)
                {
                    projectile.SetOwner(Owner.gameObject);
                    projectile.SetDamage(Dmg);
                }

                projectile.SetTarget(_aimTarget);
            }

            if (success)
            {
                if (RandomSpread)
                {
                    var x = Random.Range(-Spread.x, Spread.x);
                    var y = Random.Range(-Spread.y, Spread.y);
                    var z = Random.Range(-Spread.z, Spread.z);
                    _randomSpreadDirection = new(x, y, z);
                }
                else
                {
                    if (totalProjectiles > 1)
                    {
                        var dir = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread, Spread);
                        _randomSpreadDirection = dir;
                    }
                    else
                    {
                        _randomSpreadDirection = Vector3.zero;
                    }
                }

                var spread = Quaternion.Euler(_randomSpreadDirection);
                if (Owner == null)
                {
                    projectile.SetDirection(spread * transform.rotation * DefaultProjectileDirection, transform.rotation);
                }
                else
                {
                    Vector3 newDirection = spread * transform.right * (Flipped ? -1 : 1);
                    if (Owner.Orientation2D)
                    {
                        projectile.SetDirection(newDirection, spread * transform.rotation, Owner.Orientation2D.IsFacingRight);
                    }
                    else
                    {
                        projectile.SetDirection(newDirection, spread * transform.rotation);
                    }
                }

                if (RotateWeaponOnSpread)
                {
                    transform.rotation *= spread;
                }
            }

            return nextGameObject;
        }

        /// <summary>
        /// This method is in charge of playing feedbacks on projectile spawn
        /// </summary>
        protected virtual void PlaySpawnFeedbacks()
        {
            if (SpawnFeedbacks.Count > 0)
            {
                SpawnFeedbacks[_spawnArrayIndex].Play();
            }

            _spawnArrayIndex++;
            if (_spawnArrayIndex >= SpawnTransforms.Count)
            {
                _spawnArrayIndex = 0;
            }
        }

        /// <summary>
        /// Sets a forced projectile spawn position
        /// </summary>
        /// <param name="newSpawnTransform"></param>
        public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
        {
            _projectileSpawnTransform = newSpawnTransform;
        }

        /// <summary>
        /// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
        /// </summary>
        public virtual void DetermineSpawnPosition()
        {
            var position = Flipped switch
            {
                true => FlipWeaponOnCharacterFlip switch
                {
                    true => transform.position - transform.rotation * _flippedProjectileSpawnOffset,
                    false => transform.position - transform.rotation * ProjectileSpawnOffset
                },
                false => transform.position + transform.rotation * ProjectileSpawnOffset
            };

            if (WeaponUseTransform)
                position = WeaponUseTransform.position;

            if (SpawnTransforms.Count > 0)
            {
                switch (SpawnTransformsMode)
                {
                    case SpawnTransformsModes.Random:
                        _spawnArrayIndex = Random.Range(0, SpawnTransforms.Count);
                        position = SpawnTransforms[_spawnArrayIndex].position;
                        break;
                    case SpawnTransformsModes.Sequential:
                    default:
                        position = SpawnTransforms[_spawnArrayIndex].position;
                        break;
                }
            }

            SpawnPosition = position;
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

        public void onEvent(TopDownEngineEvent e)
        {
            switch (e.EventType)
            {
                case TopDownEngineEventTypes.LevelStart:
                    _poolInitialized = false;
                    Initialization();
                    break;
            }
        }

        /// <summary>
        /// On enable we start listening for events
        /// </summary>
        protected virtual void OnEnable()
        {
            this.addListener<TopDownEngineEvent>();
        }

        /// <summary>
        /// On disable we stop listening for events
        /// </summary>
        protected virtual void OnDisable()
        {
            this.removeListener<TopDownEngineEvent>();
        }
    }
}