using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This decision will return true if the character is currently in a Cooldown state.
    /// </summary>
    public class EnemyDetectCooldownDecision : EnemyDecision
    {
        [Tooltip("the duration of the cooldown in seconds")]
        public float CooldownDuration = 2f;

        protected float _cooldownTimer;

        public override void Initialization()
        {
            base.Initialization();
            _cooldownTimer = 0f;
        }

        public override bool Decide()
        {
            return _cooldownTimer > 0f;
        }

        public virtual void StartCooldown()
        {
            _cooldownTimer = CooldownDuration;
        }

        protected virtual void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }
    }
}
