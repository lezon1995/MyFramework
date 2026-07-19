using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 金币掉落集成器 - 将金币系统与怪物死亡事件集成
    /// 作为FrameSystem自动注册，监听 OnMonsterKilled_Wave 事件
    /// </summary>
    public class CoinDropIntegrator : MonoBehaviour
        , IEvent<OnMonsterKilled_Wave>
    {
        public CoinManager coinManager;

        #region Properties

        /// <summary>
        /// 全局金币掉落表（怪物ID -> 掉落配置）
        /// </summary>
        public CoinDropTable DropTable { get; set; }

        /// <summary>
        /// 是否启用掉落
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 默认掉落方向（相对玩家）
        /// </summary>
        public Vector2 FallbackDropDirection { get; set; } = Vector2.up;

        #endregion

        public void Awake()
        {
            DropTable = CoinDropTable.CreateDefault();

            // 监听怪物死亡事件
            this.addListener<OnMonsterKilled_Wave>();
        }

        public void OnDestroy()
        {
            this.removeListener<OnMonsterKilled_Wave>();
        }

        /// <summary>
        /// 处理怪物死亡事件
        /// </summary>
        public void onEvent(OnMonsterKilled_Wave e)
        {
            if (!Enabled || coinManager == null || e.Monster == null)
                return;

            var monsterTypeId = GetMonsterTypeId(e.Monster);
            var dropConfig = DropTable.GetConfig(monsterTypeId);

            // 计算掉落方向（基于怪物到玩家的方向）
            Vector2 monsterPos = e.Monster.transform.position;
            Vector2 playerPos = player != null ? player.transform.position : monsterPos;

            coinManager.DropCoinsByMonsterConfig(monsterPos, playerPos, dropConfig);
        }

        /// <summary>
        /// 直接掉落金币（无需通过事件）
        /// </summary>
        public void DropCoins(Vector2 position, Vector2 direction, int coinCount, int coinValue = 1, CoinDropConfig config = null)
        {
            if (!Enabled || coinManager == null)
                return;

            coinManager.DropCoins(position, direction, coinCount, coinValue, config);
        }

        /// <summary>
        /// 根据怪物类型ID获取掉落配置
        /// </summary>
        string GetMonsterTypeId(AMonster monster)
        {
            if (monster == null)
                return "default";

            // 优先使用EnemyType判断
            switch (monster.type)
            {
                case EnemyType.ELITE:
                    return "elite";
                case EnemyType.BOSS:
                    return "boss";
                case EnemyType.NORMAL:
                default:
                    return string.IsNullOrEmpty(monster.id) ? "normal" : monster.id;
            }
        }
    }
}