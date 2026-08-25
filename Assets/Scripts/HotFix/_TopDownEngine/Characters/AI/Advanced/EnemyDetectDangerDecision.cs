using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This decision will return true if the character is within a danger zone (detected by a Damager or projectile nearby).
    /// </summary>
    public class EnemyDetectDangerDecision : EnemyDecision
    {
        [Tooltip("the radius to search for danger")]
        public float DangerRadius = 3f;

        [Tooltip("the layers to check for danger sources")]
        public LayerMask DangerLayerMask = LayerManager.Ball_Mask;

        [Tooltip("the offset from the character center")]
        public Vector3 DetectionOriginOffset;

        protected Collider2D[] _results;

        public override void Initialization()
        {
            base.Initialization();
            _results ??= new Collider2D[10];
        }

        public override bool Decide()
        {
            return DetectDanger();
        }

        protected virtual bool DetectDanger()
        {
            if (_brain == null)
                return false;

            Vector3 origin = transform.position + DetectionOriginOffset;
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(DangerLayerMask);
            int count = Physics2D.OverlapCircle(origin, DangerRadius, filter, _results);

            for (int i = 0; i < count; i++)
            {
                if (_results[i] != null && _results[i].gameObject != _brain.Owner)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
