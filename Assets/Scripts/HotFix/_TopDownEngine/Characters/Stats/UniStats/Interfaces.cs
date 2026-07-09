using System;
using System.Collections.Generic;

namespace UniStats
{
    public interface IPool
    {
        void Release();
        void OnRelease();
    }

    public interface IVar<T> : IPool where T : struct
    {
        Action<T, T> OnChanged { get; }
        Handler<T> Event { get; }

        T Value { get; set; }
    }

    public interface IMod<T> : IPool where T : struct
    {
        Action<T, T> OnChanged { get; }
        Handler<T> Event { get; }

        public Priority Priority { get; set; }
        public string Name { get; }
        public bool Enabled { get; set; }
        public T Modify(T given);
    }

    public interface IAttr<T> : IVar<T> where T : struct
    {
        T Initial { get; set; }
        T BonusValue { get; }
        bool Dirty { get; }
        List<IMod<T>> Mods { get; }

        T Compute();
        void SetDirty();
        
        void AddMod(IMod<T> mod, int order = 0);
        bool RemoveMod(string key, bool release = true);
        bool GetMod(string key, out NumMod<T> result);
        bool HasMod(string key);
        void ClearMods(bool release = true);
    }

    public interface ITarget<in S, T> where T : struct
    {
        IMod<T> Mod { get; }
        IAttr<T> AppliesTo(S thing);
    }
}