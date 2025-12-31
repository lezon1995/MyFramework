using System;
using System.Collections.Generic;

namespace MarbleHero
{
    [Serializable]
    public class MonsterInfo : IComparable<MonsterInfo>
    {
        public string name;
        public float weight;

        public MonsterInfo(string name, float weight)
        {
            this.name = name;
            this.weight = weight;
        }

        public static void normalizeWeights(List<MonsterInfo> list)
        {
            list.Sort();
            float total = 0.0F;
            foreach (var info in list)
                total += info.weight;
            
            foreach (var info in list)
            {
                info.weight /= total;
                if (Settings.isInfo)
                    log(info.name + ": " + info.weight + "%");
            }
        }

        public static string roll(List<MonsterInfo> list, float roll)
        {
            float currentWeight = 0.0F;
            foreach (var info in list)
            {
                currentWeight += info.weight;
                if (roll < currentWeight)
                    return info.name;
            }

            return "ERROR";
        }

        public int CompareTo(MonsterInfo other) => weight.CompareTo(other.weight);
    }
}