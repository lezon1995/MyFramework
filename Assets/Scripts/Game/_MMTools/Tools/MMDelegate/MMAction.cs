using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    public class MMAction
    {
        class Callback
        {
            public Action Action { get; set; }
        }

        static RefPool<Callback> _pool = new();

        Dictionary<Action, Callback> Dict = new();
        List<Callback> List = new();

        public void Invoke()
        {
            foreach (var callback in List)
            {
                callback.Action();
            }
        }

        public void Add(Action action)
        {
            if (!Dict.TryGetValue(action, out var callback))
            {
                callback = _pool.Get();
                callback.Action = action;
                Dict.Add(action, callback);
            }

            List.Add(callback);
        }

        public void Rem(Action action)
        {
            if (!Dict.TryGetValue(action, out var callback))
                return;

            if (!List.Remove(callback))
                return;

            var count = 0;
            foreach (var c in List)
            {
                if (c == callback)
                    count++;
            }

            if (count == 0)
            {
                Dict.Remove(action);
                _pool.Return(callback);
            }
        }
    }

    public class MMAction<T>
    {
        class Callback
        {
            public Action<T> Action { get; set; }
        }

        static RefPool<Callback> _pool = new();

        Dictionary<Action<T>, Callback> Dict = new();
        List<Callback> List = new();

        public void Invoke(T value)
        {
            foreach (var callback in List)
            {
                callback.Action(value);
            }
        }

        public void Add(Action<T> action)
        {
            if (!Dict.TryGetValue(action, out var callback))
            {
                callback = _pool.Get();
                callback.Action = action;
                Dict.Add(action, callback);
            }

            List.Add(callback);
        }

        public void Rem(Action<T> action)
        {
            if (!Dict.TryGetValue(action, out var callback))
                return;

            if (!List.Remove(callback))
                return;

            var count = 0;
            foreach (var c in List)
            {
                if (c == callback)
                    count++;
            }

            if (count == 0)
            {
                Dict.Remove(action);
                _pool.Return(callback);
            }
        }
    }

    public class MMAction<T, T1>
    {
        class Callback
        {
            public Action<T, T1> Action { get; set; }
        }

        static RefPool<Callback> _pool = new();

        Dictionary<Action<T, T1>, Callback> Dict = new();
        List<Callback> List = new();

        public void Invoke(T value, T1 value1)
        {
            foreach (var callback in List)
            {
                callback.Action(value, value1);
            }
        }

        public void Add(Action<T, T1> action)
        {
            if (!Dict.TryGetValue(action, out var callback))
            {
                callback = _pool.Get();
                callback.Action = action;
                Dict.Add(action, callback);
            }

            List.Add(callback);
        }

        public void Rem(Action<T, T1> action)
        {
            if (!Dict.TryGetValue(action, out var callback))
                return;

            if (!List.Remove(callback))
                return;

            var count = 0;
            foreach (var c in List)
            {
                if (c == callback)
                    count++;
            }

            if (count == 0)
            {
                Dict.Remove(action);
                _pool.Return(callback);
            }
        }
    }
}