using System.Collections.Generic;

namespace MoreMountains.Tools
{
    public class RefPool<T> where T : class, new()
    {
        Stack<T> _objs;

        public RefPool()
        {
            _objs = new();
        }

        public RefPool(int count)
        {
            _objs = new(count);
            for (int i = 0; i < count; i++)
                _objs.Push(new T());
        }

        public T Get()
        {
            if (_objs.Count > 0)
                return _objs.Pop();

            return new T();
        }

        public void Return(T obj)
        {
            _objs.Push(obj);
        }
    }
}