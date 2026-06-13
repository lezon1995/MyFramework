using System;
using UnityEngine;

public class UIObject : IDisposable
{
    protected myUGUIObject obj;
    protected GameObject gameObject;

    public UIObject(myUGUIObject t)
    {
        obj = t;
        gameObject = t.getGameObject();
    }

    protected void find<T>(string name, out T result) where T : Component
    {
        gameObject.find(out result, name);
    }

    public virtual void update(float elapsedTime)
    {
    }

    public virtual void Dispose()
    {
        obj = null;
        gameObject = null;
    }
}