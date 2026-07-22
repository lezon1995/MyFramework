using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 描述 "在网格上的形状" 的纯数据结构.
    /// 由一组相对原点的整数格子点 (Vector2Int) 组成;形状可以是矩形 / L 形 / T 形 / 任意散点.
    ///
    /// 关键性质:
    /// - 形状是相对于某个 "锚点" 的局部坐标,通常锚点 = BBox 左下角;
    /// - 通过 <see cref="Pivot"/> 调整锚点 (八种常见锚点位置);
    /// - 通过 <see cref="WithOrientation"/> 生成 8 种朝向的副本;
    /// - 与 <see cref="Grid"/> 配合: 用 <see cref="CanPlaceAt"/> 检查能否放在网格某处.
    /// </summary>
    [Serializable]
    public struct GridGroupShape : IEnumerable<Vector2Int>, IEquatable<GridGroupShape>
    {
        // 内部用 hash 集合缓存 + 哈希, 用 array 保存插入序用于遍历.
        //  struct 字段仍可持有 reference type (array/hashset).
        public Vector2Int[] _cells;

        // 锚点在 BBox 中的位置 
        public GridGroupPivot _pivot;

        public GridGroupShape(IEnumerable<Vector2Int> cells, GridGroupPivot pivot = GridGroupPivot.BottomLeft)
            : this(NormalizeToArray(cells), pivot)
        {
        }

        GridGroupShape(Vector2Int[] cells, GridGroupPivot pivot)
        {
            if (cells == null || cells.Length == 0)
                throw new ArgumentException("GridGroupShape requires at least one cell.", nameof(cells));

            _cells = cells;
            _pivot = pivot;
        }

        static Vector2Int[] NormalizeToArray(IEnumerable<Vector2Int> cells)
        {
            var list = new List<Vector2Int>();
            var seen = new HashSet<Vector2Int>();
            foreach (var c in cells)
            {
                if (seen.Add(c))
                {
                    list.Add(c);
                }
            }

            if (list.Count == 0)
                throw new ArgumentException("GridGroupShape requires at least one cell.", nameof(cells));
            return list.ToArray();
        }

        public IReadOnlyList<Vector2Int> Cells => _cells;
        public int Count => _cells.Length;
        public GridGroupPivot Pivot => _pivot;

        // ---------------------------------------------------------------
        // BBox / 锚点
        // ---------------------------------------------------------------

        /// <summary>
        /// 当前形状的局部 BBox (在形状本地坐标系中).
        /// </summary>
        public (Vector2Int min, Vector2Int max) LocalBounds
        {
            get
            {
                Vector2Int min = _cells[0];
                Vector2Int max = _cells[0];
                for (int i = 1; i < _cells.Length; i++)
                {
                    var c = _cells[i];
                    if (c.x < min.x) min.x = c.x;
                    if (c.y < min.y) min.y = c.y;
                    if (c.x > max.x) max.x = c.x;
                    if (c.y > max.y) max.y = c.y;
                }

                return (min, max);
            }
        }

        public Vector2Int LocalSize
        {
            get
            {
                var (min, max) = LocalBounds;
                return new(max.x - min.x + 1, max.y - min.y + 1);
            }
        }

        /// <summary>
        /// 锚点 (即 <see cref="Pivot"/>) 在 BBox 内的偏移.
        /// 把它加到形状的 (0,0) 上, 就等于"放入全局网格后该格的世界位置".
        /// </summary>
        public Vector2Int PivotOffset
        {
            get
            {
                var (min, max) = LocalBounds;
                return _pivot switch
                {
                    GridGroupPivot.BottomLeft => new(min.x, min.y),
                    GridGroupPivot.BottomCenter => new((min.x + max.x) / 2, min.y),
                    GridGroupPivot.BottomRight => new(max.x, min.y),
                    GridGroupPivot.MiddleLeft => new(min.x, (min.y + max.y) / 2),
                    GridGroupPivot.Center => new((min.x + max.x) / 2, (min.y + max.y) / 2),
                    GridGroupPivot.MiddleRight => new(max.x, (min.y + max.y) / 2),
                    GridGroupPivot.TopLeft => new(min.x, max.y),
                    GridGroupPivot.TopCenter => new((min.x + max.x) / 2, max.y),
                    GridGroupPivot.TopRight => new(max.x, max.y),
                    _ => Vector2Int.zero,
                };
            }
        }

        // ---------------------------------------------------------------
        // 8 向变换
        // ---------------------------------------------------------------

        /// <summary>
        /// 90° 顺时针 (CW) 旋转: (x,y) -> (y, -x).
        /// 注意: 用 int 表示会因正负导致 BBox 平移; 因此旋转后通常需要再
        /// 把 BBox 拉到第一象限, <see cref="WithOrientation"/> 已经做了这件事.
        /// </summary>
        static Vector2Int Rotate90(Vector2Int p) => new(p.y, -p.x);

        /// <summary>
        /// 沿着 X 轴镜像 (左右翻转): (x, y) -> (-x, y).
        /// </summary>
        static Vector2Int MirrorX(Vector2Int p) => new(-p.x, p.y);

        /// <summary>
        /// 把 BBox 拉到非负象限 (BBox.min = (0,0)).
        /// 用于让形状的局部坐标始终从 (0,0) 开始, 便于放置时叠加 origin.
        /// </summary>
        static (Vector2Int[] normalized, Vector2Int originalMin) NormalizeAfterTransform(Vector2Int[] raw)
        {
            Vector2Int min = raw[0];
            for (int i = 1; i < raw.Length; i++)
            {
                var c = raw[i];
                if (c.x < min.x) min.x = c.x;
                if (c.y < min.y) min.y = c.y;
            }

            var result = new Vector2Int[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                result[i] = new(raw[i].x - min.x, raw[i].y - min.y);
            }

            return (result, min);
        }

        /// <summary>
        /// 返回该形状的某个 <see cref="GridGroupOrientation"/> 副本.
        /// orientation 枚举决定 8 种朝向:
        /// - rotation: 0 / 90 / 180 / 270
        /// - mirrored: 是否左右镜像
        /// </summary>
        public GridGroupShape WithOrientation(GridGroupOrientation orientation)
        {
            int rot = ((int)orientation) & 0b0011; // low 2 bits
            bool mirror = (((int)orientation) & 0b0100) != 0;

            // 在 "BBox 起点 = (0,0)" 的稳定形态上做变换, 再 normalize 回第一象限.
            var (min, _) = LocalBounds;
            var raw = new Vector2Int[_cells.Length];
            for (int i = 0; i < _cells.Length; i++)
            {
                raw[i] = new(_cells[i].x - min.x, _cells[i].y - min.y);
            }

            // 镜像 (沿当前 BBox 中线翻转 X)
            if (mirror)
            {
                int maxX = 0;
                for (int i = 0; i < raw.Length; i++)
                    if (raw[i].x > maxX)
                        maxX = raw[i].x;
                for (int i = 0; i < raw.Length; i++)
                {
                    raw[i] = new(maxX - raw[i].x, raw[i].y);
                }
            }

            // 旋转
            for (int r = 0; r < rot; r++)
            {
                for (int i = 0; i < raw.Length; i++)
                {
                    raw[i] = Rotate90(raw[i]);
                }
            }

            var normalized = NormalizeAfterTransform(raw).normalized;
            return new(normalized, _pivot);
        }

        /// <summary>
        /// 返回该形状的所有 8 个朝向 (按 <see cref="GridGroupOrientation"/> 枚举顺序).
        /// 用 <c>yield return</c> 方式给出; 调用方可直接 <c>foreach</c>.
        /// </summary>
        public IEnumerable<GridGroupShape> AllOrientations()
        {
            foreach (var o in orientations)
            {
                yield return WithOrientation(o);
            }
        }

        // ---------------------------------------------------------------
        // 在网格上"放置"与碰撞检查
        // ---------------------------------------------------------------

        /// <summary>
        /// 把形状放在以 <paramref name="origin"/> 为锚点的位置上, 返回全局网格坐标集.
        /// 调用方应先用 <see cref="CanPlaceAt"/> 校验越界与冲突.
        /// </summary>
        public void PlaceAt(Vector2Int origin, List<Vector2Int> output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            var pivot = PivotOffset;
            output.Clear();
            for (int i = 0; i < _cells.Length; i++)
            {
                var local = _cells[i];
                output.Add(new(origin.x + local.x - pivot.x, origin.y + local.y - pivot.y));
            }
        }

        public Vector2Int[] PlaceAt(Vector2Int origin)
        {
            var list = new List<Vector2Int>(_cells.Length);
            PlaceAt(origin, list);
            return list.ToArray();
        }

        /// <summary>
        /// 给定一个 Grid2D 和一个 origin (即锚点在网格中的全局坐标), 检查:
        /// - 所有 cell 是否都在网格范围内;
        /// - 给定的 occupiedCells 中有没有任何 cell 与之重叠.
        /// </summary>
        /// <param name="origin">锚点的全局坐标.</param>
        /// <param name="grid">用于范围检查.</param>
        /// <param name="occupiedCells">占用标记集合, 比如已存在的砖块.</param>
        /// <param name="occupiedCellsInGrid">当为 true 时, 使用与 (occupied set) 相同的 BBox; 否则完全外部判定.</param>
        public bool CanPlaceAt(Vector2Int origin, Grid grid, ISet<Vector2Int> occupiedCells)
        {
            var pivot = PivotOffset;
            for (int i = 0; i < _cells.Length; i++)
            {
                var local = _cells[i];
                var g = new Vector2Int(origin.x + local.x - pivot.x,
                    origin.y + local.y - pivot.y);
                if (!grid.InBounds(g))
                    return false;
                if (occupiedCells != null && occupiedCells.Contains(g))
                    return false;
            }

            return true;
        }

        public bool CanPlaceAt(Vector2Int origin, Grid grid) => CanPlaceAt(origin, grid, null);

        // ---------------------------------------------------------------
        // 工厂
        // ---------------------------------------------------------------

        public static GridGroupShape Rectangle(int width, int height, GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException();
            var list = new List<Vector2Int>(width * height);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                list.Add(new(x, y));
            return new(list, pivot);
        }

        public static GridGroupShape LineHorizontal(int length, GridGroupPivot pivot = GridGroupPivot.BottomLeft) => Rectangle(length, 1, pivot);

        public static GridGroupShape LineVertical(int length, GridGroupPivot pivot = GridGroupPivot.BottomLeft) => Rectangle(1, length, pivot);

        public static GridGroupShape LShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            return new(new Vector2Int[]
            {
                new(0, 0), new(1, 0), new(2, 0),
                new(2, 1),
            }, pivot);
        }

        public static GridGroupShape TShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            return new(new Vector2Int[]
            {
                new(0, 1), new(1, 1), new(2, 1),
                new(1, 0),
            }, pivot);
        }

        public static GridGroupShape SShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            return new(new Vector2Int[]
            {
                new(1, 0), new(2, 0),
                new(0, 1), new(1, 1),
            }, pivot);
        }

        public static GridGroupShape ZShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            return new(new Vector2Int[]
            {
                new(0, 0), new(1, 0),
                new(1, 1), new(2, 1),
            }, pivot);
        }

        public static GridGroupShape JShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            return new(new Vector2Int[]
            {
                new(0, 0), new(0, 1),
                new(1, 1), new(2, 1),
            }, pivot);
        }

        /// <summary>
        /// 用外部提供的格子点集合直接构造. 内部会自动去重.
        /// </summary>
        public static GridGroupShape FromCells(IEnumerable<Vector2Int> cells, GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            return new(cells, pivot);
        }

        // ---------------------------------------------------------------
        // IEquatable / IEnumerable
        // ---------------------------------------------------------------

        public bool Equals(GridGroupShape other)
        {
            if (other._pivot != _pivot)
                return false;

            if (other._cells.Length != _cells.Length)
                return false;

            var seen = new HashSet<Vector2Int>(other._cells);
            foreach (var c in _cells)
            {
                if (!seen.Contains(c))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is GridGroupShape other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var c in _cells)
                {
                    hash = (hash * 397) ^ (c.x * 397) ^ c.y;
                }

                hash = (hash * 397) ^ (int)_pivot;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"GridGroupShape[count={_cells.Length},pivot={_pivot}]";
        }

        public Enumerator GetEnumerator() => new(_cells);
        IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Vector2Int>
        {
            Vector2Int[] _cells;
            int _index;

            internal Enumerator(Vector2Int[] cells)
            {
                _cells = cells;
                _index = -1;
            }

            public Vector2Int Current => _cells[_index];
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index++;
                return _index < _cells.Length;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }

        public static GridGroupOrientation[] orientations =
        {
            GridGroupOrientation.Rotate0,
            GridGroupOrientation.Rotate90,
            GridGroupOrientation.Rotate180,
            GridGroupOrientation.Rotate270,
            GridGroupOrientation.Mirror,
            GridGroupOrientation.Identity,
            GridGroupOrientation.Rot90,
            GridGroupOrientation.Rot180,
            GridGroupOrientation.Rot270,
            GridGroupOrientation.Mirror0,
            GridGroupOrientation.Mirror90,
            GridGroupOrientation.Mirror180,
            GridGroupOrientation.Mirror270,
        };
    }

    /// <summary>
    /// 形状内的 "锚点" 在 BBox 中的位置. 锚点对应放置时指定的 origin 格子.
    /// </summary>
    public enum GridGroupPivot
    {
        BottomLeft,
        BottomCenter,
        BottomRight,
        MiddleLeft,
        Center,
        MiddleRight,
        TopLeft,
        TopCenter,
        TopRight,
    }

    /// <summary>
    /// 8 个朝向 (4 个旋转 × 2 个镜像).
    /// 位编码: bit0~1 = 旋转次数 (0/1/2/3), bit2 = 是否镜像.
    /// </summary>
    [Flags]
    public enum GridGroupOrientation
    {
        Rotate0 = 0,
        Rotate90 = 1,
        Rotate180 = 2,
        Rotate270 = 3,
        Mirror = 0b0100,

        Identity = Rotate0,
        Rot90 = Rotate90,
        Rot180 = Rotate180,
        Rot270 = Rotate270,
        Mirror0 = Mirror | Rotate0,
        Mirror90 = Mirror | Rotate90,
        Mirror180 = Mirror | Rotate180,
        Mirror270 = Mirror | Rotate270,
    }
}