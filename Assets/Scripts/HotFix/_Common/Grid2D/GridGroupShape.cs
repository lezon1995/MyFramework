using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 单个"基础砖块"在形状中的描述.
    /// 砖块的 Pivot 固定在所占据矩形 (width × height) 的左下角 cell 中心.
    /// 本结构使用局部坐标: 所有字段在形状自身的 BBox 原点坐标系中.
    /// </summary>
    [Serializable]
    public struct GridUnitBrick : IEquatable<GridUnitBrick>
    {
        /// <summary>砖块起点在形状局部坐标系中的 col (X).</summary>
        [Tooltip("砖块起点的列索引 (X), 相对于形状 BBox 左下角.")]
        public int col;

        /// <summary>砖块起点在形状局部坐标系中的 row (Y).</summary>
        [Tooltip("砖块起点的行索引 (Y), 相对于形状 BBox 左下角.")]
        public int row;

        /// <summary>砖块宽度 (X 方向占多少个 cell).</summary>
        [Tooltip("砖块宽度 (cell 数).")]
        public int width;

        /// <summary>砖块高度 (Y 方向占多少个 cell).</summary>
        [Tooltip("砖块高度 (cell 数).")]
        public int height;

        /// <summary>
        /// 以 col/row 为原点的左下角 cell 坐标 (即 Pivot 在局部坐标系中的位置).
        /// </summary>
        public Vector2Int PivotCell => new(col, row);

        /// <summary>
        /// 该砖块占用的 cell 总数.
        /// </summary>
        public int CellCount => width * height;

        public GridUnitBrick(int col, int row, int width, int height)
        {
            this.col = col;
            this.row = row;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// 该砖块覆盖的所有 cell 的局部坐标 (左闭右开, 即 col∈[col, col+width), row∈[row, row+height)).
        /// </summary>
        public void EnumerateCells(ref List<Vector2Int> list)
        {
            for (int dy = 0; dy < height; dy++)
            for (int dx = 0; dx < width; dx++)
                list.Add(new(col + dx, row + dy));
        }

        public bool Equals(GridUnitBrick other)
        {
            return col == other.col && row == other.row && width == other.width && height == other.height;
        }

        public override bool Equals(object obj)
        {
            return obj is GridUnitBrick other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (col, row, width, height).GetHashCode();
        }

        public override string ToString()
        {
            return $"Brick({col},{row} {width}x{height})";
        }
    }

    /// <summary>
    /// 描述"在网格上的形状"的纯数据结构.
    ///
    /// 支持两种编辑模式:
    /// - <b>砖块模式</b> (推荐): 以 <see cref="GridUnitBrick"/> 为单位配置形状,
    ///   由若干基础砖块 (1×1 ~ 3×3) 拼接而成,每一块都有明确的尺寸和位置.
    ///   调用 <see cref="FromBricks"/> 构造.
    /// - <b>格点模式</b> (兼容): 直接以格点集合配置形状,调用 <see cref="FromCells"/> 构造.
    ///
    /// 关键性质:
    /// - 形状是相对于某个"锚点"的局部坐标,通常锚点 = BBox 左下角;
    /// - 通过 <see cref="Pivot"/> 调整锚点 (九种常见锚点位置);
    /// - 通过 <see cref="WithOrientation"/> 生成 8 种朝向的副本 (砖块坐标同步变换);
    /// - 通过 <see cref="ExpandToCells"/> 展开为所有格点的集合 (用于碰撞检测);
    /// - 与 <see cref="Grid"/> 配合: 用 <see cref="CanPlaceAt"/> 检查能否放在网格某处.
    /// </summary>
    [Serializable]
    public struct GridGroupShape : IEnumerable<Vector2Int>, IEquatable<GridGroupShape>
    {
        // ---------------------------------------------------------------
        // 序列化字段 (由 Unity 序列化, 用于 Inspector 编辑)
        // ---------------------------------------------------------------

        /// <summary>砖块模式: 组成形状的基础砖块列表 (col/row/width/height 为局部坐标).</summary>
        [Tooltip("砖块列表. 每个条目表示一个基础砖块, col/row 为起点的局部坐标, width/height 为尺寸.")]
        public GridUnitBrick[] _bricks;

        /// <summary>格点模式: 形状覆盖的所有 cell 的局部坐标 (由 _bricks 展开得到, 或直接配置).</summary>
        [Tooltip("所有格点的局部坐标 (由砖块展开或直接配置).")]
        public Vector2Int[] _cells;

        /// <summary>锚点在 BBox 中的位置.</summary>
        public GridGroupPivot _pivot;

        // ---------------------------------------------------------------
        // 构造
        // ---------------------------------------------------------------

        /// <summary>
        /// 从砖块列表构造形状. 所有砖块的 col/row/width/height 必须非负.
        /// 内部自动展开到格点集合, 自动计算 BBox 并 normalize.
        /// </summary>
        /// <param name="bricks">基础砖块列表 (不允许重叠, 允许空隙).</param>
        /// <param name="pivot">形状锚点.</param>
        /// <exception cref="ArgumentException">当 bricks 为空或含非法尺寸时抛出.</exception>
        public static GridGroupShape FromBricks(IEnumerable<GridUnitBrick> bricks, GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            using var _a = new ListScope<GridUnitBrick>(out var brickList);
            brickList.AddRange(bricks);
            if (brickList.Count == 0)
                throw new ArgumentException("FromBricks requires at least one brick.", nameof(bricks));

            // 展开所有 cell
            using var _b = new HashSetScope<Vector2Int>(out var cellSet);
            using var _c = new ListScope<Vector2Int>(out var cellList);
            foreach (var b in brickList)
            {
                if (b.width <= 0 || b.height <= 0)
                    throw new ArgumentException($"Invalid brick size: {b.width}x{b.height}. Size must be positive.");

                using var _d = new ListScope<Vector2Int>(out var cells);
                b.EnumerateCells(ref cells);
                foreach (var c in cells)
                {
                    if (cellSet.Add(c))
                        cellList.Add(c);
                }
            }

            // Normalize: 把所有坐标平移到 BBox 左下角 = (0,0)
            var min = cellList[0];
            foreach (var c in cellList)
            {
                if (c.x < min.x) min.x = c.x;
                if (c.y < min.y) min.y = c.y;
            }

            var normBricks = new GridUnitBrick[brickList.Count];
            for (int i = 0; i < brickList.Count; i++)
            {
                var b = brickList[i];
                normBricks[i] = new(b.col - min.x, b.row - min.y, b.width, b.height);
            }

            var normCells = new Vector2Int[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                normCells[i] = new(cellList[i].x - min.x, cellList[i].y - min.y);

            return new(normBricks, normCells, pivot);
        }

        /// <summary>
        /// 从格点列表构造形状 (兼容模式).
        /// 内部自动去重并 normalize 到 BBox 左下角 = (0,0).
        /// </summary>
        public static GridGroupShape FromCells(IEnumerable<Vector2Int> cells, GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            using var _a = new ListScope<Vector2Int>(out var list);
            using var _b = new HashSetScope<Vector2Int>(out var seen);
            using var _c = new ListScope<Vector2Int>(out var cellList);
            cellList.AddRange(cells);
            foreach (var c in cellList)
            {
                if (seen.Add(c))
                    list.Add(c);
            }

            if (list.Count == 0)
                throw new ArgumentException("FromCells requires at least one cell.", nameof(cells));

            var min = list[0];
            foreach (var c in list)
            {
                if (c.x < min.x) min.x = c.x;
                if (c.y < min.y) min.y = c.y;
            }

            for (int i = 0; i < list.Count; i++)
                list[i] = new(list[i].x - min.x, list[i].y - min.y);

            return new GridGroupShape(null, list.ToArray(), pivot);
        }

        /// <summary>
        /// 用预置基础尺寸构造一个矩形形状 (等价于 FromCells).
        /// </summary>
        public static GridGroupShape Rectangle(int width, int height, GridGroupPivot pivot = GridGroupPivot.BottomLeft)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException();
            // 直接用砖块模式
            return FromBricks(new[] { new GridUnitBrick(0, 0, width, height) }, pivot);
        }

        GridGroupShape(GridUnitBrick[] bricks, Vector2Int[] cells, GridGroupPivot pivot)
        {
            _bricks = bricks;
            _cells = cells;
            _pivot = pivot;
        }

        GridGroupShape(Vector2Int[] cells, GridGroupPivot pivot)
        {
            _bricks = null;
            _cells = cells;
            _pivot = pivot;
        }

        // ---------------------------------------------------------------
        // 属性
        // ---------------------------------------------------------------

        /// <summary>砖块列表 (只读). 若为 null 则该形状是通过格点模式创建的.</summary>
        public IReadOnlyList<GridUnitBrick> Bricks => _bricks ?? Array.Empty<GridUnitBrick>();

        /// <summary>是否以砖块模式构造.</summary>
        public bool HasBrickData => _bricks is { Length: > 0 };

        /// <summary>展开后的格点列表 (只读).</summary>
        public IReadOnlyList<Vector2Int> Cells => _cells;

        /// <summary>形状占用的格点总数.</summary>
        public int Count => _cells.Length;

        /// <summary>形状锚点.</summary>
        public GridGroupPivot Pivot => _pivot;

        /// <summary>
        /// 当前形状的局部 BBox (在形状本地坐标系中, 最小坐标始终为 (0,0)).
        /// </summary>
        public (Vector2Int min, Vector2Int max) LocalBounds
        {
            get
            {
                if (_cells == null || _cells.Length == 0)
                    return (Vector2Int.zero, Vector2Int.zero);

                var min = _cells[0];
                var max = _cells[0];
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

        /// <summary>BBox 尺寸 (宽×高, 单位: cell 数).</summary>
        public Vector2Int LocalSize
        {
            get
            {
                var (min, max) = LocalBounds;
                return new(max.x - min.x + 1, max.y - min.y + 1);
            }
        }

        /// <summary>
        /// 锚点在 BBox 中的偏移.
        /// 在放置时: 把它从 origin 中减去, 得到形状 BBox 左下角在全局的坐标.
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
        // 8 向变换 (同步变换砖块 + 展开的格点)
        // ---------------------------------------------------------------

        /// <summary>90° 顺时针旋转: (x,y) -> (y, -x).</summary>
        static Vector2Int Rotate90(Vector2Int p) => new(p.y, -p.x);

        /// <summary>沿 X 轴镜像: (x,y) -> (-x,y).</summary>
        static Vector2Int MirrorX(Vector2Int p) => new(-p.x, p.y);

        /// <summary>
        /// 返回该形状的某个 <see cref="GridGroupOrientation"/> 副本.
        /// 砖块坐标和展开的格点坐标会同步变换, 并自动 normalize 到 BBox.min = (0,0).
        /// </summary>
        public GridGroupShape WithOrientation(GridGroupOrientation orientation)
        {
            int rot = ((int)orientation) & 0b0011;
            bool mirror = (((int)orientation) & 0b0100) != 0;

            if (_cells == null || _cells.Length == 0)
                return this;

            // 先 normalize 到 (0,0) 起点
            var (min, _) = LocalBounds;
            var normCells = new Vector2Int[_cells.Length];
            for (int i = 0; i < _cells.Length; i++)
                normCells[i] = new(_cells[i].x - min.x, _cells[i].y - min.y);

            // 镜像 (沿 BBox 中线翻转 X)
            if (mirror)
            {
                int maxX = 0;
                foreach (var c in normCells)
                    if (c.x > maxX)
                        maxX = c.x;
                for (int i = 0; i < normCells.Length; i++)
                    normCells[i] = new(maxX - normCells[i].x, normCells[i].y);
            }

            // 旋转
            for (int r = 0; r < rot; r++)
            for (int i = 0; i < normCells.Length; i++)
                normCells[i] = Rotate90(normCells[i]);

            // 再次 normalize 到 (0,0)
            Vector2Int min2 = normCells[0];
            foreach (var c in normCells)
            {
                if (c.x < min2.x) min2.x = c.x;
                if (c.y < min2.y) min2.y = c.y;
            }

            for (int i = 0; i < normCells.Length; i++)
                normCells[i] = new(normCells[i].x - min2.x, normCells[i].y - min2.y);

            // 同步变换砖块 (如果存在)
            GridUnitBrick[] normBricks = null;
            if (_bricks is { Length: > 0 })
            {
                normBricks = new GridUnitBrick[_bricks.Length];
                for (int i = 0; i < _bricks.Length; i++)
                {
                    var b = _bricks[i];
                    // 砖块的 Pivot = 左下角 cell, 先 normalize
                    var pivot = new Vector2Int(b.col - min.x, b.row - min.y);

                    // 应用 mirror: 沿 BBox 中线翻转 X (使用 normCells 的 maxX, 此时已经 mirror 过了)
                    int maxX = 0;
                    foreach (var c in normCells)
                        if (c.x > maxX)
                            maxX = c.x;
                    if (mirror)
                        pivot = new(maxX - pivot.x, pivot.y);

                    // 应用 rotate (pivot 旋转后会自动落在正确位置)
                    for (int r2 = 0; r2 < rot; r2++)
                        pivot = Rotate90(pivot);

                    // normalize (应用与 normCells 相同的偏移)
                    pivot = new(pivot.x - min2.x, pivot.y - min2.y);

                    // 宽高: 90°/270° 旋转时宽高需要交换
                    int w = b.width;
                    int h = b.height;
                    if (rot == 1 || rot == 3)
                        (w, h) = (h, w);

                    normBricks[i] = new(pivot.x, pivot.y, w, h);
                }
            }

            return new(normBricks, normCells, _pivot);
        }

        /// <summary>
        /// 返回该形状的所有 8 个朝向 (按 <see cref="GridGroupOrientation"/> 枚举顺序).
        /// </summary>
        public IEnumerable<GridGroupShape> AllOrientations()
        {
            foreach (var o in _orientations)
                yield return WithOrientation(o);
        }

        // ---------------------------------------------------------------
        // 在网格上放置
        // ---------------------------------------------------------------

        /// <summary>
        /// 把形状放在以 <paramref name="origin"/> 为锚点的位置上, 返回全局网格坐标集.
        /// 调用方应先用 <see cref="CanPlaceAt"/> 校验越界与冲突.
        /// </summary>
        public void PlaceAt(Vector2Int origin, ref List<Vector2Int> output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

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
            PlaceAt(origin, ref list);
            return list.ToArray();
        }

        /// <summary>
        /// 检查形状能否放在网格的 <paramref name="origin"/> 位置:
        /// - 所有 cell 是否都在网格范围内;
        /// - 是否与 <paramref name="occupiedCells"/> 冲突.
        /// </summary>
        public bool CanPlaceAt(Vector2Int origin, Grid grid, ISet<Vector2Int> occupiedCells)
        {
            var pivot = PivotOffset;
            for (int i = 0; i < _cells.Length; i++)
            {
                var local = _cells[i];
                var g = new Vector2Int(origin.x + local.x - pivot.x, origin.y + local.y - pivot.y);
                if (!grid.InBounds(g))
                    return false;
                if (occupiedCells != null && occupiedCells.Contains(g))
                    return false;
            }

            return true;
        }

        public bool CanPlaceAt(Vector2Int origin, Grid grid) => CanPlaceAt(origin, grid, null);

        // ---------------------------------------------------------------
        // 展开
        // ---------------------------------------------------------------

        /// <summary>
        /// 把所有砖块展开为格点集合 (已在构造时完成, 此方法供外部显式调用).
        /// </summary>
        public IEnumerable<Vector2Int> ExpandToCells()
        {
            if (_cells != null)
                foreach (var c in _cells)
                    yield return c;
        }

        // ---------------------------------------------------------------
        // 预置形状工厂 (砖块模式)
        //
        // 基础尺寸: 1×1, 1×2, 1×3, 2×1, 2×2, 2×3, 3×1, 3×2, 3×3
        // 所有工厂均使用 FromBricks, 让每个矩形都用一块基础砖块描述.
        // ---------------------------------------------------------------

        /// <summary>单格 (1×1).</summary>
        public static GridGroupShape OneByOne(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 1, 1) }, pivot);

        /// <summary>1×2 竖条.</summary>
        public static GridGroupShape OneByTwo(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 1, 2) }, pivot);

        /// <summary>2×1 横条.</summary>
        public static GridGroupShape TwoByOne(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 2, 1) }, pivot);

        /// <summary>1×3 竖条.</summary>
        public static GridGroupShape OneByThree(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 1, 3) }, pivot);

        /// <summary>3×1 横条.</summary>
        public static GridGroupShape ThreeByOne(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 3, 1) }, pivot);

        /// <summary>2×2 方块.</summary>
        public static GridGroupShape TwoByTwo(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 2, 2) }, pivot);

        /// <summary>2×3 竖长方.</summary>
        public static GridGroupShape TwoByThree(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 2, 3) }, pivot);

        /// <summary>3×2 横长方.</summary>
        public static GridGroupShape ThreeByTwo(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 3, 2) }, pivot);

        /// <summary>3×3 方块.</summary>
        public static GridGroupShape ThreeByThree(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[] { new GridUnitBrick(0, 0, 3, 3) }, pivot);

        /// <summary>
        /// L 形 (由 3×1 横条 + 2×1 横条垂直交叉组成).
        /// 布局: 底部 (0,0) 处有 3 格横条, 右侧 (2,0) 处向上延伸 1 格,
        /// 结果: 4 格, 呈 "L" 形状.
        /// </summary>
        public static GridGroupShape LShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[]
        {
            new GridUnitBrick(0, 0, 3, 1), // 底部 3 格
            new GridUnitBrick(2, 1, 1, 1), // 右侧上方 1 格
        }, pivot);

        /// <summary>
        /// T 形 (由 3×1 横条 + 1×2 竖条组成).
        /// 布局: (0,1) 处 3 格横条, 中间 (1,0) 处向上延伸 1 格.
        /// </summary>
        public static GridGroupShape TShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[]
        {
            new GridUnitBrick(0, 1, 3, 1), // 顶部 3 格
            new GridUnitBrick(1, 0, 1, 1), // 中间向下 1 格
        }, pivot);

        /// <summary>
        /// S 形 (两个 2×1 横条交错).
        /// </summary>
        public static GridGroupShape SShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[]
        {
            new GridUnitBrick(1, 0, 2, 1), // 上排 2 格
            new GridUnitBrick(0, 1, 2, 1), // 下排 2 格 (错开 1 格)
        }, pivot);

        /// <summary>
        /// Z 形 (两个 2×1 横条交错, 与 S 形镜像).
        /// </summary>
        public static GridGroupShape ZShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[]
        {
            new GridUnitBrick(0, 0, 2, 1), // 上排 2 格
            new GridUnitBrick(1, 1, 2, 1), // 下排 2 格 (错开 1 格)
        }, pivot);

        /// <summary>
        /// J 形 (与 L 形镜像).
        /// </summary>
        public static GridGroupShape JShape(GridGroupPivot pivot = GridGroupPivot.BottomLeft) => FromBricks(new[]
        {
            new GridUnitBrick(0, 0, 3, 1), // 底部 3 格
            new GridUnitBrick(0, 1, 1, 1), // 左侧上方 1 格
        }, pivot);

        // ---------------------------------------------------------------
        // IEquatable / IEnumerable
        // ---------------------------------------------------------------

        public bool Equals(GridGroupShape other)
        {
            if (other._pivot != _pivot)
                return false;

            if (other._cells == null || _cells == null)
                return false;

            if (other._cells.Length != _cells.Length)
                return false;

            using var _ = new HashSetScope<Vector2Int>(out var seen);
            seen.addRange(other._cells);
            foreach (var c in _cells)
                if (!seen.Contains(c))
                    return false;

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
                if (_cells != null)
                    foreach (var c in _cells)
                        hash = (hash * 397) ^ (c.x * 397) ^ c.y;
                hash = (hash * 397) ^ (int)_pivot;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"GridGroupShape[bricks={_bricks?.Length ?? 0},cells={_cells?.Length ?? 0},pivot={_pivot}]";
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
            public bool MoveNext() => ++_index < _cells.Length;
            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }

        public static readonly GridGroupOrientation[] _orientations =
        {
            GridGroupOrientation.Identity,
            GridGroupOrientation.Rot90,
            GridGroupOrientation.Rot180,
            GridGroupOrientation.Rot270,
            GridGroupOrientation.Mirror,
            GridGroupOrientation.Mirror0,
            GridGroupOrientation.Mirror90,
            GridGroupOrientation.Mirror180,
            GridGroupOrientation.Mirror270,
        };
    }

    /// <summary>
    /// 形状内的"锚点"在 BBox 中的位置. 锚点对应放置时指定的 origin 格子.
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