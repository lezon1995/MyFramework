using QFSW.QC;

namespace MoreMountains
{
    public static class ShieldCommands
    {
        [Command("shield-add", "调试命令：添加护盾")]
        public static void ShieldAdd(int value)
        {
            var player = GBR.player;

            if (value > 0)
            {
                player.Health.Shield.AddShield(value);
            }
            else if (value < 0)
            {
                player.Health.Shield.RemoveShield(value);
            }
        }
    }
}