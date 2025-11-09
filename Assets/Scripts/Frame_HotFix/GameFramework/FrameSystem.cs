using System;
using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;
using static MathUtility;

// 系统组件基类,一般都是管理器
public class FrameSystem : ComponentOwner
{
    protected GameObject go; // 管理器节点,一般用于作为管理物体的父节点,或者挂接调试脚本
    protected int orderDestroy; // 销毁顺序
    protected int orderUpdate; // 更新顺序
    protected int orderInit; // 初始化顺序
    protected bool mCreateObject; // 是否要创建管理器节点,默认不创建,为了避免在场景结构中显示过多不必要的系统组件节点

    protected FrameSystem()
    {
        // 由于一般FrameSystem不会使用对象池来管理,所以构造时就设置当前对象为有效
        destroyed = false;
    }

    public virtual void preInitAsync(Action callback)
    {
        callback?.Invoke();
    }

    public virtual void initAsync(Action callback)
    {
        callback?.Invoke();
    }

    public virtual void init()
    {
        if (mCreateObject)
        {
            go = createGameObject(GetType().ToString(), GameEntry.getInstanceObject());
        }

        initComponents();
    }

    // 等待所有系统组件的init调用完毕后会调用lateInit,如果在init中会有依赖于其他系统组件的初始化,则可以写在lateInit中
    public virtual void lateInit()
    {
    }

    // 即将销毁时调用,退出程序时会先调用一次全部系统的即将销毁,再全部调用一次销毁
    public virtual void willDestroy()
    {
    }

    public override void destroy()
    {
        destroyUnityObject(go);
        go = null;
        destroyed = true;
        base.destroy();
    }

    // 资源更新完毕时调用
    public virtual void resourceAvailable()
    {
    }

    public void setInitOrder(int order)
    {
        orderInit = order;
    }

    public void setUpdateOrder(int order)
    {
        orderUpdate = order;
    }

    public void setDestroyOrder(int order)
    {
        orderDestroy = order;
    }

    public void setCreateObject(bool create)
    {
        mCreateObject = create;
    }

    public GameObject getObject()
    {
        return go;
    }

    public virtual void onDrawGizmos()
    {
    }

    // a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
    public static int compareInit(FrameSystem a, FrameSystem b)
    {
        return sign(a.orderInit - b.orderInit);
    }

    // a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
    public static int compareUpdate(FrameSystem a, FrameSystem b)
    {
        return sign(a.orderUpdate - b.orderUpdate);
    }

    // a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
    public static int compareDestroy(FrameSystem a, FrameSystem b)
    {
        return sign(a.orderDestroy - b.orderDestroy);
    }
}