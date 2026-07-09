using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A thrown object type of projectile, useful for grenades and such
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/PathProjectile")]
    public class PathProjectile : Projectile
    {
        protected override void Awake()
        {
            base.Awake();

            if (_damageOnTouch)
            {
                _damageOnTouch.TriggerFilter = DamageOnTouch.TriggerMask.IgnoreAll;
            }
        }

        public override void Movement(float dt)
        {
            if (_target == null)
                return;

            transform.position = Vector3.MoveTowards(transform.position, _target.position, dt * Speed);

            var vector3 = _target.position - transform.position;
            
            CurDirection = vector3.normalized;
            
            if (vector3.sqrMagnitude == 0F)
            {
                _damageOnTouch.ForceColliding(_target.gameObject);
            }

            // We apply the acceleration to increase the speed
            Speed += Acceleration * dt;
        }
    }
}