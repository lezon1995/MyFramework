using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 序列化友好的 ScriptableObject 配置, 持有
    /// <see cref="CellSize"/> / <see cref="Rows"/> / <see cref="Columns"/> /
    /// <see cref="OriginOffset"/> / <see cref="Pivot"/>.
    ///
    /// 可保存为 .asset 资源, 通过 <see cref="BuildDefinition"/> 转给纯逻辑层的
    /// <see cref="Grid"/> 使用.
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/Grid Setting", fileName = "GridSetting")]
    public class GridSetting : ScriptableObject
    {
        [Tooltip("每一格的边长 (世界单位, 必须 > 0).")]
        [Min(0.0001f)]
        public float CellSize = 1f;

        [Tooltip("行数 (Y 方向, 必须 > 0).")]
        [Min(1)]
        public int Rows = 10;

        [Tooltip("列数 (X 方向, 必须 > 0).")]
        [Min(1)]
        public int Columns = 10;

        [Tooltip("整体偏移, 单位为世界坐标.")]
        public Vector2 OriginOffset = Vector2.zero;

        [Tooltip("原点对应网格的哪个角 / 中心.")]
        public GridPivot Pivot = GridPivot.BottomLeft;

        public GridDefinition BuildDefinition() => new(CellSize, Rows, Columns, OriginOffset, Pivot);

        /// <summary>
        /// 运行时可调用: 直接生成 <see cref="Grid"/>.
        /// </summary>
        public Grid BuildGrid() => new(BuildDefinition());

        /// <summary>
        /// 运行时也可以根据任意参数现场构造, 不必依赖资源.
        /// </summary>
        public static GridSetting CreateTransient(float cellSize, int rows, int columns, Vector2 originOffset, GridPivot pivot)
        {
            var s = CreateInstance<GridSetting>();
            s.CellSize = cellSize;
            s.Rows = rows;
            s.Columns = columns;
            s.OriginOffset = originOffset;
            s.Pivot = pivot;
            s.name = "Grid2DSetting (Transient)";
            return s;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (CellSize <= 0f) CellSize = 0.0001f;
            if (Rows < 1) Rows = 1;
            if (Columns < 1) Columns = 1;
        }
#endif
    }
}
