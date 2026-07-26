/*using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 网格砖块移动的中央控制器。
    ///
    /// 职责：
    /// - 维护全局 APlayer 引用 (供 brick 寻路使用);
    /// - 在每帧 LateUpdate 阶段驱动所有 BrickGridMover (避免与 brick 自身 Update 顺序冲突);
    /// - 提供 static 单例访问便于任何 brick 找到 player.
    ///
    /// 用法：
    /// 1. 把本组件放在场景中任何一个 GameObject 上（推荐放在 GridManager 同一物体）；
    /// 2. 在 Inspector 拖入 APlayer;
    /// 3. BrickGridMover 会自动通过 BrickGridMoveController.Player 获取玩家。
    /// </summary>
    [DisallowMultipleComponent]
    public class BrickGridMoveController : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // 单例
        // ---------------------------------------------------------------

        public static BrickGridMoveController Instance { get; private set; }

        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------

        [Header("Player")]
        [Tooltip("玩家引用. 不指定时, 首次请求时通过 FindObjectOfType 自动获取.")]
        public APlayer Player;

        // ---------------------------------------------------------------
        // 生命周期
        // ---------------------------------------------------------------

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 解析 APlayer 引用. 优先用 Inspector 字段, 否则场景查找.
        /// </summary>
        public APlayer ResolvePlayer()
        {
            if (Player != null)
                return Player;

#if UNITY_2023_1_OR_NEWER
            Player = FindFirstObjectByType<APlayer>(FindObjectsInactive.Include);
#else
            Player = Object.FindObjectOfType<APlayer>(true);
#endif
            return Player;
        }

        /// <summary>外部注入玩家引用 (例如玩家是通过 spawn 出来的).</summary>
        public void SetPlayer(APlayer p) => Player = p;
    }
}*/