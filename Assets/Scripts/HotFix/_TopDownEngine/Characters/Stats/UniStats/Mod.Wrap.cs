using System;
using System.Collections.Generic;

namespace UniStats
{
    [Serializable]
    public class WrapMod<S, T> : Mod<T>
        where S : class
        where T : struct
    {
        S _context;
        IMod<T> _mod;

        public override bool Enabled
        {
            get => _mod.Enabled;
            set => _mod.Enabled = value;
        }

        WrapMod(S context, IMod<T> mod, string name)
        {
            Construct(context);
            _mod = mod;
            _mod.Event.Add(OnChanged);
            Name = name;
        }

        WrapMod<S, T> Build(S context, IMod<T> mod, string name)
        {
            Construct(context);
            _mod = mod;
            _mod.Event.Add(OnChanged);
            Name = name;
            return this;
        }

        public override T Modify(T given)
        {
            return _mod.Modify(given);
        }

        void Construct(S context)
        {
            switch (context)
            {
                case IMod<T> mod:
                    mod.Event.Add(OnChanged);
                    break;
                case IVar<T> value:
                    value.Event.Add(OnChanged);
                    break;
            }

            _context = context;
        }

        protected void Deconstruct()
        {
            switch (_context)
            {
                case IMod<T> mod:
                    mod.Release();
                    mod.Event.Rem(OnChanged);
                    break;
                case IVar<T> value:
                    value.Release();
                    value.Event.Rem(OnChanged);
                    break;
            }

            _context = default;
        }

        public override void Release()
        {
            Release(this);
        }

        public override void OnRelease()
        {
            Deconstruct();
            _mod.Event.Rem(OnChanged);
            _mod = default;
            base.OnRelease();
        }

        public override string ToString()
        {
            return $"Wrap[{Name}][{_mod}]";
        }

        #region Pool

        static Queue<WrapMod<S, T>> pool = new();

        public static WrapMod<S, T> Get(S context, IMod<T> mod, string name)
        {
            if (pool.TryDequeue(out var wrapMod))
                return wrapMod.Build(context, mod, name);

            return new WrapMod<S, T>(context, mod, name);
        }

        static void Release(WrapMod<S, T> mod)
        {
            mod.OnRelease();
            pool.Enqueue(mod);
        }

        #endregion
    }

    public static partial class Mod
    {
        public static WrapMod<IVar<T>, T> Wrap<T>(this IMod<T> mod, IVar<T> context, string name = null) where T : struct
        {
            return WrapMod<IVar<T>, T>.Get(context, mod, name);
        }

        public static WrapMod<IMod<T>, T> Wrap<T>(this IMod<T> mod, IMod<T> context, string name = null) where T : struct
        {
            return WrapMod<IMod<T>, T>.Get(context, mod, name);
        }
    }
}