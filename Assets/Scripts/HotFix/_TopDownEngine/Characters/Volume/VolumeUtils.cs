using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    /// <summary>
    /// 体积感系统辅助工具
    /// </summary>
    public static class VolumeUtils
    {
        /// <summary>
        /// 计算两个圆形碰撞体是否相交
        /// </summary>
        public static bool CircleIntersectsCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            float distSq = (centerA - centerB).sqrMagnitude;
            float radiusSum = radiusA + radiusB;
            return distSq <= radiusSum * radiusSum;
        }

        /// <summary>
        /// 计算两个圆形碰撞体的重叠量
        /// </summary>
        public static float GetCircleOverlap(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            float dist = (centerA - centerB).magnitude;
            float combinedRadius = radiusA + radiusB;
            return combinedRadius - dist;
        }

        /// <summary>
        /// 计算点到圆的距离
        /// </summary>
        public static float DistanceToCircle(Vector2 point, Vector2 center, float radius)
        {
            return (point - center).magnitude - radius;
        }

        /// <summary>
        /// 获取从A指向B的分离向量（当两圆重叠时）
        /// </summary>
        public static Vector2 GetSeparationVector(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB, float maxOverlapRatio = 0.3f)
        {
            Vector2 direction = centerB - centerA;
            float dist = direction.magnitude;
            if (dist < 0.001f) 
                return Vector2.right;

            float combinedRadius = radiusA + radiusB;
            float overlap = combinedRadius - dist;

            if (overlap <= 0) 
                return Vector2.zero;

            float maxAllowedOverlap = combinedRadius * maxOverlapRatio;
            float requiredSeparation = overlap - maxAllowedOverlap;
            if (requiredSeparation <= 0) 
                return Vector2.zero;

            return direction.normalized * requiredSeparation;
        }

        /// <summary>
        /// 计算分离位置
        /// </summary>
        public static (Vector2 posA, Vector2 posB) CalculateSeparation(
            Vector2 posA, float radiusA, float massA,
            Vector2 posB, float radiusB, float massB,
            float maxOverlapRatio = 0.3f)
        {
            Vector2 direction = posB - posA;
            float dist = direction.magnitude;
            if (dist < 0.001f)
            {
                direction = Random.insideUnitCircle;
                dist = 0.001f;
            }

            float combinedRadius = radiusA + radiusB;
            float overlap = combinedRadius - dist;

            if (overlap <= 0) 
                return (posA, posB);

            float maxAllowedOverlap = combinedRadius * maxOverlapRatio;
            float requiredSeparation = overlap - maxAllowedOverlap;
            if (requiredSeparation <= 0) 
                return (posA, posB);

            float totalMass = massA + massB;
            if (totalMass <= 0) 
                return (posA, posB);

            float ratioA = massB / totalMass;
            float ratioB = massA / totalMass;

            Vector2 dir = direction.normalized;
            return (posA - dir * requiredSeparation * ratioA, posB + dir * requiredSeparation * ratioB);
        }

        /// <summary>
        /// 获取圆形在指定方向的最近点
        /// </summary>
        public static Vector2 GetClosestPointOnCircle(Vector2 point, Vector2 center, float radius)
        {
            Vector2 direction = point - center;
            float dist = direction.magnitude;
            if (dist < 0.001f) 
                return center + Vector2.right * radius;

            return center + direction.normalized * radius;
        }

        /// <summary>
        /// 计算推挤效果
        /// </summary>
        public static (Vector2 velA, Vector2 velB) CalculateSqueeze(
            Vector2 velA, float massA,
            Vector2 velB, float massB,
            Vector2 collisionDir,
            float relativeSpeed,
            float dt,
            float massInfluence = 0.5f,
            float velocityInfluence = 0.3f)
        {
            if (relativeSpeed < 0.01f) 
                return (velA, velB);

            float totalMass = massA + massB;
            if (totalMass <= 0) 
                return (velA, velB);

            float squeezeStrength = relativeSpeed * (1f + massInfluence) * (1f + velocityInfluence);
            float massRatioA = massB / totalMass;
            float massRatioB = massA / totalMass;

            Vector2 squeezeDir = -collisionDir;

            float squeezeA = relativeSpeed * massRatioA * squeezeStrength * dt * 0.5f;
            float squeezeB = relativeSpeed * massRatioB * squeezeStrength * dt * 0.5f;

            return (velA + squeezeDir * squeezeA, velB + squeezeDir * squeezeB);
        }

        /// <summary>
        /// 获取圆内随机点
        /// </summary>
        public static Vector2 RandomPointInCircle(Vector2 center, float radius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(0f, radius);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        /// <summary>
        /// 在圆环内随机点
        /// </summary>
        public static Vector2 RandomPointInAnnulus(Vector2 center, float innerRadius, float outerRadius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(innerRadius, outerRadius);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        /// <summary>
        /// 计算角度（弧度）到向量的投影系数
        /// </summary>
        public static float ProjectOnDirection(Vector2 vector, Vector2 direction)
        {
            return Vector2.Dot(vector, direction);
        }

        /// <summary>
        /// 角度转方向向量
        /// </summary>
        public static Vector2 AngleToDirection(float angle)
        {
            return new(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>
        /// 方向向量转角度
        /// </summary>
        public static float DirectionToAngle(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x);
        }

        /// <summary>
        /// 线性衰减
        /// </summary>
        public static float LinearDecay(float value, float min, float max)
        {
            if (max <= min)
                return 1f;

            float t = Mathf.Clamp01((value - min) / (max - min));
            return 1f - t;
        }

        /// <summary>
        /// 指数衰减
        /// </summary>
        public static float ExponentialDecay(float value, float decayRate, float dt)
        {
            return value * Mathf.Pow(1f - decayRate, dt);
        }

        /// <summary>
        /// 距离反比衰减
        /// </summary>
        public static float InverseDistanceDecay(float force, float distance, float minDistance = 0.1f)
        {
            float effectiveDist = Mathf.Max(distance, minDistance);
            return force / effectiveDist;
        }
    }

    /// <summary>
    /// 体积感配置预设
    /// </summary>
    [Serializable]
    public class VolumePreset
    {
        public string Name = "Preset";
        public float Radius = 0.5f;
        public float Mass = 1f;
        public float MaxOverlapRatio = 0.3f;
        public float KnockbackResistance;
        public float PushForceWeight = 1f;

        public void ApplyTo(TopDownController2D body)
        {
            if (body == null) 
                return;

            body.Radius = Radius;
            body.Mass = Mass;
            body.MaxOverlapRatio = MaxOverlapRatio;
            body.KnockbackResistance = KnockbackResistance;
            body.PushForceWeight = PushForceWeight;
        }

        public static VolumePreset LightWeight => new()
        {
            Name = "Light",
            Radius = 0.3f,
            Mass = 0.5f,
            MaxOverlapRatio = 0.4f,
            KnockbackResistance = 0f,
            PushForceWeight = 0.5f
        };

        public static VolumePreset Normal => new()
        {
            Name = "Normal",
            Radius = 0.5f,
            Mass = 1f,
            MaxOverlapRatio = 0.3f,
            KnockbackResistance = 0f,
            PushForceWeight = 1f
        };

        public static VolumePreset Heavy => new()
        {
            Name = "Heavy",
            Radius = 0.8f,
            Mass = 3f,
            MaxOverlapRatio = 0.1f,
            KnockbackResistance = 0.3f,
            PushForceWeight = 2f
        };

        public static VolumePreset Player => new()
        {
            Name = "Player",
            Radius = 0.5f,
            Mass = 2f,
            MaxOverlapRatio = 0.2f,
            KnockbackResistance = 0.5f,
            PushForceWeight = 3f
        };
    }

    /// <summary>
    /// 体积感管理器扩展方法
    /// </summary>
    public static class VolumeManagerExtensions
    {
        /// <summary>
        /// 批量注册怪物
        /// </summary>
        public static void RegisterMonsters(this VolumeManager manager, List<Brick> bricks)
        {
            if (manager == null) 
                return;

            foreach (var brick in bricks)
            {
                var body = brick.GetComponent<TopDownController2D>();
                if (body)
                {
                    manager.Register(body);
                }
            }
        }

        /// <summary>
        /// 批量注册带BrickVolumeBody的怪物
        /// </summary>
        public static void RegisterBrickVolumeBodies(this VolumeManager manager, List<Brick> bricks)
        {
            if (manager == null) 
                return;

            foreach (var brick in bricks)
            {
                var body = brick.GetComponent<BrickVolumeBody>();
                if (body)
                {
                    manager.Register(body.Body);
                }
            }
        }

        /// <summary>
        /// 获取所有在圆形区域内的实体
        /// </summary>
        public static List<TopDownController2D> GetEntitiesInCircle(this VolumeManager manager, Vector2 center, float radius)
        {
            if (manager == null) 
                return new();
            return manager.GetEntitiesInRadius(center, radius);
        }

        /// <summary>
        /// 从玩家向外施打击退力
        /// </summary>
        public static void ApplyRadialKnockbackFrom(this VolumeManager manager, TopDownController2D source, float force, float? radius = null)
        {
            if (manager == null || source == null) 
                return;

            float checkRadius = radius ?? source.Radius * 5f;
            var entities = manager.GetEntitiesInRadius(source.Position, checkRadius);

            foreach (var entity in entities)
            {
                if (entity == source) 
                    continue;

                Vector2 direction = (entity.Position - source.Position);
                float dist = direction.magnitude;
                if (dist < 0.01f) 
                    continue;

                float distanceFactor = 1f - (dist / checkRadius);
                manager.ApplyKnockback(entity, direction.normalized, force * distanceFactor);
            }
        }
    }
}