using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Movement helpers
    /// </summary>
    public static class MMMovement
    {
        /// <summary>
        /// Moves an object from point A to point B in a given time
        /// </summary>
        /// <param name="movingObject">Moving object.</param>
        /// <param name="pointA">Point a.</param>
        /// <param name="pointB">Point b.</param>
        /// <param name="duration">Time.</param>
        /// <param name="curve">Curve.</param>
        public static IEnumerator<float> MoveFromTo(GameObject movingObject, Vector3 pointA, Vector3 pointB, float duration, AnimationCurve curve = null)
        {
            float journey = 0f;

            while (journey < duration)
            {
                float percent = Mathf.Clamp01(journey / duration);
                if (curve != null)
                {
                    percent = curve.Evaluate(percent);
                }

                var newPosition = Vector3.Lerp(pointA, pointB, percent);

                movingObject.transform.position = newPosition;

                journey += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }
        }

        public static IEnumerator<float> AnimateScale(Transform targetTransform, Vector3 vector, float duration, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float multiplier = 1f)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            if (curveX == null || curveY == null || curveZ == null)
            {
                yield break;
            }

            if (duration == 0f)
            {
                yield break;
            }

            float journey = 0f;

            while (journey < duration)
            {
                float percent = Mathf.Clamp01(journey / duration);

                vector.x = curveX.Evaluate(percent);
                vector.y = curveY.Evaluate(percent);
                vector.z = curveZ.Evaluate(percent);
                targetTransform.localScale = multiplier * vector;

                journey += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            yield return Timing.WaitForOneFrame;
        }

        public static IEnumerator<float> AnimateRotation(Transform targetTransform,
            Vector3 vector,
            float duration,
            AnimationCurve curveX,
            AnimationCurve curveY,
            AnimationCurve curveZ,
            float multiplier)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            if ((curveX == null) || (curveY == null) || (curveZ == null))
            {
                yield break;
            }

            if (duration == 0f)
            {
                yield break;
            }

            float journey = 0f;

            while (journey < duration)
            {
                float percent = Mathf.Clamp01(journey / duration);

                vector.x = curveX.Evaluate(percent) * multiplier;
                vector.y = curveY.Evaluate(percent) * multiplier;
                vector.z = curveZ.Evaluate(percent) * multiplier;
                targetTransform.localEulerAngles = vector;

                journey += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            yield return Timing.WaitForOneFrame;
        }
    }
}