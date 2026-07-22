using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 演示如何把一块砖块 (<see cref="GameObject"/>) 按网格坐标摆放到指定位置.
    /// 该示例不依赖任何具体的 Brick prefab, 仅展示 API 形态;
    /// 项目中可替换为实际的 BrickManager / Brick 资源.
    /// </summary>
    public class GridUsageExample : MonoBehaviour
    {
        [Tooltip("网格规格: 每格边长 / 行数 / 列数 / 整体偏移 / 锚点.")]
        public GridSetting Setting = GridSetting.CreateTransient(1f, 8, 12, new(-6f, -4f), GridPivot.BottomLeft);

        [Tooltip("场景中的根, 用作额外偏移.")] public Transform Anchor;

        void Start()
        {
            // 1. 从 ScriptableObject 构造网格
            var def = Setting.BuildDefinition();
            var grid = new Grid(def);

            Debug.Log($"网格总尺寸: {grid.WorldSize}, 原点: {grid.Origin}");

            // 2. 把鼠标点转换为网格坐标
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cellUnderMouse = grid.WorldToCell(mouse);
            Debug.Log($"鼠标 -> 网格坐标: {cellUnderMouse}");

            // 3. 反向: 把网格坐标转换为世界坐标 (单元中心)
            Vector2 worldCenter = grid.CellToWorld(cellUnderMouse);
            Debug.Log($"({cellUnderMouse.x},{cellUnderMouse.y}) 的中心世界坐标: {worldCenter}");

            // 4. 遍历所有单元 (从下到上, 从左到右)
            foreach (var cell in grid)
            {
                // 这里可以按业务规则 spawn brick:
                // BrickManager.Spawn(cell, grid.CellToWorld(cell));
                if ((cell.x + cell.y) % 5 == 0)
                {
                    Debug.Log($"在 {cell} 放置砖块, 世界坐标 = {grid.CellToWorld(cell)}");
                }
            }

            // 5. 越界检查
            if (grid.InBounds(cellUnderMouse))
            {
                // do something
            }
            else
            {
                Debug.Log($"鼠标位置 {mouse} 落在网格之外.");
            }
        }
    }
}