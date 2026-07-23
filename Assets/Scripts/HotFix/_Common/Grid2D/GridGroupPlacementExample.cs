using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 演示 "用形状模板去填充空置区域" 玩法:
    ///
    /// 1. 在一个大网格里先标好 "已占用" 格子 (例如已有砖块);
    /// 2. 把形状模板 (例如 L 形) 跑遍 8 个朝向, 找出所有可放置位;
    /// 3. 任选一个 origin, 用 <see cref="GridGroupShape.PlaceAt"/> 获取所有 cell;
    /// 4. 在这些 cell 上 spawn 一组砖块 (这里只打印日志).
    /// </summary>
    public class GridGroupPlacementExample : MonoBehaviour
    {
        [Header("Grid Setup")]
        public float CellSize = 1f;

        public int Rows = 8;
        public int Columns = 12;
        public Vector2 OriginOffset = new(-6f, -4f);

        [Header("Already Occupied Cells (模拟已有砖块)")]
        public List<Vector2Int> PreOccupied = new();

        [Header("Shape Library")]
        [Tooltip("选中要测试的形状.")]
        public ShapeKind Kind = ShapeKind.L;

        [Tooltip("是否遍历 8 个朝向 (推荐 true).")]
        public bool AllOrientations = true;

        [Tooltip("是否在 Start 自动执行.")]
        public bool RunOnStart = true;

        void Start()
        {
            if (RunOnStart)
                RunExample();
        }

        [ContextMenu("Run Example")]
        public void RunExample()
        {
            // 1. 构造网格
            var grid = Grid.Create(CellSize, Rows, Columns, OriginOffset);
            var occupied = new HashSet<Vector2Int>(PreOccupied);

            // 2. 选形状
            GridGroupShape shape = Kind switch
            {
                ShapeKind.OneByOne => GridGroupShape.Rectangle(1, 1),
                ShapeKind.OneByTwo => GridGroupShape.Rectangle(2, 1),
                ShapeKind.TwoByOne => GridGroupShape.Rectangle(1, 2),
                ShapeKind.TwoByTwo => GridGroupShape.Rectangle(2, 2),
                ShapeKind.L => GridGroupShape.LShape(),
                ShapeKind.T => GridGroupShape.TShape(),
                ShapeKind.S => GridGroupShape.SShape(),
                ShapeKind.Z => GridGroupShape.ZShape(),
                ShapeKind.J => GridGroupShape.JShape(),
                _ => GridGroupShape.Rectangle(1, 1),
            };

            // 3. 找空置区域并打印
            var emptySet = new HashSet<Vector2Int>();
            foreach (var cell in grid)
            {
                if (!occupied.Contains(cell))
                    emptySet.Add(cell);
            }

            var regions = GridGroupPlacer.FindEmptyRegions(grid, emptySet);
            Debug.Log($"[Grid2D/Example] 形状 = {Kind}, 共 {regions.Count} 个空置区域.");

            // 4. 在所有 origin 上测试形状
            int count = 0;
            foreach (var placement in GridGroupPlacer.FindAllPlacements(shape, grid, occupied, AllOrientations))
            {
                count++;
                Debug.Log($"可放置 #{count}: origin={placement.Origin}, 朝向={placement.Orientation}, " + $"cells=[{string.Join(",", System.Array.ConvertAll(placement.Cells, c => $"({c.x},{c.y})"))}]");

                if (count >= 6)
                    break; // 只演示前 6 个
            }

            Debug.Log($"[Grid2D/Example] 共 {count} 个候选放置位.");
        }

        public enum ShapeKind
        {
            OneByOne,
            OneByTwo,
            TwoByOne,
            TwoByTwo,
            L,
            T,
            S,
            Z,
            J,
        }
    }
}