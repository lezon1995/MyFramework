using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 随机空位刷怪策略
    /// 特点：
    /// - 砖块在随机的空格子处生成
    /// - 可以选择是否优先在边缘生成
    /// - 支持多格砖块
    /// - 会检查多格砖块所需的全部格子是否都为空
    /// </summary>
    public class RandomEmptyStrategy : SubSpawnStrategy
    {
        private bool _preferEdge;
        private float _edgeBias;
        private int _consecutiveFailures;
        private int _maxRetries;

        public override void Initialize(
            WaveManager waveManager,
            WaveConfig waveConfig,
            WaveLevelConfig levelConfig,
            int gridCols,
            int gridRows,
            SubSpawnStrategyConfig config,
            List<BrickSizeConfig> brickSizeConfigs)
        {
            base.Initialize(waveManager, waveConfig, levelConfig, gridCols, gridRows, config, brickSizeConfigs);

            // 获取边缘偏好配置
            _preferEdge = config.preferEdge;
            _edgeBias = _preferEdge ? 0.7f : 0.3f;
            _consecutiveFailures = 0;
            _maxRetries = 20;
        }

        public override bool TrySpawnOne()
        {
            var brickMgr = brickManager;
            if (brickMgr == null)
                return false;

            // 获取砖块尺寸
            Vector2Int size = GetRandomBrickSize();

            // 收集候选位置
            using var _ = new ListScope<Vector2Int>(out var candidates);
            CollectCandidateCells(size, ref candidates);

            if (candidates.Count == 0)
            {
                _consecutiveFailures++;
                return false;
            }

            // 根据边缘偏好选择位置
            Vector2Int chosenCell;
            if (_preferEdge && candidates.Count > 1)
            {
                chosenCell = SelectEdgeBiasedCell(candidates);
            }
            else
            {
                chosenCell = candidates[_random.Next(candidates.Count)];
            }

            // 生成砖块
            var brick = SpawnBrickAt(chosenCell, size);
            if (brick != null)
            {
                _consecutiveFailures = 0;
                return true;
            }

            _consecutiveFailures++;
            return false;
        }

        /// <summary>
        /// 收集所有可以放置砖块的候选位置
        /// </summary>
        private void CollectCandidateCells(Vector2Int size, ref List<Vector2Int> candidates)
        {
            candidates.Clear();
            // 遍历所有格子，寻找可以放置的位置
            for (int y = 0; y < _gridRows; y++)
            {
                for (int x = 0; x < _gridCols; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (CanPlaceBrickSizeAt(cell, size))
                    {
                        candidates.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// 根据边缘偏好选择一个位置
        /// </summary>
        private Vector2Int SelectEdgeBiasedCell(List<Vector2Int> candidates)
        {
            if (candidates.Count == 0)
                return Vector2Int.zero;

            // 分离边缘和非边缘格子
            using var _ = new ListScope2<Vector2Int>(out var edgeCells, out var interiorCells);
            foreach (var cell in candidates)
            {
                if (IsEdgeCell(cell))
                {
                    edgeCells.Add(cell);
                }
                else
                {
                    interiorCells.Add(cell);
                }
            }

            // 根据边缘偏好概率选择
            float roll = (float)_random.NextDouble();
            if (roll < _edgeBias && edgeCells.Count > 0)
            {
                return edgeCells[_random.Next(edgeCells.Count)];
            }
            else if (interiorCells.Count > 0)
            {
                return interiorCells[_random.Next(interiorCells.Count)];
            }
            else if (edgeCells.Count > 0)
            {
                return edgeCells[_random.Next(edgeCells.Count)];
            }

            return candidates[_random.Next(candidates.Count)];
        }

        /// <summary>
        /// 判断格子是否为边缘格子
        /// </summary>
        private bool IsEdgeCell(Vector2Int cell)
        {
            // 第一行或最后一行
            if (cell.y == 0 || cell.y == _gridRows - 1)
                return true;

            // 第一列或最后一列
            if (cell.x == 0 || cell.x == _gridCols - 1)
                return true;

            // 距离边缘一个格子内也算边缘
            if (cell.y == 1 || cell.y == _gridRows - 2)
                return true;
            if (cell.x == 1 || cell.x == _gridCols - 2)
                return true;

            return false;
        }

        /// <summary>
        /// 设置是否优先边缘
        /// </summary>
        public void SetPreferEdge(bool prefer)
        {
            _preferEdge = prefer;
            _edgeBias = prefer ? 0.7f : 0.3f;
        }

        /// <summary>
        /// 设置边缘偏差值
        /// </summary>
        public void SetEdgeBias(float bias)
        {
            _edgeBias = Mathf.Clamp01(bias);
        }

        /// <summary>
        /// 获取最大重试次数
        /// </summary>
        public int GetMaxRetries()
        {
            return _maxRetries;
        }
    }
}
