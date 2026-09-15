using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 随机行刷怪策略
    /// 特点：
    /// - 砖块在随机的行刷出若干个
    /// - 每次生成会随机选择一行，然后在该行的空位上生成砖块
    /// - 支持多格砖块
    /// </summary>
    public class RandomRowStrategy : SubSpawnStrategy
    {
        private int _batchSize;
        private int _consecutiveFailures;

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

            // 获取批大小配置
            _batchSize = config.batchSize > 0 ? config.batchSize : 3;
            _consecutiveFailures = 0;
        }

        public override bool TrySpawnOne()
        {
            var brickMgr = brickManager;
            if (brickMgr == null)
                return false;

            // 随机选择一行
            int targetRow = _random.Next(_gridRows);
            
            // 获取该行的所有空位
            using var _ = new ListScope<Vector2Int>(out var emptyCells);
            GetEmptyCellsInRow(targetRow, ref emptyCells);
            if (emptyCells.Count == 0)
            {
                // 如果该行没有空位，尝试找其他行
                emptyCells = FindAnyEmptyRowCells(targetRow);
            }

            if (emptyCells.Count == 0)
            {
                _consecutiveFailures++;
                return false;
            }

            // 获取砖块尺寸
            Vector2Int size = GetRandomBrickSize();

            // 在空位中找到可以放置的位置
            using var __ = new ListScope<Vector2Int>(out var validPositions);
            foreach (var cell in emptyCells)
            {
                // 对于多格砖块，需要检查从该位置开始是否可以放置
                if (CanPlaceBrickSizeAt(cell, size))
                {
                    validPositions.Add(cell);
                }
            }

            if (validPositions.Count == 0)
            {
                // 如果没有有效位置，尝试缩小尺寸
                size = new Vector2Int(1, 1);
                foreach (var cell in emptyCells)
                {
                    if (CanPlaceBrickSizeAt(cell, size))
                    {
                        validPositions.Add(cell);
                    }
                }
            }

            if (validPositions.Count == 0)
            {
                _consecutiveFailures++;
                return false;
            }

            // 随机选择一个位置
            var chosenCell = validPositions[_random.Next(validPositions.Count)];
            
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
        /// 尝试在指定行生成多个砖块
        /// </summary>
        public int TrySpawnBatch()
        {
            int spawned = 0;
            for (int i = 0; i < _batchSize; i++)
            {
                if (TrySpawnOne())
                {
                    spawned++;
                }
            }
            return spawned;
        }

        /// <summary>
        /// 在指定行周围寻找有空位的行
        /// </summary>
        private List<Vector2Int> FindAnyEmptyRowCells(int preferredRow)
        {
            using var _ = new ListScope<Vector2Int>(out var result);
            
            // 优先在附近行寻找
            for (int offset = 1; offset <= _gridRows; offset++)
            {
                // 向上搜索
                int upRow = preferredRow + offset;
                if (upRow < _gridRows)
                {
                    GetEmptyCellsInRow(upRow, ref result);
                    if (result.Count > 0)
                        return result;
                }

                // 向下搜索
                int downRow = preferredRow - offset;
                if (downRow >= 0)
                {
                    GetEmptyCellsInRow(downRow, ref result);
                    if (result.Count > 0)
                        return result;
                }
            }

            return result;
        }

        /// <summary>
        /// 设置批大小
        /// </summary>
        public void SetBatchSize(int size)
        {
            _batchSize = Mathf.Max(1, size);
        }

        /// <summary>
        /// 获取批大小
        /// </summary>
        public int GetBatchSize()
        {
            return _batchSize;
        }
    }
}
