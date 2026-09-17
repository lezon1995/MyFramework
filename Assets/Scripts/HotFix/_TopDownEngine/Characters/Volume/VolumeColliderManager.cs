using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreMountains
{
    [Serializable]
    public class VolumeColliderManager
    {
        [Header("空间分区设置")]
        [Tooltip("空间分区网格大小（建议设置为最大碰撞半径的2-4倍）")]
        public float SpatialHashCellSize = 1.5f;

        [Header("固体碰撞体（边界/障碍物）")]
        [Tooltip("启用固体碰撞体碰撞检测")]
        public bool EnableSolidColliders = true;

        [Tooltip("是否自动检测场景中的 VolumeCollider")]
        public bool AutoDetectVolumeColliders = true;

        [Tooltip("空间分区网格透明度")]
        [Range(0.1f, 1f)]
        public float SpatialHashGridAlpha = 0.3f;

        VolumeColliderSpatialHash _spatialHash;

        // 运行时数据 - 固体碰撞体
        List<VolumeCollider> _registeredColliders = new();
        List<VolumeCollider> _potentialColliders = new();

        public void InitializeSpatialHash()
        {
            _spatialHash = new(SpatialHashCellSize);
        }

        public void UpdateSpatialHash()
        {
            // 固体碰撞体通常不移动，保持全量重建
            if (EnableSolidColliders && _spatialHash != null)
            {
                _spatialHash.Rebuild(_registeredColliders);
            }
        }

        public void Register(VolumeCollider collider)
        {
            if (_registeredColliders.Contains(collider))
                return;

            _registeredColliders.Add(collider);
            _spatialHash?.Insert(collider);
        }

        public void Unregister(VolumeCollider collider)
        {
            _registeredColliders.Remove(collider);
            _spatialHash?.Remove(collider);
        }


        /// <summary>
        /// 处理实体与固体碰撞体的碰撞
        /// </summary>
        public void ProcessSolidColliderCollisions(List<TopDownController2D> entities, float dt)
        {
            if (!EnableSolidColliders)
                return;
            
            if (_registeredColliders.Count == 0)
                return;

            foreach (var entity in entities)
            {
                GetPotentialSolidColliders(entity);

                foreach (var solid in _potentialColliders)
                {
                    if (!solid.IsEnabled())
                        continue;

                    ProcessEntitySolidCollision(entity, solid, dt);
                }
            }
        }

        /// <summary>
        /// 获取指定实体的潜在固体碰撞体
        /// </summary>
        void GetPotentialSolidColliders(TopDownController2D entity)
        {
            _potentialColliders.Clear();
            _spatialHash.GetPotentialSolids(entity, _potentialColliders);
        }

        /// <summary>
        /// 处理单个实体与固体碰撞体的碰撞
        /// </summary>
        protected virtual void ProcessEntitySolidCollision(TopDownController2D entity, VolumeCollider solid, float dt)
        {
            var result = new VolumeColliderCollisionResult(entity, solid);

            if (!result.IsColliding)
                return;

            // 1. 位置分离：把实体推到表面外（重叠量 + 一点点缓冲，避免下一帧又穿透）
            //    必须保证能在一帧内清掉所有重叠，否则会被持续推 → 抖动
            var pushDistance = result.Overlap + 0.001f;
            var pushDir = result.SurfaceNormal;
            entity.MovePositionBy(pushDir * pushDistance);

            // 2. 速度处理：实体朝墙方向的速度分量需要清除
            //    SurfaceNormal 是从墙指向实体的方向，所以沿这个方向的速度是"远离墙"的，
            //    沿 -SurfaceNormal 的速度才是"撞向墙"，要被消除
            Vector3 totalVel = entity.IntentVelocity + entity.KnockbackVelocity;
            float velIntoWall = Vector2.Dot(totalVel, -pushDir);

            if (velIntoWall > 0)
            {
                // 撞墙中，清除指向墙的速度分量（按质量比保留部分动能）
                // 固体质量视为无限大，所以击退速度完全被挡
                float restitution = 0f; // 不反弹
                Vector3 reflectedVel = totalVel - (-pushDir) * (velIntoWall * (1f + restitution));
                // 把反射后的总速度拆分到 IntentVelocity 和 KnockbackVelocity
                // 简单起见，全部作用在 KnockbackVelocity（IntentVelocity 通常较小）
                Vector3 newTotal = reflectedVel;

                // 保留 IntentVelocity 的切向分量，把垂直分量设为 0
                float intentNormal = Vector2.Dot(entity.IntentVelocity, -pushDir);
                if (intentNormal > 0)
                {
                    entity.IntentVelocity += pushDir * intentNormal;
                }

                // KnockbackVelocity 剩余部分补到 total
                Vector3 intentRemaining = entity.IntentVelocity;
                Vector3 neededKnockback = newTotal - intentRemaining;
                entity.KnockbackVelocity = neededKnockback;
            }
        }

        public void TryAutoDetectSolidColliders()
        {
            if (AutoDetectVolumeColliders)
            {
                AutoDetectSolidColliders();
            }
        }

        /// <summary>
        /// 自动检测场景中的 VolumeCollider 并注册
        /// </summary>
        public void AutoDetectSolidColliders()
        {
            _registeredColliders.Clear();

            var colliders = Object.FindObjectsByType<VolumeCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var col in colliders)
            {
                if (col.IsEnabled() && col.AutoRegister)
                {
                    Register(col);
                }
            }

            Debug.Log($"[VolumeManager] 自动检测到 {_registeredColliders.Count} 个固体碰撞体");
        }


        public void DrawSolidSpatialHashGrid(Color color)
        {
            if (!EnableSolidColliders)
                return;

            float cellSize = _spatialHash.CellSize;
            float alpha = SpatialHashGridAlpha;
            Color c = new Color(color.r, color.g, color.b, alpha);

            foreach (var solid in _registeredColliders)
            {
                var bounds = solid.Collider.bounds;
                int minX = Mathf.FloorToInt(bounds.min.x / cellSize);
                int maxX = Mathf.FloorToInt(bounds.max.x / cellSize);
                int minY = Mathf.FloorToInt(bounds.min.y / cellSize);
                int maxY = Mathf.FloorToInt(bounds.max.y / cellSize);
                for (int cx = minX; cx <= maxX; cx++)
                {
                    for (int cy = minY; cy <= maxY; cy++)
                    {
                        Vector3 center = new Vector3((cx + 0.5f) * cellSize, (cy + 0.5f) * cellSize, 0f);
                        Gizmos.color = c;
                        Gizmos.DrawWireCube(center, Vector3.one * cellSize);
                    }
                }
            }
        }
    }
}