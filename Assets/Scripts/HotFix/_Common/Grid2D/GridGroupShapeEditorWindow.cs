#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains
{
    /// <summary>
    /// GridGroupShape 可视化编辑器窗口.
    /// 双击 GridGroupShapeLibrary 资产即可打开.
    ///
    /// 布局:
    ///  ┌─ Palette ─┐ ┌─ Canvas ─────────────────┐ ┌─ Properties ─┐
    ///  │ [1x1]    │ │                             │ │ Name: [...]  │
    ///  │ [1x2]    │ │  (网格画布,鼠标放置/删除)    │ │ Pivot: [▼]   │
    ///  │ ...      │ │                             │ │ Cells: 7     │
    ///  └──────────┘ └─────────────────────────────┘ └─────────────┘
    ///  ┌─ Library Shapes ──────────────────────────────────────────┐
    ///  │ ▶ LShape_01 (4 bricks)          [Load] [Del]             │
    ///  └───────────────────────────────────────────────────────────┘
    /// </summary>
    public class GridGroupShapeEditorWindow : EditorWindow
    {
        // ---------------------------------------------------------------
        // 静态入口
        // ---------------------------------------------------------------

        [MenuItem("Window/MoreMountains/Grid Group Shape Editor")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<GridGroupShapeEditorWindow>("Shape Editor", true);
            wnd.minSize = new Vector2(800, 580);
            wnd.Show();
        }

        // 双击 Library 资产时自动打开
        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);
            if (asset is GridGroupShapeLibrary library)
            {
                OpenWindow();
                _instance?.LoadLibrary(library);
                return true;
            }
            return false;
        }

        static GridGroupShapeEditorWindow _instance;

        // ---------------------------------------------------------------
        // 状态
        // ---------------------------------------------------------------

        const int grid_count = 14;

        GridGroupShapeLibrary _library;
        ShapeEntry _currentEntry;

        // 当前编辑的形状 (临时的 ShapeEntry 拷贝)
        ShapeEntry _working;

        // 砖块调色板选中项
        Vector2Int _selectedBrickSize = new(1, 1);

        // 当前悬停 cell
        Vector2Int? _hoverCell;

        // 拖拽状态
        bool _isDragging;
        GridUnitBrick _draggingBrick;
        int _draggingIndex = -1;

        // 缩放/平移
        float _zoom = 1f;
        Vector2 _pan = Vector2.zero;

        // 画布区域 rect
        Rect _canvasRect;
        Vector2 _canvasOrigin;
        float _cellPixelSize = 28f;
        const float _constCellPixelSize = 28f;

        // 左键按下时的 cell (用于放置)
        Vector2Int? _mouseDownCell;
        Vector2Int? _mouseUpCell;

        // 预定义尺寸按钮
        static readonly Vector2Int[] _presetSizes = new[]
        {
            new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(1,3),
            new Vector2Int(2,1), new Vector2Int(2,2), new Vector2Int(2,3),
            new Vector2Int(3,1), new Vector2Int(3,2), new Vector2Int(3,3),
        };

        // ---------------------------------------------------------------
        // 生命周期
        // ---------------------------------------------------------------

        void OnEnable()
        {
            _instance = this;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        void OnDisable()
        {
            _instance = null;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                SaveCurrent();
        }

        void OnGUI()
        {
            if (_library == null)
            {
                DrawWelcome();
                return;
            }

            DrawToolbar();
            EditorGUILayout.Space(2);

            // 三栏: 固定宽度 Palette + 固定宽度 Properties, 中间 Canvas 自适应
            float paletteW = 82f;
            float propW = 210f;

            EditorGUILayout.BeginHorizontal();

            // 左: 砖块调色板
            DrawPalette(paletteW);

            GUILayout.Space(2);

            // 中: 网格画布 (自适应剩余宽度)
            DrawCanvas();

            GUILayout.Space(2);

            // 右: 属性面板
            DrawProperties(propW);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // 下: 库形状列表
            DrawLibraryList();

            DrawFooter();
        }

        // ---------------------------------------------------------------
        // Toolbar
        // ---------------------------------------------------------------

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                NewEntry();

            GUI.enabled = _currentEntry != null;
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
                SaveCurrent();

            GUI.enabled = _working != null && _working.expandedCells.Count > 0;
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
                ClearWorking();

            GUI.enabled = true;

            GUILayout.Space(10);
            GUILayout.Label($"Library: {(_library ? _library.name : "None")}", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            GUI.enabled = _library != null;
            if (GUILayout.Button("Open Library...", EditorStyles.toolbarButton, GUILayout.Width(100)))
                PickLibrary();

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------------------------------------------
        // 砖块调色板
        // ---------------------------------------------------------------

        static readonly Color[] _brickColors = new[]
        {
            new Color(0.45f, 0.85f, 1.0f),     // 1x1
            new Color(0.30f, 0.75f, 0.40f),     // 1x2
            new Color(0.20f, 0.65f, 0.85f),     // 1x3
            new Color(0.95f, 0.60f, 0.20f),     // 2x1
            new Color(0.90f, 0.85f, 0.15f),     // 2x2
            new Color(0.75f, 0.35f, 0.85f),     // 2x3
            new Color(0.95f, 0.30f, 0.30f),     // 3x1
            new Color(0.55f, 0.30f, 0.80f),     // 3x2
            new Color(0.25f, 0.55f, 0.25f),     // 3x3
        };

        void DrawPalette(float width)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width), GUILayout.ExpandHeight(true));

            GUILayout.Label("Brick", EditorStyles.boldLabel, GUILayout.Height(20));

            for (int i = 0; i < _presetSizes.Length; i++)
            {
                var sz = _presetSizes[i];
                bool isSelected = _selectedBrickSize == sz;
                var col = _brickColors[i];

                GUI.backgroundColor = col;
                var content = new GUIContent($"{sz.x}x{sz.y}");
                bool clicked = GUILayout.Button(content,
                    isSelected ? EditorStyles.toolbarButton : EditorStyles.miniButton,
                    GUILayout.Width(width - 4), GUILayout.Height(24));

                GUI.backgroundColor = Color.white;

                if (clicked)
                {
                    _selectedBrickSize = sz;
                    Repaint();
                }
            }

            GUILayout.FlexibleSpace();

            // 选中预览
            int selIdx = Array.IndexOf(_presetSizes, _selectedBrickSize);
            var selCol = selIdx >= 0 ? _brickColors[selIdx] : Color.white;
            EditorGUILayout.LabelField("Selected", EditorStyles.centeredGreyMiniLabel);
            GUI.backgroundColor = selCol;
            GUILayout.Box(GUIContent.none, GUILayout.Width(width - 8), GUILayout.Height(Mathf.Max(_selectedBrickSize.y * 14f, 14f)));
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        // ---------------------------------------------------------------
        // 网格画布
        // ---------------------------------------------------------------

        void DrawCanvas()
        {
            // 固定高度,自适应宽度
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // 工具栏
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(22));
            GUILayout.Label("Canvas", EditorStyles.label);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                _zoom = Mathf.Min(_zoom * 1.25f, 5f);
                Repaint();
            }
            GUILayout.Label($"{_zoom:P0}", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(40));
            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                _zoom = Mathf.Max(_zoom / 1.25f, 0.2f);
                Repaint();
            }
            if (GUILayout.Button("\u29C9", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                _zoom = 1f;
                _pan = Vector2.zero;
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            // 画布区域 (固定最大高度,避免 GUILayout 撑到 16384)
            var availableRect = GUILayoutUtility.GetRect(1, 16384, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            availableRect.width = Mathf.Min(availableRect.width, _constCellPixelSize * grid_count * _zoom);
            availableRect.height = Mathf.Min(availableRect.height, _constCellPixelSize * grid_count * _zoom);
            _canvasRect = EditorGUI.IndentedRect(availableRect);

            // 处理输入事件
            HandleCanvasEvents();

            // 绘制只在 Repaint
            if (Event.current.type != EventType.Repaint)
            {
                _hoverCell = ScreenToCanvas(Event.current.mousePosition);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUIUtility.AddCursorRect(_canvasRect, MouseCursor.Arrow);

            // 计算网格参数: 左下角原点
            _cellPixelSize = Mathf.Min(_canvasRect.width, _canvasRect.height) / grid_count;
            _cellPixelSize = Mathf.Clamp(_cellPixelSize, 8f, 80f);

            float gridW = grid_count * _cellPixelSize;
            float gridH = grid_count * _cellPixelSize;

            _canvasOrigin = new(
                _canvasRect.x + (_canvasRect.width - gridW) / 2f - _pan.x,
                _canvasRect.yMax - (_canvasRect.height) / 2f - _pan.y - gridH / 2f);

            // 背景
            GUI.Box(_canvasRect, GUIContent.none, GUI.skin.box);

            // 网格线
            DrawGridLines();

            // 砖块
            DrawPlacedBricks();

            // 悬停预览
            if (_hoverCell.HasValue/* && !_isDragging*/)
                DrawHoverPreview();

            // 拖拽
            if (_isDragging)
                DrawDraggingBrick();

            EditorGUILayout.EndVertical();
        }

        // ---------------------------------------------------------------
        // 画布绘制 (Handles API)
        // ---------------------------------------------------------------

        void DrawGridLines()
        {
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

            for (int y = 0; y <= grid_count; y++)
            {
                Vector3 p0 = CanvasToScreen(new(0, y));
                Vector3 p1 = CanvasToScreen(new(grid_count, y));
                Handles.DrawLine(p0, p1);
            }
            for (int x = 0; x <= grid_count; x++)
            {
                Vector3 p0 = CanvasToScreen(new(x, 0));
                Vector3 p1 = CanvasToScreen(new(x, grid_count));
                Handles.DrawLine(p0, p1);
            }

            // 边框
            Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.6f);
            Handles.DrawLine(CanvasToScreen(new(0,0)), CanvasToScreen(new(grid_count, 0)));
            Handles.DrawLine(CanvasToScreen(new(grid_count, 0)), CanvasToScreen(new(grid_count, grid_count)));
            Handles.DrawLine(CanvasToScreen(new(grid_count, grid_count)), CanvasToScreen(new(0, grid_count)));
            Handles.DrawLine(CanvasToScreen(new(0, grid_count)), CanvasToScreen(new(0,0)));

            // 坐标标注
            Handles.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            var numStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 8,
                alignment = TextAnchor.MiddleCenter,
            };
            for (int x = 0; x <= grid_count; x++)
                Handles.Label(CanvasToScreen(new(x, 0)) + new Vector3(0, 8, 0), x.ToString(), numStyle);
            for (int y = 0; y <= grid_count; y++)
                Handles.Label(CanvasToScreen(new(0, y)) + new Vector3(-8, 0, 0), y.ToString(), numStyle);
        }

        void DrawPlacedBricks()
        {
            if (_working == null) return;

            int colorIdx = 0;
            foreach (var brick in _working.bricks)
            {
                var col = _brickColors[colorIdx++ % _brickColors.Length];
                DrawSingleBrick(brick.col, brick.row, brick.width, brick.height, col, 0.9f);
            }
        }

        void DrawHoverPreview()
        {
            if (!_hoverCell.HasValue) 
                return;

            if (_working != null && BrickExistsAt(_working, _hoverCell.Value))
                return;

            int selIdx = Array.IndexOf(_presetSizes, _selectedBrickSize);
            var col = selIdx >= 0 ? _brickColors[selIdx] : Color.white;
            DrawSingleBrick(_hoverCell.Value.x, _hoverCell.Value.y, _selectedBrickSize.x, _selectedBrickSize.y, col, 0.35f);
        }

        void DrawDraggingBrick()
        {
            int bx = Mathf.Clamp(_draggingBrick.col, 0, grid_count - _draggingBrick.width);
            int by = Mathf.Clamp(_draggingBrick.row, 0, grid_count - _draggingBrick.height);
            DrawSingleBrick(bx, by, _draggingBrick.width, _draggingBrick.height, new Color(1f, 1f, 0f, 0.55f), 0.6f);
        }

        void DrawSingleBrick(int col, int row, int w, int h, Color fillColor, float alpha)
        {
            int x0 = Mathf.Clamp(col, 0, grid_count);
            int y0 = Mathf.Clamp(row, 0, grid_count);
            int x1 = Mathf.Clamp(col + w, 0, grid_count);
            int y1 = Mathf.Clamp(row + h, 0, grid_count);
            if (x0 >= x1 || y0 >= y1) 
                return;

            Vector3 min = CanvasToScreen(new(x0, y0));
            Vector3 max = CanvasToScreen(new(x1, y1));

            Color fill = new Color(fillColor.r, fillColor.g, fillColor.b, alpha);
            Color border = new Color(fillColor.r * 0.7f, fillColor.g * 0.7f, fillColor.b * 0.7f, 1f);

            Handles.DrawSolidRectangleWithOutline(new Rect(min.x, min.y, max.x - min.x, max.y - min.y), fill, border);

            if (_cellPixelSize >= 14f)
            {
                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = Mathf.Clamp((int)(_cellPixelSize * 0.4f), 7, 14),
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                Handles.Label(new((min.x + max.x) / 2f, (min.y + max.y) / 2f, 0), $"{w}x{h}", labelStyle);
            }
        }

        /// <summary>形状坐标 (col, row) 转为屏幕像素坐标. 原点左下角, y 向 上.</summary>
        Vector3 CanvasToScreen(Vector2Int canvasPos)
        {
            return new(
                _canvasOrigin.x + canvasPos.x * _cellPixelSize,
                _canvasOrigin.y + (grid_count - canvasPos.y) * _cellPixelSize,
                0);
        }

        /// <summary>屏幕像素坐标转为形状坐标. 原点左下角, y 向 上.</summary>
        Vector2Int? ScreenToCanvas(Vector2 screenPos)
        {
            if (!_canvasRect.Contains(screenPos)) 
                return null;

            float x = (screenPos.x - _canvasOrigin.x) / _cellPixelSize;
            float y = (screenPos.y - _canvasOrigin.y) / _cellPixelSize;
            var coordX = Mathf.FloorToInt(x);
            var coordY = grid_count - Mathf.FloorToInt(y) - 1;
            Debug.Log($"coord = {new Vector2Int(coordX, coordY)}");
            return new(coordX, coordY);
        }

        // ---------------------------------------------------------------
        // 画布交互
        // ---------------------------------------------------------------

        void HandleCanvasEvents()
        {
            var ev = Event.current;
            var evType = ev.type;

            // 鼠标位置变化时实时更新悬停 (所有事件类型都处理)
            if (evType == EventType.MouseMove || evType == EventType.MouseDrag || evType == EventType.Repaint || evType == EventType.Layout)
            {
                _hoverCell = ScreenToCanvas(ev.mousePosition);
            }

            // 只处理鼠标相关事件,且必须在 canvas 内
            if (evType != EventType.MouseDown &&
                evType != EventType.MouseUp &&
                evType != EventType.MouseDrag)
                return;

            if (!_canvasRect.Contains(ev.mousePosition))
                return;

            var cell = ScreenToCanvas(ev.mousePosition);
            if (!cell.HasValue) 
                return;

            switch (evType)
            {
                case EventType.MouseDown:
                    if (ev.button == 0)
                    {
                        _mouseDownCell = cell.Value;
                        _mouseUpCell = null;

                        // 检测是否点在了已有砖块上 -> 拖拽
                        var hit = HitBrickAt(_working, cell.Value);
                        if (hit.HasValue)
                        {
                            _isDragging = true;
                            _draggingBrick = hit.Value;
                            _draggingIndex = _working.bricks.IndexOf(hit.Value);
                        }
                    }
                    else if (ev.button == 1)
                    {
                        RemoveBrickAt(_working, cell.Value);
                    }
                    ev.Use();
                    Repaint();
                    break;

                case EventType.MouseDrag:
                    if (ev.button == 0 && _isDragging && cell.HasValue)
                    {
                        // 跟随鼠标拖拽
                        _draggingBrick = new GridUnitBrick(
                            cell.Value.x, cell.Value.y,
                            _draggingBrick.width, _draggingBrick.height);
                    }
                    ev.Use();
                    Repaint();
                    break;

                case EventType.MouseUp:
                    if (ev.button == 0)
                    {
                        _mouseUpCell = cell.Value;

                        if (_isDragging)
                        {
                            // 结束拖拽
                            _draggingBrick = new GridUnitBrick(
                                Mathf.Clamp(_draggingBrick.col, 0, grid_count - _draggingBrick.width),
                                Mathf.Clamp(_draggingBrick.row, 0, grid_count - _draggingBrick.height),
                                _draggingBrick.width, _draggingBrick.height);

                            if (_draggingIndex >= 0 && _draggingIndex < _working.bricks.Count)
                            {
                                _working.bricks[_draggingIndex] = _draggingBrick;
                                _working.RebuildExpandedCells();
                            }
                            _isDragging = false;
                            _draggingIndex = -1;
                        }
                        else if (_mouseDownCell.HasValue && _mouseDownCell.Value == cell.Value)
                        {
                            // 点击放置
                            TryPlaceBrick(_working, cell.Value, _selectedBrickSize);
                        }
                    }
                    ev.Use();
                    Repaint();
                    break;
            }
        }

        // ---------------------------------------------------------------
        // 砖块操作辅助
        // ---------------------------------------------------------------

        bool BrickExistsAt(ShapeEntry entry, Vector2Int cell)
        {
            if (entry == null) 
                return false;

            return HitBrickAt(entry, cell).HasValue;
        }

        GridUnitBrick? HitBrickAt(ShapeEntry entry, Vector2Int cell)
        {
            if (entry == null) 
                return null;

            foreach (var b in entry.bricks)
            {
                if (cell.x >= b.col && cell.x < b.col + b.width && cell.y >= b.row && cell.y < b.row + b.height)
                    return b;
            }
            return null;
        }

        bool TryPlaceBrick(ShapeEntry entry, Vector2Int cell, Vector2Int size)
        {
            if (entry == null) 
                return false;

            for (int dy = 0; dy < size.y; dy++)
            {
                for (int dx = 0; dx < size.x; dx++)
                {
                    var c = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (c.x < 0 || c.x >= grid_count || c.y < 0 || c.y >= grid_count) 
                        continue;

                    if (BrickExistsAt(entry, c)) 
                        return false;
                }
            }

            entry.bricks.Add(new(cell.x, cell.y, size.x, size.y));
            entry.RebuildExpandedCells();
            return true;
        }

        void RemoveBrickAt(ShapeEntry entry, Vector2Int cell)
        {
            if (entry == null) 
                return;

            var hit = HitBrickAt(entry, cell);
            if (hit.HasValue)
            {
                entry.bricks.Remove(hit.Value);
                entry.RebuildExpandedCells();
            }
        }

        // ---------------------------------------------------------------
        // 属性面板
        // ---------------------------------------------------------------

        Vector2 _libraryScroll;

        void DrawProperties(float width)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width), GUILayout.ExpandHeight(true));

            GUILayout.Label("Properties", EditorStyles.boldLabel, GUILayout.Height(20));

            if (_working == null || _working.expandedCells.Count == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Empty shape.\nClick canvas to place bricks.", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(40));
                EditorGUILayout.EndVertical();
                return;
            }

            // 名称
            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField("Name", _working.name);
            if (EditorGUI.EndChangeCheck())
                _working.name = name;

            // Pivot
            EditorGUI.BeginChangeCheck();
            var pivot = (GridGroupPivot)EditorGUILayout.EnumPopup("Pivot", _working.pivot);
            if (EditorGUI.EndChangeCheck())
                _working.pivot = pivot;

            // 统计
            EditorGUILayout.LabelField("Bricks", _working.bricks.Count.ToString());
            EditorGUILayout.LabelField("Cells", _working.expandedCells.Count.ToString());

            var sz = _working.LocalSize;
            EditorGUILayout.LabelField("Size", $"{sz.x} x {sz.y} cells");

            EditorGUILayout.Space(4);

            GUILayout.Label("Bricks Detail", EditorStyles.boldLabel);
            _libraryScroll = EditorGUILayout.BeginScrollView(_libraryScroll,
                GUI.skin.box, GUILayout.ExpandHeight(true));

            for (int i = 0; i < _working.bricks.Count; i++)
            {
                var b = _working.bricks[i];
                EditorGUILayout.LabelField($"  [{i}]", $"{b.width}x{b.height} at ({b.col},{b.row})", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Delete All Bricks", GUILayout.Height(26)))
                ClearWorking();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        // ---------------------------------------------------------------
        // 库列表
        // ---------------------------------------------------------------

        void DrawLibraryList()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Library Shapes ({_library?.shapes.Count ?? 0})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            GUI.enabled = _working != null && _working.expandedCells.Count > 0;
            if (GUILayout.Button("Save to Library", GUILayout.Width(110)))
                SaveCurrent();

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (_library == null || _library.shapes.Count == 0)
            {
                GUILayout.Label("  No shapes. Create one above and click 'Save to Library'.", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(150));
            EditorGUILayout.LabelField("Bricks", GUILayout.Width(50));
            EditorGUILayout.LabelField("Cells", GUILayout.Width(50));
            EditorGUILayout.LabelField("Size", GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            foreach (var entry in _library.shapes)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                bool isSelected = _currentEntry == entry;
                if (isSelected) 
                    GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);

                EditorGUILayout.LabelField(entry.name, GUILayout.Width(150));
                EditorGUILayout.LabelField(entry.bricks.Count.ToString(), GUILayout.Width(50));
                EditorGUILayout.LabelField(entry.expandedCells.Count.ToString(), GUILayout.Width(50));
                var sz = entry.LocalSize;
                EditorGUILayout.LabelField($"{sz.x}x{sz.y}", GUILayout.Width(80));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Load", GUILayout.Width(50)))
                    LoadEntry(entry);

                if (GUILayout.Button("Del", GUILayout.Width(40)))
                {
                    if (EditorUtility.DisplayDialog("Delete Shape", $"Delete '{entry.name}'?", "Delete", "Cancel"))
                    {
                        _library.shapes.Remove(entry);
                        if (_currentEntry == entry)
                        {
                            _currentEntry = null;
                            _working = null;
                        }
                        EditorUtility.SetDirty(_library);
                    }
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        // ---------------------------------------------------------------
        // 欢迎界面
        // ---------------------------------------------------------------

        void DrawWelcome()
        {
            GUILayout.Space(20);
            GUILayout.Label("Grid Group Shape Editor", EditorStyles.foldoutHeader);
            GUILayout.Space(10);

            GUILayout.Label("Open a GridGroupShapeLibrary asset to start editing.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Open Library...", GUILayout.Width(150), GUILayout.Height(30)))
                PickLibrary();

            GUILayout.Space(10);
            GUILayout.Label("Or create a new one:", EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Create New Library", GUILayout.Width(180), GUILayout.Height(28)))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "New GridGroupShapeLibrary", "GridGroupShapeLibrary", "asset",
                    "Choose where to save the new library.");
                if (!string.IsNullOrEmpty(path))
                {
                    var lib = ScriptableObject.CreateInstance<GridGroupShapeLibrary>();
                    AssetDatabase.CreateAsset(lib, path);
                    AssetDatabase.SaveAssets();
                    LoadLibrary(lib);
                }
            }
        }

        // ---------------------------------------------------------------
        // 底部提示
        // ---------------------------------------------------------------

        void DrawFooter()
        {
            if (Event.current?.type != EventType.Layout && Event.current?.type != EventType.Repaint)
                return;

            GUILayout.Space(-4);
            EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label("LMB: Place  |  RMB: Remove  |  Drag: Move  |  Scroll: Zoom  |  Drag BG: Pan",
                EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------------------------------------------
        // 库操作
        // ---------------------------------------------------------------

        void PickLibrary()
        {
            string path = EditorUtility.OpenFilePanel("Open GridGroupShapeLibrary", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Replace(Application.dataPath, "").Replace('\\', '/');
                var lib = AssetDatabase.LoadAssetAtPath<GridGroupShapeLibrary>(path);
                if (lib) LoadLibrary(lib);
            }
        }

        void LoadLibrary(GridGroupShapeLibrary lib)
        {
            _library = lib;
            _currentEntry = null;
            _working = null;
            _zoom = 1f;
            _pan = Vector2.zero;
            titleContent = new GUIContent($"Shape Editor - {lib.name}");
            Repaint();
        }

        void NewEntry()
        {
            _currentEntry = null;
            _working = new ShapeEntry
            {
                name = $"Shape_{(_library?.shapes.Count ?? 0) + 1:D2}",
                id = Guid.NewGuid().ToString("N").Substring(0, 8),
                bricks = new List<GridUnitBrick>(),
            };
            _working.RebuildExpandedCells();
            Repaint();
        }

        void LoadEntry(ShapeEntry entry)
        {
            _currentEntry = entry;
            _working = new ShapeEntry
            {
                name = entry.name,
                id = entry.id,
                bricks = new List<GridUnitBrick>(entry.bricks),
                pivot = entry.pivot,
                expandedCells = new List<Vector2Int>(entry.expandedCells),
            };
            Repaint();
        }

        void SaveCurrent()
        {
            if (_working == null || _library == null) return;
            if (string.IsNullOrEmpty(_working.name))
                _working.name = $"Shape_{_library.shapes.Count + 1:D2}";

            _working.EnsureId();
            _working.RebuildExpandedCells();

            var existing = _library.GetById(_working.id);
            if (existing != null)
            {
                existing.name = _working.name;
                existing.pivot = _working.pivot;
                existing.bricks.Clear();
                existing.bricks.AddRange(_working.bricks);
                existing.RebuildExpandedCells();
            }
            else
            {
                _library.shapes.Add(_working);
                _currentEntry = _working;
            }

            EditorUtility.SetDirty(_library);
            AssetDatabase.SaveAssets();
            Repaint();
        }

        void ClearWorking()
        {
            if (_working == null) return;
            _working.bricks.Clear();
            _working.RebuildExpandedCells();
            Repaint();
        }
    }
}
#endif
