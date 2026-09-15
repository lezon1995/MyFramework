using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Makes the character move vertically downward at a constant speed.
    /// </summary>
    public class EnemyMoveDownAction : EnemyAction
    {
        [Tooltip("the downward movement speed multiplier")]
        public float MoveDownSpeedMultiplier = 1f;

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            // 始终向下移动（竖直方向）
            var movement = Vector2.down * (_movement._movementSpeed * MoveDownSpeedMultiplier);
            _movement.SetMovement(movement);
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
        }
    }
}
