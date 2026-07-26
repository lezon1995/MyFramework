using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 砖块允许的移动方向（4 方向，按位枚举）。
    /// </summary>
    [Flags]
    public enum BrickGridMoveDirection
    {
        None = 0,
        Up = 1 << 0, // +Y
        Down = 1 << 1, // -Y
        Left = 1 << 2, // -X
        Right = 1 << 3, // +X
    }

    /// <summary>
    /// 砖块在网格上的移动配置。
    /// 可作为 ScriptableObject 在多个砖块间共享。
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/Brick Grid Move Config", fileName = "BrickGridMoveConfig")]
    public class BrickGridMoveConfig : ScriptableObject
    {
        [Header("Move Directions")]
        [Tooltip("允许的移动方向. 默认全方向 (上下左右).")]
        public BrickGridMoveDirection allowedDirections =
            BrickGridMoveDirection.Up | BrickGridMoveDirection.Down |
            BrickGridMoveDirection.Left | BrickGridMoveDirection.Right;

        [Header("Timing")]
        [Tooltip("每 X 秒考虑下一步移动. 从上一次完全到达开始计时.")]
        [Min(0f)]
        public float moveInterval = 1.0f;

        [Tooltip("从起点 cell 完全走到终点 cell 所需的时间 (秒).")]
        [Min(0.01f)]
        public float moveDuration = 0.5f;

        [Tooltip("移动过渡时间内，是否在到达终点前先解锁起点（false 意味着移动中起点也保持锁定）.")]
        public bool unlockStartOnArrive = true;

        [Header("Chase")]
        [Tooltip("追击半径 (世界单位). 0 视为无限, 玩家始终是目标.")]
        [Min(0f)]
        public float chaseRadius = 8f;

        [Tooltip("追击玩家的目标 cell 半径 (cells). 玩家所在的 cell ±radius 都视为可到目标.")]
        [Min(0)]
        public int chaseRadiusCells;

        [Header("Pathfinding")]
        [Tooltip("在网格外或被锁的 cell 上尝试次数上限, 0 表示无限 (但寻路超时会被截断).")]
        [Min(1)]
        public int maxPathfindingAttempts = 100;

        [Tooltip("如果 BFS 找不到到玩家的路径, 是否仍然尝试向玩家方向走 1 步 (greedy fallback).")]
        public bool useGreedyFallback = true;

        [Tooltip("移动完成后停留多久再开始下一次移动 (秒).")]
        [Min(0f)]
        public float restAfterMove;

        static Comparison<(BrickGridMoveDirection dir, Vector2Int target)> comparison = Comparison;

        /// <summary>
        /// 在 BFS 中获取方向掩码对应的 4 个相邻方向偏移.
        /// </summary>
        public static void GetAllowedOffsets(BrickGridMoveDirection dir, Vector2Int targetDir, ref List<Vector2Int> offsets)
        {
            offsets.Clear();
            if (dir == BrickGridMoveDirection.None)
                return;

            using var _ = new ListScope<(BrickGridMoveDirection dir, Vector2Int target)>(out var sortedDirections);
            if ((dir & BrickGridMoveDirection.Up) != 0) sortedDirections.Add((BrickGridMoveDirection.Up, targetDir));
            if ((dir & BrickGridMoveDirection.Down) != 0) sortedDirections.Add((BrickGridMoveDirection.Down, targetDir));
            if ((dir & BrickGridMoveDirection.Left) != 0) sortedDirections.Add((BrickGridMoveDirection.Left, targetDir));
            if ((dir & BrickGridMoveDirection.Right) != 0) sortedDirections.Add((BrickGridMoveDirection.Right, targetDir));
            sortedDirections.Sort(comparison);

            foreach (var t in sortedDirections)
                offsets.Add(getDir(t.dir));
        }

        static int Comparison((BrickGridMoveDirection dir, Vector2Int target) t1, (BrickGridMoveDirection dir, Vector2Int target) t2)
        {
            var target = t1.target;
            var ax = target.x;
            var ay = target.y;
            var a = getDir(t1.dir);
            var b = getDir(t2.dir);

            int da = a.x * ax + a.y * ay;
            int db = b.x * ax + b.y * ay;
            return db.CompareTo(da);
        }

        static Vector2Int getDir(BrickGridMoveDirection d)
        {
            return d switch
            {
                BrickGridMoveDirection.Up => new(0, 1),
                BrickGridMoveDirection.Down => new(0, -1),
                BrickGridMoveDirection.Left => new(-1, 0),
                BrickGridMoveDirection.Right => new(1, 0),
                BrickGridMoveDirection.None => new(0, 0),
                _ => new(0, 0)
            };
        }
    }
}