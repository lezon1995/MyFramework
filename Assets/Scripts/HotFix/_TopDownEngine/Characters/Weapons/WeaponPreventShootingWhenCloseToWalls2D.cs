using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this class to a weapon and it'll prevent shooting when close to an obstacle (as defined by the ObstacleLayerMask)
    /// </summary>
    [RequireComponent(typeof(Weapon))]
    [AddComponentMenu("TopDown Engine/Weapons/WeaponPreventShootingWhenCloseToWalls2D")]
    public class WeaponPreventShootingWhenCloseToWalls2D : WeaponPreventShooting
    {
        [Tooltip("the angle to consider when deciding whether or not there's a wall in front of the weapon (usually 5 degrees is fine)")]
        public float Angle = 5f;

        [Tooltip("the max distance to the wall we want to prevent shooting from")]
        public float Distance = 2f;

        [Tooltip("the offset to apply to the detection (in addition and relative to the weapon's position)")]
        public Vector3 RaycastOriginOffset = Vector3.zero;

        [Tooltip("the layers to consider as obstacles")]
        public LayerMask ObstacleLayerMask = LayerManager.Obstacles_Mask;

        protected RaycastHit2D _hitLeft;
        protected RaycastHit2D _hitMiddle;
        protected RaycastHit2D _hitRight;
        protected WeaponAim _weaponAim;

        /// <summary>
        /// On Awake we grab our weapon
        /// </summary>
        protected virtual void Awake()
        {
            _weaponAim = GetComponent<WeaponAim>();
        }

        /// <summary>
        /// Casts rays in front of the weapon to check for obstacles
        /// Returns true if an obstacle was found
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckForObstacles()
        {
            Vector2 origin = transform.position + _weaponAim.CurrentRotation * RaycastOriginOffset;

            Vector2 dirLeft = (Quaternion.Euler(0f, 0f, -Angle / 2F) * _weaponAim.CurrentAimAbsolute).normalized;
            _hitLeft = MMDebug.RayCast(origin, dirLeft, Distance, ObstacleLayerMask, Color.yellow, true);

            Vector2 dirMiddle = _weaponAim.CurrentAimAbsolute.normalized;
            _hitMiddle = MMDebug.RayCast(origin, dirMiddle, Distance, ObstacleLayerMask, Color.yellow, true);

            Vector2 dirRight = (Quaternion.Euler(0f, 0f, Angle / 2F) * _weaponAim.CurrentAimAbsolute).normalized;
            _hitRight = MMDebug.RayCast(origin, dirRight, Distance, ObstacleLayerMask, Color.yellow, true);

            if (_hitLeft.collider || _hitMiddle.collider || _hitRight.collider)
                return true;
            
            return false;
        }

        /// <summary>
        /// Shooting is allowed if no obstacle is in front of the weapon
        /// </summary>
        /// <returns></returns>
        public override bool ShootingAllowed()
        {
            return !CheckForObstacles();
        }
    }
}