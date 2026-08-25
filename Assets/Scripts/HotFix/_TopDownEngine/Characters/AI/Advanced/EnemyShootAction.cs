using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An Action that shoots using the currently equipped weapon. If your weapon is in auto mode, will shoot until you exit the state, and will only shoot once in SemiAuto mode. You can optionally have the character face (left/right) the target, and aim at it (if the weapon has a WeaponAim component).
    /// </summary>
    //[RequireComponent(typeof(CharacterOrientation2D))]
    //[RequireComponent(typeof(CharacterHandleWeapon))]
    public class EnemyShootAction : EnemyAction
    {
        public enum AimOrigins
        {
            Transform,
            SpawnPoint
        }

        [Header("Binding")]
        [Tooltip("the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;

        [Header("Behaviour")]
        [Tooltip("the origin we'll take into account when computing the aim direction towards the target")]
        public AimOrigins AimOrigin = AimOrigins.Transform;

        [Tooltip("if true, the Character will face the target (left/right) when shooting")]
        public bool FaceTarget = true;

        [Tooltip("if true the Character will aim at the target when shooting")]
        public bool AimAtTarget;

        [Tooltip("whether or not to only perform aim when in this state")]
        [MMCondition("AimAtTarget")]
        public bool OnlyAimWhenInState;

        protected CharacterOrientation2D _orientation2D;
        protected Character _character;
        protected WeaponAim _weaponAim;
        protected ProjectileWeapon _projectileWeapon;
        protected Vector3 _weaponAimDirection;
        protected bool _shooting;

        /// <summary>
        /// On init, we grab our CharacterHandleWeapon ability
        /// </summary>
        public override void Initialization()
        {
            if (!ShouldInitialize)
                return;

            base.Initialization();
            this.TryGetComponentInParent(out _character);
            _character?.FindAbility(out _orientation2D);
            if (TargetHandleWeaponAbility == null)
            {
                _character?.FindAbility(out TargetHandleWeaponAbility);
            }
        }

        /// <summary>
        /// On PerformAction we face and aim if needed, and we shoot
        /// </summary>
        /// <param name="dt"></param>
        public override void PerformAction(float dt)
        {
            MakeChangesToTheWeapon();
            TestFaceTarget();
            TestAimAtTarget();
            Shoot();
        }

        /// <summary>
        /// Sets the current aim if needed
        /// </summary>
        protected virtual void Update()
        {
            if (OnlyAimWhenInState && !_shooting)
                return;

            if (TargetHandleWeaponAbility == null)
                return;

            if (TargetHandleWeaponAbility.CurrentWeapon)
            {
                if (_weaponAim)
                {
                    if (_shooting)
                    {
                        _weaponAim.SetCurrentAim(_weaponAimDirection);
                    }
                    else
                    {
                        if (_orientation2D)
                        {
                            if (_orientation2D.IsFacingRight)
                                _weaponAim.SetCurrentAim(Vector3.right);
                            else
                                _weaponAim.SetCurrentAim(Vector3.left);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Makes changes to the weapon to ensure it works ok with AI scripts
        /// </summary>
        protected virtual void MakeChangesToTheWeapon()
        {
            if (TargetHandleWeaponAbility.CurrentWeapon)
            {
                TargetHandleWeaponAbility.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
            }
        }

        /// <summary>
        /// Faces the target if required
        /// </summary>
        protected virtual void TestFaceTarget()
        {
            if (!FaceTarget)
                return;

            if (_orientation2D)
            {
                if (transform.position.x > _brain.Target.position.x)
                    _orientation2D.FaceDirection(-1);
                else
                    _orientation2D.FaceDirection(1);
            }
        }

        /// <summary>
        /// Aims at the target if required
        /// </summary>
        protected virtual void TestAimAtTarget()
        {
            if (!AimAtTarget)
                return;

            if (TargetHandleWeaponAbility == null)
                return;

            if (TargetHandleWeaponAbility.CurrentWeapon)
            {
                if (_weaponAim == null)
                    TargetHandleWeaponAbility.CurrentWeapon.TryGetComponent(out _weaponAim);

                if (_weaponAim)
                {
                    if (AimOrigin == AimOrigins.SpawnPoint && _projectileWeapon)
                    {
                        _projectileWeapon.DetermineSpawnPosition();
                        _weaponAimDirection = _brain.Target.position - _projectileWeapon.SpawnPosition;
                    }
                    else
                    {
                        _weaponAimDirection = _brain.Target.position - _character.transform.position;
                    }
                }
            }
        }

        /// <summary>
        /// Activates the weapon
        /// </summary>
        protected virtual void Shoot()
        {
            TargetHandleWeaponAbility.ShootStart();
        }

        /// <summary>
        /// When entering the state we reset our shoot counter and grab our weapon
        /// </summary>
        public override void OnEnterState()
        {
            base.OnEnterState();
            _shooting = true;
            var weapon = TargetHandleWeaponAbility.CurrentWeapon;
            if (weapon)
            {
                weapon.TryGetComponent(out _weaponAim);
                weapon.TryGetComponent(out _projectileWeapon);
            }
        }

        /// <summary>
        /// When exiting the state we make sure we're not shooting anymore
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();
            if (TargetHandleWeaponAbility)
            {
                TargetHandleWeaponAbility.ForceStop();
            }

            _shooting = false;
        }
    }
}