using System;
using System.Collections.Generic;

namespace UniStats
{
    [Serializable]
    public class CastMod<S, T> : Mod<T> where T : struct where S : struct
#if NET7_0_OR_GREATER
        where S : INumber<S>
        where T : INumber<T>
#endif
    {
        public new Action<S, S> OnChanged { get; }

        IMod<S> _mod;

        CastMod()
        {
            OnChanged = (_, _) => Event.Invoke(default, default);
        }

        CastMod(IMod<S> mod, string name) : this()
        {
            Construct(mod);
            Name = name;
        }

        CastMod<S, T> Build(IMod<S> mod, string name)
        {
            Construct(mod);
            Name = name;
            return this;
        }

        public override T Modify(T given)
        {
#if NET7_0_OR_GREATER
            return T.CreateChecked(_mod.Modify(S.CreateChecked(given)));
#else
            var sOperator = Mod.GetOperator<S>();
            var tOperator = Mod.GetOperator<T>();
            return tOperator.Create(_mod.Modify(sOperator.Create(given)));
#endif
        }

        void Construct(IMod<S> mod)
        {
            _mod = mod;
            _mod.Event.Add(OnChanged);
        }

        protected void Deconstruct()
        {
            _mod.Release();
            _mod.Event.Rem(OnChanged);
            _mod = null;
        }

        public override void Release()
        {
            Release(this);
        }

        public override void OnRelease()
        {
            Deconstruct();
            base.OnRelease();
        }

        public override string ToString()
        {
            return $"Cast[{Name}]";
        }

        #region Pool

        static Queue<CastMod<S, T>> pool = new();

        public static CastMod<S, T> Get(IMod<S> mod, string name)
        {
            if (pool.TryDequeue(out var castMod))
                return castMod.Build(mod, name);

            return new CastMod<S, T>(mod, name);
        }

        static void Release(CastMod<S, T> mod)
        {
            mod.OnRelease();
            pool.Enqueue(mod);
        }

        #endregion
    }

    public static partial class Mod
    {
        public static Mod<T> Cast<S, T>(this IMod<S> mod, string name = null)
            where S : struct
            where T : struct
#if NET7_0_OR_GREATER
            where S : INumber<S>
            where T : INumber<T>
#endif
        {
            return CastMod<S, T>.Get(mod, name);
        }
    }
}