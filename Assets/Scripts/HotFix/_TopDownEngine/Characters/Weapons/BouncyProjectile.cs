using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Projectile class that will bounce off walls instead of exploding on impact
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/BouncyProjectile")]
    public class BouncyProjectile : Projectile
    {
        [Header("Bounciness Tech")]
        [Tooltip("the length of the raycast used to detect bounces, should be proportionate to the size and speed of your projectile")]
        public float BounceRaycastLength = 1f;

        [Tooltip("the layers you want this projectile to bounce on")]
        public LayerMask BounceLayers = LayerManager.Obstacles_Mask;

        [Tooltip("a feedback to trigger at every bounce")]
        public MMFeedbacks BounceFeedback;

        [Header("Bounciness")]
        [Tooltip("the min and max amount of bounces (a value will be picked at random between both bounds)")]
        [MMVector("Min", "Max")]
        public Vector2Int AmountOfBounces = new Vector2Int(10, 10);

        [Tooltip("the min and max speed multiplier to apply at every bounce (a value will be picked at random between both bounds)")]
        [MMVector("Min", "Max")]
        public Vector2 SpeedModifier = Vector2.one;

        protected Vector3 _positionLastFrame;
        protected Vector3 _raycastDirection;
        protected Vector3 _reflectedDirection;
        protected int _randomAmountOfBounces;
        protected int _bouncesLeft;
        protected float _randomSpeedModifier;

        /// <summary>
        /// On init we randomize our values, refresh our 2D collider because Unity is weird sometimes
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            _randomAmountOfBounces = Random.Range(AmountOfBounces.x, AmountOfBounces.y);
            _randomSpeedModifier = Random.Range(SpeedModifier.x, SpeedModifier.y);
            _bouncesLeft = _randomAmountOfBounces;
            if (_collider2D)
            {
                _collider2D.enabled = false;
                _collider2D.enabled = true;
            }
        }

        /// <summary>
        /// On trigger enter 2D, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// On trigger enter, we call our colliding endpoint
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
        {
            Colliding(collider.gameObject);
        }

        /// <summary>
        /// Colliding endpoint
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
        {
            if (!BounceLayers.MMContains(collider.layer))
                return;

            _raycastDirection = transform.position - _positionLastFrame;

            switch (BoundsBasedOn)
            {
                case WaysToDetermineBounds.Collider:
                {
                    RaycastHit hit = MMDebug.Raycast3D(transform.position, Direction.normalized, BounceRaycastLength, BounceLayers, MMColors.DarkOrange, true);
                    EvaluateHit3D(hit);
                    break;
                }
                case WaysToDetermineBounds.Collider2D:
                {
                    RaycastHit2D hit = MMDebug.RayCast(transform.position, Direction.normalized, BounceRaycastLength, BounceLayers, MMColors.DarkOrange, true);
                    EvaluateHit2D(hit);
                    break;
                }
            }
        }

        /// <summary>
        /// Decides whether or not we should bounce
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void EvaluateHit3D(RaycastHit hit)
        {
            if (hit.collider)
            {
                if (_bouncesLeft > 0)
                {
                    Bounce3D(hit);
                }
                else
                {
                    _health.Kill();
                    _damageOnTouch.HitNonDamageableFeedback.Play();
                }
            }
        }

        /// <summary>
        /// Decides whether or not we should bounce
        /// </summary>
        protected virtual void EvaluateHit2D(RaycastHit2D hit)
        {
            if (hit.collider)
            {
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
        }

        /// <summary>
        /// If we get a prevent collision 2D message, we check if we should bounce
        /// </summary>
        /// <param name="hit"></param>
        public void PreventedCollision2D(RaycastHit2D hit)
        {
            _raycastDirection = transform.position - _positionLastFrame;
            if (_health.CurrentHealth <= 0)
                return;

            EvaluateHit2D(hit);
        }

        /// <summary>
        /// If we get a prevent collision 3D message, we check if we should bounce
        /// </summary>
        /// <param name="hit"></param>
        public void PreventedCollision3D(RaycastHit hit)
        {
            _raycastDirection = transform.position - _positionLastFrame;
            if (_health.CurrentHealth <= 0)
                return;

            EvaluateHit3D(hit);
        }

        /// <summary>
        /// Applies a bounce in 2D
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void Bounce2D(RaycastHit2D hit)
        {
            BounceFeedback.Play();
            _reflectedDirection = Vector3.Reflect(_raycastDirection, hit.normal);
            float angle = Vector3.Angle(_raycastDirection, _reflectedDirection);
            Direction = _reflectedDirection.normalized;
            transform.right = _spawnerIsFacingRight ? _reflectedDirection.normalized : -_reflectedDirection.normalized;
            Speed *= _randomSpeedModifier;
            _bouncesLeft--;
        }

        /// <summary>
        /// Applies a bounce in 3D
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void Bounce3D(RaycastHit hit)
        {
            BounceFeedback.Play();
            _reflectedDirection = Vector3.Reflect(_raycastDirection, hit.normal);
            float angle = Vector3.Angle(_raycastDirection, _reflectedDirection);
            Direction = _reflectedDirection.normalized;
            transform.forward = _spawnerIsFacingRight ? _reflectedDirection.normalized : -_reflectedDirection.normalized;
            Speed *= _randomSpeedModifier;
            _bouncesLeft--;
        }

        /// <summary>
        /// On late update we store our position
        /// </summary>
        protected virtual void LateUpdate()
        {
            _positionLastFrame = transform.position;
        }
    }
}