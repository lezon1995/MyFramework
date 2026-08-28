using QFSW.QC;
using UnityEngine;

namespace MoreMountains
{
    public static class BallCommands
    {
        /// <summary>
        /// 调试用：免费获取指定类型与等级的球（绕过金币校验，调用的是商店真正落地的方法）。
        /// 用法：ball-get &lt;BallType&gt; &lt;level&gt;
        /// 例如：ball-get Normal 1
        /// </summary>
        [Command("ball-get", "调试命令：免费购买一个指定 BallType、等级的球，走商店入包流程。")]
        public static void BallGet(BallType type, int level = 1)
        {
            var player = GBR.player;
            if (player == null)
            {
                Debug.LogError("[ball-get] 当前没有玩家，请先进入游戏。");
                return;
            }

            var def = ballManager.getDef(type);
            if (def == null)
            {
                Debug.LogError($"[ball-get] BallDefLibrary 里找不到 BallType={type} 的定义。");
                return;
            }

            var item = BallItem.New(def, level);
            if (!player.BallManagement.Shop.PurchaseAndStore(item))
            {
                Debug.LogWarning($"[ball-get] 入包失败（背包可能已满）：{type} Lv.{level}");
            }
        }
    }
}
