using UnityEngine;

namespace MoreMountains.Tools
{
    public static class ComponentExtensions
    {
        public static void SetActive(this Component self, bool active)
        {
            if (self == null)
                return;

            if (self.gameObject == null)
                return;

            self.gameObject.SetActive(active);
        }

        public static void SetScale(this Component self, float scale)
        {
            self.transform.localScale = Vector3.one * scale;
        }

        public static Transform MMFind(this Component c, string childName, bool useBFS = true)
        {
            if (useBFS)
                return c.transform.MMFindDeepChildBreadthFirst(childName);

            return c.transform.MMFindDeepChildDepthFirst(childName);
        }

        public static T MMFind<T>(this Component c, string childName, bool useBFS = true) where T : class
        {
            var child = c.transform.MMFind(childName, useBFS);
            if (child)
                return child.GetComponent<T>();
            return null;
        }
    }
}