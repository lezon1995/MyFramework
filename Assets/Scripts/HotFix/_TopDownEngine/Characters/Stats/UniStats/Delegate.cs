using System;
using System.Collections.Generic;

namespace UniStats
{
    public class Handler<T>
    {
        List<Action<T, T>> _list = new();

        public void Invoke(T t1, T t2)
        {
            foreach (var action in _list)
                action(t1, t2);
        }

        public void Add(Action<T, T> action) => _list.Add(action);
        public void Rem(Action<T, T> action) => _list.Remove(action);
        public void Clear() => _list.Clear();
    }
}