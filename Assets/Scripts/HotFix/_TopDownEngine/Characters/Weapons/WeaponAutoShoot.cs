using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Adds this component on a weapon with a WeaponAutoAim (2D or 3D) and it will automatically shoot at targets after an optional delay
    /// To prevent/stop auto shoot, simply disable this component, and enable it again to resume auto shoot
    /// </summary>
    public class WeaponAutoShoot : TopDownMonoBehaviour
    {
        [Header("Auto Shoot")]
        [Tooltip("the delay (in seconds) between acquiring a target and starting shooting at it")]
        public float DelayBeforeShootAfterAcquiringTarget = 0.1f;

        [Tooltip("if this is true, the weapon will only auto shoot if its owner is idle")]
        public bool OnlyAutoShootIfOwnerIsIdle;

        protected WeaponAutoAim _weaponAutoAim;
        protected Weapon _weapon;
        protected bool _hasWeaponAndAutoAim;
        protected float _targetAcquiredAt;
        protected Transform _lastTarget;

        /// <summary>
        /// On Awake we initialize our component
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Grabs auto aim and weapon
        /// </summary>
        protected virtual void Initialization()
        {
            TryGetComponent(out _weaponAutoAim);
            TryGetComponent(out _weapon);
            if (_weaponAutoAim == null)
            {
                Debug.LogWarning(name + " : the WeaponAutoShoot on this object requires that you add either a WeaponAutoAim2D or WeaponAutoAim3D component to your weapon.");
                return;
            }

            _hasWeaponAndAutoAim = _weapon && _weaponAutoAim;
        }

        /// <summary>
        /// A public method you can use to update the cached Weapon
        /// </summary>
        /// <param name="newWeapon"></param>
        public virtual void SetCurrentWeapon(Weapon newWeapon)
        {
            _weapon = newWeapon;
        }

        /// <summary>
        /// On Update we handle auto shoot
        /// </summary>
        protected virtual void LateUpdate()
        {
            HandleAutoShoot();
        }

        /// <summary>
        /// Returns true if this weapon can auto shoot, false otherwise
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanAutoShoot()
        {
            if (_hasWeaponAndAutoAim)
            {
                if (OnlyAutoShootIfOwnerIsIdle)
                {
                    if (_weapon.Owner.motionState.Not(Character.Motions.Idle))
                        return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if we have a target for enough time, and shoots if needed
        /// </summary>
        protected virtual void HandleAutoShoot()
        {
            if (!CanAutoShoot())
                return;

            if (_weaponAutoAim.Target)
            {
                if (_lastTarget != _weaponAutoAim.Target)
                {
                    _targetAcquiredAt = Time.time;
                }

                if (Time.time - _targetAcquiredAt >= DelayBeforeShootAfterAcquiringTarget)
                {
                    _weapon.WeaponInputStart();
                }

                _lastTarget = _weaponAutoAim.Target;
            }
        }
    }
}