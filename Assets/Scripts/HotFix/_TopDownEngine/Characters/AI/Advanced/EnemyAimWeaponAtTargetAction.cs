using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Aims the weapon at the current target
    /// </summary>
    public class EnemyAimWeaponAtTargetAction : EnemyAction
    {
        public enum AimOrigins
        {
            Transform,
            SpawnPoint
        }

        [Header("Binding")]
        /// the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.
        [Tooltip("the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;

        [Header("Behaviour")]
        /// the origin we'll take into account when computing the aim direction towards the target
        [Tooltip("the origin we'll take into account when computing the aim direction towards the target")]
        public AimOrigins AimOrigin = AimOrigins.Transform;

        /// if true, the Character will face the target (left/right)
        [Tooltip("if true, the Character will face the target (left/right)")]
        public bool FaceTarget = true;

        /// if true the Character will aim at the target
        [Tooltip("if true the Character will aim at the target")]
        public bool AimAtTarget = true;

        protected CharacterOrientation2D _orientation2D;
        protected Character _character;
        protected WeaponAim _weaponAim;
        protected ProjectileWeapon _projectileWeapon;
        protected Vector3 _weaponAimDirection;

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

            if (TargetHandleWeaponAbility)
            {
                _projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon as ProjectileWeapon;
            }
        }

        /// <summary>
        /// On PerformAction we face and aim if needed, and we shoot
        /// </summary>
        /// <param name="dt"></param>
        public override void PerformAction(float dt)
        {
            if (_brain.Target == null)
                return;

            TestFaceTarget();
            TestAimAtTarget();
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

            if (TargetHandleWeaponAbility.CurrentWeapon)
            {
                if (_weaponAim == null)
                {
                    TargetHandleWeaponAbility.CurrentWeapon.TryGetComponent(out _weaponAim);
                }

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

            _weaponAim.SetCurrentAim(_weaponAimDirection);
        }
    }
}