using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains
{
    /// <summary>
    /// 在场景中编辑/可视化 <see cref="GridGroupShape"/> 的 MonoBehaviour.
    ///
    /// 支持两种编辑模式:
    /// - <b>手动模式</b>: 直接编辑 ToggleGrid (单个格点列表, 兼容旧用法);
    /// - <b>库模式</b>: 拖入 GridGroupShapeLibrary + 指定 shapeId, 自动加载对应形状的 bricks.
    ///
    /// 同时支持:
    /// - 当前朝向预览;
    /// - 把 ExternalOccupied 作为 occupied, 实时显示"能否放进去";
    /// - 场景 Gizmo 中用不同颜色区分每个基础砖块.
    /// </summary>
    [AddComponentMenu("Grid2D/GridGroup Shape View")]
    [ExecuteAlways]
    public class GridGroupShapeView : MonoBehaviour
    {
        [Header("Grid")]
        [Tooltip("可选. 拖入 Grid2DSetting 以复用其他网格的参数; 留空则使用下方内联值.")]
        public GridSetting Setting;

        [Min(0.0001f)]
        public float CellSize = 1f;

        [Min(1)]
        public int Rows = 6;

        [Min(1)]
        public int Columns = 6;

        public Vector2 OriginOffset = Vector2.zero;
        public GridPivot Pivot = GridPivot.BottomLeft;
        public bool UseTransformAsAdditionalOffset = true;

        [Header("Shape (手动模式)")]
        [Tooltip("形状格点. 行列 (0,0) 在左下, 直接修改即可手动编辑形状.")]
        public List<Vector2Int> ToggleGrid = new();

        [Tooltip("形状锚点.")]
        public GridGroupPivot ShapePivot = GridGroupPivot.BottomLeft;

        [Header("Shape (库模式, 优先于手动模式)")]
        [Tooltip("形状库资源. 拖入后可用 ShapeId 指定要预览的形状.")]
        public GridGroupShapeLibrary Library;

        [Tooltip("从库中选中形状的 ID. 更改后自动刷新预览.")]
        public string ShapeId;

        [Tooltip("当前预览的朝向.")]
        public GridGroupOrientation Orientation = GridGroupOrientation.Identity;

        [Header("Visual")]
        [Tooltip("是否在场景中绘制形状 Gizmo.")]
        public bool DrawShape = true;

        [Tooltip("正常可放置时的颜色 (每个砖块颜色不同, 此为基础色).")]
        public Color ShapeColor = new(0.95f, 0.65f, 0.2f, 0.9f);

        [Tooltip("与其他物体冲突时/超出网格时的颜色.")]
        public Color ShapeInvalidColor = new(1f, 0.3f, 0.3f, 0.9f);

        [Tooltip("每个砖块使用不同颜色 (启用后每个基础砖块着色不同, 更直观).")]
        public bool UseBrickColors = true;

        [Tooltip("可选: 用于实时检查形状能否摆入当前网格 (并避开外部 occupied cells).")]
        public bool LiveValidityCheck = true;

        [Tooltip("外部已占用的格子集合 (用于合法性检查).")]
        public List<Vector2Int> ExternalOccupied = new();

        // ---------------------------------------------------------------
        // 数据访问
        // ---------------------------------------------------------------

        /// <summary>
        /// 当前生效的形状 (运行时读取). 优先用库模式, 否则用手动 ToggleGrid.
        /// </summary>
        public GridGroupShape CurrentShape()
        {
            var oriented = BuildShape().WithOrientation(Orientation);
            return oriented;
        }

        /// <summary>
        /// 构建当前编辑的形状 (不包含朝向).
        /// 库模式: 从 Library + ShapeId 加载; 手动模式: 从 ToggleGrid 构建.
        /// </summary>
        public GridGroupShape BuildShape()
        {
            // 库模式优先
            if (Library && !string.IsNullOrEmpty(ShapeId))
            {
                if (Library.GetById(ShapeId, out var entry))
                    return entry.ToShape();
            }
            // 手动模式
            return GridGroupShape.FromCells(ToggleGrid, ShapePivot);
        }

        /// <summary>
        /// 加载库中的指定形状 (供 Inspector 按钮或代码调用).
        /// </summary>
        public void LoadShapeFromLibrary(string id)
        {
            if (Library == null) 
                return;

            ShapeId = id;
            if (Library.GetById(id, out var entry))
            {
                // 同步到手动模式 (方便在库模式关闭时也能看到)
                ToggleGrid = new List<Vector2Int>(entry.expandedCells);
                ShapePivot = ShapePivot; // 不变
            }
        }

        /// <summary>
        /// 加载库中的指定形状 (按名称).
        /// </summary>
        public void LoadShapeFromLibraryByName(string name)
        {
            if (Library == null) 
                return;

            if (Library.GetByName(name, out var entry))
                LoadShapeFromLibrary(entry.id);
        }

        /// <summary>
        /// 获取当前形状的砖块列表 (用于 Gizmos 着色).
        /// </summary>
        public IReadOnlyList<GridUnitBrick> CurrentBricks()
        {
            if (Library && !string.IsNullOrEmpty(ShapeId))
            {
                if (Library.GetById(ShapeId, out var entry))
                    return entry.bricks;
            }
            // 没有库数据时返回空
            return Array.Empty<GridUnitBrick>();
        }

        public Grid CurrentGrid()
        {
            GridDefinition def;
            if (Setting)
            {
                def = Setting.BuildDefinition();
                CellSize = Setting.CellSize;
                Rows = Setting.Rows;
                Columns = Setting.Columns;
                OriginOffset = Setting.OriginOffset;
                Pivot = Setting.Pivot;
            }
            else
            {
                def = new GridDefinition(CellSize, Rows, Columns, OriginOffset, Pivot);
            }

            var effectiveOffset = OriginOffset;
            if (UseTransformAsAdditionalOffset)
            {
                var pos = transform.position;
                effectiveOffset += new Vector2(pos.x, pos.y);
            }

            return new Grid(new GridDefinition(CellSize, Rows, Columns, effectiveOffset, Pivot));
        }

        public bool IsValidAtAnchor(Vector2Int anchorCell)
        {
            var shape = CurrentShape();
            var grid = CurrentGrid();
            var occupied = new HashSet<Vector2Int>(ExternalOccupied);
            return shape.CanPlaceAt(anchorCell, grid, occupied);
        }

        // ---------------------------------------------------------------
        // Gizmos
        // ---------------------------------------------------------------

        void OnValidate()
        {
            if (CellSize <= 0f) CellSize = 0.0001f;
            if (Rows < 1) Rows = 1;
            if (Columns < 1) Columns = 1;
            ToggleGrid ??= new List<Vector2Int>();
        }

        /// <summary>预定义砖块调色板颜色 (9种尺寸, 与编辑器一致).</summary>
        static readonly Color[] BrickPalette = new[]
        {
            new Color(0.45f, 0.85f, 1.0f),    // 1x1
            new Color(0.30f, 0.75f, 0.40f),    // 1x2
            new Color(0.20f, 0.65f, 0.85f),    // 1x3
            new Color(0.95f, 0.60f, 0.20f),    // 2x1
            new Color(0.90f, 0.85f, 0.15f),    // 2x2
            new Color(0.75f, 0.35f, 0.85f),    // 2x3
            new Color(0.95f, 0.30f, 0.30f),    // 3x1
            new Color(0.55f, 0.30f, 0.80f),    // 3x2
            new Color(0.25f, 0.55f, 0.25f),    // 3x3
        };

        static readonly Vector2Int[] PresetSizes = new[]
        {
            new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(1,3),
            new Vector2Int(2,1), new Vector2Int(2,2), new Vector2Int(2,3),
            new Vector2Int(3,1), new Vector2Int(3,2), new Vector2Int(3,3),
        };

        static Color GetBrickColor(int width, int height)
        {
            for (int i = 0; i < PresetSizes.Length; i++)
            {
                if (PresetSizes[i].x == width && PresetSizes[i].y == height)
                    return BrickPalette[i];
            }
            return Color.white;
        }

        void OnDrawGizmos()
        {
            // 1. 底层网格线
            var grid = CurrentGrid();
            Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
            for (int x = 0; x <= grid.Columns; x++)
            {
                var a = new Vector3(grid.Origin.x + x * grid.CellSize, grid.Origin.y, transform.position.z);
                var b = new Vector3(a.x, a.y + grid.WorldSize.y, a.z);
                Gizmos.DrawLine(a, b);
            }

            for (int y = 0; y <= grid.Rows; y++)
            {
                var a = new Vector3(grid.Origin.x, grid.Origin.y + y * grid.CellSize, transform.position.z);
                var b = new Vector3(a.x + grid.WorldSize.x, a.y, a.z);
                Gizmos.DrawLine(a, b);
            }

            // 2. 形状
            if (!DrawShape)
                return;

            var shape = BuildShape().WithOrientation(Orientation);
            var pivotOffset = shape.PivotOffset;

            // 画以 (0,0) 为锚点 (放在第一个 cell 位置)
            var originInGrid = Vector2Int.zero - pivotOffset;
            _scratchCells.Clear();
            shape.PlaceAt(originInGrid, ref _scratchCells);

            // 构建 cell -> brick index 映射 (用于不同砖块着色)

            using var _ = new DicScope<Vector2Int, int>(out var cellToBrickIdx);
            BuildCellToBrickIndex(shape, ref cellToBrickIdx);

            // 收集每个 cell 的颜色
            int cellIdx = 0;
            foreach (var localCell in _scratchCells)
            {
                var w = grid.CellToWorld(localCell);
                var center = new Vector3(w.x, w.y, transform.position.z);

                bool insideGrid = grid.InBounds(localCell);
                bool conflict = ExternalOccupied.Contains(localCell);
                bool valid = insideGrid && !conflict;

                Color fillColor;
                if (valid)
                {
                    // 库模式: 根据砖块尺寸着色
                    if (UseBrickColors && Library && !string.IsNullOrEmpty(ShapeId))
                    {
                        if (cellToBrickIdx.TryGetValue(localCell, out int brickIdx))
                        {
                            var bricks = shape.HasBrickData ? shape.Bricks : new List<GridUnitBrick>();
                            if (brickIdx >= 0 && brickIdx < bricks.Count)
                            {
                                var b = bricks[brickIdx];
                                fillColor = GetBrickColor(b.width, b.height);
                            }
                            else
                            {
                                fillColor = ShapeColor;
                            }
                        }
                        else
                        {
                            fillColor = ShapeColor;
                        }
                    }
                    else
                    {
                        fillColor = ShapeColor;
                    }
                }
                else
                {
                    fillColor = ShapeInvalidColor;
                }

                Gizmos.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.35f);
                Gizmos.DrawCube(center, new Vector3(grid.CellSize * 0.92f, grid.CellSize * 0.92f, 0.01f));

                Gizmos.color = new Color(fillColor.r * 0.7f, fillColor.g * 0.7f, fillColor.b * 0.7f, 0.9f);
                DrawCellFrame(center, grid.CellSize);

                // 在 cell 中显示尺寸标签
                if (UseBrickColors && valid && grid.CellSize >= 0.5f)
                {
                    if (cellToBrickIdx.TryGetValue(localCell, out int brickIdx))
                    {
                        var bricks = shape.HasBrickData ? shape.Bricks : new List<GridUnitBrick>();
                        if (brickIdx >= 0 && brickIdx < bricks.Count)
                        {
                            var b = bricks[brickIdx];
                            // 只在左上角 cell 显示一次
                            if (b.col + b.width - 1 == localCell.x && b.row + b.height - 1 == localCell.y)
                            {
                                #if UNITY_EDITOR
                                var style = new GUIStyle
                                {
                                    alignment = TextAnchor.MiddleCenter,
                                    fontSize = Mathf.Clamp((int)(grid.CellSize * 20), 8, 18),
                                    normal = { textColor = new Color(fillColor.r, fillColor.g, fillColor.b) }
                                };
                                UnityEditor.Handles.Label(center, $"{b.width}x{b.height}", style);
                                #endif
                            }
                        }
                    }
                }

                cellIdx++;
            }

            // 3. 锚点
            Vector2 anchorWorld = grid.CellToWorld(-pivotOffset);
            Vector3 anchorPos = new Vector3(anchorWorld.x, anchorWorld.y, transform.position.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchorPos, grid.CellSize * 0.25f);

            // 4. 库模式: 在 Gizmos 标题显示形状名
            #if UNITY_EDITOR
            if (Library && !string.IsNullOrEmpty(ShapeId))
            {
                if (Library.GetById(ShapeId, out var entry))
                {
                    var labelStyle = new GUIStyle
                    {
                        alignment = TextAnchor.UpperLeft,
                        fontSize = 10,
                        normal = { textColor = new Color(0.6f, 0.9f, 1f) }
                    };
                    Vector3 labelPos = anchorPos + new Vector3(-grid.CellSize * 0.4f, grid.CellSize * 0.3f, 0);
                    UnityEditor.Handles.Label(labelPos, $"{entry.name} [{Orientation}]", labelStyle);
                }
            }
            #endif
        }

        void BuildCellToBrickIndex(GridGroupShape shape, ref Dictionary<Vector2Int, int> map)
        {
            map.Clear();
            if (!shape.HasBrickData)
                return;

            var bricks = shape.Bricks;
            using var _ = new ListScope<Vector2Int>(out var cells);
            for (int i = 0; i < bricks.Count; i++)
            {
                var b = bricks[i];
                cells.Clear();
                b.EnumerateCells(ref cells);
                foreach (var c in cells)
                    map[c] = i;
            }
        }

        void DrawCellFrame(Vector3 center, float size)
        {
            float half = size * 0.5f;
            var a = center + new Vector3(-half, -half, 0);
            var b = center + new Vector3(half, -half, 0);
            var c = center + new Vector3(half, half, 0);
            var d = center + new Vector3(-half, half, 0);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }

        static List<Vector2Int> _scratchCells = new();
    }
}