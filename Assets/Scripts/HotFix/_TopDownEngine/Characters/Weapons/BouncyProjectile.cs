using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Projectile class that will bounce off walls instead of exploding on impact
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/BouncyProjectile")]
    public class BouncyProjectile : Projectile
    {
        [Header("Bounciness Tech")] [Tooltip("the length of the raycast used to detect bounces, should be proportionate to the size and speed of your projectile")]
        public float BounceRaycastLength = 1f;

        public bool IsPenetrable; //是否可穿透砖块

        [Tooltip("the layers you want this projectile to bounce on")]
        public LayerMask BounceLayers = LayerManager.Obstacles_Mask | LayerManager.Brick_Mask;

        public LayerMask PenetrableLayers = LayerManager.Brick_Mask;

        [Tooltip("a feedback to trigger at every bounce")]
        public MMFeedbacks BounceFeedback;

        [Header("Bounciness")] [Tooltip("the min and max amount of bounces (a value will be picked at random between both bounds)")] [MMVector("Min", "Max")]
        public Vector2Int AmountOfBounces = new(10, 10);

        protected int _amountOfBounces;
        protected int _bouncesLeft;
        protected Collider2D hitCollider;

        /// <summary>
        /// On init, we randomize our values, refresh our 2D collider because Unity is weird sometimes
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            _amountOfBounces = Random.Range(AmountOfBounces.x, AmountOfBounces.y);
            _bouncesLeft = _amountOfBounces;
            if (_hasCollider2D)
            {
                _collider2D.enabled = false;
                _collider2D.enabled = true;
            }
        }

        /// <summary>
        /// On trigger enter 2D, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D c)
        {
            if (ManuallyColliding)
                return;

            Colliding(c);
        }

        public override void OnFixedUpdate(float dt)
        {
            if (!_shouldMove)
                return;

            if (IsPenetrable)
            {
                if (ManuallyColliding)
                {
                    if (willPassingThroughThisFrame && curPos == correctPos)
                    {
                        willPassingThroughThisFrame = false;
                        correctPos = Vector3.zero;
                        CollidingManually(willPassingThroughHit);
                        willPassingThroughHit = default;
                        return;
                    }
                }
                
                willPassingThroughThisFrame = CheckWillPassingThrough(dt, BounceLayers, out correctPos, out willPassingThroughHit);
                if (willPassingThroughThisFrame)
                {
                    MovementTo(correctPos);
                }
                else
                {
                    Movement(dt);
                }
            }
            else
            {
                if (ManuallyColliding)
                {
                    if (willPassingThroughThisFrame && curPos == correctPos)
                    {
                        willPassingThroughThisFrame = false;
                        correctPos = Vector3.zero;
                        CollidingManually(willPassingThroughHit);
                        willPassingThroughHit = default;
                        return;
                    }
                }

                willPassingThroughThisFrame = CheckWillPassingThrough(dt, BounceLayers, out correctPos, out willPassingThroughHit);
                if (willPassingThroughThisFrame)
                {
                    MovementTo(correctPos);
                }
                else
                {
                    Movement(dt);
                }
            }

            if (FaceMovement)
                FaceMovementDirection(Direction);
        }

        protected virtual bool CheckWillPassingThrough(float dt, LayerMask targetLayer, out Vector3 correctPos, out RaycastHit2D hitInfo)
        {
            correctPos = Vector3.zero;
            hitInfo = default;
            return false;
        }

        /// <summary>
        /// Colliding endpoint
        /// </summary>
        /// <param name="c"></param>
        protected virtual void Colliding(Collider2D c)
        {
            if (!BounceLayers.MMContains(c.gameObject.layer))
                return;

            if (IsPenetrable)
                return;

            var hit = MMDebug.RayCast(transform.position, Direction.normalized, BounceRaycastLength, BounceLayers, MMColors.DarkOrange, true);
            EvaluateHit2D(hit);
        }

        /// <summary>
        /// Decides whether we should bounce
        /// </summary>
        protected virtual void EvaluateHit2D(RaycastHit2D hit)
        {
            if (!hit)
                return;

            if (_bouncesLeft > 0)
            {
                Bounce2D(hit);
            }
            else
            {
                _health.Kill();
                _damageOnTouch.HitNonDamageableFeedback.Play();
            }
        }

        /// <summary>
        /// If we get a prevention collision 2D message, we check if we should bounce
        /// </summary>
        /// <param name="hit"></param>
        public virtual void PreventedCollision2D(RaycastHit2D hit)
        {
            if (_health.CurrentHealth <= 0)
                return;

            EvaluateHit2D(hit);
        }

        /// <summary>
        /// Applies a bounce in 2D
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void Bounce2D(RaycastHit2D hit)
        {
            BounceFeedback.Play();
            var reflectDir = Vector3.Reflect(Direction, hit.normal).normalized;
            float angle = Vector3.Angle(Direction, reflectDir);
            SetDirection(reflectDir, Quaternion.identity);
            transform.right = _spawnerIsFacingRight ? reflectDir.normalized : -reflectDir.normalized;
            _bouncesLeft--;
        }
    }
}