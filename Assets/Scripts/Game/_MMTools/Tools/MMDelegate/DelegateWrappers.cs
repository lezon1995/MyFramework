using System;

namespace MoreMountains.Tools
{
    public abstract class DelegateCache<T>
    {
        T _action;
        protected DelegateCache(T action) => _action = action;

        public static implicit operator T(DelegateCache<T> value)
        {
            return value._action;
        }
    }

    public class ActionCache : DelegateCache<Action>
    {
        public ActionCache(Action action) : base(action)
        {
        }
    }

    public class ActionCache<T> : DelegateCache<Action<T>>
    {
        public ActionCache(Action<T> action) : base(action)
        {
        }
    }

    public class ActionCache<T, T1> : DelegateCache<Action<T, T1>>
    {
        public ActionCache(Action<T, T1> action) : base(action)
        {
        }
    }
}