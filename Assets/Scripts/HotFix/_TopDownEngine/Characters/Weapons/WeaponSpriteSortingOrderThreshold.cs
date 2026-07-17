using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This class will modify the sprite associated with it's sorting order based on the current rotation of the weapon.
    /// Useful to get the weapon get in front or behind your character based on this angle on 2D weapons
    /// </summary>
    [RequireComponent(typeof(WeaponAim2D))]
    [AddComponentMenu("TopDown Engine/Weapons/WeaponSpriteSortingOrderThreshold")]
    public class WeaponSpriteSortingOrderThreshold : TopDownMonoBehaviour
    {
        [Tooltip("the angle threshold at which to switch the sorting order")]
        public float Threshold;

        [Tooltip("the sorting order to apply when the weapon's rotation is below threshold")]
        public int BelowThresholdSortingOrder = 1;

        [Tooltip("the sorting order to apply when the weapon's rotation is above threshold")]
        public int AboveThresholdSortingOrder = -1;

        [Tooltip("the sprite whose sorting order we want to modify")]
        public SpriteRenderer Sprite;

        protected WeaponAim2D _weaponAim2D;

        /// <summary>
        /// On Awake we grab our weapon aim component
        /// </summary>
        protected virtual void Awake()
        {
            _weaponAim2D = GetComponent<WeaponAim2D>();
        }

        /// <summary>
        /// On update we change our sorting order based on current weapon angle
        /// </summary>
        protected virtual void Update()
        {
            if (_weaponAim2D == null || Sprite == null)
                return;

            Sprite.sortingOrder = _weaponAim2D.CurrentAngleRelative > Threshold ? AboveThresholdSortingOrder : BelowThresholdSortingOrder;
        }
    }
}