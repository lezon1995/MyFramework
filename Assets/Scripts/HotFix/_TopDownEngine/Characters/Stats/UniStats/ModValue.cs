using System;
using System.Collections.Generic;

namespace UniStats
{
    [Serializable]
    public class Attr : Attr<float>
    {
    }

    [Serializable]
    public class Attr<T> : IAttr<T> where T : struct
    {
        public Handler<T> Event { get; }
        public Action<T, T> OnChanged { get; }

        public Attr(T initial)
        {
            Dirty = true;
            Event = new();
            OnChanged = (_, _) => SetDirty();

            Initial = initial;
        }

        public Attr() : this(default)
        {
        }

        List<IMod<T>> _mods = new();
        public List<IMod<T>> Mods => _mods;

        public T Initial { get; set; }
        public T Cache { get; private set; }
        public T BonusCache { get; private set; }
        public bool Dirty { get; private set; }
        public int Age { get; private set; }

        public T BonusValue
        {
            get
            {
                if (Dirty)
                    Compute();

                return BonusCache;
            }
        }

        public virtual T Value
        {
            get
            {
                if (Dirty)
                    Compute();

                return Cache;
            }
            set { }
        }

        public void SetDirty()
        {
            Dirty = true;
            Event.Invoke(Cache, Value);
        }

        public T Compute()
        {
            var initial = Initial;
            T v = initial;
            for (var i = 0; i < _mods.Count; i++)
            {
                var mod = _mods[i];
                if (mod.Enabled)
                    v = mod.Modify(v);
            }

            var op = Mod.GetOperator<T>();
            var bonus = op.Add(v, op.Negate(initial));

            Cache = v;
            BonusCache = bonus;
            Dirty = false;
            return v;
        }

        public bool GetMod(string name, out NumMod<T> result)
        {
            for (var i = 0; i < _mods.Count; i++)
            {
                var mod = _mods[i];
                if (mod.Name == name && mod is NumMod<T> numMod)
                {
                    result = numMod;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public void AddMod(IMod<T> mod, int order = 0)
        {
            InternalAdd(mod, order);
        }

        void InternalAdd(IMod<T> mod, int order)
        {
            mod.Event.Rem(OnChanged);
            mod.Event.Add(OnChanged);
            mod.Priority = new(order, ++Age);

            var index = BinarySearchIndex(mod);
            _mods.Insert(index, mod);
            SetDirty();
        }

        int BinarySearchIndex(IMod<T> item)
        {
            int low = 0;
            int high = _mods.Count - 1;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                var result = _mods[mid].Priority.CompareTo(item.Priority);
                switch (result)
                {
                    case 0:
                        return mid;
                    case < 0:
                        low = mid + 1;
                        break;
                    case > 0:
                        high = mid - 1;
                        break;
                }
            }

            return low;
        }

        public bool RemoveMod(string key, bool release = true)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            bool success = false;
            for (var i = _mods.Count - 1; i >= 0; i--)
            {
                var mod = _mods[i];
                if (mod.Name == key)
                {
                    InternalRemove(mod, i, release);
                    success = true;
                }
            }

            return success;
        }

        public void ClearMods(bool release)
        {
            for (var i = _mods.Count - 1; i >= 0; i--)
            {
                InternalRemove(_mods[i], i, release);
            }
        }

        void InternalRemove(IMod<T> mod, int index, bool release)
        {
            _mods.RemoveAt(index);

            if (release)
            {
                mod.Event.Rem(OnChanged);
                mod.Release();
            }

            SetDirty();
        }

        public bool HasMod(string key)
        {
            for (var i = 0; i < _mods.Count; i++)
            {
                if (_mods[i].Name == key)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return Value switch
            {
                int i => i.ToString(),
                float f => f.ToString("F1"),
                double d => d.ToString("F2"),
                _ => Value.ToString()
            };
        }

        public virtual void Release()
        {
        }

        public virtual void OnRelease()
        {
            _mods.Clear();
            Initial = default;
            Cache = default;
            BonusCache = default;
            Dirty = true;
            Event.Clear();
        }

        public static implicit operator bool(Attr<T> self)
        {
            return self != null;
        }
    }

    [Serializable]
    public class RangedAttr<T> : Attr<T> where T : struct
#if NET7_0_OR_GREATER
        where T : System.Numerics.INumber<T>
#endif
    {
        public override T Value => RangedVar<T>.Clamp(base.Value, Min, Max);

        IVar<T> _min;
        IVar<T> _max;

        public T Min => _min.Value;
        public T Max => _max.Value;

        Action releaseAction;

        public RangedAttr(T initial, IVar<T> min, IVar<T> max) : base(initial)
        {
            _min = min;
            _max = max;
        }

        public RangedAttr(T initial, T lower, IVar<T> upper) : this(initial, Property<T>.Get(lower), upper)
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)_min);
            };
        }

        public RangedAttr(T initial, IVar<T> lower, T upper) : this(initial, lower, Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)_max);
            };
        }

        public RangedAttr(T initial, T lower, T upper) : this(initial, Property<T>.Get(lower), Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                Property<T>.Release((Property<T>)_min);
                Property<T>.Release((Property<T>)_max);
            };
        }


        public override void OnRelease()
        {
            releaseAction?.Invoke();
            base.OnRelease();
        }
    }
}