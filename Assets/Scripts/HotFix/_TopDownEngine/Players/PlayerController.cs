using UnityEngine;

namespace MoreMountains
{
    public class PlayerController : TopDownController2D
    {
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        
        protected override void ApplyVelocity(float dt)
        {
            // 玩家：意图速度由 CurrentMovement 提供，平滑过渡
            IntentVelocity = Vector2.Lerp(IntentVelocity, CurrentMovement, dt * 10f);
            Vector2 totalVel = TotalVelocity;
            Position += totalVel * dt;
            transform.position = Position;
        }
    }
}