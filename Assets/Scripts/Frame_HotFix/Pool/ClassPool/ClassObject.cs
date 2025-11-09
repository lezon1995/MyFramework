using System;

// 可使用对象池进行创建和销毁的对象
public class ClassObject : IEquatable<ClassObject>, IEventListener, IResetProperty
{
    protected static long instanceIDSeed; // 对象实例ID的种子
    public long instanceId { get; }
    public long id { get; private set; } // 重新分配时的ID,每次分配都会设置一个新的唯一执行ID
    protected bool destroyed; // 当前对象是否已经被回收
    protected bool destroying; // 当前对象是否正在回收中

    protected ClassObject()
    {
        destroyed = true;
        instanceId = ++instanceIDSeed;
    }

    public virtual void resetProperty()
    {
        id = 0;
        destroyed = true;
        destroying = false;
    }

    public virtual void setDestroy(bool isDestroy)
    {
        destroyed = isDestroy;
    }

    public virtual void destroy()
    {
    }

    public void setAssignID(long assignID)
    {
        id = assignID;
    }

    public void setPendingDestroy(bool pending)
    {
        destroying = pending;
    }

    public bool isDestroy()
    {
        return destroyed;
    }

    public bool Equals(ClassObject obj)
    {
        return instanceId == obj.instanceId;
    }

    public bool isPendingDestroy()
    {
        return destroying;
    }

    public static implicit operator bool(ClassObject obj) => obj != null;
}