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
        /// <summary>实体中心到碰撞体表面的距离（穿透时为 0）</summary>
        public float SurfaceDistance;
        /// <summary>从碰撞体表面指向实体中心的法线（用于推出方向）</summary>
        public Vector2 SurfaceNormal;
        /// <summary>重叠量（> 0 表示有碰撞，等于 entity.Radius - SurfaceDistance）</summary>
        public float Overlap;
        public bool IsColliding => Overlap > 0;

        public VolumeColliderCollisionResult(TopDownController2D entity, VolumeCollider collider)
        {
            Collider = collider;
            Entity = entity;

            collider.TryGetDistanceAndNormal(entity.Position, entity.Radius, out float surfaceDist, out Vector2 normal);

            SurfaceDistance = surfaceDist;
            SurfaceNormal = normal;
            Overlap = entity.Radius - surfaceDist;
        }
    }
}
