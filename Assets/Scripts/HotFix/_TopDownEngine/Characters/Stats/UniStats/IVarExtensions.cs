using System;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UniStats
{
    public static class IVarExtensions
    {
        public static IVar<T> Select<S, T>(this IVar<S> v, Func<S, T> getter) where T : struct where S : struct
        {
            var valueT = Proxy.Create(() => getter(v.Value), out var onChange);
            v.Event.Add((pre, now) => onChange(getter(pre), getter(now)));
            return valueT;
        }

        public static IVar<U> Zip<S, T, U>(this IVar<S> s, IVar<T> t, Func<S, T, U> getter) where U : struct where S : struct where T : struct
        {
            var valueU = Proxy.Create(() => getter(s.Value, t.Value), out var onChange);
            s.Event.Add((pre, now) => onChange(getter(pre, t.Value), getter(now, t.Value)));
            t.Event.Add((pre, now) => onChange(getter(s.Value, pre), getter(s.Value, now)));
            return valueU;
        }

        public static IVar<T> Select<S, T>(this IVar<S> v, Func<S, T> getter, Action<IVar<S>, T> setter) where T : struct where S : struct
        {
            var valueT = Proxy.Create(() => getter(v.Value), x => setter(v, x), out var onChange);
            v.Event.Add((pre, now) => onChange(getter(pre), getter(now)));
            return valueT;
        }

        class ActionDisposable : IDisposable
        {
            Action _action;

            public ActionDisposable(Action action) => _action = action;

            public void Dispose() => _action();
        }

        /// <summary>
        /// Subscribes to the property change events of an object and executes the specified action.
        /// </summary>
        /// <typeparam name="T">The type of the object implementing <see cref="IVar{T}"/>.</typeparam>
        /// <param name="v">The object to subscribe to.</param>
        /// <param name="action">The action to execute on property change.</param>
        /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
        public static IDisposable OnChange<T>(this IVar<T> v, Action<IVar<T>> action) where T : struct
        {
            Action<T, T> callback = PropertyChange;
            v.Event.Add(callback);
            return new ActionDisposable(() => v.Event.Rem(callback));
            void PropertyChange(T pre, T now) => action(v);
        }
    }
}