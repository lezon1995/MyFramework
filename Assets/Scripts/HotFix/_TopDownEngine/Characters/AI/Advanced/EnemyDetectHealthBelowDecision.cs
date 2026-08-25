using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This decision will return true if the character's health is below the specified threshold percentage.
    /// </summary>
    public class EnemyDetectHealthBelowDecision : EnemyDecision
    {
        [Tooltip("the percentage of health below which this decision will return true (0 to 1)")]
        [Range(0f, 1f)]
        public float HealthThreshold = 0.3f;

        [Tooltip("if true, will also return true when dead")]
        public bool ReturnTrueWhenDead = false;

        protected Health _health;

        public override void Initialization()
        {
            base.Initialization();
            if (brick != null)
            {
                _health = brick.Health;
            }
        }

        public override bool Decide()
        {
            return EvaluateHealth();
        }

        protected virtual bool EvaluateHealth()
        {
            if (_health == null)
                return false;

            if (ReturnTrueWhenDead && _health.IsDead())
                return true;

            float healthPercent = _health.CurrentHealth / _health.MaximumHealth;
            return healthPercent < HealthThreshold;
        }
    }
}
