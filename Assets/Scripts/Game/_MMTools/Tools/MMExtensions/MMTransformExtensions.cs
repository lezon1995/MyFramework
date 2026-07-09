using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Transform extensions
    /// </summary>
    public static class TransformExtensions
    {
        public static Transform MMFind(this Transform transform, string childName, bool useBFS = true)
        {
            if (useBFS)
                return transform.MMFindDeepChildBreadthFirst(childName);

            return transform.MMFindDeepChildDepthFirst(childName);
        }

        public static T MMFind<T>(this Transform transform, string childName, bool useBFS = true) where T : class
        {
            var child = transform.MMFind(childName, useBFS);
            if (child)
                return child.GetComponent<T>();
            return null;
        }

        public static bool Find<T>(this Transform transform, string n, out T result) where T : class
        {
            var child = transform.Find(n);
            if (child)
            {
                result = child.GetComponent<T>();
                return true;
            }

            result = null;
            return false;
        }

        public static bool Find<T>(this Transform transform, out T result) where T : class
        {
            return transform.TryGetComponent(out result);
        }

        /// <summary>
        /// Destroys a transform's children
        /// </summary>
        public static void MMDestroyAllChildren(this Transform transform)
        {
            for (int t = transform.childCount - 1; t >= 0; t--)
            {
                if (Application.isPlaying)
                    Object.Destroy(transform.GetChild(t).gameObject);
                else
                    Object.DestroyImmediate(transform.GetChild(t).gameObject);
            }
        }

        /// <summary>
        /// Finds children by name, breadth first
        /// </summary>
        public static Transform MMFindDeepChildBreadthFirst(this Transform parent, string transformName)
        {
            using var _ = ListPool<Transform>.Get(out var list);
            list.Add(parent);
            while (list.Count > 0)
            {
                var child = list[0];
                list.RemoveAt(0);
                if (child.name == transformName)
                    return child;

                for (int i = 0; i < child.childCount; i++)
                {
                    var t = child.GetChild(i);
                    list.Add(t);
                }
            }

            return null;
        }

        /// <summary>
        /// Finds children by name, depth first
        /// </summary>
        public static Transform MMFindDeepChildDepthFirst(this Transform parent, string transformName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == transformName)
                    return child;

                Transform result = child.MMFindDeepChildDepthFirst(transformName);
                if (result)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Changes the layer of a transform and all its children to the new one
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="layerName"></param>
        public static void ChangeLayersRecursively(this Transform transform, string layerName)
        {
            transform.gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerName);
            }
        }

        /// <summary>
        /// Changes the layer of a transform and all its children to the new one
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="layerIndex"></param>
        public static void ChangeLayersRecursively(this Transform transform, int layerIndex)
        {
            transform.gameObject.layer = layerIndex;
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerIndex);
            }
        }


        /// <summary>
        /// Enumerates all parents of a transform
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="includeSelf"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> MMEnumerateAllParents(this Transform targetTransform, bool includeSelf = false)
        {
            if (!includeSelf)
                targetTransform = targetTransform?.parent;

            while (targetTransform)
            {
                yield return targetTransform;
                targetTransform = targetTransform.parent;
            }
        }
    }
}