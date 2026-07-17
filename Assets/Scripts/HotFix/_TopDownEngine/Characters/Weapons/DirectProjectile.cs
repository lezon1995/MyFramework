using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    [AddComponentMenu("TopDown Engine/Weapons/DirectProjectile")]
    public class DirectProjectile : Projectile
    {
        public UnitLength MaxLength;

        [ShowInInspector]
        public float MovementLength { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            MovementLength = 0;
        }

        public override void Movement(float dt)
        {
            var deltaLength = moveSpeed * dt;
            var reachEnd = false;
            _movement = Direction * deltaLength;

            MovementLength += deltaLength;
            if (MovementLength > MaxLength)
            {
                var exceedLength = MovementLength - MaxLength;
                MovementLength = MaxLength;
                _movement = Direction * (deltaLength - exceedLength);
                reachEnd = true;
            }

            transform.Translate(_movement, Space.World);

            // We apply the acceleration to increase the speed
            Speed += Acceleration * dt;

            if (reachEnd)
            {
                _health.Kill();
            }
        }
    }
}