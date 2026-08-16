using System;
using System.Collections.Generic;
using UnityEngine;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UniStats
{
    public class Stat : Stat<float>
    {
        public Stat(float initial, float bonusRatio = 1F) : base(initial, bonusRatio)
        {
        }

        public Stat(Func<float> initialGetter, float bonusRatio = 1F) : base(initialGetter, bonusRatio)
        {
        }
    }

    /// <summary>
    /// Represents a stat that can be modified.
    /// Initial           基础数值
    /// 
    /// BonusFlat          额外数值
    /// BonusRatio  额外数值收益率
    /// 
    /// Value = Initial + BonusFlat * BonusPct.
    /// </summary>
    public class Stat<T> : IAttr<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : struct
#endif
    {
        static IOperator<T> op = Mod.GetOperator<T>();
        public string Name { get; set; }

        public Attr<T> BonusFlat { get; }
        public Attr<T> BonusPct { get; }
        public Attr<T> BonusRatio { get; }

        public Stat(Func<T> initialGetter, T bonusRatio) : this(initialGetter(), bonusRatio)
        {
            InitialGetter = initialGetter;
        }

        public Stat(T initial, T bonusRatio)
        {
            Initial = initial;
            BonusFlat = new();
            BonusPct = new();
            BonusRatio = new(bonusRatio);

            Event = new();
            OnChanged = (_, _) => SetDirty();
            BonusFlat.Event.Add(OnChanged);
            BonusPct.Event.Add(OnChanged);

            Dirty = true;
        }

        public Action<T, T> OnChanged { get; }
        public Handler<T> Event { get; }
        T _initial;

        public T Initial
        {
            get => InitialGetter?.Invoke() ?? _initial;
            set => _initial = value;
        }

        public Func<T> InitialGetter { get; set; }
        public Func<string> DisplayValueGetter { get; set; }
        public Sprite DisplayIcon { get; set; }

        public T Value
        {
            get
            {
                if (Dirty)
                    Compute();

                return _cache;
            }
            set { }
        }

        public T BonusValue
        {
            get
            {
                if (Dirty)
                    Compute();

                return _cacheBonus;
            }
        }

        bool _dirty;

        public bool Dirty
        {
            get
            {
                if (EqualityComparer<T>.Default.Equals(Initial, _initial))
                    return _dirty;

                return true;
            }
            private set => _dirty = value;
        }

        public List<IMod<T>> Mods => BonusFlat.Mods;

        T _cache;
        T _cacheBonus;

        public T Compute()
        {
            var bonus = BonusFlat.Value;
            var bonusPct = BonusPct.Value;
            var bonusRatio = BonusRatio.Value;

            var initial = Initial;
            _initial = initial;
            var realBonus = op.Add(bonus, op.Mul(initial, bonusPct));
            realBonus = op.Mul(realBonus, bonusRatio);
            var v = op.Add(initial, realBonus);
            _cache = v;
            _cacheBonus = realBonus;
            Dirty = false;
            return v;
        }

        public void SetDirty()
        {
            Dirty = true;
            Event.Invoke(_cache, Value);
        }

        public void AddMod(IMod<T> mod, int order = 0)
        {
            BonusFlat.AddMod(mod, order);
        }

        public bool RemoveMod(string key, bool release = true)
        {
            if (BonusFlat.RemoveMod(key, release))
                return true;

            if (BonusPct.RemoveMod(key, release))
                return true;

            return false;
        }

        public bool GetMod(string key, out NumMod<T> result)
        {
            if (BonusFlat.GetMod(key, out result))
                return true;

            if (BonusPct.GetMod(key, out result))
                return true;

            return false;
        }

        public bool HasMod(string key)
        {
            if (BonusFlat.HasMod(key))
                return true;

            if (BonusPct.HasMod(key))
                return true;

            return false;
        }

        public void ClearMods(bool release = true)
        {
            BonusFlat.ClearMods(release);
            BonusPct.ClearMods(release);
        }

        public static implicit operator bool(Stat<T> self)
        {
            return self != null;
        }

        public override string ToString()
        {
            return $"{Value} = {Initial} + ({BonusFlat.Value} + {Initial} * {BonusPct.Value}) * {BonusRatio.Value}";
        }

        public void Release()
        {
        }

        public void OnRelease()
        {
        }
    }
}