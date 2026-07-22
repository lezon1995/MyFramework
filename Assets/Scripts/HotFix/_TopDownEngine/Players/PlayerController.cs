using UnityEngine;

namespace MoreMountains
{
    public class PlayerController : TopDownController2D
    {
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            // 玩家：意图速度由 CurrentMovement 提供，平滑过渡
            IntentVelocity = Vector2.Lerp(IntentVelocity, CurrentMovement, Time.fixedDeltaTime * 10f);
            Vector2 totalVel = IntentVelocity + KnockbackVelocity;
            Position += totalVel * Time.fixedDeltaTime;
            transform.position = Position;
        }
    }
}