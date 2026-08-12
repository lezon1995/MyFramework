using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 击退源接口
    /// 用于各种可以产生击退效果的对象
    /// </summary>
    public interface IKnockbackSource
    {
        float GetKnockbackForce(); // 获取击退力的强度
        Vector2 GetKnockbackDirection(); // 获取击退方向
        Vector2 GetKnockbackPosition(); // 获取击退位置（用于计算方向）
        bool IsChainKnockbackEnabled(); // 是否启用链式击退
    }

    /// <summary>
    /// 击退力信息结构
    /// </summary>
    [Serializable]
    public struct KnockbackInfo
    {
        [Tooltip("击退方向")] public Vector2 Direction;
        [Tooltip("击退力大小")] public float Force;
        [Tooltip("击退位置")] public Vector2 Position;
        [Tooltip("是否触发链式击退")] public bool EnableChain;
        [Tooltip("链式击退衰减率")] [Range(0f, 1f)] public float ChainDecay;

        public KnockbackInfo(Vector2 direction, float force, Vector2 position, bool enableChain = true, float chainDecay = 0.6f)
        {
            Direction = direction.normalized;
            Force = force;
            Position = position;
            EnableChain = enableChain;
            ChainDecay = chainDecay;
        }

        /// <summary>
        /// 从碰撞法线创建击退信息
        /// </summary>
        public static KnockbackInfo FromHitNormal(Vector2 hitPoint, Vector2 hitNormal, float force, bool enableChain = true, float chainDecay = 0.6f)
        {
            return new(-hitNormal, force, hitPoint, enableChain, chainDecay);
        }

        /// <summary>
        /// 从目标位置创建击退信息
        /// </summary>
        public static KnockbackInfo ToTarget(Vector2 source, Vector2 target, float force, bool enableChain = true, float chainDecay = 0.6f)
        {
            Vector2 direction = target - source;
            return new(direction, force, source, enableChain, chainDecay);
        }
    }

    /// <summary>
    /// 击退力应用器
    /// 提供统一的击退力应用接口
    /// </summary>
    public static class KnockbackApplier
    {
        /// <summary>
        /// 对目标施打击退力
        /// </summary>
        public static void Apply(TopDownController2D target, in KnockbackInfo info)
        {
            if (target == null || volumeManager == null) 
                return;
            
            if (info.Force < 0.01f)
                return;

            volumeManager.ApplyKnockback(target, info.Direction, info.Force);
        }

        /// <summary>
        /// 对目标施打击退力（从击退源）
        /// </summary>
        public static void Apply(TopDownController2D target, IKnockbackSource source)
        {
            if (target == null || source == null || volumeManager == null) 
                return;

            var direction = source.GetKnockbackDirection();
            var force = source.GetKnockbackForce();
            volumeManager.ApplyKnockback(target, direction, force);
        }

        /// <summary>
        /// 对区域内的所有实体施打击退力
        /// </summary>
        public static void ApplyAreaKnockback(Vector2 center, float radius, in KnockbackInfo info)
        {
            if (volumeManager == null) 
                return;

            using var _ = new ListScope<TopDownController2D>(out var entities);
            volumeManager.GetEntitiesInRadius(center, radius, ref entities);
            foreach (var entity in entities)
            {
                Vector2 toEntity = entity.Position - center;
                float dist = toEntity.magnitude;
                if (dist < 0.01f) 
                    continue;

                // 距离衰减
                float distanceFactor = 1f - (dist / radius);
                float adjustedForce = info.Force * distanceFactor;

                KnockbackInfo adjustedInfo = info;
                adjustedInfo.Force = adjustedForce;
                adjustedInfo.Direction = toEntity.normalized;

                Apply(entity, adjustedInfo);
            }
        }

        /// <summary>
        /// 对扇形区域内的实体施打击退力
        /// </summary>
        public static void ApplyConeKnockback(Vector2 origin, Vector2 direction, float angle, float radius, in KnockbackInfo info)
        {
            if (volumeManager == null)
                return;

            float halfAngle = angle * 0.5f;
            float cosThreshold = Mathf.Cos(halfAngle * Mathf.Deg2Rad);

            using var _ = new ListScope<TopDownController2D>(out var entities);
            volumeManager.GetEntitiesInRadius(origin, radius, ref entities);
            foreach (var entity in entities)
            {
                Vector2 toEntity = entity.Position - origin;
                float dist = toEntity.magnitude;
                if (dist < 0.01f) 
                    continue;

                toEntity = toEntity.normalized;
                float dot = Vector2.Dot(direction, toEntity);

                if (dot >= cosThreshold)
                {
                    float angleFactor = (dot - cosThreshold) / (1f - cosThreshold);
                    float distanceFactor = 1f - (dist / radius);
                    float adjustedForce = info.Force * angleFactor * distanceFactor;

                    KnockbackInfo adjustedInfo = info;
                    adjustedInfo.Force = adjustedForce;
                    adjustedInfo.Direction = toEntity;

                    Apply(entity, adjustedInfo);
                }
            }
        }
    }

    /// <summary>
    /// 简单击退源实现
    /// </summary>
    [Serializable]
    public class SimpleKnockbackSource : IKnockbackSource
    {
        [Tooltip("击退方向")] public Vector2 Direction = Vector2.right;
        [Tooltip("击退力大小")] public float Force = 10f;
        [Tooltip("击退位置")] public Vector2 Position = Vector2.zero;
        [Tooltip("是否启用链式击退")] public bool EnableChain = true;
        public virtual float GetKnockbackForce() => Force;
        public virtual Vector2 GetKnockbackDirection() => Direction.normalized;
        public virtual Vector2 GetKnockbackPosition() => Position;
        public virtual bool IsChainKnockbackEnabled() => EnableChain;
    }

    /// <summary>
    /// 基于点的击退源
    /// 击退方向由自身指向目标
    /// </summary>
    [Serializable]
    public class PointKnockbackSource : IKnockbackSource
    {
        [Tooltip("击退力大小")] public float Force = 10f;
        [Tooltip("击退位置")] public Vector2 Position = Vector2.zero;
        [Tooltip("目标位置（击退方向从这里指向自身）")] public Vector2 TargetPosition = Vector2.zero;
        [Tooltip("是否启用链式击退")] public bool EnableChain = true;

        public virtual float GetKnockbackForce() => Force;

        public virtual Vector2 GetKnockbackDirection()
        {
            Vector2 dir = Position - TargetPosition;
            return dir.magnitude > 0.01f ? dir.normalized : Vector2.right;
        }

        public virtual Vector2 GetKnockbackPosition() => Position;
        public virtual bool IsChainKnockbackEnabled() => EnableChain;
    }
}