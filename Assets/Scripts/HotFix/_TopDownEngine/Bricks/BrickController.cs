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
            Vector3 totalVel;
            
            // 优先使用 CharacterMovement 的输出（经过加速/减速处理）
            if (_characterMovement && _characterMovement.ShouldSetMovement)
            {
                totalVel = CurrentMovement;
                // 同步 CurrentMovement 到 IntentVelocity，供 VolumeManager 等系统使用
                IntentVelocity = CurrentMovement;
            }
            else
            {
                // 没有 CharacterMovement 或它不设置移动时，使用 IntentVelocity
                totalVel = IntentVelocity;
            }
            
            // 叠加击退速度（被动效果，独立于主动移动）
            totalVel += KnockbackVelocity;
            
            // 计算位移 = 速度 × 时间
            Position += (Vector2)totalVel * dt;
            transform.position = Position;
        }
    }
}