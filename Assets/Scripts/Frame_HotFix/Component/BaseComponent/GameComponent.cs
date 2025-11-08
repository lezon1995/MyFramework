using System;

// 组件基类,只是使用了MonoBehaviour的组件思想
public abstract class GameComponent : ClassObject
{
    protected ComponentOwner owner; // 该组件的拥有者
    protected bool ignoreTimeScale; // 更新时是否忽略时间缩放
    protected bool defaultActive; // 默认的启用状态
    protected bool active = true; // 是否激活组件

    protected GameComponent()
    {
    }

    public virtual void init(ComponentOwner owner)
    {
        this.owner = owner;
    }

    public virtual void update(float dt)
    {
    }

    public virtual void fixedUpdate(float dt)
    {
    }

    public virtual void lateUpdate(float dt)
    {
    }

    public override void destroy()
    {
        base.destroy();
        owner = null;
    }

    public bool isActive()
    {
        return active;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        owner = null;
        ignoreTimeScale = false;
        defaultActive = false;
        active = true;
    }

    public virtual void setActive(bool active)
    {
        this.active = active;
        if (this.active)
        {
            owner.notifyComponentStart(this);
        }
    }

    public void setDefaultActive(bool active)
    {
        defaultActive = active;
    }

    public virtual void setIgnoreTimeScale(bool ignore)
    {
        ignoreTimeScale = ignore;
    }

    // 获得成员变量
    public ComponentOwner getOwner()
    {
        return owner;
    }

    public bool isComponentActive()
    {
        return active;
    }

    public bool isIgnoreTimeScale()
    {
        return ignoreTimeScale;
    }

    public bool isDefaultActive()
    {
        return defaultActive;
    }

    public Type getType()
    {
        return GetType();
    }

    // 通知
    public virtual void notifyOwnerActive(bool active)
    {
    }
}