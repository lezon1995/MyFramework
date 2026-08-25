using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This decision will return true if there are multiple targets within detection radius.
    /// Useful for area-of-effect attacks or special behavior when surrounded.
    /// </summary>
    public class EnemyDetectMultipleTargetsDecision : EnemyDecision
    {
        [Tooltip("the radius to search for targets")]
        public float Radius = 5f;

        [Tooltip("the layers to search for targets on")]
        public LayerMask TargetLayer;

        [Tooltip("the minimum number of targets required to return true")]
        public int MinimumTargetCount = 3;

        protected Collider2D[] _results;

        public override void Initialization()
        {
            base.Initialization();
            _results ??= new Collider2D[20];
        }

        public override bool Decide()
        {
            return CountTargets() >= MinimumTargetCount;
        }

        protected virtual int CountTargets()
        {
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(TargetLayer);
            int count = Physics2D.OverlapCircle(transform.position, Radius, filter, _results);
            int validCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (_results[i] != null && _results[i].gameObject != _brain.Owner)
                {
                    validCount++;
                }
            }

            return validCount;
        }
    }
}
