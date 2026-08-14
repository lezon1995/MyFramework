using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    public delegate void ValueModifier(ref float raw);

    public static class ValueModifierExtensions
    {
        public static float SafeInvoke(this ValueModifier modifier, ref float value)
        {
            if (modifier == null)
                return value;

            modifier(ref value);
            return value;
        }
    }

    public interface IStatsTemplate
    {
        bool useExpression { get; }
        Dictionary<string, float> configs { get; }
        Dictionary<string, Func<float>> configExpressions { get; }
        Dictionary<string, float> ratios { get; }
        bool TryGetDisplayConfig(string statName, out DisplayConfig c);
    }

    public abstract class StatsTemplate : SerializedScriptableObject, IStatsTemplate
    {
        public StatsDisplayConfig displayConfig;
        public bool useExpression => false;
        public Dictionary<string, float> configs => Configs;
        public Dictionary<string, Func<float>> configExpressions => null;
        public Dictionary<string, float> ratios => Ratios;

        [DictionaryDrawerSettings(KeyLabel = "Stat", ValueLabel = "Initial")]
        public Dictionary<string, float> Configs = new();

        [DictionaryDrawerSettings(KeyLabel = "Stat", ValueLabel = "Ratio")]
        public Dictionary<string, float> Ratios = new();

        public List<DisplayConfig> DisplayConfigs = new();

        List<string> names = new();

        void Awake()
        {
            FillConfigs();
        }

        void OnEnable()
        {
            FillConfigs();
        }

        void OnValidate()
        {
            FillConfigs();
        }

        void FillConfigs()
        {
            var enumerable = GetNames();
            using var enumerator = enumerable.GetEnumerator();
            names.Clear();
            while (enumerator.MoveNext())
            {
                var statName = enumerator.Current;
                names.Add(statName);
                Configs.TryAdd(statName, 0F);
            }

            var keys = Configs.Keys.ToArray();
            foreach (var key in keys)
            {
                if (!names.Contains(key))
                {
                    Configs.Remove(key);
                }
            }
        }

        protected abstract IEnumerable<string> GetNames();

        public bool TryGetDisplayConfig(string statName, out DisplayConfig c)
        {
            if (displayConfig == null)
            {
                c = DisplayConfig.Default;
                return false;
            }

            return displayConfig.TryGetDisplayConfig(statName, out c);
        }
    }

    [Serializable]
    public class DisplayConfig
    {
        public static DisplayConfig Default = new()
        {
            displayType = DisplayType.Flat,
            displayDecimalDigits = 0,
        };

        public enum DisplayType
        {
            Flat,
            Pct,
        }

        public string statName;
        public DisplayType displayType;
        public int displayDecimalDigits;

        public string displayValue(float raw)
        {
            using var _ = new MyStringBuilderScope(out var sb);
            switch (displayType)
            {
                case DisplayType.Flat:
                    sb.add(raw.FToS(displayDecimalDigits));
                    break;
                case DisplayType.Pct:
                    sb.add(raw.toPercent(displayDecimalDigits));
                    break;
            }

            return sb.ToString();
        }
    }
}