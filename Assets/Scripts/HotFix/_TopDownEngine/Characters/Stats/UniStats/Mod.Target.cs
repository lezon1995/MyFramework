using System;
using System.Collections.Generic;

namespace UniStats
{
    public static partial class Mod
    {
        /// <summary>
        /// Creates a target for modifying a value in a list.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="mod">The modifier to apply to the value.</param>
        /// <param name="index">The index of the value in the list.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the list value.</returns>
        public static ITarget<IList<IAttr<T>>, T> TargetList<T>(this IMod<T> mod, int index, string name = null) where T : struct
        {
            return new ListTarget<T>(mod, index, name);
        }

        /// <summary>
        /// Creates a target for modifying a value in a dictionary.
        /// </summary>
        /// <typeparam name="K">The type of the dictionary key.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="mod">The modifier to apply to the value.</param>
        /// <param name="key">The key of the value in the dictionary.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the dictionary value.</returns>
        public static ITarget<IDictionary<K, IAttr<T>>, T> TargetDictionary<K, T>(this IMod<T> mod, K key, string name = null) where T : struct
        {
            return new DictionaryTarget<K, T>(mod, key, name);
        }

        /// <summary>
        /// Creates a target for modifying a value based on a custom target function.
        /// </summary>
        /// <typeparam name="S">The type of the target context.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="mod">The modifier to apply to the value.</param>
        /// <param name="getter">The function that provides the target value.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the value based on the custom target function.</returns>
        public static ITarget<S, T> Target<S, T>(this IMod<T> mod, Func<S, IAttr<T>> getter, string name = null) where T : struct
        {
            return new FuncTarget<S, T>(mod, getter, name);
        }

        /// <summary>
        /// Represents a base class for targets that apply modifications to values.
        /// </summary>
        /// <typeparam name="R">The type of the target context.</typeparam>
        /// <typeparam name="S">The type of the target object.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal abstract class BaseTarget<S, T> : ITarget<S, T> where T : struct
        {
            public string Name { get; set; }
            public IMod<T> Mod { get; set; }

            protected BaseTarget(IMod<T> mod, string name)
            {
                Mod = mod;
                Name = name;
            }

            public abstract IAttr<T> AppliesTo(S bag);

            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// Represents a target that applies modifications to a value based on a custom target function.
        /// </summary>
        /// <typeparam name="S">The type of the target context.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class FuncTarget<S, T> : BaseTarget<S, T> where T : struct
        {
            Func<S, IAttr<T>> _getter;

            public override IAttr<T> AppliesTo(S bag)
            {
                return _getter(bag);
            }

            public FuncTarget(IMod<T> mod, Func<S, IAttr<T>> getter, string name) : base(mod, name)
            {
                _getter = getter;
            }
        }

        /// <summary>
        /// Represents a target that applies modifications to a value in a list.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class ListTarget<T> : BaseTarget<IList<IAttr<T>>, T> where T : struct
        {
            int _index;

            public ListTarget(IMod<T> mod, int index, string name) : base(mod, name)
            {
                _index = index;
            }

            public override IAttr<T> AppliesTo(IList<IAttr<T>> bag)
            {
                return bag[_index];
            }
        }

        /// <summary>
        /// Represents a target that applies modifications to a value in a dictionary.
        /// </summary>
        /// <typeparam name="K">The type of the dictionary key.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class DictionaryTarget<K, T> : BaseTarget<IDictionary<K, IAttr<T>>, T> where T : struct
        {
            K _key;

            public DictionaryTarget(IMod<T> mod, K key, string name) : base(mod, name)
            {
                _key = key;
            }

            public override IAttr<T> AppliesTo(IDictionary<K, IAttr<T>> bag)
            {
                return bag[_key];
            }
        }
    }

    public static class TargetedModifierExtensions
    {
        /// <summary>
        /// Adds the modifier associated with the applicator to the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag to which the modifier will be added.</param>
        public static void AddToBag<S, T>(this ITarget<S, T> applicator, S bag) where T : struct
        {
            applicator.AppliesTo(bag).AddMod(applicator.Mod);
        }

        /// <summary>
        /// Removes the modifier associated with the applicator from the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag from which the modifier will be removed.</param>
        /// <returns>True if the modifier was successfully removed, otherwise false.</returns>
        public static bool RemoveFromBag<S, T>(this ITarget<S, T> applicator, S bag) where T : struct
        {
            return applicator.AppliesTo(bag).RemoveMod(applicator.Mod.Name);
        }

        /// <summary>
        /// Checks if the modifier associated with the applicator is contained in the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag to check for the presence of the modifier.</param>
        /// <returns>True if the modifier is contained in the bag, otherwise false.</returns>
        public static bool ContainedInBag<S, T>(this ITarget<S, T> applicator, S bag) where T : struct
        {
            return applicator.AppliesTo(bag).HasMod(applicator.Mod.Name);
        }
    }
}