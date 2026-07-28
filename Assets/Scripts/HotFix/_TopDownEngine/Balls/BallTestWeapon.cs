using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    public class BallTestWeapon : ProjectileWeapon
    {
        APlayer _player;

        [MMInspectorGroup("ID")]
        public BallType ballType;

        public BallType OverrideBallType;

        int count;

        public override void SetOwner(Character owner, CharacterHandleWeapon handleWeapon = null)
        {
            base.SetOwner(owner, handleWeapon);
            _player = owner as APlayer;
        }

        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            BallType t = (count++ % 5) switch
            {
                // 0 => BallType.Normal,
                // 1 => BallType.LaserBeam,
                // 2 => BallType.LaserBullet,
                // 3 => BallType.LightningStrike,
                // 4 => BallType.ElectricityStrike,
                _ => OverrideBallType
            };

            var ball = _player.BallManagement.Instance.acquireBall(t, spawnPosition);

            // mandatory checks
            if (ball == null)
                return null;

            ball.setTeleportPosition(spawnPosition);
            if (_projectileSpawnTransform)
            {
                ball.setTeleportPosition(_projectileSpawnTransform.position);
            }

            // we activate the object
            ball.setActive(true);

            var success = ball != null;
            if (success)
            {
                ball.SetWeapon(this);
                if (Owner)
                {
                    ball.SetOwner(Owner);
                    ball.SetPlayer(_player);
                    ball.SetDamage(Dmg);
                }

                ball.SetTarget(_aimTarget);
            }

            if (success)
            {
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
            }

            // if (triggerObjectActivation)
            // {
            //     poolableObject.TriggerOnSpawnComplete();
            // }

            return ball.getGameObject();
        }
    }
}