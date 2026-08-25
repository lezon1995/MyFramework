using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An action that makes the character patrol between waypoints.
    /// Requires a CharacterMovement ability.
    /// </summary>
    public class EnemyPatrolAction : EnemyAction
    {
        [Tooltip("the waypoints to patrol between")]
        public Transform[] Waypoints;

        [Tooltip("the speed multiplier during patrol")]
        public float PatrolSpeedMultiplier = 0.5f;

        [Tooltip("the distance threshold to consider a waypoint reached")]
        public float WaypointReachedThreshold = 0.5f;

        [Tooltip("whether to loop the patrol")]
        public bool LoopPatrol = true;

        protected int _currentWaypointIndex;
        protected Vector2 _direction;

        public override void Initialization()
        {
            base.Initialization();
            _currentWaypointIndex = 0;
        }

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Patrol();
        }

        protected virtual void Patrol()
        {
            if (Waypoints == null || Waypoints.Length == 0)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Transform targetWaypoint = Waypoints[_currentWaypointIndex];
            if (targetWaypoint == null)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Vector3 selfPos = transform.position;
            Vector3 waypointPos = targetWaypoint.position;
            float distance = Vector3.Distance(selfPos, waypointPos);

            // 检查是否到达路点
            if (distance < WaypointReachedThreshold)
            {
                if (LoopPatrol)
                {
                    _currentWaypointIndex = (_currentWaypointIndex + 1) % Waypoints.Length;
                }
                else
                {
                    if (_currentWaypointIndex < Waypoints.Length - 1)
                    {
                        _currentWaypointIndex++;
                    }
                    else
                    {
                        _movement.SetMovement(Vector2.zero);
                        return;
                    }
                }
            }

            // 移动向路点
            _direction = (waypointPos - selfPos).normalized * PatrolSpeedMultiplier;

            // 只通过 CharacterMovement 设置移动
            _movement.SetMovement(_direction);
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
        }
    }
}
