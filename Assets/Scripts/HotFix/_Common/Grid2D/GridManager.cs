using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains
{
    /// <summary>
    /// 场景中可视化/管理 <see cref="Grid"/> 的 MonoBehaviour.
    ///
    /// 用法:
    /// 1. 直接挂在 GameObject 上, 在 Inspector 配置 CellSize/Rows/Columns/OriginOffset/Pivot;
    /// 2. 或拖一个 <see cref="GridSetting"/> 进来复用配置 (优先级更高);
    /// 3. 调用 <see cref="CurrentGrid"/> / <see cref="WorldToCell"/> / <see cref="CellToWorld"/> 等.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class GridManager : MonoBehaviour
    {
        [Header("Settings (可选: 拖入 ScriptableObject 配置)")]
        [Tooltip("可选. 若提供, 将由它定义网格参数 (覆盖下方内联参数).")]
        public GridSetting Setting;
        
        [Header("网格形状库")]
        public List<GridGroupShapeLibrary> ShapesLibrary = new();

        [Header("Inline (未指定 ScriptableObject 时使用以下内联值)")]
        [Min(0.0001f)]
        public float CellSize = 1f;

        [Min(1)]
        public int Rows = 10;

        [Min(1)]
        public int Columns = 10;

        public Vector2 OriginOffset;

        public GridPivot Pivot = GridPivot.BottomLeft;

        [Tooltip("true = 在 OriginOffset 之上再叠加 transform.position 作为整体偏移.")]
        public bool UseTransformAsAdditionalOffset = true;

        [Header("Gizmos")]
        public bool DrawGizmos = true;

        public Color LineColor = new(0.45f, 0.85f, 1f, 0.9f);
        public Color BackgroundColor = new(0.45f, 0.85f, 1f, 0.18f);

        [Tooltip("true = 选中本组件时在每个单元格里绘制索引 (x,y). 仅在编辑器模式生效.")]
        public bool DrawCellIndices;

        [Range(1, 16)]
        public int IndexStride = 1;

        // ---------------------------------------------------------------
        // 运行时访问
        // ---------------------------------------------------------------

        GridDefinition _cachedDefinition;
        bool _cacheDirty = true;

        void InvalidateCache() => _cacheDirty = true;

        void RebuildCache()
        {
            if (Setting != null)
            {
                CellSize = Setting.CellSize;
                Rows = Setting.Rows;
                Columns = Setting.Columns;
                OriginOffset = Setting.OriginOffset;
                Pivot = Setting.Pivot;
                _cachedDefinition = Setting.BuildDefinition();
            }
            else
            {
                _cachedDefinition = new(CellSize, Rows, Columns, OriginOffset, Pivot);
            }

            _cacheDirty = false;
        }

        public GridDefinition Definition
        {
            get
            {
                if (_cacheDirty) RebuildCache();
                return _cachedDefinition;
            }
        }

        /// <summary>
        /// 当前生效的网格 (按当前配置 + transform 偏移).
        /// </summary>
        public Grid CurrentGrid()
        {
            if (_cacheDirty)
                RebuildCache();

            var effectiveOffset = OriginOffset;
            if (UseTransformAsAdditionalOffset)
            {
                var pos = transform.position;
                effectiveOffset += new Vector2(pos.x, pos.y);
            }

            return new(new(CellSize, Rows, Columns, effectiveOffset, Pivot));
        }

        // ---------------------------------------------------------------
        // 便捷 API (转发)
        // ---------------------------------------------------------------

        public Vector2Int WorldToCell(Vector2 worldPosition) => CurrentGrid().WorldToCell(worldPosition);
        public Vector2 CellToWorld(Vector2Int cell) => CurrentGrid().CellToWorld(cell);
        public Vector2 CellToWorld(int x, int y) => CurrentGrid().CellToWorld(x, y);
        public Vector2Int WorldToCellClamped(Vector2 worldPosition) => CurrentGrid().WorldToCellClamped(worldPosition);

        public Vector2 WorldToCellPos(Vector2 worldPosition)
        {
            var grid = CurrentGrid();
            var cell = grid.WorldToCellClamped(worldPosition);
            var cellPos = grid.CellToWorld(cell);
            return cellPos;
        }

        public bool InBounds(Vector2Int cell) => CurrentGrid().InBounds(cell);

        /// <summary>
        /// 把 <paramref name="count"/> 个对象按顺序摆到网格的中心列 (从底向上).
        /// </summary>
        public IEnumerable<Vector2> EnumerateCenterColumn(int count)
        {
            var grid = CurrentGrid();
            int middleX = grid.Columns / 2;
            for (int i = 0; i < count; i++)
            {
                int y = Mathf.Clamp(i, 0, grid.Rows - 1);
                yield return grid.CellToWorld(new(middleX, y));
            }
        }

        // ---------------------------------------------------------------
        // Gizmos
        // ---------------------------------------------------------------

        void OnValidate()
        {
            if (CellSize <= 0f) CellSize = 0.0001f;
            if (Rows < 1) Rows = 1;
            if (Columns < 1) Columns = 1;
            InvalidateCache();
        }

        void OnDrawGizmos()
        {
            if (!DrawGizmos)
                return;

            var grid = CurrentGrid();
            int rows = grid.Rows;
            int cols = grid.Columns;
            float size = grid.CellSize;
            Vector2 origin = grid.Origin;

            // 背景填充
            var prev = Gizmos.color;
            Gizmos.color = BackgroundColor;
            Vector3 size3 = new(grid.WorldSize.x, grid.WorldSize.y, 0f);
            Gizmos.DrawCube(new Vector3(origin.x + size3.x * 0.5f, origin.y + size3.y * 0.5f, transform.position.z), size3);

            // 网格线
            Gizmos.color = LineColor;

            for (int x = 0; x <= cols; x++)
            {
                Vector2 a = origin + new Vector2(x * size, 0f);
                Vector2 b = origin + new Vector2(x * size, rows * size);
                Gizmos.DrawLine(a, b);
            }

            for (int y = 0; y <= rows; y++)
            {
                Vector2 a = origin + new Vector2(0f, y * size);
                Vector2 b = origin + new Vector2(cols * size, y * size);
                Gizmos.DrawLine(a, b);
            }

            // 外框 (略亮一点)
            Gizmos.color = new Color(LineColor.r, LineColor.g, LineColor.b, 1f);
            Vector2 bl = origin;
            Vector2 br = origin + new Vector2(cols * size, 0f);
            Vector2 tr = origin + new Vector2(cols * size, rows * size);
            Vector2 tl = origin + new Vector2(0f, rows * size);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);

            Gizmos.color = prev;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!DrawGizmos || !DrawCellIndices)
                return;

            var grid = CurrentGrid();
            int stride = Mathf.Max(1, IndexStride);
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = LineColor },
                fontSize = 11,
            };

            for (int y = 0; y < grid.Rows; y++)
            {
                for (int x = 0; x < grid.Columns; x++)
                {
                    if ((x % stride != 0) || (y % stride != 0))
                        continue;
                    Vector2 world = grid.CellToWorld(x, y);
                    Handles.Label(new Vector3(world.x, world.y, transform.position.z), $"({x},{y})", style);
                }
            }
        }
#endif
    }
}