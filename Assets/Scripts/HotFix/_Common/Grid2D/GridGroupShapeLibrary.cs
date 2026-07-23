#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 库中单个形状的序列化物联网关.
    /// 所有数据直接放在这里(不引用 GridGroupShape 实例),
    /// 确保 ScriptableObject 在 Project 视图中直接可见、可序列化.
    /// </summary>
    [Serializable]
    public class ShapeEntry
    {
        [Tooltip("形状名称,在库中唯一标识.")]
        public string name;

        [Tooltip("可选的唯一ID(用于引用).留空时自动生成.")]
        public string id;

        [Tooltip("组成该形状的基础砖块列表 (col/row 为起点的局部坐标, width/height 为尺寸).")]
        public List<GridUnitBrick> bricks = new();

        [Tooltip("形状锚点在 BBox 中的位置.")]
        public GridGroupPivot pivot = GridGroupPivot.BottomLeft;

        /// <summary>预计算的展开格点 (由编辑器自动维护, 也可运行时 RebuildFromBricks). </summary>
        [HideInInspector]
        public List<Vector2Int> expandedCells = new();

        /// <summary>
        /// 从当前 bricks 列表重新构建 expandedCells.
        /// </summary>
        public void RebuildExpandedCells()
        {
            expandedCells.Clear();
            using var _ = new HashSetScope<Vector2Int>(out var seen);
            foreach (var b in bricks)
            {
                for (int dy = 0; dy < b.height; dy++)
                for (int dx = 0; dx < b.width; dx++)
                {
                    var cell = new Vector2Int(b.col + dx, b.row + dy);
                    if (seen.Add(cell))
                        expandedCells.Add(cell);
                }
            }
        }

        /// <summary>根据 name/id 生成唯一短ID.</summary>
        public string EnsureId()
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString("N").Substring(0, 8);
            return id;
        }

        /// <summary>构建为运行时可用的 GridGroupShape.</summary>
        public GridGroupShape ToShape()
        {
            RebuildExpandedCells();
            return GridGroupShape.FromBricks(bricks, pivot);
        }

        /// <summary>从已有 GridGroupShape 导入 bricks 列表.</summary>
        public void FromShape(GridGroupShape shape)
        {
            bricks.Clear();
            pivot = shape.Pivot;
            if (shape.HasBrickData)
            {
                foreach (var b in shape.Bricks)
                    bricks.Add(b);
            }

            RebuildExpandedCells();
        }

        /// <summary>该形状的 BBox 尺寸 (宽×高, 单位: cell).</summary>
        public Vector2Int LocalSize
        {
            get
            {
                if (expandedCells.Count == 0)
                    RebuildExpandedCells();

                if (expandedCells.Count == 0)
                    return Vector2Int.one;

                int minX = expandedCells[0].x;
                int maxX = expandedCells[0].x;
                int minY = expandedCells[0].y;
                int maxY = expandedCells[0].y;
                foreach (var c in expandedCells)
                {
                    if (c.x < minX) minX = c.x;
                    if (c.x > maxX) maxX = c.x;
                    if (c.y < minY) minY = c.y;
                    if (c.y > maxY) maxY = c.y;
                }

                return new(maxX - minX + 1, maxY - minY + 1);
            }
        }

        public override string ToString() => $"{name} ({bricks.Count} bricks, {expandedCells.Count} cells)";
    }

    /// <summary>
    /// 形状库 ScriptableObject. 在 Project 中右键创建, 可序列化保存所有形状.
    ///
    /// 用法:
    /// 1. [Assets > Create > MoreMountains > GridGroupShapeLibrary] 新建资源;
    /// 2. 双击打开 <see cref="GridGroupShapeEditorWindow"/> 进行可视化编辑;
    /// 3. 在代码中通过 <c>library.GetShape("LShape_01")</c> 获取运行时形状.
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/Grid Group Shape Library", fileName = "GridGroupShapeLibrary")]
    public class GridGroupShapeLibrary : ScriptableObject
    {
        [Tooltip("库中所有形状条目.")]
        public List<ShapeEntry> shapes = new();

        /// <summary>根据 ID 查找形状.</summary>
        public ShapeEntry GetById(string id)
        {
            foreach (var s in shapes)
                if (s.id == id)
                    return s;
            return null;
        }

        /// <summary>根据名称查找形状.</summary>
        public ShapeEntry GetByName(string name)
        {
            foreach (var s in shapes)
                if (s.name == name)
                    return s;
            return null;
        }

        /// <summary>添加或更新一个形状(按 id 去重).</summary>
        public void AddOrUpdate(ShapeEntry entry)
        {
            entry.EnsureId();
            var exist = GetById(entry.id);
            if (exist != null)
            {
                exist.name = entry.name;
                exist.bricks.Clear();
                exist.bricks.AddRange(entry.bricks);
                exist.RebuildExpandedCells();
            }
            else
            {
                shapes.Add(entry);
            }
        }

        /// <summary>删除指定 ID 的形状.</summary>
        public bool RemoveById(string id)
        {
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i].id == id)
                {
                    shapes.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>获取运行时形状列表.</summary>
        public IEnumerable<GridGroupShape> GetAllShapes()
        {
            foreach (var s in shapes)
                yield return s.ToShape();
        }
    }
}
#endif