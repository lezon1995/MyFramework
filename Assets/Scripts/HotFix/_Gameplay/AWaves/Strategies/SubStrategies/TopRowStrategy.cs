using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 顶部行刷怪策略
    /// 特点：
    /// - 砖块在网格的最上方行生成
    /// - 可以配置从顶部开始的行偏移
    /// - 当上方刷新的砖块向下移动出所在的格子后，随后又接着刷新砖块
    /// - 支持多格砖块（会智能寻找合适的空位）
    /// </summary>
    public class TopRowStrategy : SubSpawnStrategy
    {
        private int _topRowOffset;
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

            // 获取顶部行偏移配置
            _topRowOffset = config.topRowStartOffset;
            _consecutiveFailures = 0;
        }

        public override bool TrySpawnOne()
        {
            // 计算目标行（从顶部算起，0是最顶部）
            int targetRow = _gridRows - 1 - _topRowOffset;
            if (targetRow < 0 || targetRow >= _gridRows)
            {
                targetRow = _gridRows - 1; // 默认为最顶部行
            }

            // 获取砖块尺寸
            Vector2Int size = GetRandomBrickSize();

            // 尝试在该行找到一个可以放置的位置
            // 对于多格砖块，需要找到左上角位置
            int maxAttempts = 10;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 随机选择起始列
                int startCol = _random.Next(_gridCols);

                // 根据砖块尺寸计算有效的起始列范围
                int maxStartCol = _gridCols - size.x;
                if (maxStartCol < 0)
                {
                    // 砖块比网格宽，尝试缩小尺寸
                    size = new(Mathf.Min(size.x, _gridCols), size.y);
                    maxStartCol = _gridCols - size.x;
                }

                if (maxStartCol < 0)
                    continue;

                // 生成所有可能的起始位置
                using var _ = new ListScope<Vector2Int>(out var candidateStarts);
                for (int x = 0; x <= maxStartCol; x++)
                {
                    // 计算该位置砖块左上角所在的行
                    int topY = targetRow - size.y + 1;
                    if (topY < 0)
                        topY = 0;

                    var startCell = new Vector2Int(x, topY);
                    if (CanPlaceBrickSizeAt(startCell, size))
                    {
                        candidateStarts.Add(startCell);
                    }
                }

                // 随机选择一个可行的位置
                if (candidateStarts.Count > 0)
                {
                    var chosenStart = candidateStarts[_random.Next(candidateStarts.Count)];

                    // 生成砖块
                    var brick = SpawnBrickAt(chosenStart, size);
                    if (brick != null)
                    {
                        _consecutiveFailures = 0;
                        return true;
                    }
                }
            }

            // 如果在目标行找不到位置，尝试在附近行寻找
            _consecutiveFailures++;
            if (_consecutiveFailures >= 3)
            {
                // 向下扩展搜索范围
                for (int rowOffset = 1; rowOffset <= 3; rowOffset++)
                {
                    int searchRow = targetRow - rowOffset;
                    if (searchRow < 0)
                        break;

                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        int startCol = _random.Next(_gridCols);
                        int maxStartCol = _gridCols - size.x;
                        if (maxStartCol < 0)
                            continue;

                        for (int x = 0; x <= maxStartCol; x++)
                        {
                            int topY = searchRow - size.y + 1;
                            if (topY < 0)
                                topY = 0;

                            var startCell = new Vector2Int(x, topY);
                            if (CanPlaceBrickSizeAt(startCell, size))
                            {
                                var brick = SpawnBrickAt(startCell, size);
                                if (brick != null)
                                {
                                    _consecutiveFailures = 0;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 设置顶部行偏移
        /// </summary>
        public void SetTopRowOffset(int offset)
        {
            _topRowOffset = Mathf.Clamp(offset, 0, _gridRows - 1);
        }

        /// <summary>
        /// 获取当前顶部行
        /// </summary>
        public int GetCurrentTopRow()
        {
            return _gridRows - 1 - _topRowOffset;
        }
    }
}
