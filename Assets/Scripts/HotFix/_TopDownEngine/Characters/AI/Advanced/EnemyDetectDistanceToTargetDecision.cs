using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This Decision will return true if the current Brain's Target is within the specified range, false otherwise.
    /// </summary>
    public class EnemyDetectDistanceToTargetDecision : EnemyDecision
    {
        /// The possible comparison modes
        public enum ComparisonModes
        {
            Lower,
            LowerEqual,
            Equals,
            GreaterEqual,
            Greater
        }

        /// the possible rotation modes for the character
        public enum ComparisonDirections
        {
            Free,
            Horizontal,
            Vertical,
        }

        /// the comparison mode
        [Tooltip("the comparison mode")]
        public ComparisonModes ComparisonMode = ComparisonModes.GreaterEqual;

        public ComparisonDirections ComparisonDirection = ComparisonDirections.Free;

        /// the distance to compare with
        [Tooltip("the distance to compare with")]
        public float Distance;

        /// <summary>
        /// On Decide we check our distance to the Target
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
        {
            return EvaluateDistance();
        }

        /// <summary>
        /// Returns true if the distance conditions are met
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateDistance()
        {
            if (_brain.NoTarget)
                return false;

            float distance = 0F;
            var selfPos = transform.position;
            var targetPos = getTargetPosition();
            switch (ComparisonDirection)
            {
                case ComparisonDirections.Free:
                    distance = Vector3.Distance(selfPos, targetPos);
                    break;
                case ComparisonDirections.Horizontal:
                    selfPos.y = 0;
                    targetPos.y = 0;
                    distance = (selfPos.x - targetPos.x).abs();
                    break;
                case ComparisonDirections.Vertical:
                    selfPos.x = 0;
                    targetPos.x = 0;
                    distance = (selfPos.y - targetPos.y).abs();
                    break;
            }

            return ComparisonMode switch
            {
                ComparisonModes.Lower => distance < Distance,
                ComparisonModes.LowerEqual => distance <= Distance,
                ComparisonModes.Equals => distance.isEqual(Distance),
                ComparisonModes.GreaterEqual => distance >= Distance,
                ComparisonModes.Greater => distance > Distance,
                _ => false
            };
        }
    }
}