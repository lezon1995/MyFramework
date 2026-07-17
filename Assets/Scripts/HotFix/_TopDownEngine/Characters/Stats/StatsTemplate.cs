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
    }

    public abstract class StatsTemplate : SerializedScriptableObject, IStatsTemplate
    {
        public bool useExpression => false;
        public Dictionary<string, float> configs => Configs;
        public Dictionary<string, Func<float>> configExpressions => null;
        public Dictionary<string, float> ratios => Ratios;

        [DictionaryDrawerSettings(KeyLabel = "Stat", ValueLabel = "Initial")]
        public Dictionary<string, float> Configs = new();

        [DictionaryDrawerSettings(KeyLabel = "Stat", ValueLabel = "Ratio")]
        public Dictionary<string, float> Ratios = new();

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
    }
}