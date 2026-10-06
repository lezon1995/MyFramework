using QFSW.QC;
using UnityEngine;

namespace MoreMountains
{
    public static class RelicCommands
    {
        /// <summary>
        /// 调试用：免费获取指定类型的遗物（绕过金币校验，调用的是商店真正落地的方法）。
        /// 用法：relic-get &lt;RelicType&gt;
        /// 例如：relic-get AmmoSupply
        /// </summary>
        [Command("relic-get", "调试命令：免费购买一个指定 RelicType、走商店入包流程。")]
        public static void RelicGet(RelicType type)
        {
            var player = GBR.player;
            if (player == null)
            {
                Debug.LogError("[ball-get] 当前没有玩家，请先进入游戏。");
                return;
            }

            var def = relicManager.getDef(type);
            if (def == null)
            {
                Debug.LogError($"[ball-get] relicManager 里找不到 RelicType={type} 的定义。");
                return;
            }

            player.gainRelic(def);
        }
    }
}