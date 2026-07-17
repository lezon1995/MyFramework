using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// A thrown object type of projectile, useful for grenades and such
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("TopDown Engine/Weapons/ThrownObject")]
    public class ThrownObject : Projectile
    {
        protected Vector2 _throwingForce;
        protected bool _forceApplied;

        protected override void Initialization()
        {
            base.Initialization();
            _rigidBody2D = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// On enable, we reset the object's speed
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _forceApplied = false;
        }

        /// <summary>
        /// Handles the projectile's movement, every frame
        /// </summary>
        /// <param name="f"></param>
        public override void Movement(float f)
        {
            if (!_forceApplied && Direction != Vector3.zero)
            {
                _throwingForce = Direction * moveSpeed;
                _rigidBody2D.AddForce(_throwingForce);
                _forceApplied = true;
            }
        }
    }
}