using System;
using System.Text;

namespace MoreMountains
{
    /// <summary>
    /// RarityRollService 的使用示例 / 自校验：
    /// - PrintOne() 打印单次计算结果，方便挂到 Debug UI 或菜单。
    /// - ValidateAgainstWikiTable() 把我们的输出与 Wiki 的 0/50/100 Luck 表格对比并打印。
    /// </summary>
    public static class RarityRollServiceExample
    {
        static readonly string[] RarityNames = new[] { "Tier1", "Tier2", "Tier3", "Tier4" };

        public static string SummarizeChances(int wave, float luck)
        {
            var svc = new RarityRollService();
            using var _ = new DicScope<ItemRarity, float>(out var chances);
            svc.ComputeChances(wave, luck, ref chances);
            var sb = new StringBuilder();
            sb
                .Append("Wave ").Append(wave)
                .Append("  Luck ").Append(luck.ToString("0.##"))
                .Append("  →  ");
            for (int i = 0; i < 4; i++)
            {
                var r = (ItemRarity)(3 - i); // Tier4..Tier1
                float p = chances.TryGetValue(r, out var v) ? v : 0F;
                if (i > 0) 
                    sb.Append("  ");

                sb.Append(RarityNames[3 - i]).Append(' ').Append((p * 100F).ToString("0.0")).Append('%');
            }

            return sb.ToString();
        }

        public static string RollOnce(int wave, float luck)
        {
            var svc = new RarityRollService();
            var picked = svc.RollItem(wave, luck);
            return "Wave " + wave + " / Luck " + luck + "  →  picked " + picked;
        }

        /// <summary>
        /// 与 Wiki 0/50/100 Luck 表格的对照：
        ///   0   Luck → Wave 1..11 的 T1/T2/T3/T4 应该是 (100/0/0/0), (94/6/0/0), (88/12/0/0), (82/16/2/0),
        ///                 (76/20/4/0), (70/24/6/0), (64/28/8/0), (58/32/9.8/0.2), (52/36/11.5/0.5),
        ///                 (46/40/13.3/0.7), (40/44/15.1/0.9)
        ///   50  Luck → (100/0/0/0), (91/9/0/0), (82/18/0/0), (73/24/3/0), (64/30/6/0), (55/36/9/0),
        ///                 (46/42/12/0), (40/45/14.7/0.3), (40/42/17.3/0.7), (40/39/20/1), (40/36/22.6/1.4)
        ///   100 Luck → (100/0/0/0), (88/12/0/0), (76/24/0/0), (64/32/4/0), (52/40/8/0), (40/48/12/0),
        ///                 (40/44/16/0), (40/40/19.5/0.5), (40/36/23.1/0.9), (40/35/23.6/1.4), (40/35/23.2/1.8)
        /// 误差精度默认 0.15%（Wiki 表格本身只到 0.1%，加上浮点 + 累减带来的舍入）。
        /// </summary>
        public static string ValidateAgainstWikiTable(float tolerancePct = 0.15F)
        {
            // 数据按 Wiki 0/50/100-Luck 表格 1~11 波的 T1/T2/T3/T4 真实概率（百分比）。
            var expected0 = new (int wave, float t1, float t2, float t3, float t4)[]
            {
                (01, 100F, 00F, 0.0F, 0.0F),
                (02, 94F, 06F, 0.0F, 0.0F),
                (03, 88F, 12F, 0.0F, 0.0F),
                (04, 82F, 16F, 2.0F, 0.0F),
                (05, 76F, 20F, 4.0F, 0.0F),
                (06, 70F, 24F, 6.0F, 0.0F),
                (07, 64F, 28F, 8.0F, 0.0F),
                (08, 58F, 32F, 9.8F, 0.2F),
                (09, 52F, 36F, 11.5F, 0.5F),
                (10, 46F, 40F, 13.3F, 0.7F),
                (11, 40F, 44F, 15.1F, 0.9F),
            };
            var expected50 = new (int wave, float t1, float t2, float t3, float t4)[]
            {
                (01, 100F, 00F, 0F, 0F),
                (02, 91F, 09F, 0F, 0F),
                (03, 82F, 18F, 0F, 0F),
                (04, 73F, 24F, 3F, 0F),
                (05, 64F, 30F, 6F, 0F),
                (06, 55F, 36F, 9F, 0F),
                (07, 46F, 42F, 12F, 0F),
                (08, 40F, 45F, 14.7F, 0.3F),
                (09, 40F, 42F, 17.3F, 0.7F),
                (10, 40F, 39F, 20F, 1F),
                (11, 40F, 36F, 22.6F, 1.4F),
            };
            var expected100 = new (int wave, float t1, float t2, float t3, float t4)[]
            {
                (01, 100F, 0F, 0F, 0F),
                (02, 88F, 12F, 0F, 0F),
                (03, 76F, 24F, 0F, 0F),
                (04, 64F, 32F, 4F, 0F),
                (05, 52F, 40F, 8F, 0F),
                (06, 40F, 48F, 12F, 0F),
                (07, 40F, 44F, 16F, 0F),
                (08, 40F, 40F, 19.5F, 0.5F),
                (09, 40F, 36F, 23.1F, 0.9F),
                (10, 40F, 35F, 23.6F, 1.4F),
                (11, 40F, 35F, 23.2F, 1.8F),
            };

            var svc = new RarityRollService();
            var sb = new StringBuilder();
            int maxMis = 0;
            int totalRows = 0;

            sb.AppendLine("=== RarityRollService vs Wiki ===");
            maxMis = Math.Max(maxMis, AppendCheck(sb, svc, expected0, luck: 0F, label: "0 Luck"));
            maxMis = Math.Max(maxMis, AppendCheck(sb, svc, expected50, luck: 0.5F, label: "50 Luck"));
            maxMis = Math.Max(maxMis, AppendCheck(sb, svc, expected100, luck: 1F, label: "100 Luck"));

            sb.AppendLine(maxMis == 0
                ? "All waves within tolerance " + tolerancePct + "%."
                : "Max mismatch count per wave (across all tiers): " + maxMis);
            return sb.ToString();
        }

