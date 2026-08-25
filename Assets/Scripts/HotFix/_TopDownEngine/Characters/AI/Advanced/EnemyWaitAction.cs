using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An action that makes the character stationary and wait, with optional random duration.
    /// Useful for idle behavior or timing attacks.
    /// </summary>
    public class EnemyWaitAction : EnemyAction
    {
        [Tooltip("the minimum wait duration in seconds")]
        public float MinWaitDuration = 1f;

        [Tooltip("the maximum wait duration in seconds")]
        public float MaxWaitDuration = 3f;

        [Tooltip("if true, will wait for a random duration between min and max")]
        public bool RandomDuration = true;

        [Tooltip("if RandomDuration is false, the fixed duration to wait")]
        public float FixedDuration = 2f;

        protected float _waitTimer;
        protected float _currentWaitDuration;

        public override void Initialization()
        {
            base.Initialization();
            StartNewWait();
        }

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            _waitTimer += dt;

            if (_waitTimer >= _currentWaitDuration)
            {
                _waitTimer = 0f;
                StartNewWait();
            }

            _movement.SetMovement(Vector2.zero);
        }

        protected virtual void StartNewWait()
        {
            if (RandomDuration)
            {
                _currentWaitDuration = Random.Range(MinWaitDuration, MaxWaitDuration);
            }
            else
            {
                _currentWaitDuration = FixedDuration;
            }
            _waitTimer = 0f;
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
            StartNewWait();
        }
    }
}
