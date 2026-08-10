using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Brick的体积感扩展
    /// 集成EntityBody2D和VolumeManager
    /// </summary>
    public static class BrickVolumeExtensions
    {
        /// <summary>
        /// 获取Brick的体积数据组件
        /// </summary>
        public static TopDownController2D GetVolumeBody(this Brick brick)
        {
            return brick?.GetComponent<TopDownController2D>();
        }

        /// <summary>
        /// 为Brick添加或获取体积数据组件
        /// </summary>
        public static TopDownController2D EnsureVolumeBody(this Brick brick)
        {
            if (brick == null) 
                return null;

            var body = brick.GetComponent<TopDownController2D>();
            if (body == null)
            {
                body = brick.gameObject.AddComponent<TopDownController2D>();
                // 根据Brick的size设置默认体积
                body.Volume = new VolumeShape
                {
                    Shape = VolumeShapeType.Circle,
                    Radius = Mathf.Min(brick.size.x, brick.size.y) * 0.5f
                };
            }
            return body;
        }

        /// <summary>
        /// 应用外部击退力
        /// </summary>
        public static void ApplyKnockbackForce(this Brick brick, Vector2 direction, float force)
        {
            if (brick == null || VolumeManager.Instance == null) 
                return;

            var body = brick.GetVolumeBody();
            if (body != null)
            {
                VolumeManager.Instance.ApplyKnockback(body, direction, force);
            }
        }

        /// <summary>
        /// 获取Brick当前的重叠信息
        /// </summary>
        public static List<VolumeCollisionResult> GetOverlapInfo(this Brick brick)
        {
            var results = new List<VolumeCollisionResult>();
            if (brick == null || VolumeManager.Instance == null) 
                return results;

            var body = brick.GetVolumeBody();
            if (body == null) 
                return results;

            using var _ = new ListScope<TopDownController2D>(out var entities);
            VolumeManager.Instance.GetEntitiesInRadius(body.VolumeCenter, body.Volume.BoundingRadius * 3f, ref entities);
            foreach (var entity in entities)
            {
                if (entity == body) 
                    continue;

                var result = new VolumeCollisionResult(body, entity);
                if (result.IsColliding)
                {
                    results.Add(result);
                }
            }

            return results;
        }
    }

    /// <summary>
    /// Brick的挤压效果参数
    /// </summary>
    [Serializable]
    public struct BrickSqueezeEffect
    {
        [Tooltip("是否启用挤压效果")]
        public bool Enabled;

        [Tooltip("挤压速度倍率")]
        [Range(0f, 2f)]
        public float SqueezeSpeedMultiplier;

        [Tooltip("挤压后的恢复速度")]
        [Range(0f, 2f)]
        public float RecoverySpeed;

        [Tooltip("最大挤压量（0-1）")]
        [Range(0f, 0.5f)]
        public float MaxSqueezeAmount;
    }

    /// <summary>
    /// Brick体积感Power
    /// 管理Brick的体积感表现
    /// </summary>
    public class BrickPower_Volume : BrickPower
    {
        [NonSerialized] public TopDownController2D VolumeBody;
        [NonSerialized] public BrickSqueezeEffect SqueezeEffect;
        [NonSerialized] public Vector2 LastKnockbackVelocity;
        [NonSerialized] public float SqueezeTimer;

        public override void onCreate()
        {
            base.onCreate();
            ID = "Volume";
        }

        public override void resetProperty()
        {
            base.resetProperty();
            VolumeBody = null;
            SqueezeEffect = default;
            LastKnockbackVelocity = default;
            SqueezeTimer = 0;
        }

        public override void onGainPower(Brick brick)
        {
            VolumeBody = brick.EnsureVolumeBody();
            if (VolumeBody != null)
            {
                VolumeManager.Instance?.Register(VolumeBody);
            }
        }

        public override void onLosePower(Brick brick)
        {
            if (VolumeBody != null && VolumeManager.Instance != null)
            {
                VolumeManager.Instance.Unregister(VolumeBody);
            }
        }

        protected override void onUpdate(float dt)
        {
            if (!SqueezeEffect.Enabled || VolumeBody == null) 
                return;

            // 恢复挤压效果
            if (SqueezeTimer > 0)
            {
                SqueezeTimer -= dt * SqueezeEffect.RecoverySpeed;
                if (SqueezeTimer < 0) SqueezeTimer = 0;
            }
        }

        public override void onKnockbackReceived(Brick brick, Vector2 direction, float force)
        {
            if (VolumeBody == null) 
                return;

            LastKnockbackVelocity = direction.normalized * force;
            VolumeBody.AddImpact(direction, force);

            if (SqueezeEffect.Enabled)
            {
                SqueezeTimer = Mathf.Min(SqueezeTimer + force * 0.1f, SqueezeEffect.MaxSqueezeAmount);
            }
        }

        /// <summary>
        /// 获取当前挤压百分比
        /// </summary>
        public float GetCurrentSqueezePercent()
        {
            if (!SqueezeEffect.Enabled || SqueezeEffect.MaxSqueezeAmount <= 0) 
                return 0;
            return SqueezeTimer / SqueezeEffect.MaxSqueezeAmount;
        }
    }
}
