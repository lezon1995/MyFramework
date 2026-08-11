using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    /// <summary>
    /// 怪物经验值掉落配置 - 定义不同怪物类型/品质的经验值掉落规则
    /// </summary>
    [Serializable]
    public class MonsterExpDropConfig
    {
        public string monsterTypeId; // 怪物类型标识（"normal", "elite", "boss", 或者具体怪物ID）
        public int minExpCount = 1; // 最小掉落经验值物品数量
        public int maxExpCount = 3; // 最大掉落经验值物品数量
        public int expValue = 1; // 每枚经验值物品的数值
        [Range(0f, 1f)] public float dropChance = 1f; // 掉落概率（0-1）

        /// <summary>
        /// 掉落方向（参考 CoinDropConfig 的 DropDirectionType 复用）
        /// </summary>
        public DropDirectionType dropDirection = DropDirectionType.AwayFromPlayer;

        public ExpDropConfig dropConfigOverride; // 掉落动画配置（null则使用全局配置）
        public ExpPickupConfig pickupConfigOverride; // 拾取动画配置（null则使用全局配置）

        public Vector3 CustomDropDirection { get; set; }

        /// <summary>
        /// 计算本次击杀应该掉落的经验值物品数（基于概率）
        /// </summary>
        public int RollExpCount()
        {
            if (Random.value > dropChance)
                return 0;

            return Random.Range(minExpCount, maxExpCount + 1);
        }

        /// <summary>
        /// 获取掉落方向
        /// </summary>
        public Vector2 GetDropDirection(Vector2 monsterPos, Vector2 playerPos)
        {
            switch (dropDirection)
            {
                case DropDirectionType.AwayFromPlayer:
                    Vector2 away = monsterPos - playerPos;
                    return away.sqrMagnitude < 0.0001f ? Random.insideUnitCircle.normalized : away.normalized;

                case DropDirectionType.TowardPlayer:
                    Vector2 toward = playerPos - monsterPos;
                    return toward.sqrMagnitude < 0.0001f ? Random.insideUnitCircle.normalized : toward.normalized;

                case DropDirectionType.Random:
                    return Random.insideUnitCircle.normalized;
                case DropDirectionType.Custom:
                    return CustomDropDirection;
                default:
                    return Random.insideUnitCircle.normalized;
            }
        }
    }

    /// <summary>
    /// 经验值掉落表 - 管理所有怪物的经验值掉落配置
    /// </summary>
    [Serializable]
    public class ExpDropTable
    {
        /// <summary>
        /// 默认掉落配置（用于未配置的具体怪物）
        /// </summary>
        public MonsterExpDropConfig defaultConfig = new()
        {
            monsterTypeId = "default",
            minExpCount = 1,
            maxExpCount = 2,
            expValue = 1,
            dropChance = 1f,
            dropDirection = DropDirectionType.Custom
        };

        /// <summary>
        /// 各怪物类型的掉落配置
        /// </summary>
        public List<MonsterExpDropConfig> monsterConfigs = new();

        /// <summary>
        /// 获取指定怪物类型的掉落配置
        /// </summary>
        public MonsterExpDropConfig GetConfig(string monsterTypeId)
        {
            if (string.IsNullOrEmpty(monsterTypeId))
                return defaultConfig;

            foreach (var config in monsterConfigs)
            {
                if (config.monsterTypeId == monsterTypeId)
                    return config;
            }

            return defaultConfig;
        }

        /// <summary>
        /// 添加或更新配置
        /// </summary>
        public void AddConfig(MonsterExpDropConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.monsterTypeId))
                return;

            for (int i = 0; i < monsterConfigs.Count; i++)
            {
                if (monsterConfigs[i].monsterTypeId == config.monsterTypeId)
                {
                    monsterConfigs[i] = config;
                    return;
                }
            }

            monsterConfigs.Add(config);
        }

        /// <summary>
        /// 创建默认经验值掉落表
        /// </summary>
        public static ExpDropTable CreateDefault()
        {
            var table = new ExpDropTable();

            // 普通怪物
            table.AddConfig(new()
            {
                monsterTypeId = "normal",
                minExpCount = 1,
                maxExpCount = 2,
                expValue = 1,
                dropChance = 1f,
                dropDirection = DropDirectionType.AwayFromPlayer
            });

            // 精英怪
            table.AddConfig(new()
            {
                monsterTypeId = "elite",
                minExpCount = 3,
                maxExpCount = 5,
                expValue = 5,
                dropChance = 1f,
                dropDirection = DropDirectionType.AwayFromPlayer,
                dropConfigOverride = ExpDropConfig.FancyDrop
            });

            // Boss
            table.AddConfig(new()
            {
                monsterTypeId = "boss",
                minExpCount = 8,
                maxExpCount = 12,
                expValue = 10,
                dropChance = 1f,
                dropDirection = DropDirectionType.Random,
                dropConfigOverride = ExpDropConfig.LargeDrop
            });

            return table;
        }
    }

    /// <summary>
    /// 经验值掉落辅助器 - 提供便捷的掉落经验值静态方法
    /// </summary>
    public static class ExpDropHelper
    {
        /// <summary>
        /// 简单的经验值掉落（最常用）
        /// </summary>
        public static void DropExps(Vector2 position, Vector2 direction, int value, int count, ExpDropConfig config = null)
        {
            var manager = ExpManager.Instance;
            if (manager == null)
                return;

            config ??= manager.DropConfig;
            float spread = config.DirectionSpreadAngle;

            Vector2 baseDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;

            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                manager.DropExp(position, scatteredDir, value, config);
            }
        }

        /// <summary>
        /// 根据配置表掉落经验值
        /// </summary>
        public static void DropExpsByConfig(Vector2 monsterPos, Vector2 playerPos, ExpDropTable table, string monsterTypeId)
        {
            if (table == null)
                return;

            var config = table.GetConfig(monsterTypeId);
            int expCount = config.RollExpCount();
            if (expCount <= 0)
                return;

            Vector2 direction = config.GetDropDirection(monsterPos, playerPos);
            var manager = ExpManager.Instance;
            if (manager == null)
                return;

            float spread = (config.dropConfigOverride?.DirectionSpreadAngle) ?? 25f;
            var dropCfg = config.dropConfigOverride ?? manager.DropConfig;

            Vector2 baseDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;

            for (int i = 0; i < expCount; i++)
            {
                float angle = Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                manager.DropExp(monsterPos, scatteredDir, config.expValue, dropCfg);
            }
        }
    }

    /// <summary>
    /// 椭圆射线交点工具
    ///
    /// 模型说明：
    /// 椭圆中心 = DropPoint（掉落起点）。
    /// 椭圆默认与世界坐标对齐（X 轴水平，Y 轴垂直）：
    /// - X 轴半径 = HorizontalSpread
    /// - Y 轴半径 = DropHeight
    ///
    /// 给定一个方向 rayDir（任意），射线从椭圆中心出发沿 rayDir 方向与椭圆边界的交点距离为：
    ///     r = 1 / √((cos α / a)² + (sin α / b)²)
    /// 其中 α = atan2(rayDir.y, rayDir.x)，a = HorizontalSpread，b = DropHeight。
    ///
    /// 落点位置（相对 DropPoint 的偏移）= rayDir * r。
    /// 360 度均匀分布时，多枚经验值物品的落点集合自然形成椭圆（不再是正圆）。
    /// </summary>
    public static class ExpOrbEllipseScatter
    {
        /// <summary>
        /// 沿 rayDir 方向射线与椭圆的交点距离（从椭圆中心出发的距离 r）
        /// </summary>
        /// <param name="rayDir">掉落方向（任意方向，已归一化或将被归一化）</param>
        /// <param name="horizontalRadius">椭圆 X 轴半径 = HorizontalSpread</param>
        /// <param name="verticalRadius">椭圆 Y 轴半径 = DropHeight</param>
        /// <returns>交点距离（>0）</returns>
        public static float RayEllipseIntersectionDistance(Vector2 rayDir, float horizontalRadius, float verticalRadius)
        {
            if (rayDir.sqrMagnitude < 0.0001f)
                rayDir = Vector2.right;
            rayDir.Normalize();

            float a = Mathf.Max(0.01f, horizontalRadius);
            float b = Mathf.Max(0.01f, verticalRadius);

            // 椭圆方程 (X, Y 半径分别为 a, b)：
            //     (X / a)² + (Y / b)² = 1
            // 代入 X = r·rayDir.x, Y = r·rayDir.y：
            //     r² · ((rayDir.x / a)² + (rayDir.y / b)²) = 1
            //     r = 1 / √((rayDir.x / a)² + (rayDir.y / b)²)
            float termA = rayDir.x / a;
            float termB = rayDir.y / b;
            float denominator = Mathf.Sqrt(termA * termA + termB * termB);
            return 1f / Mathf.Max(0.0001f, denominator);
        }

        /// <summary>
        /// 沿 rayDir 方向射线与椭圆的交点位置（相对椭圆中心）
        /// </summary>
        public static Vector2 RayEllipseIntersection(Vector2 rayDir, float horizontalRadius, float verticalRadius)
        {
            float distance = RayEllipseIntersectionDistance(rayDir, horizontalRadius, verticalRadius);
            return rayDir * distance;
        }

        /// <summary>
        /// 椭圆周长采样（用于可视化）
        /// - X 轴半径 = horizontalRadius
        /// - Y 轴半径 = verticalRadius
        /// </summary>
        public static Vector2[] SampleEllipsePerimeter(Vector2 center, float horizontalRadius, float verticalRadius, int samples = 64)
        {
            float a = Mathf.Max(0.01f, horizontalRadius);
            float b = Mathf.Max(0.01f, verticalRadius);

            var points = new Vector2[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                float angle = t * Mathf.PI * 2f;
                points[i] = center + new Vector2(a * Mathf.Cos(angle), b * Mathf.Sin(angle));
            }

            return points;
        }
    }
}
