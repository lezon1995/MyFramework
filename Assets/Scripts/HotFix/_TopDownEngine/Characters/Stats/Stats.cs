using System.Collections.Generic;
using MoreMountains.Tools;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    [AddComponentMenu("TopDown Engine/Character/Core/Stats")]
    public class Stats : MonoBehaviour
    {
        public const string HealthMax = "HealthMax";
        public const string HealthRegen = "HealthRegen";
        public const string ManaMax = "ManaMax";
        public const string ManaRegen = "ManaRegen";
        public const string AD = "AD";
        public const string AR = "AR";
        public const string AD_PT = "AD_PT";
        public const string AD_PT_Rate = "AD_PT_Rate";
        public const string AP = "AP";
        public const string MR = "MR";
        public const string AP_PT = "AP_PT";
        public const string AP_PT_Rate = "AP_PT_Rate";
        public const string AS = "AS";
        public const string CD = "CD";
        public const string MS = "MS";
        public const string CritChance = "CritChance";
        public const string CritDamage = "CritDamage";
        public const string DmgRate = "DmgRate";
        public const string AF = "AF";
        public const string LifeSteal = "LifeSteal";
        public const string Range = "Range";
        public const string DodgeChance = "DodgeChance";
        public const string KnockbackResistance = "KnockbackResistance";
        public const string BallisticSpeed = "BallisticSpeed";
        public const string HitEffectChance = "HitEffectChance";
        public const string Knockback = "Knockback";
        public const string Duration = "Duration";
        public const string Luck = "Luck";
        public const string Greed = "Greed";
        public const string AF_Mod = "AdaptiveForceMod";

        public bool AutoInitialize = true;

        [SerializeField]
        StatsTemplate StatsConfig;

        IStatsTemplate _statsTemplate;

        //自定义数值
        Dictionary<string, UniStats.Stat> _stats = new();

        const float AF_CoeffAD = 0.6F;
        const float AF_CoeffAP = 1.0F;
        public UniStats.Stat StatAD;
        public UniStats.Stat StatAP;
        public UniStats.Stat StatAF;
        MMObservable<bool> IsBonusAdOverAp;

        void Awake()
        {
            if (AutoInitialize)
            {
                InitializeStats(StatsConfig);
            }
        }

        public void InitializeStats(IStatsTemplate template)
        {
            if (template is StatsTemplate statsTemplate)
            {
                StatsConfig = statsTemplate;
            }

            _statsTemplate = template;

            if (_statsTemplate == null)
                return;

            _stats.Clear();
            StatAD = null;
            StatAP = null;
            StatAF = null;

            if (_statsTemplate.useExpression)
            {
                foreach (var (statName, initialGetter) in _statsTemplate.configExpressions)
                {
                    float ratio = 1F;
                    if (_statsTemplate.ratios.TryGetValue(statName, out float value))
                    {
                        ratio = value;
                    }

                    if (_stats.TryGetValue(statName, out var stat))
                    {
                        stat.Initial = initialGetter();
                        stat.InitialGetter = initialGetter;
                        stat.BonusRatio.Initial = ratio;
                        continue;
                    }

                    stat = new(initialGetter, ratio);
                    _stats[statName] = stat;
                    stat.Event.Add(Action);

                    void Action(float pre, float cur)
                    {
                        // UnityEngine.Debug.Log($"{statName} {pre:F2} -> {cur:F2}");
                    }

                    switch (statName)
                    {
                        case AD:
                            StatAD = stat;
                            break;
                        case AP:
                            StatAP = stat;
                            break;
                        case AF:
                            StatAF = stat;
                            break;
                    }
                }
            }
            else
            {
                foreach (var (statName, initial) in _statsTemplate.configs)
                {
                    float ratio = 1F;
                    if (_statsTemplate.ratios.TryGetValue(statName, out float value))
                    {
                        ratio = value;
                    }

                    if (_stats.TryGetValue(statName, out var stat))
                    {
                        stat.Initial = initial;
                        stat.InitialGetter = null;
                        stat.BonusRatio.Initial = ratio;
                        continue;
                    }

                    stat = new(initial, ratio);
                    _stats[statName] = stat;
                    stat.Name = statName;
                    stat.Event.Add(Action);

                    void Action(float pre, float cur)
                    {
                        // UnityEngine.Debug.Log($"{statName} {pre:F2} -> {cur:F2}");
                    }

                    switch (statName)
                    {
                        case AD:
                            StatAD = stat;
                            break;
                        case AP:
                            StatAP = stat;
                            break;
                        case AF:
                            StatAF = stat;
                            break;
                    }
                }
            }

            IsBonusAdOverAp.OnValueChangedTo = CheckIsBonusAdOverAp;

            if (StatAF)
            {
                if (StatAD)
                    StatAD.AddFlat(StatAF.Select(f => f * AF_CoeffAD), AF_Mod);
                if (StatAP)
                    StatAP.AddFlat(StatAF.Select(f => f * AF_CoeffAP), AF_Mod);
            }

            void Check(IVar<float> stat)
            {
                IsBonusAdOverAp.Value = GetIsBonusAdOverAp();
            }

            bool GetIsBonusAdOverAp()
            {
                if (StatAD && StatAP)
                    return StatAD.PeekBonus(AF_Mod) >= StatAP.Peek(AF_Mod);

                return false;
            }

            if (StatAD)
                StatAD.OnChange(Check);
            if (StatAP)
                StatAP.OnChange(Check);

            if (StatAF)
            {
                StatAF.OnChange(var =>
                {
                    StatAD.SetDirty();
                    StatAP.SetDirty();
                });
            }

            CheckIsBonusAdOverAp(GetIsBonusAdOverAp());
        }

        void CheckIsBonusAdOverAp(bool b)
        {
            if (b)
            {
                if (StatAD)
                    StatAD.SetModActive(AF_Mod, true);
                if (StatAP)
                    StatAP.SetModActive(AF_Mod, false);
            }
            else
            {
                if (StatAD)
                    StatAD.SetModActive(AF_Mod, false);
                if (StatAP)
                    StatAP.SetModActive(AF_Mod, true);
            }
        }

        public UniStats.Stat GetStat(string key)
        {
            if (_stats.TryGetValue(key, out var stat))
                return stat;
            return null;
        }

        public bool GetStat(string key, out UniStats.Stat stat)
        {
            return _stats.TryGetValue(key, out stat);
        }

        public bool TryGetStat(string key, out UniStats.Stat stat)
        {
            return _stats.TryGetValue(key, out stat);
        }

        public void ClearStats()
        {
            foreach (var (key, stat) in _stats)
            {
                stat.ClearMods();
            }
        }
    }
}