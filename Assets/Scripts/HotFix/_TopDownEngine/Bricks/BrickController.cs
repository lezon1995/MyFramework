using UnityEngine;

namespace MoreMountains
{
    public class BrickController : TopDownController2D
    {
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        
        protected override void ApplyVelocity(float dt)
        {
            // 怪物：意图速度由 AI 控制，这里只应用总速度
            Vector2 totalVel = TotalVelocity;
            Position += totalVel * dt;
            transform.position = Position;
        }
    }
}