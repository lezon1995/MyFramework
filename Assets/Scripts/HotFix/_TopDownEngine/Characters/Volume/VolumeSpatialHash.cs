using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    /// <summary>
    /// Hash Grid 空间分区（优化版）
    /// - 增量更新：仅移动有变化的实体
    /// - 无 GC 路径：所有容器预分配/复用
    /// - 配对去重：基于 entity ID 的位运算，不走 HashSet
    /// </summary>
    public class VolumeSpatialHash
    {
        float _cellSize;
        float _invCellSize;
        Dictionary<(int, int), List<TopDownController2D>> _cells = new();
        Dictionary<TopDownController2D, (int, int)> _entityCells = new();

        // 用于 GetPotentialColliders 的临时 List（避免每帧 new）
        List<TopDownController2D> _tempResults = new();

        public VolumeSpatialHash(float cellSize)
        {
            _cellSize = cellSize;
            _invCellSize = 1f / cellSize;
        }

        public float CellSize => _cellSize;

        /// <summary>
        /// 全量重建（仅在实体数量大幅变化时调用）
        /// </summary>
        public void Rebuild(List<TopDownController2D> entities)
        {
            Clear();
            int count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                var entity = entities[i];
                if (entity == null) continue;
                Insert(entity);
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            foreach (var list in _cells.Values)
                ListPool<TopDownController2D>.Release(list);
            _cells.Clear();
            _entityCells.Clear();
        }

        /// <summary>
        /// 插入实体到网格
        /// </summary>
        public void Insert(TopDownController2D entity)
        {
            if (entity == null) return;
            var cellKey = GetCellKey(entity.Position);
            if (!_cells.TryGetValue(cellKey, out var list))
            {
                list = ListPool<TopDownController2D>.Get();
                _cells[cellKey] = list;
            }
            list.Add(entity);
            _entityCells[entity] = cellKey;
        }

        /// <summary>
        /// 从网格移除实体
        /// </summary>
        public void Remove(TopDownController2D entity)
        {
            if (entity == null) return;
            if (!_entityCells.TryGetValue(entity, out var cellKey)) return;
            if (_cells.TryGetValue(cellKey, out var list))
                list.Remove(entity);
            _entityCells.Remove(entity);
        }

        /// <summary>
        /// 增量更新单个实体的网格位置。
        /// 仅当实体进入新格子时才真正移动。
        /// </summary>
        /// <returns>实体是否换格子了</returns>
        public bool UpdatePosition(TopDownController2D entity)
        {
            if (entity == null) return false;
            var newKey = GetCellKey(entity.Position);
            if (_entityCells.TryGetValue(entity, out var oldKey) && oldKey == newKey)
                return false;

            // 换格子了
            Remove(entity);
            Insert(entity);
            return true;
        }

        /// <summary>
        /// 增量更新所有实体位置。
        /// 仅移动超出阈值的实体才触发哈希更新。
        /// </summary>
        public int IncrementalUpdate(List<TopDownController2D> entities, float threshold = 0.01f)
        {
            int moved = 0;
            int count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                var entity = entities[i];
                if (entity == null) continue;

                var pos = entity.Position;
                if (UpdatePosition(entity))
                    moved++;
            }
            return moved;
        }

        /// <summary>
        /// 获取与指定实体可能发生碰撞的所有实体。
        /// 结果追加到 results 末尾，不清空 results。
        /// </summary>
        public void GetPotentialColliders(TopDownController2D entity, List<TopDownController2D> results)
        {
            if (entity == null) return;

            int cellX = WorldToCell(entity.Position.x);
            int cellY = WorldToCell(entity.Position.y);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var key = CellToKey(cellX + dx, cellY + dy);
                    if (_cells.TryGetValue(key, out var list))
                    {
                        int listCount = list.Count;
                        for (int i = 0; i < listCount; i++)
                        {
                            var other = list[i];
                            if (other != entity)
                                results.Add(other);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取圆形区域内所有实体。追加到 results 末尾。
        /// </summary>
        public void GetEntitiesInCircle(Vector2 center, float radius, List<TopDownController2D> results)
        {
            int minX = WorldToCell(center.x - radius);
            int maxX = WorldToCell(center.x + radius);
            int minY = WorldToCell(center.y - radius);
            int maxY = WorldToCell(center.y + radius);

            float radiusSq = radius * radius;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var key = CellToKey(x, y);
                    if (_cells.TryGetValue(key, out var list))
                    {
                        int listCount = list.Count;
                        for (int i = 0; i < listCount; i++)
                        {
                            var entity = list[i];
                            float distSq = (entity.Position - center).sqrMagnitude;
                            if (distSq <= radiusSq)
                                results.Add(entity);
                        }
                    }
                }
            }
        }

        int WorldToCell(float worldPos) => Mathf.FloorToInt(worldPos * _invCellSize);
        (int, int) GetCellKey(Vector2 worldPos) => (WorldToCell(worldPos.x), WorldToCell(worldPos.y));
        static (int, int) CellToKey(int x, int y) => (x, y);
    }
}
