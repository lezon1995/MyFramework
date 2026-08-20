using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This class will automatically draw a circle to match the radius of the auto aim weapon if there's one
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [AddComponentMenu("TopDown Engine/Weapons/WeaponAutoAimRadiusCircle")]
    public class WeaponAutoAimRadiusCircle : MMLineRendererCircle
    {
        [Header("Weapon Radius")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;

        /// <summary>
        /// On initialization, hooks itself to weapon changes
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            _line = GetComponent<LineRenderer>();
            _line.enabled = false;

            if (TargetHandleWeaponAbility)
            {
                TargetHandleWeaponAbility.OnWeaponChange += OnWeaponChange;
            }

            if (TryGetComponent<WeaponAutoAim>(out var autoAim))
            {
                HorizontalRadius = autoAim.scanRadius;
                VerticalRadius = autoAim.scanRadius;
            }
        }

        /// <summary>
        /// When the weapon changes, if it has auto aim, draws a circle around it
        /// </summary>
        void OnWeaponChange()
        {
            if (TargetHandleWeaponAbility.CurrentWeapon == null)
                return;

            if (TargetHandleWeaponAbility.CurrentWeapon.TryGetComponent<WeaponAutoAim>(out var autoAim))
            {
                HorizontalRadius = autoAim.scanRadius;
                VerticalRadius = autoAim.scanRadius;
                _line.enabled = true;
            }
            else
            {
                HorizontalRadius = 0;
                VerticalRadius = 0;
                _line.enabled = false;
            }

            DrawCircle();
        }

        /// <summary>
        /// On disables we unhook from our delegate
        /// </summary>
        void OnDisable()
        {
            if (TargetHandleWeaponAbility)
            {
                TargetHandleWeaponAbility.OnWeaponChange -= OnWeaponChange;
            }
        }

        public void SetRenderer(bool active)
        {
            if (TryGetComponent<WeaponAutoAim>(out var autoAim))
            {
                HorizontalRadius = autoAim.scanRadius;
                VerticalRadius = autoAim.scanRadius;
            }

            _line.enabled = active;
        }
    }
}