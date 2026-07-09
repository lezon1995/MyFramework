using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
    /// <summary>
    /// List extensions
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Swaps two items in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public static void MMSwap<T>(this IList<T> list, int i, int j)
        {
            (list[i], list[j]) = (list[j], list[i]);
        }

        /// <summary>
        /// Shuffles a list randomly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void MMShuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list.MMSwap(i, Random.Range(i, list.Count));
            }
        }

        public static void Shuffle<T>(this IList<T> list, System.Random rnd)
        {
            for (int i = list.Count; i > 1; i--)
            {
                var next = rnd.Next(i);
                (list[i - 1], list[next]) = (list[next], list[i - 1]);
            }
        }

        public static T Get<T>(this IList<T> array, int index)
        {
            if (array == null)
                return default;

            if (index < 0 || index >= array.Count)
                return default;

            return array[index];
        }

        public static bool TryGet<T>(this IList<T> array, int index, out T result)
        {
            if (array == null)
            {
                result = default;
                return false;
            }

            if (index < 0 || index >= array.Count)
            {
                result = default;
                return false;
            }

            result = array[index];
            return true;
        }

        public static void ForeachRemove<T>(this IList<T> list, Func<T, bool> canRemove, Action<T> onRemove = null, Action<T> onUpdate = null)
        {
            int i = 0;
            while (i < list.Count)
            {
                var e = list[i];
                onUpdate?.Invoke(e);
                if (canRemove(e))
                {
                    onRemove?.Invoke(e);
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }
}