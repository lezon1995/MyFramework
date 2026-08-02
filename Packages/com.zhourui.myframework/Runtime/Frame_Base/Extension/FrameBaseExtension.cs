using UnityEngine;
using static FrameBaseUtility;

public static class FrameBaseExtension
{
    public static Transform find(this Transform parent, string name)
    {
        var o = findGameObject(name, parent.gameObject, true, true);
        if (o == null)
            return null;

        return o.transform;
    }
    
    public static bool find(this Transform parent, string name, out Transform result)
    {
        var o = findGameObject(name, parent.gameObject, true, true);
        if (o == null)
        {
            result = null;
            return false;
        }

        result = o.transform;
        return true;
    }
    
    public static RectTransform find(this RectTransform parent, string name)
    {
        var o = findGameObject(name, parent.gameObject, true, true);
        if (o == null)
            return null;

        return o.transform as RectTransform;
    }

    public static GameObject find(this GameObject parent, string name)
    {
        var o = findGameObject(name, parent.gameObject, true, true);
        if (o == null)
            return null;

        return o;
    }
    
    public static bool find(this GameObject parent, out GameObject result, string name)
    {
        var o = findGameObject(name, parent.gameObject, true, true);
        if (o == null)
        {
            result = null;
            return false;
        }

        result = o;
        return true;
    }
    
    public static bool find<T>(this GameObject go, out T component) where T : Component
    {
        if (go)
            return go.TryGetComponent(out component);

        component = null;
        return false;
    }
    
    public static bool find<T>(this GameObject go, out T component, string name) where T : Component
    {
        var o = findGameObject(name, go);
        if (o)
            return o.TryGetComponent(out component);

        component = null;
        return false;
    }

    public static bool find<T>(this Transform t, out T component) where T : Component
    {
        if (t)
            return t.TryGetComponent(out component);

        component = null;
        return false;
    }
    
    public static bool find<T>(this Transform t, out T component, string name) where T : Component
    {
        if (t == null)
        {
            component = null;
            return false;
        }

        var o = findGameObject(name, t.gameObject);
        if (o)
            return o.TryGetComponent(out component);

        component = null;
        return false;
    }
}