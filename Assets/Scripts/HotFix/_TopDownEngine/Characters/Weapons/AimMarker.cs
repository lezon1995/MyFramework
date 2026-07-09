using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class used to handle aim markers, (usually circular) visual elements 
    /// </summary>
    public class AimMarker : TopDownMonoBehaviour
    {
        /// the possible movement modes for aim markers
        public enum MovementModes
        {
            Instant,
            Interpolate
        }

        [Header("Movement")]
        [Tooltip("The selected movement mode for this aim marker. Instant will move the marker instantly to its target, Interpolate will animate its position over time")]
        public MovementModes MovementMode;

        [Tooltip("an offset to apply to the position of the target (useful if you want, for example, the marker to appear above the target)")]
        public Vector3 Offset;

        [Tooltip("When in Interpolate mode, the duration of the movement animation")]
        [MMEnumCondition(nameof(MovementMode), (int)MovementModes.Interpolate)]
        public float MovementDuration = 0.2f;

        [Tooltip("When in Interpolate mode, the curve to animate the movement on")]
        [MMEnumCondition(nameof(MovementMode), (int)MovementModes.Interpolate)]
        public MMTween.MMTweenCurve MovementCurve = MMTween.MMTweenCurve.EaseInCubic;

        [Tooltip("When in Interpolate mode, the delay before the marker moves when changing target")]
        [MMEnumCondition(nameof(MovementMode), (int)MovementModes.Interpolate)]
        public float MovementDelay;

        [Header("Feedbacks")]
        [Tooltip("A feedback to play when a target is found and we didn't have one already")]
        public MMFeedbacks FirstTargetFeedback;

        [Tooltip("a feedback to play when we already had a target and just found a new one")]
        public MMFeedbacks NewTargetAssignedFeedback;

        [Tooltip("a feedback to play when no more targets are found, and we just lost our last target")]
        public MMFeedbacks NoMoreTargetFeedback;

        protected Transform _target;
        protected Transform _targetLastFrame;
        protected float _lastTargetChangeAt;

        /// <summary>
        /// On Awake we initialize our feedbacks and delay
        /// </summary>
        protected virtual void Awake()
        {
            FirstTargetFeedback.Initialize(gameObject);
            NewTargetAssignedFeedback.Initialize(gameObject);
            NoMoreTargetFeedback.Initialize(gameObject);
        }

        /// <summary>
        /// On Update we check if we've changed target, and follow it if needed
        /// </summary>
        protected virtual void Update()
        {
            HandleTargetChange();
            FollowTarget();
            _targetLastFrame = _target;
        }

        /// <summary>
        /// Makes this object follow the target's position
        /// </summary>
        protected virtual void FollowTarget()
        {
            if (MovementMode == MovementModes.Instant)
            {
                transform.position = _target.transform.position + Offset;
            }
            else
            {
                if (_target && Time.time - _lastTargetChangeAt > MovementDuration)
                {
                    transform.position = _target.transform.position + Offset;
                }
            }
        }

        /// <summary>
        /// Sets a new target for this aim marker
        /// </summary>
        /// <param name="newTarget"></param>
        public virtual void SetTarget(Transform newTarget)
        {
            _target = newTarget;

            if (newTarget == null)
                return;

            gameObject.SetActive(true);

            if (_targetLastFrame == null)
            {
                transform.position = _target.transform.position + Offset;
            }

            if (MovementMode == MovementModes.Instant)
            {
                transform.position = _target.transform.position + Offset;
            }
            else
            {
                MMTween.MoveTransform(transform, transform.position, _target.transform.position + Offset, MovementDelay, MovementDelay, MovementDuration, MovementCurve);
            }
        }

        /// <summary>
        /// Checks for target changes and triggers the appropriate methods if needed
        /// </summary>
        protected virtual void HandleTargetChange()
        {
            if (_target == _targetLastFrame)
                return;

            _lastTargetChangeAt = Time.time;

            if (_target == null)
            {
                NoMoreTargets();
                return;
            }

            if (_targetLastFrame == null)
            {
                FirstTargetFound();
                return;
            }

            if (_targetLastFrame && _target)
            {
                NewTargetFound();
            }
        }

        /// <summary>
        /// When no more targets are found, and we just lost one, we play a dedicated feedback
        /// </summary>
        protected virtual void NoMoreTargets()
        {
            NoMoreTargetFeedback.Play();
        }

        /// <summary>
        /// When a new target is found and we didn't have one already, we play a dedicated feedback
        /// </summary>
        protected virtual void FirstTargetFound()
        {
            FirstTargetFeedback.Play();
        }

        /// <summary>
        /// When a new target is found, and we previously had another, we play a dedicated feedback
        /// </summary>
        protected virtual void NewTargetFound()
        {
            NewTargetAssignedFeedback.Play();
        }

        /// <summary>
        /// Hides this object
        /// </summary>
        public virtual void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}