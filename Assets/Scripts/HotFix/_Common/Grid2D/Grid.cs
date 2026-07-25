using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 一个不可变的网格配置,描述:
    /// - 每格的尺寸 <see cref="CellSize"/>
    /// - 行数 <see cref="Rows"/> / 列数 <see cref="Columns"/>
    /// - 整体偏移 <see cref="OriginOffset"/> (相对于 (0,0) 世界坐标)
    /// - 可选的锚点 <see cref="Pivot"/>,决定原点对应网格哪一个角/中心.
    ///
    /// 本结构只描述"网格长什么样",不持有任何运行时状态 (无 GameObject 依赖),
    /// 适合被 ScriptableObject / MonoBehaviour / 纯逻辑层共享.
    /// </summary>
    [Serializable]
    public struct GridDefinition : IEquatable<GridDefinition>
    {
        [Tooltip("每一格的边长 (世界单位, 必须 > 0).")]
        public float cellSize;

        [Tooltip("行数 (Y 方向, 必须 > 0).")]
        public int rows;

        [Tooltip("列数 (X 方向, 必须 > 0).")]
        public int columns;

        [Tooltip("整体偏移, 单位为世界坐标. 加到世界坐标 ↔ 网格坐标的换算中.")]
        public Vector2 originOffset;

        [Tooltip("网格原点在网格中的位置. 默认 BottomLeft 表示原点对应第 0 行第 0 列的左下角.")]
        public GridPivot pivot;

        public GridDefinition(float cellSize, int rows, int columns, Vector2 originOffset, GridPivot pivot)
        {
            if (cellSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cellSize), "CellSize must be greater than zero.");
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be greater than zero.");
            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be greater than zero.");

            this.cellSize = cellSize;
            this.rows = rows;
            this.columns = columns;
            this.originOffset = originOffset;
            this.pivot = pivot;
        }

        public float CellSize => cellSize;
        public int Rows => rows;
        public int Columns => columns;
        public Vector2 OriginOffset => originOffset;
        public GridPivot Pivot => pivot;

        /// <summary>
        /// 网格在世界空间的总尺寸.
        /// </summary>
        public Vector2 WorldSize => new(columns * cellSize, rows * cellSize);

        /// <summary>
        /// 网格左下角的世界坐标 (= pivot 为 <see cref="GridPivot.BottomLeft"/> 时的原点).
        /// </summary>
        public Vector2 Origin => pivot switch
        {
            GridPivot.BottomLeft => originOffset,
            GridPivot.BottomRight => originOffset + new Vector2(-WorldSize.x, 0f),
            GridPivot.TopLeft => originOffset + new Vector2(0f, -WorldSize.y),
            GridPivot.TopRight => originOffset - WorldSize,
            GridPivot.Center => originOffset - WorldSize * 0.5f,
            _ => originOffset,
        };

        public bool Equals(GridDefinition other)
        {
            return cellSize.Equals(other.cellSize)
                   && rows == other.rows
                   && columns == other.columns
                   && originOffset.Equals(other.originOffset)
                   && pivot == other.pivot;
        }

        public override bool Equals(object obj)
        {
            return obj is GridDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = cellSize.GetHashCode();
                hash = (hash * 397) ^ rows;
                hash = (hash * 397) ^ columns;
                hash = (hash * 397) ^ originOffset.GetHashCode();
                hash = (hash * 397) ^ (int)pivot;
                return hash;
            }
        }

        public override string ToString() => $"Grid2D[r={rows},c={columns},cell={cellSize},origin={originOffset},pivot={pivot}]";
    }

    /// <summary>
    /// 网格原点在网格中的位置.
    /// </summary>
    public enum GridPivot
    {
        BottomLeft = 0,
        BottomRight = 1,
        TopLeft = 2,
        TopRight = 3,
        Center = 4,
    }

    /// <summary>
    /// 纯逻辑层的 2D 网格: 根据一个 <see cref="GridDefinition"/>
    /// 在世界坐标和网格坐标之间互相转换, 并提供越界检查 / 单元格遍历.
    /// 不依赖任何 Unity 运行时 API (除 <see cref="Vector2"/>).
    /// </summary>
    public struct Grid : IEnumerable<Vector2Int>
    {
        GridDefinition _definition;

        public Grid(in GridDefinition definition)
        {
            _definition = definition;
        }

        public GridDefinition Definition => _definition;

        public static int rows { get; set; }
        public static int cols { get; set; }

        public int Rows => _definition.Rows;
        public int Columns => _definition.Columns;
        public float CellSize => _definition.CellSize;
        public Vector2 OriginOffset => _definition.OriginOffset;
        public GridPivot Pivot => _definition.Pivot;
        public Vector2 Origin => _definition.Origin;
        public Vector2 WorldSize => _definition.WorldSize;

        // ---------------------------------------------------------------
        // 边界 / 索引
        // ---------------------------------------------------------------

        public bool InBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < Columns && cell.y >= 0 && cell.y < Rows;
        }

        public bool InBounds(int x, int y) => InBounds(new(x, y));

        /// <summary>
        /// 将任意 (x,y) 网格坐标包裹到 [0, Columns) × [0, Rows) 范围内 (环绕取模).
        /// </summary>
        public Vector2Int WrapCell(Vector2Int cell)
        {
            int cx = ((cell.x % Columns) + Columns) % Columns;
            int cy = ((cell.y % Rows) + Rows) % Rows;
            return new(cx, cy);
        }

        // ---------------------------------------------------------------
        // 世界坐标 ↔ 网格坐标
        // ---------------------------------------------------------------

        /// <summary>
        /// 世界坐标 -> 网格坐标 (向下取整, 即单元格的左下角所在的整数索引).
        /// </summary>
        public Vector2Int WorldToCell(Vector2 worldPosition)
        {
            Vector2 local = worldPosition - Origin;
            int x = Mathf.FloorToInt(local.x / CellSize);
            int y = Mathf.FloorToInt(local.y / CellSize);
            return new(x, y);
        }

        /// <summary>
        /// 网格坐标 -> 该单元格的中心在世界的坐标.
        /// </summary>
        public Vector2 CellToWorld(Vector2Int cell)
        {
            return CellToWorld(cell.x, cell.y);
        }

        public Vector2 CellToWorld(int x, int y)
        {
            return Origin + new Vector2((x + 0.5f) * CellSize, (y + 0.5f) * CellSize);
        }

        /// <summary>
        /// 网格坐标 -> 该单元格左下角的世界坐标.
        /// </summary>
        public Vector2 CellToWorldMin(Vector2Int cell)
        {
            return CellToWorldMin(cell.x, cell.y);
        }

        public Vector2 CellToWorldMin(int x, int y)
        {
            return Origin + new Vector2(x * CellSize, y * CellSize);
        }

        /// <summary>
        /// 返回覆盖指定世界点的所有可能单元格 (用于点采样边界场景, 通常只返回 1 个).
        /// </summary>
        public Vector2Int WorldToCellClamped(Vector2 worldPosition)
        {
            Vector2Int c = WorldToCell(worldPosition);
            c.x = Mathf.Clamp(c.x, 0, Columns - 1);
            c.y = Mathf.Clamp(c.y, 0, Rows - 1);
            return c;
        }

        // ---------------------------------------------------------------
        // 迭代
        // ---------------------------------------------------------------

        /// <summary>
        /// 按 "从下到上, 从左到右" (Y 0 → Rows-1, X 0 → Columns-1) 遍历所有单元格.
        /// </summary>
        public Enumerator GetEnumerator() => new(this);

        IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Vector2Int>
        {
            Grid _grid;
            int _index;

            internal Enumerator(Grid grid)
            {
                _grid = grid;
                _index = -1;
            }

            public Vector2Int Current
            {
                get
                {
                    int x = _index % _grid.Columns;
                    int y = _index / _grid.Columns;
                    return new(x, y);
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                int total = _grid.Rows * _grid.Columns;
                _index++;
                return _index < total;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }

        // ---------------------------------------------------------------
        // 工厂方法
        // ---------------------------------------------------------------

        public static Grid Create(float cellSize, int rows, int columns) => new(new(cellSize, rows, columns, Vector2.zero, GridPivot.BottomLeft));
        public static Grid Create(float cellSize, int rows, int columns, Vector2 originOffset) => new(new(cellSize, rows, columns, originOffset, GridPivot.BottomLeft));
        public static Grid Create(float cellSize, int rows, int columns, Vector2 originOffset, GridPivot pivot) => new(new(cellSize, rows, columns, originOffset, pivot));
    }
    
    public static class GridExtensions
    {
        public static int ToIndex(this Vector2Int coord)
        {
            return coord.y * Grid.cols + coord.x;
        }
        public static Vector2Int ToCoord(this int index)
        {
            return new(index % Grid.cols, index / Grid.cols);
        }
    }
}