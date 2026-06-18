using System;
using UnityEngine;

public class UIObject : IDisposable
{
    protected myUGUIObject obj;
    protected GameObject gameObject;
    protected RectTransform transform;

    public UIObject(myUGUIObject t)
    {
        obj = t;
        gameObject = t.getGameObject();
        transform = t.getRectTransform();
    }

    protected void find<T>(string name, out T result) where T : Component
    {
        gameObject.find(out result, name);
    }

    public virtual void update(float elapsedTime)
    {
    }

    public void setActive(bool active) => gameObject?.SetActive(active);

    public virtual void Dispose()
    {
        obj = null;
        gameObject = null;
        transform = null;
    }
}

public class UIItem : ClassObject, IArgs<myUGUIObject>
{
    protected myUGUIObject obj;
    protected GameObject gameObject;
    protected RectTransform transform;

    public virtual void onCreate(myUGUIObject t)
    {
        obj = t;
        gameObject = t.getGameObject();
        transform = t.getRectTransform();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        obj = null;
        gameObject = null;
        transform = null;
    }

    protected void find<T>(string name, out T result) where T : Component
    {
        gameObject.find(out result, name);
    }

    public virtual void update(float elapsedTime)
    {
    }

    public void setActive(bool active) => gameObject?.SetActive(active);
}