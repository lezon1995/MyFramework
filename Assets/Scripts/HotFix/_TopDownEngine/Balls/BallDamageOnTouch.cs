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
        
        public override void OnTriggerEnter2D(Collider2D c)
        {
            if (ManuallyColliding)
                return;
            
            if (0 == (TriggerFilter & TriggerMask.OnTriggerEnter2D))
                return;

            switch (c.gameObject.layer)
            {
                case LayerManager.Brick:
                    if (c.TryGetComponent(out Brick brick))
                    {
                        var dmg = ball.getHitDmg(brick, Vector2.up);
                        Colliding(brick, dmg);
                    }
                    break;
                case LayerManager.Obstacles:
                    if (c.TryGetComponent(out Obstacle obstacle))
                    {
                        var dmg = ball.getHitDmg(obstacle, Vector2.up);
                        Colliding(obstacle, dmg);
                    }
                    break;
            }
        }

        public void Colliding(Brick target, Dmg dmg)
        {
            if (target == null)
                return;

            var o = target.gameObject;
            if (!EvaluateAvailability(o))
                return;

            // cache reset 
            _colliderTopDownController = null;

            // if what we're colliding with is damageable
            _colliderHealth = target.Health;
            OnCollideWithBrick(target, dmg);

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
        }

        protected void OnCollideWithBrick(Brick brick, Dmg dmg)
        {
            if (brick.Health.CanTakeDamageThisFrame(out var resistDamageType))
            {
                // if what we're colliding with is a TopDownController, we apply a knockback force
                _colliderTopDownController = brick.Controller;

                HitDamageableFeedback.Play(transform.position);
                HitDamageableEvent?.Invoke(_colliderHealth);

                DetermineDamageDirection();
                _colliderHealth.Damage(ref dmg, gameObject, Source, InvincibilityDuration, _damageDirection);
                ApplyKnockback(dmg);
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
                        brick.Event.trigger(new DoDashDodge());
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
        }

        protected void SelfDamage(Dmg dmg, GameObject instigator, Brick brick)
        {
            if (DamageTakenHealth)
            {
                _damageDirection = Vector3.up;
                DamageTakenHealth.Damage(ref dmg, instigator, brick, DamageTakenInvincibilityDuration, _damageDirection);
            }
        }


        public void Colliding(Border target, Dmg dmg)
        {
            if (target == null)
                return;

            var o = target.gameObject;
            if (!EvaluateAvailability(o))
                return;

            // cache reset 
            _colliderTopDownController = null;

            // if what we're colliding with is damageable
            _colliderHealth = null;
            OnCollideWithBorder(target, dmg);

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
        }


        protected void OnCollideWithBorder(Border border, Dmg dmg)
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

            if (ball.getSelfDamage(border, out var selfDamage))
            {
                var selfDmg = Dmg.True(selfDamage).SetSelf();
                SelfDamage(selfDmg, ball.gameObject, border);
            }
        }

        protected void SelfDamage(Dmg dmg, GameObject instigator, Border border)
        {
            if (DamageTakenHealth)
            {
                _damageDirection = Vector3.up;
                DamageTakenHealth.Damage(ref dmg, instigator, null, DamageTakenInvincibilityDuration, _damageDirection);
            }
        }

        public void Colliding(Obstacle target, Dmg dmg)
        {
            if (target == null)
                return;

            var o = target.gameObject;
            if (!EvaluateAvailability(o))
                return;

            // cache reset 
            _colliderTopDownController = null;

            // if what we're colliding with is damageable
            _colliderHealth = null;
            OnCollideWithObstacle(target, dmg);

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
        }

        protected void OnCollideWithObstacle(Obstacle obstacle, Dmg dmg)
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

            if (ball.getSelfDamage(obstacle, out var selfDamage))
            {
                var selfDmg = Dmg.True(selfDamage).SetSelf();
                SelfDamage(selfDmg, ball.gameObject, obstacle);
            }
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