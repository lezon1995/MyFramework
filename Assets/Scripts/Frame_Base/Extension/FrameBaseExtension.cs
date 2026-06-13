using UnityEngine;
using static FrameBaseUtility;

public static class FrameBaseExtension
{
    public static Transform find(this Transform parent, string name)
    {
        var o = getGameObject(name, parent.gameObject, true, true);
        if (o == null)
            return null;

        return o.transform;
    }
    
    public static bool find(this Transform parent, string name, out Transform result)
    {
        var o = getGameObject(name, parent.gameObject, true, true);
        if (o == null)
        {
            result = null;
            return false;
        }

        result = o.transform;
        return true;
    }

    public static GameObject find(this GameObject parent, string name)
    {
        var o = getGameObject(name, parent.gameObject, true, true);
        if (o == null)
            return null;

        return o;
    }
    
    public static bool find(this GameObject parent, out GameObject result, string name)
    {
        var o = getGameObject(name, parent.gameObject, true, true);
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
        var o = getGameObject(name, go);
        if (o)
            return o.TryGetComponent(out component);

        component = null;
        return false;
    }
}