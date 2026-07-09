using System;
using System.Collections.Generic;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UniStats
{
    [Serializable]
    public class Property<T> : IVar<T> where T : struct
    {
        public Handler<T> Event { get; }
        public Action<T, T> OnChanged { get; }

        T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    var pre = _value;
                    _value = value;
                    Event.Invoke(pre, value);
                }
            }
        }

        public Property(T value)
        {
            Event = new();
            OnChanged = null;
            _value = value;
        }

        Property<T> Build(T value)
        {
            _value = value;
            return this;
        }

        public void Release()
        {
            Release(this);
        }

        public void OnRelease()
        {
            _value = default;
            Event.Clear();
        }

        public static implicit operator bool(Property<T> self) => self != null;

        #region Pool

        static Queue<Property<T>> pool = new();

        public static Property<T> Get(T initial = default)
        {
            if (pool.TryDequeue(out var property))
                return property.Build(initial);

            return new Property<T>(initial);
        }

        public static void Release(Property<T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        #endregion
    }

    public static class Proxy
    {
        public static IVar<T> Create<T>(Func<T> getter, out Action<T, T> onChange) where T : struct
        {
            return Proxy<T>.Get(getter, out onChange);
        }

        public static IVar<T> Create<T>(Func<T> getter, Action<T> setter, out Action<T, T> onChange) where T : struct
        {
            return Proxy<T>.Get(getter, setter, out onChange);
        }
    }

    public class Proxy<T> : IVar<T> where T : struct
    {
        public Handler<T> Event { get; }
        public Action<T, T> OnChanged { get; }

        Func<T> _getter;
        Action<T> _setter;

        public T Value
        {
            get => _getter();
            set => _setter?.Invoke(value);
        }

        Proxy()
        {
            Event = new();
            OnChanged = Event.Invoke;
        }

        Proxy(Func<T> getter, out Action<T, T> onChange) : this()
        {
            _getter = getter;
            _setter = null;
            onChange = OnChanged;
        }

        Proxy(Func<T> getter, Action<T> setter, out Action<T, T> onChange) : this()
        {
            _getter = getter;
            _setter = setter;
            onChange = OnChanged;
        }

        Proxy<T> Build(Func<T> getter, out Action<T, T> onChange)
        {
            _getter = getter;
            _setter = null;
            onChange = OnChanged;
            return this;
        }

        Proxy<T> Build(Func<T> getter, Action<T> setter, out Action<T, T> onChange)
        {
            _getter = getter;
            _setter = setter;
            onChange = OnChanged;
            return this;
        }

        static void Release(Proxy<T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public void Release()
        {
            Release(this);
        }

        public void OnRelease()
        {
            _getter = null;
            _setter = null;
            Event.Clear();
        }

        #region Pool

        static Queue<Proxy<T>> pool = new();

        public static Proxy<T> Get(Func<T> getter, out Action<T, T> onChange)
        {
            if (pool.TryDequeue(out var value))
                return value.Build(getter, out onChange);

            return new Proxy<T>(getter, out onChange);
        }

        public static Proxy<T> Get(Func<T> getter, Action<T> setter, out Action<T, T> onChange)
        {
            if (pool.TryDequeue(out var value))
                return value.Build(getter, setter, out onChange);

            return new Proxy<T>(getter, setter, out onChange);
        }

        #endregion
    }

    [Serializable]
    public partial class RangedVar<T> : IVar<T> where T : struct
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        public Handler<T> Event { get; }
        public Action<T, T> OnChanged { get; }
        T _value;

        public T Value
        {
            get => _value;
            set
            {
                var now = Clamp(value, Min, Max);
#if NET7_0_OR_GREATER
                if (_value != now)
#else
                if (!EqualityComparer<T>.Default.Equals(_value, now))
#endif
                {
                    var pre = _value;
                    _value = now;
                    OnChange(pre, now);
                }
            }
        }

        public T Min => Lower.Value;
        public T Max => Upper.Value;

        public IVar<T> Lower;
        public IVar<T> Upper;

        public static T Clamp(T value, T min, T max)
        {
#if NET7_0_OR_GREATER
            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            return value;
#else
            var op = Mod.GetOperator<T>();
            return op.Max(min, op.Min(max, value));
#endif
        }

        #region Constructor

        Action releaseAction;

        public RangedVar(T value, IVar<T> lower, IVar<T> upper)
        {
            Event = new();
            OnChanged = BoundChanged;

            _value = value;
            Lower = lower;
            Lower.Event.Add(OnChanged);

            Upper = upper;
            Upper.Event.Add(OnChanged);
        }

        public RangedVar(T value, T lower, IVar<T> upper) : this(value, Property<T>.Get(lower), upper)
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Lower);
            };
        }

        public RangedVar(T value, IVar<T> lower, T upper) : this(value, lower, Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Upper);
            };
        }

        public RangedVar(T value, T lower, T upper) : this(value, Property<T>.Get(lower), Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Lower);
                Property<T>.Release((Property<T>)Upper);
            };
        }

        RangedVar<T> Build(T value, IVar<T> lower, IVar<T> upper)
        {
            _value = value;
            Lower = lower;
            Lower.Event.Add(OnChanged);

            Upper = upper;
            Upper.Event.Add(OnChanged);
            return this;
        }


        RangedVar<T> Build(T value, T lower, IVar<T> upper)
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Lower);
            };

            return Build(value, Property<T>.Get(lower), upper);
        }

        RangedVar<T> Build(T value, IVar<T> lower, T upper)
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Upper);
            };

            return Build(value, lower, Property<T>.Get(upper));
        }

        RangedVar<T> Build(T value, T lower, T upper)
        {
            releaseAction = () =>
            {
                Property<T>.Release((Property<T>)Lower);
                Property<T>.Release((Property<T>)Upper);
            };

            return Build(value, Property<T>.Get(lower), Property<T>.Get(upper));
        }

        #endregion

        void BoundChanged(T pre, T now)
        {
            Value = _value;
        }

        protected void OnChange(T pre, T now) => Event.Invoke(pre, now);

        public void OnRelease()
        {
            _value = default;

            Lower.Event.Rem(OnChanged);
            Upper.Event.Rem(OnChanged);

            releaseAction?.Invoke();
            releaseAction = null;

            Lower = null;
            Upper = null;

            Event.Clear();
        }
    }

    public partial class RangedVar<T> where T : struct
    {
        static Queue<RangedVar<T>> pool = new();

        public static RangedVar<T> Get(T initial, IVar<T> lower, IVar<T> upper)
        {
            if (pool.TryDequeue(out var value))
                return value.Build(initial, lower, upper);

            return new RangedVar<T>(initial, lower, upper);
        }

        public static RangedVar<T> Get(T initial, T lower, IVar<T> upper)
        {
            if (pool.TryDequeue(out var value))
                return value.Build(initial, lower, upper);

            return new RangedVar<T>(initial, lower, upper);
        }

        public static RangedVar<T> Get(T initial, IVar<T> lower, T upper)
        {
            if (pool.TryDequeue(out var value))
                return value.Build(initial, lower, upper);

            return new RangedVar<T>(initial, lower, upper);
        }

        public static RangedVar<T> Get(T initial, T lower, T upper)
        {
            if (pool.TryDequeue(out var value))
                return value.Build(initial, lower, upper);

            return new RangedVar<T>(initial, lower, upper);
        }

        public static void Release(RangedVar<T> _var)
        {
            _var.OnRelease();
            pool.Enqueue(_var);
        }

        public void Release()
        {
            Release(this);
        }
    }
}