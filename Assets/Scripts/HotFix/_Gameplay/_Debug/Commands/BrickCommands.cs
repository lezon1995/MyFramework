using QFSW.QC;
using UnityEngine;

namespace MoreMountains
{
    public static class BrickCommands
    {
        /// <summary>
        /// 调试用：免费获取指定类型与等级的球（绕过金币校验，调用的是商店真正落地的方法）。
        /// 用法：ball-get &lt;BallType&gt; &lt;level&gt;
        /// 例如：ball-get Normal 1
        /// </summary>
        [Command("brick-gen", "调试命令：生成一个砖块")]
        public static void BrickGen(int w, int h, int health)
        {
            var def = brickManager.GetRandomDef(new(w, h));
            var brick = brickManager.acquireBrick(def, Vector2.zero);
            brick.currentHealth = health;
            brick.maxHealth = health;

            brick.Controller2D.enabled = false;
        }
    }
}