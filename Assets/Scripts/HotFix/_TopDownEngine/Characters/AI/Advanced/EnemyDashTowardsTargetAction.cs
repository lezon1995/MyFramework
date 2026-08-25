using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An action that makes the character dash towards the target. Useful for charge attacks or quick approach.
    /// </summary>
    public class EnemyDashTowardsTargetAction : EnemyAction
    {
        [Tooltip("the dash speed multiplier")]
        public float DashSpeedMultiplier = 3f;

        [Tooltip("the duration of the dash in seconds")]
        public float DashDuration = 0.3f;

        [Tooltip("the cooldown between dashes in seconds")]
        public float DashCooldown = 2f;

        [Tooltip("the minimum distance to trigger a dash")]
        public float MinDistanceToDash = 5f;

        [Tooltip("the maximum distance to dash")]
        public float MaxDashDistance = 10f;

        protected float _dashTimer;
        protected float _cooldownTimer;
        protected bool _isDashing;
        protected Vector2 _dashDirection;

        public override void Initialization()
        {
            base.Initialization();
            _dashTimer = 0f;
            _cooldownTimer = 0f;
            _isDashing = false;
        }

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            if (_isDashing)
            {
                PerformDash();
            }
            else
            {
                CheckForDash();
            }

            UpdateTimers(dt);
        }

        protected virtual void CheckForDash()
        {
            if (_cooldownTimer > 0f || _brain.Target == null)
                return;

            float distance = Vector3.Distance(transform.position, _brain.Target.position);
            if (distance >= MinDistanceToDash && distance <= MaxDashDistance)
            {
                StartDash();
            }
        }

        protected virtual void StartDash()
        {
            if (_brain.Target == null)
                return;

            _isDashing = true;
            _dashTimer = DashDuration;
            _dashDirection = (_brain.Target.position - transform.position).normalized;
        }

        protected virtual void PerformDash()
        {
            if (!_isDashing)
                return;

            var direction = _dashDirection * DashSpeedMultiplier;
            _movement.SetMovement(direction);
        }

        protected virtual void UpdateTimers(float dt)
        {
            if (_isDashing)
            {
                _dashTimer -= dt;
                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                    _cooldownTimer = DashCooldown;
                    _movement.SetMovement(Vector2.zero);
                }
            }
            else if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= dt;
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _isDashing = false;
            _movement.SetMovement(Vector2.zero);
        }
    }
}
