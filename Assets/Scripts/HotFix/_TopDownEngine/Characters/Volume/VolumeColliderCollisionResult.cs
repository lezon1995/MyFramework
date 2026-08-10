using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 实体与固体碰撞体的碰撞结果
    /// </summary>
    public struct VolumeColliderCollisionResult
    {
        public VolumeCollider Collider;             // 固体碰撞体
        public TopDownController2D Entity;          // 实体
        /// <summary>实体中心到碰撞体表面的距离（当实体中心在碰撞体内部时为负数）</summary>
        public float SurfaceDistance;
        /// <summary>从碰撞体表面指向实体中心的法线（用于推出方向）</summary>
        public Vector2 SurfaceNormal;
        /// <summary>重叠量（> 0 表示有碰撞）</summary>
        public float Overlap;
        public bool IsColliding => Overlap > 0;

        public VolumeColliderCollisionResult(TopDownController2D entity, VolumeCollider collider)
        {
            Collider = collider;
            Entity = entity;

            // 获取实体中心到碰撞体表面的距离
            // 如果实体中心在碰撞体内部，distance 为负数或 0
            // 如果实体中心在碰撞体外部，distance 为正数
            // 使用包围圆半径作为穿透深度基准
            collider.TryGetDistanceAndNormal(entity.VolumeCenter, entity.Volume.BoundingRadius, out float distance, out Vector2 normal);

            SurfaceDistance = distance;
            SurfaceNormal = normal;

            // 重叠量 = 实体包围圆半径 - 实体中心到碰撞体表面的距离
            Overlap = entity.Volume.BoundingRadius - distance;
        }
    }
}
