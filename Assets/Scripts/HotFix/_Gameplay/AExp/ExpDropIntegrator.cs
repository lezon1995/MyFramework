using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 经验值掉落集成器 - 将经验值掉落系统与怪物死亡事件集成
    /// 作为 FrameSystem 自动注册，监听 OnMonsterKilled_Wave 事件
    /// 怪物死亡 → 在怪物位置掉落经验值物品 → 玩家靠近时自动拾取
    /// </summary>
    public class ExpDropIntegrator : MonoBehaviour
        , IEvent<OnMonsterKilled_Wave>
    {
        public ExpManager expManager;

        #region Properties

        /// <summary>
        /// 全局经验值掉落表（怪物ID -> 掉落配置）
        /// </summary>
        public ExpDropTable DropTable { get; set; }

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
            DropTable = ExpDropTable.CreateDefault();

            // 监听怪物死亡事件
            this.addListener<OnMonsterKilled_Wave>();
        }

        public void OnDestroy()
        {
            this.removeListener<OnMonsterKilled_Wave>();
        }

        /// <summary>
        /// 处理怪物死亡事件 - 在怪物位置掉落经验值物品
        /// </summary>
        public void onEvent(OnMonsterKilled_Wave e)
        {
            if (!Enabled || expManager == null || e.Monster == null)
                return;

            var monsterTypeId = GetMonsterTypeId(e.Monster);
            var dropConfig = DropTable.GetConfig(monsterTypeId);
            dropConfig.CustomDropDirection = e.Monster.Health.LastDamageDirection;

            // 掉落位置 = 怪物死亡位置
            Vector2 monsterPos = e.Monster.transform.position;
            // 用于决定掉落方向（远离玩家）
            Vector2 playerPos = player != null ? player.transform.position : monsterPos;

            expManager.DropExpsByMonsterConfig(monsterPos, playerPos, dropConfig);
        }

        /// <summary>
        /// 直接掉落经验值（无需通过事件）
        /// </summary>
        public void DropExps(Vector2 position, Vector2 direction, int expCount, int expValue = 1, ExpDropConfig config = null)
        {
            if (!Enabled || expManager == null)
                return;

            expManager.DropExps(position, direction, expCount, expValue, config);
        }

        /// <summary>
        /// 根据怪物类型ID获取掉落配置
        /// </summary>
        string GetMonsterTypeId(AMonster monster)
        {
            if (monster == null)
                return "default";

            // 优先使用 EnemyType 判断
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
