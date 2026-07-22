using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 在一个 <see cref="Grid"/> 上批量枚举可放置位置的工具.
    /// 适用于: 用 "找出网格中的空置格子, 然后用形状模板去匹配" 这类玩法 (例如你的刷怪/刷砖块系统).
    ///
    /// 用法示例:
    /// <code>
    ///   var grid = ...;
    ///   var occupied = new HashSet&lt;Vector2Int&gt; { ... };
    ///   var shape = GridGroupShape.Rectangle(2, 2);
    ///
    ///   foreach (var placement in GridGroupPlacer.FindAllPlacements(shape, grid, occupied))
    ///   {
    ///       // placement.Cells 即该形状在网格上的所有格点.
    ///       SpawnBricksAt(placement.Cells);
    ///   }
    /// </code>
    /// </summary>
    public static class GridGroupPlacer
    {
        /// <summary>
        /// 一次放置的结果.
        /// </summary>
        public struct Placement
        {
            /// <summary>形状锚点在全局网格中的坐标.</summary>
            public Vector2Int Origin;

            /// <summary>形状放置后的所有格点 (全局网格坐标).</summary>
            public Vector2Int[] Cells;

            /// <summary>使用的是哪个朝向 (用于 spawn 多样化).</summary>
            public GridGroupOrientation Orientation;

            public Placement(Vector2Int origin, Vector2Int[] cells, GridGroupOrientation orientation)
            {
                Origin = origin;
                Cells = cells;
                Orientation = orientation;
            }
        }

        /// <summary>
        /// 在 <paramref name="grid"/> 上枚举所有能塞下 (形状 + 朝向) 且不与 <paramref name="occupied"/> 冲突的位置.
        /// 仅返回锚点(origin)坐标; 调用方若需要 cells, 用 <see cref="GridGroupShape.PlaceAt"/>.
        /// </summary>
        public static IEnumerable<Placement> FindAllPlacements(GridGroupShape shape, Grid grid, ISet<Vector2Int> occupied, bool includeAllOrientations = true)
        {
            if (includeAllOrientations)
            {
                foreach (var o in GridGroupShape.orientations)
                {
                    var oriented = shape.WithOrientation(o);
                    foreach (var p in FindPlacementsForShape(oriented, grid, occupied, o))
                        yield return p;
                }
            }
            else
            {
                foreach (var p in FindPlacementsForShape(shape, grid, occupied, GridGroupOrientation.Identity))
                    yield return p;
            }
        }

        /// <summary>
        /// 在 <paramref name="grid"/> 上枚举锚点 origin = (x,y) 中所有能塞下当前形状的坐标.
        /// 不考虑镜像/旋转, 由调用方控制.
        /// </summary>
        public static IEnumerable<Vector2Int> FindAllOrigins(GridGroupShape shape, Grid grid, ISet<Vector2Int> occupied)
        {
            for (int y = 0; y < grid.Rows; y++)
            {
                for (int x = 0; x < grid.Columns; x++)
                {
                    var origin = new Vector2Int(x, y);
                    if (shape.CanPlaceAt(origin, grid, occupied))
                        yield return origin;
                }
            }
        }

        // ---------------------------------------------------------------

        private static IEnumerable<Placement> FindPlacementsForShape(GridGroupShape shape, Grid grid, ISet<Vector2Int> occupied, GridGroupOrientation orientation)
        {
            var bufferedCells = new List<Vector2Int>(shape.Count);
            for (int y = 0; y < grid.Rows; y++)
            {
                for (int x = 0; x < grid.Columns; x++)
                {
                    var origin = new Vector2Int(x, y);
                    if (!shape.CanPlaceAt(origin, grid, occupied))
                        continue;
                    shape.PlaceAt(origin, bufferedCells);
                    var copy = bufferedCells.ToArray();
                    yield return new(origin, copy, orientation);
                }
            }
        }

        // ---------------------------------------------------------------
        // 高级: "在空置连通区域" 内放置
        // ---------------------------------------------------------------

        /// <summary>
        /// 用 BFS/DFS 找出 <paramref name="emptyCells"/> 中的所有 4-连通区域.
        /// 调用方可以再按形状尺寸过滤 (例如 "宽度 ≥ 形状宽度").
        ///
        /// 返回中每个区域用 region.Bounds.Min/Max 给出外接矩形.
        /// </summary>
        public static IReadOnlyList<EmptyRegion> FindEmptyRegions(Grid grid, ISet<Vector2Int> emptyCells)
        {
            var result = new List<EmptyRegion>();
            var visited = new HashSet<Vector2Int>();

            for (int y = 0; y < grid.Rows; y++)
            {
                for (int x = 0; x < grid.Columns; x++)
                {
                    var seed = new Vector2Int(x, y);
                    if (!emptyCells.Contains(seed) || !visited.Add(seed))
                        continue;

                    var region = FloodFill(seed, emptyCells, visited);
                    result.Add(region);
                }
            }

            return result;
        }

        private static EmptyRegion FloodFill(Vector2Int seed, ISet<Vector2Int> emptyCells, HashSet<Vector2Int> visited)
        {
            var stack = new Stack<Vector2Int>();
            stack.Push(seed);

            var cells = new List<Vector2Int>();
            Vector2Int min = seed;
            Vector2Int max = seed;

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                if (!visited.Add(p)) 
                    continue;

                if (!emptyCells.Contains(p)) 
                    continue;


                cells.Add(p);
                if (p.x < min.x) min.x = p.x;
                if (p.y < min.y) min.y = p.y;
                if (p.x > max.x) max.x = p.x;
                if (p.y > max.y) max.y = p.y;

                // 4-邻接
                stack.Push(new(p.x + 1, p.y));
                stack.Push(new(p.x - 1, p.y));
                stack.Push(new(p.x, p.y + 1));
                stack.Push(new(p.x, p.y - 1));
            }

            return new EmptyRegion(cells, new(min.x, min.y), new(max.x, max.y));
        }

        /// <summary>
        /// 判断某个 <paramref name="shape"/> 在 <paramref name="region"/> 内
        /// 是否存在任意合法放置位 (会把 region 内 (含边界)的所有格子遍历一边作为 origin,
        /// 如果形状的所有格点都在 region.cells 中, 即视为合法).
        /// 用于 "形状必须完全落在空置区域里" 的强约束.
        /// </summary>
        public static bool ShapeFitsInRegion(GridGroupShape shape, EmptyRegion region, Grid grid)
        {
            var bufferedCells = new List<Vector2Int>(shape.Count);
            for (int y = region.Min.y; y <= region.Max.y; y++)
            {
                for (int x = region.Min.x; x <= region.Max.x; x++)
                {
                    var origin = new Vector2Int(x, y);
                    if (!shape.CanPlaceAt(origin, grid))
                        continue;
                    shape.PlaceAt(origin, bufferedCells);
                    bool ok = true;
                    for (int i = 0; i < bufferedCells.Count; i++)
                    {
                        if (!region.Contains(bufferedCells[i]))
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok) return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 一个连通的 "空置区域". <see cref="Cells"/> 按发现顺序给出,
    /// <see cref="Min"/> / <see cref="Max"/> 是 BBox 范围.
    /// </summary>
    public struct EmptyRegion
    {
        public IReadOnlyList<Vector2Int> Cells;
        public Vector2Int Min;
        public Vector2Int Max;

        public EmptyRegion(IReadOnlyList<Vector2Int> cells, Vector2Int min, Vector2Int max)
        {
            Cells = cells;
            Min = min;
            Max = max;
        }

        public Vector2Int Size => new(Max.x - Min.x + 1, Max.y - Min.y + 1);
        public int Count => Cells.Count;

        public bool Contains(Vector2Int p)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].x == p.x && Cells[i].y == p.y)
                    return true;
            }

            return false;
        }
    }
}