using System;
using System.Collections.Generic;
using System.Linq;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UniStats
{
    public static class ModValueExtensions
    {
        /// <summary>
        /// Collects how a particular modifier changes the value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="attr">The modifiable value.</param>
        /// <param name="mod">The modifier to probe.</param>
        /// <returns>An enumerable of before and after values.</returns>
        public static IEnumerable<(T before, T after)> ProbeAffects<T>(this IAttr<T> attr, IMod<T> mod) where T : struct
        {
            T before = attr.Initial;
            var mods = attr.Mods;
            for (var i = 0; i < mods.Count; i++)
            {
                var m = mods[i];
                T after = before;
                if (m.Enabled)
                    after = m.Modify(before);

                if (mod == m)
                    yield return (before, after);

                before = after;
            }
        }

#if NET7_0_OR_GREATER
        public static string AddFlat<T>(this IModValue<T> modValue, T delta, string key = null) where T : struct, INumber<T>
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var mod = Mod.Add(delta, key);
            modValue.Add(mod);
            return key;
        }

        public static string AddPct<T>(this IModValue<T> modValue, T delta, string key = null) where T : struct, INumber<T>
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var one = T.One;
            var sum = one + delta;
            var mod = Mod.Mul(sum, key);
            modValue.Add(mod);
            return key;
        }

        public static string SubFlat<T>(this IModValue<T> modValue, T delta, string key = null) where T : struct, INumber<T>
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var mod = Mod.Sub(delta, key);
            modValue.Add(mod);
            return key;
        }

        public static string SubPercent<T>(this IModValue<T> modValue, T delta, string key = null) where T : struct, INumber<T>
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var one = T.One;
            var sum = one - delta;
            var mod = Mod.Mul(sum, key);
            modValue.Add(mod);
            return key;
        }

        public static bool Remove<T>(this IModValue<IValue<T>, T> modValue, string key) where T : struct
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return modValue.Remove(key);
        }


        /// <summary>
        /// Returns the delta a modifier (may be multiple) does.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modValue">The modifiable value.</param>
        /// <param name="mod">The modifier to probe.</param>
        /// <returns>The accumulated delta.</returns>
        public static T ProbeDelta<T>(this IModValue<IValue<T>, T> modValue, IMod<T> mod) where T : INumber<T>
        {
            T accum = T.Zero;
            foreach (T delta in modValue.ProbeAffects(mod).Select(x => x.after - x.before))
            {
                accum += delta;
            }

            return accum;
        }
#else
        public static string AddFlat<T>(this IAttr<T> attr, T delta, string key = null) where T : struct
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var mod = Mod.Add(delta, key);
            attr.AddMod(mod);
            return key;
        }

        public static string AddFlat<T>(this IAttr<T> attr, IVar<T> v, string key = null) where T : struct
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var mod = Mod.Add(v, key);
            attr.AddMod(mod);
            return key;
        }

        public static string AddPct<T>(this IAttr<T> attr, T delta, string key = null) where T : struct
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var op = Mod.GetOperator<T>();
            var one = op.One;
            var sum = op.Add(one, delta);
            var mod = Mod.Mul(sum, key);
            attr.AddMod(mod);
            return key;
        }

        public static string AddFunc<T>(this IAttr<T> attr, Func<T, T> func, string key = null) where T : struct
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var mod = Mod.Func(func, key);
            attr.AddMod(mod);
            return key;
        }

        public static string AddFunc<T>(this IAttr<T> attr, Func<T, T> func, out Action<T, T> onChange, string key = null) where T : struct
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            var mod = Mod.Func(func, out onChange, key);
            attr.AddMod(mod);
            return key;
        }

        // public static bool Remove<T>(this IModValue<T> modValue, string key) where T : struct
        // {
        //     if (string.IsNullOrEmpty(key))
        //     {
        //         return false;
        //     }
        //
        //     return modValue.Remove(key);
        // }

        public static T ProbeDelta<T>(this IAttr<T> attr, IMod<T> mod) where T : struct
        {
            var op = Mod.GetOperator<T>();
            T accum = op.Zero;
            var enumerable = attr.ProbeAffects(mod).Select(x =>
            {
                var negativeBefore = Mod.GetOperator<T>().Negate(x.before);
                return op.Add(x.after, negativeBefore);
            });
            foreach (var delta in enumerable)
            {
                accum = op.Add(accum, delta);
            }

            return accum;
        }
#endif

        /// <summary>
        /// Removes all occurrences of an item from a collection. Returns the number of items removed.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns>The number of items removed.</returns>
        public static int RemoveAll<T>(this ICollection<T> collection, T item)
        {
            int count = 0;
            while (collection.Remove(item))
            {
                count++;
            }

            return count;
        }

        public static T Peek<T>(this IAttr<T> attr, string withoutMod = null) where T : struct
        {
            var initial = attr.Initial;
            T v = initial;
            var list = attr.Mods;
            for (var i = 0; i < list.Count; i++)
            {
                var mod = list[i];
                if (mod.Name == withoutMod)
                    continue;

                if (mod.Enabled)
                    v = mod.Modify(v);
            }

            return v;
        }

        public static T PeekBonus<T>(this IAttr<T> attr, string withoutMod = null) where T : struct
        {
            var initial = attr.Initial;
            T v = initial;
            var list = attr.Mods;
            for (var i = 0; i < list.Count; i++)
            {
                var mod = list[i];
                if (mod.Name == withoutMod)
                    continue;

                if (mod.Enabled)
                    v = mod.Modify(v);
            }

            var op = Mod.GetOperator<T>();
            var bonus = op.Add(v, op.Negate(initial));
            return bonus;
        }

        public static bool SetModActive<T>(this IAttr<T> attr, string name, bool active) where T : struct
        {
            var list = attr.Mods;
            for (var i = 0; i < list.Count; i++)
            {
                var mod = list[i];
                if (mod.Name == name)
                {
                    mod.Enabled = active;
                    return true;
                }
            }

            return false;
        }
    }
}