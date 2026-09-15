using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace MoreMountains
{
    /// <summary>
    /// 子刷怪策略基类
    /// 定义各种子策略的通用行为
    /// </summary>
    public abstract class SubSpawnStrategy
    {
        protected WaveManager _waveManager;
        protected WaveConfig _waveConfig;
        protected WaveLevelConfig _levelConfig;
        protected SubSpawnStrategyConfig _config;
        protected List<BrickSizeConfig> _brickSizeConfigs;
        protected Random _random;
        protected int _gridCols;
        protected int _gridRows;

        public virtual void Initialize(
            WaveManager waveManager,
            WaveConfig waveConfig,
            WaveLevelConfig levelConfig,
            int gridCols,
            int gridRows,
            SubSpawnStrategyConfig config,
            List<BrickSizeConfig> brickSizeConfigs)
        {
            _waveManager = waveManager;
            _waveConfig = waveConfig;
            _levelConfig = levelConfig;
            _gridCols = gridCols;
            _gridRows = gridRows;
            _config = config;
            _brickSizeConfigs = brickSizeConfigs ?? new List<BrickSizeConfig>();
            _random = new Random();
        }

        /// <summary>
        /// 尝试生成一个砖块
        /// </summary>
        /// <returns>是否成功生成</returns>
        public abstract bool TrySpawnOne();

        /// <summary>
        /// 检查指定位置的砖块尺寸是否可以放置
        /// </summary>
        protected bool CanPlaceBrickAt(Vector2Int cell, Vector2Int size)
        {
            // 检查砖块是否完全在网格范围内
            if (cell.x < 0 || cell.y < 0)
                return false;
            if (cell.x + size.x > _gridCols || cell.y + size.y > _gridRows)
                return false;

            // 检查所有被砖块覆盖的格子是否都为空
            var brickMgr = brickManager;
            for (int dy = 0; dy < size.y; dy++)
            {
                for (int dx = 0; dx < size.x; dx++)
                {
                    var checkCell = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (brickMgr.IsCellEmpty(checkCell.ToPos())) 
                        continue;

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 在指定位置生成砖块
        /// </summary>
        protected Brick SpawnBrickAt(Vector2Int cell, Vector2Int size)
        {
            var brickMgr = brickManager;
            if (brickMgr == null)
                return null;

            // 获取该尺寸的砖块定义
            var brickDef = brickMgr.GetRandomDef(size);
            if (brickDef == null)
            {
                // 如果没有该尺寸的定义，尝试获取1x1的
                brickDef = brickMgr.GetRandomDef(new(1, 1));
                if (brickDef == null)
                    return null;
            }

            // 计算砖块的世界位置
            var gridManager = _waveManager?.ResolveGridManager();
            Vector3 worldPos;
            if (gridManager != null)
            {
                var grid = gridManager.CurrentGrid();
                worldPos = grid.CellToWorld(cell);
            }
            else
            {
                // 使用BrickGridLayout
                var layout = brickMgr.brickLayout;
                worldPos = layout.getPos(cell.x, cell.y);
            }

            // 生成砖块
            var brick = brickMgr.acquireBrick(brickDef, worldPos);
            return brick;
        }

        /// <summary>
        /// 从可用尺寸中根据权重随机选择一个尺寸
        /// </summary>
        protected Vector2Int GetRandomBrickSize()
        {
            return new(1, 1);
            if (_brickSizeConfigs == null || _brickSizeConfigs.Count == 0)
                return new(1, 1);

            // 计算总权重
            float totalWeight = 0f;
            foreach (var config in _brickSizeConfigs)
            {
                totalWeight += config.weight;
            }

            if (totalWeight <= 0)
                return new(1, 1);

            // 根据权重随机选择
            float roll = (float)_random.NextDouble() * totalWeight;
            float cumulativeWeight = 0f;

            foreach (var config in _brickSizeConfigs)
            {
                cumulativeWeight += config.weight;
                if (roll <= cumulativeWeight)
                {
                    return config.size;
                }
            }

            return _brickSizeConfigs[0].size;
        }

        /// <summary>
        /// 获取顶部某行的空置格子
        /// </summary>
        protected void GetEmptyCellsInRow(int row, ref List<Vector2Int> emptyCells)
        {
            emptyCells.Clear();
            var brickMgr = brickManager;
            if (row < 0 || row >= _gridRows)
                return;

            for (int x = 0; x < _gridCols; x++)
            {
                var cell = new Vector2Int(x, row);
                if (brickMgr.IsCellEmpty(cell.ToPos()))
                {
                    emptyCells.Add(cell);
                }
            }
        }

        /// <summary>
        /// 获取指定列的空置格子
        /// </summary>
        protected void GetEmptyCellsInCol(int col, ref List<Vector2Int> emptyCells)
        {
            emptyCells.Clear();
            var brickMgr = brickManager;
            if (col < 0 || col >= _gridCols)
                return;

            for (int y = 0; y < _gridRows; y++)
            {
                var cell = new Vector2Int(col, y);
                if (brickMgr.IsCellEmpty(cell.ToPos()))
                {
                    emptyCells.Add(cell);
                }
            }
        }

        /// <summary>
        /// 获取所有空置格子
        /// </summary>
        protected List<Vector2Int> GetAllEmptyCells()
        {
            var emptyCells = new List<Vector2Int>();
            var brickMgr = brickManager;
            for (int y = 0; y < _gridRows; y++)
            {
                for (int x = 0; x < _gridCols; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (brickMgr.IsCellEmpty(cell.ToPos()))
                    {
                        emptyCells.Add(cell);
                    }
                }
            }

            return emptyCells;
        }

        /// <summary>
        /// 检查尺寸是否能在指定位置放置（考虑所有覆盖的格子）
        /// </summary>
        protected bool CanPlaceBrickSizeAt(Vector2Int topLeft, Vector2Int size)
        {
            var brickMgr = brickManager;
            // 检查右边界
            if (topLeft.x + size.x > _gridCols)
                return false;

            // 检查上边界
            if (topLeft.y + size.y > _gridRows)
                return false;

            // 检查所有被覆盖的格子
            for (int dy = 0; dy < size.y; dy++)
            {
                for (int dx = 0; dx < size.x; dx++)
                {
                    var cell = new Vector2Int(topLeft.x + dx, topLeft.y + dy);
                    if (brickMgr.IsCellEmpty(cell.ToPos())) 
                        continue;

                    return false;
                }
            }

            return true;
        }
    }
}