        static int AppendCheck(StringBuilder sb, RarityRollService svc
            , (int wave, float t1, float t2, float t3, float t4)[] expected
            , float luck, string label)
        {
            sb.AppendLine();
            sb.AppendLine("--- " + label + " ---");
            int maxMis = 0;
            foreach (var row in expected)
            {
                using var _ = new DicScope<ItemRarity, float>(out var chances);
                svc.ComputeChances(row.wave, luck, ref chances);
                float t1 = chances[ItemRarity.Tier1] * 100F;
                float t2 = chances[ItemRarity.Tier2] * 100F;
                float t3 = chances[ItemRarity.Tier3] * 100F;
                float t4 = chances[ItemRarity.Tier4] * 100F;

                int mis = 0;
                if (Math.Abs(t1 - row.t1) > 0.15F) mis++;
                if (Math.Abs(t2 - row.t2) > 0.15F) mis++;
                if (Math.Abs(t3 - row.t3) > 0.15F) mis++;
                if (Math.Abs(t4 - row.t4) > 0.15F) mis++;
                if (mis > maxMis) 
                    maxMis = mis;

                sb.Append("Wave ").Append(row.wave).Append("  ")
                    .Append("T1=").Append(t1.ToString("0.00")).Append(" (exp ").Append(row.t1.ToString("0.0")).Append(")  ")
                    .Append("T2=").Append(t2.ToString("0.00")).Append(" (exp ").Append(row.t2.ToString("0.0")).Append(")  ")
                    .Append("T3=").Append(t3.ToString("0.00")).Append(" (exp ").Append(row.t3.ToString("0.0")).Append(")  ")
                    .Append("T4=").Append(t4.ToString("0.00")).Append(" (exp ").Append(row.t4.ToString("0.0")).Append(")  ")
                    .Append(mis == 0 ? "OK" : ("DIFF " + mis))
                    .AppendLine();
            }

            return maxMis;
        }
    }
}