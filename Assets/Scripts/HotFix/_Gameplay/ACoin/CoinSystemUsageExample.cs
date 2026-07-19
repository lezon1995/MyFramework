using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 金币系统使用示例 - 展示如何在不同场景下使用金币掉落拾取系统
    /// </summary>
    public class CoinSystemUsageExample : MonoBehaviour
    {
        public CoinManager coinManager;
        public CoinDropIntegrator coinDropIntegrator;
        
        #region 示例 1: 基础使用 - 在指定位置掉落金币
        /// <summary>
        /// 示例1: 怪物死亡时在它的位置掉落1个金币
        /// </summary>
        public void Example1_DropCoinAtMonsterDeath(Transform monsterTransform)
        {
            if (coinManager == null)
                return;

            // 掉落1枚价值1的金币，方向为远离玩家（朝上）
            Vector2 monsterPos = monsterTransform.position;
            Vector2 dropDir = player != null
                ? (monsterPos - (Vector2)player.transform.position).normalized
                : Vector2.up;

            coinManager.DropCoin(monsterPos, dropDir);
        }
        #endregion

        #region 示例 2: 批量掉落多个金币（带散射）
        /// <summary>
        /// 示例2: 怪物死亡时掉落多个金币（散射效果）
        /// </summary>
        public void Example2_DropMultipleCoins(Transform monsterTransform)
        {
            if (coinManager == null)
                return;

            Vector2 monsterPos = monsterTransform.position;
            Vector2 dropDir = player != null
                ? (monsterPos - (Vector2)player.transform.position).normalized
                : Vector2.up;

            // 掉落5枚金币，每枚价值1，自动散射
            coinManager.DropCoins(monsterPos, dropDir, 5);
        }
        #endregion

        #region 示例 3: 使用自定义掉落配置
        /// <summary>
        /// 示例3: 使用自定义掉落配置（更华丽）
        /// </summary>
        public void Example3_DropWithCustomConfig(Transform bossTransform)
        {
            if (coinManager == null)
                return;

            // 自定义掉落配置：3段椭圆弧（1次初始+2次反弹），长时长
            // 椭圆模型：长半轴 a=HorizontalSpread 决定落点水平距离，短半轴 b=DropHeight 决定高度
            var fancyConfig = new CoinDropConfig
            {
                DropDuration = 1.0f,             // 总动画时长1秒
                BounceCount = 2,                 // 2次反弹（共3段抛物线）
                BounceDecayRatio = 0.5f,         // 反弹力度衰减50%
                HorizontalSpread = 2.5f,         // 椭圆长半轴 = 落点水平距离（米）
                DropHeight = 2.0f,               // 椭圆短半轴 = 抛物线高度（米）
                DirectionSpreadAngle = 45f       // 多枚金币散射角度范围
            };

            // Boss掉落15枚金币
            coinManager.DropCoins(bossTransform.position, Vector2.up, 15, 10, fancyConfig);
        }
        #endregion

        #region 示例 4: 一次性掉落指定总价值的金币
        /// <summary>
        /// 示例4: 一次性掉落指定总价值的金币（自动拆分成多个1价值的金币）
        /// </summary>
        public void Example4_DropTotalValue(Transform monsterTransform, int totalGoldValue)
        {
            if (coinManager == null)
                return;

            // 掉落总价值100的金币，分成10枚
            coinManager.DropCoinBurst(monsterTransform.position, Vector2.up, totalGoldValue, 10);
        }
        #endregion

        #region 示例 5: 修改全局掉落/拾取配置
        /// <summary>
        /// 示例5: 在游戏开始时修改全局配置
        /// </summary>
        public void Example5_ConfigureGlobalSettings()
        {
            if (coinManager == null)
                return;

            // 修改全局掉落配置（椭圆模型）
            coinManager.DropConfig = new CoinDropConfig
            {
                DropDuration = 0.6f,
                BounceCount = 2,
                BounceDecayRatio = 0.6f,
                HorizontalSpread = 1.5f,         // 椭圆长半轴
                DropHeight = 1.2f,               // 椭圆短半轴
                DirectionSpreadAngle = 30f       // 散射角度
            };

            // 修改全局拾取配置
            coinManager.PickupConfig = new CoinPickupConfig
            {
                PickupDuration = 0.3f,
                RotationDegrees = 720f,
                MinScale = 0.3f
            };

            // 设置拾取范围
            coinManager.PickupRange = 5f;
        }
        #endregion

        #region 示例 6: 监听金币事件
        /// <summary>
        /// 示例6: 监听金币事件以触发特效或音效
        /// </summary>
        public void Example6_ListenToCoinEvents()
        {
            if (coinManager == null)
                return;

            // 金币生成时
            coinManager.OnCoinSpawned += coin =>
            {
                Debug.Log($"[CoinSystem] Coin spawned at {coin.Position}, value={coin.Value}");
            };

            // 金币落地时（掉落动画结束）
            coinManager.OnCoinLanded += coin =>
            {
                Debug.Log($"[CoinSystem] Coin landed at {coin.Position}, value={coin.Value}");
                // 播放落地音效
            };

            // 金币拾取动画完成时（金币真正到账）
            coinManager.OnCoinCollected += coin =>
            {
                Debug.Log($"[CoinSystem] Coin collected at {coin.Position}, value={coin.Value}");
                // 播放拾取音效、显示金币+1动画等
            };

            // 总金币变化
            coinManager.OnGoldCollected += amount =>
            {
                Debug.Log($"[CoinSystem] Total gold received: {amount}");
            };
        }
        #endregion

        #region 示例 7: 手动触发拾取
        /// <summary>
        /// 示例7: 玩家手动触发拾取（不自动）
        /// </summary>
        public void Example7_ManualPickup()
        {
            if (coinManager == null || player == null)
                return;

            // 关闭自动拾取
            coinManager.AutoPickupEnabled = false;

            // 在特定时机（例如玩家按键）手动触发拾取
            coinManager.TryPickupCoinsInRange(player.transform);
        }
        #endregion

        #region 示例 8: 自定义不同怪物的掉落
        /// <summary>
        /// 示例8: 自定义不同怪物的掉落配置
        /// </summary>
        public void Example8_CustomMonsterDrop()
        {
            if (coinManager == null)
                return;

            // 创建掉落表
            var dropTable = CoinDropTable.CreateDefault();

            // 自定义特定怪物的掉落
            dropTable.AddConfig(new MonsterCoinDropConfig
            {
                monsterTypeId = "goblin_king",  // 怪物ID
                minCoinCount = 5,
                maxCoinCount = 8,
                coinValue = 5,
                dropChance = 1f,
                dropDirection = DropDirectionType.Random,
                dropConfigOverride = CoinDropConfig.FancyDrop  // 使用华丽掉落配置
            });

            // 应用到CoinDropIntegrator
            if (coinDropIntegrator != null)
            {
                coinDropIntegrator.DropTable = dropTable;
            }
        }
        #endregion

        #region 示例 9: 直接使用CoinDropIntegrator手动掉落
        /// <summary>
        /// 示例9: 直接通过CoinDropIntegrator手动掉落金币（不通过事件）
        /// </summary>
        public void Example9_DirectDrop()
        {
            if (coinDropIntegrator == null)
                return;

            // 在指定位置掉落金币
            coinDropIntegrator.DropCoins(transform.position, Vector2.up, 3);
        }
        #endregion

        #region 示例 10: 完整集成 - 自定义金币拾取范围
        /// <summary>
        /// 示例10: 通过CoinPickerAdapter自定义拾取范围
        /// </summary>
        public void Example10_CustomPickupRange(CoinPickerAdapter adapter)
        {
            if (adapter == null)
                return;

            // 设置拾取范围为10
            adapter.SetPickupRange(10f);

            // 监听金币拾取事件
            adapter.OnGoldCollectedEvent += amount =>
            {
                Debug.Log($"[Player] Received {amount} gold! Total: {adapter.TotalGoldCollected}");
            };
        }
        #endregion

        #region 示例 11: 完整流程 - 在Monster处触发金币掉落
        /// <summary>
        /// 示例11: 完整的金币掉落流程
        /// </summary>
        public void Example11_FullWorkflow(AMonster monster)
        {
            if (monster == null || coinManager == null)
                return;

            // 1. 确定掉落位置（怪物中心）
            Vector2 dropPos = monster.transform.position;

            // 2. 计算掉落方向（远离玩家）
            Vector2 toPlayer = player != null
                ? (Vector2)player.transform.position - dropPos
                : Vector2.down;
            Vector2 dropDir = toPlayer.sqrMagnitude > 0.01f
                ? -toPlayer.normalized  // 反向：远离玩家
                : Vector2.up;

            // 3. 决定掉落金币数量和价值
            int coinCount;
            int coinValue;
            CoinDropConfig config;

            switch (monster.type)
            {
                case EnemyType.BOSS:
                    coinCount = 15;
                    coinValue = 10;
                    config = CoinDropConfig.FancyDrop;
                    break;
                case EnemyType.ELITE:
                    coinCount = 5;
                    coinValue = 5;
                    config = CoinDropConfig.Default;
                    break;
                default:
                    coinCount = 1;
                    coinValue = 1;
                    config = CoinDropConfig.QuickDrop;
                    break;
            }

            // 4. 生成金币
            coinManager.DropCoins(dropPos, dropDir, coinCount, coinValue, config);

            // 5. 拾取过程由CoinPickerAdapter自动处理
            //    - 玩家走进拾取范围时自动启动拾取动画
            //    - 拾取动画结束后调用 APlayer.gainGold()
        }
        #endregion
    }
}
