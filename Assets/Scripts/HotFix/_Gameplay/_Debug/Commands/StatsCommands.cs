using QFSW.QC;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    public static class StatsCommands
    {
        [Command("stat-add", "调试命令：新增玩家属性的Flat修改器")]
        public static void AddStatFlat(Character.Stat key, float value, string name)
        {
            var player = GBR.player;
            if (player == null)
            {
                Debug.LogError("[ball-get] 当前没有玩家，请先进入游戏。");
                return;
            }

            if (player.GetStat(key, out var stat))
            {
                stat.BonusFlat.AddFlat(value, name);
            }
        }

        [Command("stat-add-pct", "调试命令：新增玩家属性的Pct修改器")]
        public static void AddStatPct(Character.Stat key, float value, string name)
        {
            var player = GBR.player;
            if (player == null)
            {
                Debug.LogError("[ball-get] 当前没有玩家，请先进入游戏。");
                return;
            }

            if (player.GetStat(key, out var stat))
            {
                stat.BonusPct.AddFlat(value, name);
            }
        }

        [Command("stat-remove", "调试命令：移除玩家属性的Flat修改器")]
        public static void RemoveStatFlat(Character.Stat key, string name)
        {
            var player = GBR.player;
            if (player == null)
            {
                Debug.LogError("[ball-get] 当前没有玩家，请先进入游戏。");
                return;
            }

            if (player.GetStat(key, out var stat))
            {
                stat.BonusFlat.RemoveMod(name);
            }
        }

        [Command("stat-remove-pct", "调试命令：移除玩家属性的Pct修改器")]
        public static void RemoveStatPct(Character.Stat key, string name)
        {
            var player = GBR.player;
            if (player == null)
            {
                Debug.LogError("[ball-get] 当前没有玩家，请先进入游戏。");
                return;
            }

            if (player.GetStat(key, out var stat))
            {
                stat.BonusPct.RemoveMod(name);
            }
        }


        [Command("stat-show", "调试命令：显示玩家属性的所有修改器")]
        public static string ShowStat(Character.Stat key)
        {
            var player = GBR.player;
            if (player == null)
            {
                Debug.LogError("[ball-get] 当前没有玩家，请先进入游戏。");
                return null;
            }

            if (player.GetStat(key, out var stat))
            {
                using var _ = new MyStringBuilderScope(out var sb);

                sb.addLine("Flat Mods:");
                for (var i = 0; i < stat.BonusFlat.Mods.Count; i++)
                {
                    var mod = stat.BonusFlat.Mods[i];
                    if (mod is NumMod<float> numMod)
                    {
                        sb.add($"{i} : ");
                        if (numMod.Value > 0)
                        {
                            sb.add($"+");
                        }

                        sb.add($"{numMod.Value}");
                        sb.add(" : ");
                        sb.add($"\"{numMod.Name}\"");
                    }
                }

                sb.addLine();
                sb.addLine("Pct Mods:");
                for (var i = 0; i < stat.BonusPct.Mods.Count; i++)
                {
                    var mod = stat.BonusPct.Mods[i];
                    if (mod is NumMod<float> numMod)
                    {
                        sb.add($"{i} : ");
                        if (numMod.Value > 0)
                        {
                            sb.add($"+");
                        }

                        sb.add($"{numMod.Value.toPercent(0)}");
                        sb.add(" : ");
                        sb.add($"\"{numMod.Name}\"");
                    }
                }

                return sb.ToString();
            }

            return null;
        }
    }
}