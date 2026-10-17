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

            var p = e.Player;
            var monsterTypeId = GetMonsterTypeId(e.Monster);
            var dropConfig = DropTable.GetConfig(monsterTypeId);
            dropConfig.CustomDropDirection = e.Monster.Health.LastDamageDirection;

            // 计算掉落方向（基于怪物到玩家的方向）
            Vector2 monsterPos = e.Monster.transform.position;
            Vector2 playerPos = p ? p.transform.position : monsterPos;

            coinManager.DropCoinsByMonsterConfig(monsterPos, playerPos, dropConfig, p);
        }

        /// <summary>
        /// 直接掉落金币（无需通过事件）
        /// </summary>
        public void DropCoins(Vector2 position, Vector2 direction, int coinCount, int coinValue = 1, CoinDropConfig config = null, APlayer p = null)
        {
            if (!Enabled || coinManager == null)
                return;

            coinManager.DropCoins(position, direction, coinCount, coinValue, config, p);
        }

        /// <summary>
        /// 根据怪物类型ID获取掉落配置
        /// </summary>
        string GetMonsterTypeId(AMonster monster)
        {
            if (monster == null)
                return "default";

            // 优先使用EnemyType判断
            return monster.type switch
            {
                EnemyType.ELITE => "elite",
                EnemyType.BOSS => "boss",
                EnemyType.NORMAL => string.IsNullOrEmpty(monster.id) ? "normal" : monster.id,
                _ => string.IsNullOrEmpty(monster.id) ? "normal" : monster.id
            };
        }
    }
}