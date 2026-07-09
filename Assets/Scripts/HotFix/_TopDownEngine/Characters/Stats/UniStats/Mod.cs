using System;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UniStats
{
    public enum Operator
    {
        Add,
        Mul,
        Set,
    }

    [Serializable]
    public abstract class Mod<T> : IMod<T> where T : struct
    {
        public Action<T, T> OnChanged { get; }

        public Handler<T> Event { get; }
        public Priority Priority { get; set; }
        public string Name { get; set; }

        bool _enabled = true;

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    Event.Invoke(default, default);
                }
            }
        }


        protected Mod()
        {
            Event = new();
            OnChanged = Event.Invoke;
        }

        public abstract T Modify(T given);
        public abstract void Release();

        public virtual void OnRelease()
        {
            Priority = default;
            Name = null;
            Enabled = true;
            Event.Clear();
        }
    }

    [Serializable]
    public struct Priority : IComparable<Priority>
    {
        int Order;
        int Age;

        public Priority(int order, int age)
        {
            Order = order;
            Age = age;
        }

        public override string ToString()
        {
            return $"{Order}-{Age}";
        }

        public int CompareTo(Priority other)
        {
            var result = Order.CompareTo(other.Order);
            if (result != 0)
                return result;
            return Age.CompareTo(other.Age);
        }
    }
}