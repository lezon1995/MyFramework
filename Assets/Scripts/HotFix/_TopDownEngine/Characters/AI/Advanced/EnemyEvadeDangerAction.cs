using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Requires a CharacterMovement ability. Makes the character move away from danger sources (projectiles, explosions, etc.).
    /// </summary>
    public class EnemyEvadeDangerAction : EnemyAction
    {
        [Tooltip("the layers to check for danger sources")]
        public LayerMask DangerLayerMask = LayerManager.Ball_Mask;

        [Tooltip("the radius to detect danger")]
        public float DangerDetectionRadius = 3f;

        [Tooltip("the force multiplier for evasion")]
        public float EvasionForce = 1.5f;

        [Tooltip("the minimum distance to maintain from danger")]
        public float SafeDistance = 2f;

        protected Collider2D[] _results;

        public override void Initialization()
        {
            base.Initialization();
            _results ??= new Collider2D[10];
        }

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Evade();
        }

        protected virtual void Evade()
        {
            Vector2 evasionDirection = Vector2.zero;
            Vector3 selfPos = transform.position;
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(DangerLayerMask);
            int dangerCount = Physics2D.OverlapCircle(selfPos, DangerDetectionRadius, filter, _results);

            for (int i = 0; i < dangerCount; i++)
            {
                if (_results[i] != null)
                {
                    Vector2 awayFromDanger = (selfPos - _results[i].transform.position).normalized;
                    float distance = Vector3.Distance(selfPos, _results[i].transform.position);
                    float weight = 1f / Mathf.Max(distance, 0.1f);

                    evasionDirection += awayFromDanger * weight;
                }
            }

            if (evasionDirection.magnitude > 0.1f)
            {
                evasionDirection = evasionDirection.normalized * EvasionForce;
                _movement.SetMovement(evasionDirection);
            }
            else
            {
                _movement.SetMovement(Vector2.zero);
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
        }
    }
}