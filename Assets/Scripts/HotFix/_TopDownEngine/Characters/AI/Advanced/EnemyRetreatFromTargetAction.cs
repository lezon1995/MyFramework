using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Requires a CharacterMovement ability. Makes the character move away from the target (retreat).
    /// </summary>
    public class EnemyRetreatFromTargetAction : EnemyAction
    {
        [Tooltip("the speed multiplier for retreat movement")]
        public float RetreatSpeedMultiplier = 1.5f;

        [Tooltip("the minimum distance to maintain from the target")]
        public float MinimumRetreatDistance = 3f;

        [Tooltip("if true, will stop retreating once outside the maximum retreat distance")]
        public bool StopAtMaxDistance;

        [Tooltip("the maximum distance to retreat to")]
        public float MaximumRetreatDistance = 10f;

        protected Vector2 _direction;

        public override void PerformAction(float dt)
        {
            if (_brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Retreat();
        }

        protected virtual void Retreat()
        {
            if (_brain.NoTarget)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            var targetPos = _brain.Target.position;
            var selfPos = transform.position;

            // 计算远离目标的方向
            _direction = (selfPos - targetPos).normalized;

            // 检查是否超过最大撤退距离
            if (StopAtMaxDistance)
            {
                float distanceToTarget = Vector3.Distance(selfPos, targetPos);
                if (distanceToTarget > MaximumRetreatDistance)
                {
                    _movement.SetMovement(Vector2.zero);
                    return;
                }
            }

            // 只通过 CharacterMovement 设置移动
            _movement.SetMovement(_direction, RetreatSpeedMultiplier);
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
        }
    }
}
