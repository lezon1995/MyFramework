using System;
using System.Collections.Generic;

namespace UniStats
{
    [Serializable]
    public class FuncMod<T> : Mod<T> where T : struct
    {
        Func<T, T> _func { get; set; }

        FuncMod(Func<T, T> func, string name)
        {
            _func = func;
            Name = name;
        }

        FuncMod(Func<T, T> func, string name, out Action<T, T> onChange) : this(func, name)
        {
            onChange = OnChanged;
        }

        FuncMod<T> Build(Func<T, T> func, string name)
        {
            _func = func;
            Name = name;
            return this;
        }

        FuncMod<T> Build(Func<T, T> func, string name, out Action<T, T> onChange)
        {
            _func = func;
            Name = name;
            onChange = OnChanged;
            return this;
        }

        public override T Modify(T given)
        {
            return _func(given);
        }

        public override string ToString()
        {
            return $"Func[{Name}]";
        }

        public override void Release()
        {
            Release(this);
        }

        public override void OnRelease()
        {
            _func = null;
            base.OnRelease();
        }

        #region Pool

        static Queue<FuncMod<T>> pool = new();

        public static FuncMod<T> Get(Func<T, T> func, string name)
        {
            if (pool.TryDequeue(out var funcMod))
                return funcMod.Build(func, name);

            return new FuncMod<T>(func, name);
        }

        public static FuncMod<T> Get(Func<T, T> func, out Action<T, T> onChange, string name)
        {
            if (pool.TryDequeue(out var funcMod))
                return funcMod.Build(func, name, out onChange);

            return new FuncMod<T>(func, name, out onChange);
        }

        static void Release(FuncMod<T> mod)
        {
            mod.OnRelease();
            pool.Enqueue(mod);
        }

        #endregion
    }

    public static partial class Mod
    {
        public static IMod<T> Func<T>(Func<T, T> func, out Action<T, T> onChange, string name = null) where T : struct
        {
            return FuncMod<T>.Get(func, out onChange, name);
        }

        public static IMod<T> Func<T>(Func<T, T> func, string name = null) where T : struct
        {
            return FuncMod<T>.Get(func, name);
        }
    }
}