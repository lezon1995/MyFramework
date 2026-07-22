using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 在场景中编辑/可视化 <see cref="GridGroupShape"/> 的 MonoBehaviour.
    /// 通过 ToggleGrid 编辑形状: ToggleGrid[x,y] = true 即占用该格.
    ///
    /// 同时支持:
    /// - 当前朝向预览;
    /// - 把 Grid2D 占用格作为 occupied, 实时显示"能否放进去".
    /// </summary>
    [AddComponentMenu("Grid2D/GridGroup Shape View")]
    [ExecuteAlways]
    public class GridGroupShapeView : MonoBehaviour
    {
        [Header("Grid")]
        [Tooltip("可选. 拖入 Grid2DSetting 以复用其他网格的参数; 留空则使用下方 inline.")]
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

        [Header("Shape")]
        [Tooltip("形状格点. 行列 (0,0) 在左下, 编辑时按 Shift 点击可单格拖拽; 编辑器中直接修改 ToggleGrid 数组即可.")]
        public List<Vector2Int> ToggleGrid = new();

        [Tooltip("形状锚点.")]
        public GridGroupPivot ShapePivot = GridGroupPivot.BottomLeft;

        [Tooltip("当前预览的朝向.")]
        public GridGroupOrientation Orientation = GridGroupOrientation.Identity;

        [Header("Visual")]
        public bool DrawShape = true;

        public Color ShapeColor = new(0.95f, 0.65f, 0.2f, 0.9f);
        public Color ShapeInvalidColor = new(1f, 0.3f, 0.3f, 0.9f);

        [Tooltip("可选: 用于实时检查形状能否摆入当前网格 (并避开外部 occupied cells).")]
        public bool LiveValidityCheck = true;

        public List<Vector2Int> ExternalOccupied = new();

        // ---------------------------------------------------------------
        // 数据访问
        // ---------------------------------------------------------------

        public GridGroupShape CurrentShape()
        {
            var oriented = BuildShape().WithOrientation(Orientation);
            return oriented;
        }

        public GridGroupShape BuildShape()
        {
            return GridGroupShape.FromCells(ToggleGrid, ShapePivot);
        }

        public Grid CurrentGrid()
        {
            GridDefinition def;
            if (Setting != null)
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

            return new(new(CellSize, Rows, Columns, effectiveOffset, Pivot));
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

        static readonly Vector3[] CubeVerts = new Vector3[8];

        void OnDrawGizmos()
        {
            // 1. 画底层网格线 (浅灰)
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

            // 2. 画形状
            if (!DrawShape || ToggleGrid == null || ToggleGrid.Count == 0)
                return;

            var shape = BuildShape().WithOrientation(Orientation);
            var pivotOffset = shape.PivotOffset;

            // 画以 (0,0) 为锚点 (即放在第一个 cell 的位置)
            var originInGrid = Vector2Int.zero - pivotOffset;
            shape.PlaceAt(originInGrid, _scratchCells);

            foreach (var localCell in _scratchCells)
            {
                var w = grid.CellToWorld(localCell);
                var center = new Vector3(w.x, w.y, transform.position.z);

                bool insideGrid = grid.InBounds(localCell);
                bool conflict = ExternalOccupied.Contains(localCell);
                bool validColor = insideGrid && !conflict;

                Gizmos.color = validColor
                    ? new Color(ShapeColor.r, ShapeColor.g, ShapeColor.b, 0.4f)
                    : new Color(ShapeInvalidColor.r, ShapeInvalidColor.g, ShapeInvalidColor.b, 0.4f);
                Gizmos.DrawCube(center, new(grid.CellSize * 0.92f, grid.CellSize * 0.92f, 0f));

                Gizmos.color = validColor ? ShapeColor : ShapeInvalidColor;
                DrawCellFrame(center, grid.CellSize);
            }

            // 3. 描出锚点
            Vector2 anchorWorld = grid.CellToWorld(-pivotOffset);
            Vector3 anchorPos = new(anchorWorld.x, anchorWorld.y, transform.position.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchorPos, grid.CellSize * 0.25f);
        }

        void DrawCellFrame(Vector3 center, float size)
        {
            var half = size * 0.5f;
            var a = center + new Vector3(-half, -half, 0);
            var b = center + new Vector3(half, -half, 0);
            var c = center + new Vector3(half, half, 0);
            var d = center + new Vector3(-half, half, 0);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }

        static readonly List<Vector2Int> _scratchCells = new();
    }
}