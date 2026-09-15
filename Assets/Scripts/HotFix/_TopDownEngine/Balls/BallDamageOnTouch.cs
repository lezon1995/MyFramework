using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Ball))]
    public class BallDamageOnTouch : DamageOnTouch
    {
        Ball ball;

        protected override void Awake()
        {
            base.Awake();
            TryGetComponent(out ball);
        }

        protected override void ApplyKnockback(Health colliderHealth, TopDownController controller, Dmg damage)
        {
            if (DamageCausedKnockbackType != KnockbackStyles.AddForce)
                return;

            var knockbackForce = ball.getKnockbackForce(colliderHealth, damage.IsLethal);
            ApplyKnockback2D(ref knockbackForce);

            colliderHealth.ApplyKnockback(knockbackForce, damage);
        }

        public override void OnTriggerEnter2D(Collider2D c)
        {
            if (ball.ManuallyColliding)
                return;

            if (0 == (TriggerFilter & TriggerMask.OnTriggerEnter2D))
                return;

            Vector2 normal;
            switch (c.gameObject.layer)
            {
                case LayerManager.Brick:
                    if (c.TryGetComponent(out Brick brick))
                    {
                        var dir = (Vector2)ball.getWorldPosition() - brick.getCenterPosition();
                        if (dir.x.abs() > dir.y.abs())
                            normal = dir.x.sign() > 0 ? Vector2.right : Vector2.left;
                        else
                            normal = dir.y.sign() > 0 ? Vector2.up : Vector2.down;

                        ball.CollidingWithBrick(brick, normal);
                    }

                    break;
                case LayerManager.Obstacles:
                    if (c.TryGetComponent(out Obstacle obstacle))
                    {
                        var dir = ball.getWorldPosition() - obstacle.getWorldPosition();
                        if (dir.x.abs() > dir.y.abs())
                            normal = dir.x.sign() > 0 ? Vector2.right : Vector2.left;
                        else
                            normal = dir.y.sign() > 0 ? Vector2.up : Vector2.down;

                        ball.CollidingWithObstacle(obstacle, normal);
                    }

                    break;
                case LayerManager.Border:
                    if (c.TryGetComponent(out Border border))
                    {
                        var dir = ball.getWorldPosition() - border.getWorldPosition();
                        if (dir.x.abs() > dir.y.abs())
                            normal = dir.x.sign() > 0 ? Vector2.right : Vector2.left;
                        else
                            normal = dir.y.sign() > 0 ? Vector2.up : Vector2.down;

                        ball.CollidingWithBorder(border, normal);
                    }

                    break;
            }
        }

        protected override void DetermineDamageDirection()
        {
            _damageDirection = ball.Direction;
        }

        public bool Colliding(Brick target, Vector3 normal)
        {
            var o = target.gameObject;
            if (!EvaluateAvailability(o))
                return false;

            // cache reset 
            _colliderController = null;

            // if what we're colliding with is damageable
            _colliderHealth = target.Health;
            var b = OnCollideWithBrick(target, normal);

            if (_colliderHealth.CurrentHealth > 0)
            {
                if (BuffOnTouch && BuffOnTouch.DriveByDamageOnTouch)
                {
                    BuffOnTouch.Colliding(o);
                }
            }

            OnAnyCollision(o);
            HitAnythingEvent?.Invoke(o);
            HitAnythingFeedback.Play(transform.position);
            return b;
        }

        protected bool OnCollideWithBrick(Brick brick, Vector2 normal)
        {
            bool triggerCollide = true;
            if (brick.Health.CanTakeDamageThisFrame(out var resistDamageType))
            {
                if (ball.IsTheBrickBeingIgnoredToHit(brick))
                {
                    triggerCollide = false;
                }
                else
                {
                    ball.lastHittable = brick;
                    var dmg = ball.getHitDmg(brick, normal);
                    brick.onHitEnter(ball, normal);
                    ball.onHitEnter(brick, normal, out var triggerRegularHit);
                    ball.collidingBrick = brick;

                    if (triggerRegularHit)
                    {
                        ball.counters.hit.count();
                        ball.counters.hitBrick.count();
                        ball.hasBeenCollided = true;
                    }

                    ball.ResetIgnoredToHitBricks();
                    ball.brickHitTimers.add(brick, 0.2F);

                    // if what we're colliding with is a TopDownController, we apply a knockback force
                    _colliderController = brick.Controller;

                    HitDamageableFeedback.Play(transform.position);
                    HitDamageableEvent?.Invoke(_colliderHealth);

                    DetermineDamageDirection();
                    _colliderHealth.Damage(ref dmg, gameObject, Source, InvincibilityDuration, _damageDirection);
                    ApplyKnockback(_colliderHealth, _colliderController, dmg);
                }
            }
            else
            {
                switch (resistDamageType)
                {
                    case ResistDamageType.None:
                        break;
                    case ResistDamageType.Invincible:
                        break;
                    case ResistDamageType.ImmuneToDamage:
                        break;
                    case ResistDamageType.Dead:
                        break;
                    case ResistDamageType.Disabled:
                        break;
                }
            }

            if (ball.getSelfDamage(brick, out var selfDamage))
            {
                var selfDmg = Dmg.True(selfDamage).SetSelf();
                SelfDamage(selfDmg, ball.gameObject, brick);
            }

            return triggerCollide;
        }

        protected void SelfDamage(Dmg dmg, GameObject instigator, Brick brick)
        {
            if (DamageTakenHealth)
            {
                DamageTakenHealth.Damage(ref dmg, instigator, brick, DamageTakenInvincibilityDuration, -_damageDirection);
            }
        }


        public bool Colliding(Border target, Vector3 normal)
        {
            var o = target.gameObject;
            // cache reset 
            _colliderController = null;

            // if what we're colliding with is damageable
            _colliderHealth = null;
            var b = OnCollideWithBorder(target, normal);
            OnAnyCollision(o);
            HitAnythingEvent?.Invoke(o);
            HitAnythingFeedback.Play(transform.position);
            return b;
        }


        protected bool OnCollideWithBorder(Border border, Vector2 normal)
        {
            /*if (border.Health.CanTakeDamageThisFrame(out var resistDamageType))
            {
                // if what we're colliding with is a TopDownController, we apply a knockback force
                _colliderTopDownController = border.Controller;

                HitDamageableFeedback.Play(transform.position);
                HitDamageableEvent?.Invoke(_colliderHealth);

                ApplyKnockback(dmg);
                DetermineDamageDirection();
                _colliderHealth.Damage(ref dmg, gameObject, Source, InvincibilityDuration, _damageDirection);
            }
            else
            {
                switch (resistDamageType)
                {
                    case ResistDamageType.None:
                        break;
                    case ResistDamageType.Invincible:
                        break;
                    case ResistDamageType.DashInvincible:
                        border.Event.trigger(new DoDashDodge());
                        break;
                    case ResistDamageType.ImmuneToDamage:
                        break;
                    case ResistDamageType.Dead:
                        break;
                    case ResistDamageType.Disabled:
                        break;
                }
            }*/

            ball.lastHittable = border;
            foreach (var p in ball.powers)
                p.onHitBorder(border);

            ball.counters.hit.count();
            ball.hasBeenCollided = true;

            ball.Player.onBallHitBorder(ball, border, ref normal);
            ball.playHitBorderSfx();

            if (ball.getSelfDamage(border, out var selfDamage))
            {
                var selfDmg = Dmg.True(selfDamage).SetSelf();
                SelfDamage(selfDmg, ball.gameObject, border);
            }

            return true;
        }

        protected void SelfDamage(Dmg dmg, GameObject instigator, Border border)
        {
            if (DamageTakenHealth)
            {
                _damageDirection = Vector3.up;
                DamageTakenHealth.Damage(ref dmg, instigator, null, DamageTakenInvincibilityDuration, _damageDirection);
            }
        }

        public bool Colliding(Obstacle target, Vector3 normal)
        {
            var o = target.gameObject;
            if (!EvaluateAvailability(o))
                return false;

            // cache reset 
            _colliderController = null;

            // if what we're colliding with is damageable
            _colliderHealth = null;
            var b = OnCollideWithObstacle(target, normal);

            OnAnyCollision(o);
            HitAnythingEvent?.Invoke(o);
            HitAnythingFeedback.Play(transform.position);
            return b;
        }

        protected bool OnCollideWithObstacle(Obstacle obstacle, Vector2 normal)
        {
            /*if (obstacle.Health.CanTakeDamageThisFrame(out var resistDamageType))
            {
                // if what we're colliding with is a TopDownController, we apply a knockback force
                _colliderTopDownController = obstacle.Controller;

                HitDamageableFeedback.Play(transform.position);
                HitDamageableEvent?.Invoke(_colliderHealth);

                ApplyKnockback(dmg);
                DetermineDamageDirection();
                _colliderHealth.Damage(ref dmg, gameObject, Source, InvincibilityDuration, _damageDirection);
            }
            else
            {
                switch (resistDamageType)
                {
                    case ResistDamageType.None:
                        break;
                    case ResistDamageType.Invincible:
                        break;
                    case ResistDamageType.DashInvincible:
                        obstacle.Event.trigger(new DoDashDodge());
                        break;
                    case ResistDamageType.ImmuneToDamage:
                        break;
                    case ResistDamageType.Dead:
                        break;
                    case ResistDamageType.Disabled:
                        break;
                }
            }*/

            ball.lastHittable = obstacle;
            foreach (var p in ball.powers)
                p.onHitObstacle(obstacle);

            ball.counters.hit.count();
            ball.hasBeenCollided = true;

            ball.Player.onBallHitObstacle(ball, obstacle, ref normal);
            ball.playHitObstacleSfx();

            if (ball.getSelfDamage(obstacle, out var selfDamage))
            {
                var selfDmg = Dmg.True(selfDamage).SetSelf();
                SelfDamage(selfDmg, ball.gameObject, obstacle);
            }

            return true;
        }

        protected void SelfDamage(Dmg dmg, GameObject instigator, Obstacle obstacle)
        {
            if (DamageTakenHealth)
            {
                _damageDirection = Vector3.up;
                DamageTakenHealth.Damage(ref dmg, instigator, null, DamageTakenInvincibilityDuration, _damageDirection);
            }
        }
    }
}