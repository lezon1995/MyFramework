using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    public class BallSlotWeapon : ProjectileWeapon
    {
        APlayer _player;

        public override void SetOwner(Character owner, CharacterHandleWeapon handleWeapon = null)
        {
            base.SetOwner(owner, handleWeapon);
            _player = owner as APlayer;
        }

        public override void ShootRequest()
        {
            if (_player.BallManagement.Slots.HasAnySlotReadyToShoot())
            {
                State.ChangeState(States.Use);
            }
            else
            {
                State.ChangeState(States.Idle);
            }
        }

        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            Ball ball = null;
            var success = _player.BallManagement.Slots.TryGetFirstReadyToShootSlot(out var slot);
            if (success)
                success &= slot.TryShoot(out ball);

            // mandatory checks
            if (!success)
                return null;

            ball.setTeleportPosition(spawnPosition);
            if (_projectileSpawnTransform)
            {
                ball.setTeleportPosition(_projectileSpawnTransform.position);
            }

            // we activate the object
            ball.setActive(true);

            ball.SetWeapon(this);
            if (Owner)
            {
                ball.SetOwner(Owner);
                ball.SetPlayer(_player);
                ball.SetDamage(Dmg);
            }

            ball.SetTarget(_aimTarget);

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
                var direction = spread * transform.rotation * DefaultProjectileDirection;
                ball.setShootDirection(direction);
                ball.SetDirection(direction, transform.rotation);
            }
            else
            {
                Vector3 newDirection = spread * transform.right * (Flipped ? -1 : 1);
                if (Owner.Orientation2D)
                {
                    ball.setShootDirection(newDirection);
                    ball.SetDirection(newDirection, spread * transform.rotation, Owner.Orientation2D.IsFacingRight);
                }
                else
                {
                    ball.setShootDirection(newDirection);
                    ball.SetDirection(newDirection, spread * transform.rotation);
                }
            }

            if (RotateWeaponOnSpread)
            {
                transform.rotation *= spread;
            }

            // if (triggerObjectActivation)
            // {
            //     poolableObject.TriggerOnSpawnComplete();
            // }

            return ball.getGameObject();
        }
    }
}