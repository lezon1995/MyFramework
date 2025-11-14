using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseHotFix;

public abstract class WindowObjectBase : ILocalizationCollection, IWindowObjectOwner
{
    protected WindowObjectBase parent; // 因为会有一些WindowObject中嵌套WindowObject,所以需要存储一个父节点
    protected List<WindowObjectBase> childList; // 当前WindowObject的所有子节点
    protected List<IDragViewLoop> dragViewLoopList; // 当前WindowObject拥有的DragViewLoop列表,用于调用列表的update
    protected List<WindowStructPoolBase> poolList; // 此物体创建的对象池列表
    protected HashSet<IUGUIObject> localizationObjectList; // 需要本地化的文本对象
    protected LayoutScript script; // 所属的布局脚本
    protected bool destroyed; // 是否已经销毁过了,用于检测重复销毁的
    protected bool initialized; // 是否已经初始化过了,用于检测重复初始化
    protected bool calledOnHide; // 是否已经调用过了onHide
    protected bool needUpdate; // 是否需要调用此对象的update,默认不调用update

    protected WindowObjectBase(IWindowObjectOwner _parent)
    {
        switch (_parent)
        {
            case WindowObjectBase objBase:
                script = objBase.script;
                parent = objBase;
                break;
            case LayoutScript _script:
                script = _script;
                parent = null;
                break;
        }

        parent?.addChild(this);
    }

    // 第一次创建时调用,如果是对象池中的物体,则由对象池调用,非对象池物体需要在使用的地方自己调用
    public virtual void init()
    {
        if (initialized)
        {
            logError("已经初始化过了,type:" + GetType());
            return;
        }

        initialized = true;
        destroyed = false;
        foreach (WindowObjectBase item in childList.safe())
        {
            item.init();
        }
    }

    // 每次被分配使用时调用,如果是对象池中的物体,则由对象池调用,非对象池物体需要在使用的地方自己调用
    public virtual void reset()
    {
        calledOnHide = false;
        foreach (WindowObjectBase item in childList.safe())
        {
            item.reset();
        }
    }

    public bool hasDragViewLoopList()
    {
        return dragViewLoopList.count() > 0;
    }

    public void updateDragViewLoop()
    {
        // 更新自己的滚动列表
        foreach (IDragViewLoop item in dragViewLoopList.safe())
        {
            if (item.isActive())
            {
                item.updateDragView();
            }
        }

        // 更新所有子节点的滚动列表
        foreach (WindowObjectBase item in childList.safe())
        {
            if (item.isActive() && item.hasDragViewLoopList())
            {
                item.updateDragViewLoop();
            }
        }
    }

    // 这个update需要主动调用,界面管理器不会自动去调用这个update,因为对效率影响比较大
    public virtual void update()
    {
    }

    // 被隐藏时调用,界面被隐藏时也会调用所有子窗口的onHide
    public virtual void onHide()
    {
        if (calledOnHide)
        {
            logError("已经调用过onHide了,type:" + GetType() + ", hash:" + GetHashCode());
            return;
        }

        calledOnHide = true;
        foreach (WindowObjectBase item in childList.safe())
        {
            if (item.isActive())
            {
                item.onHide();
            }
        }

        foreach (WindowStructPoolBase item in poolList.safe())
        {
            item.unuseAll();
        }
    }

    public virtual void destroy()
    {
        if (destroyed)
        {
            logWarning("WindowObject重复销毁对象:" + GetType() + ",hash:" + GetHashCode());
        }

        destroyed = true;

        foreach (WindowObjectBase item in childList.safe())
        {
            item.destroy();
        }

        mLocalizationManager?.unregisterLocalization(localizationObjectList);
        localizationObjectList?.Clear();
    }

    public void addLocalizationObject(IUGUIObject obj)
    {
        localizationObjectList ??= new();
        localizationObjectList.Add(obj);
    }

    public virtual bool isActive()
    {
        return false;
    }

    public virtual void setActive(bool active)
    {
        if (active)
        {
            calledOnHide = false;
            // 需要标记所有子节点也允许再调用onHide
            foreach (WindowObjectBase item in childList.safe())
            {
                item.calledOnHide = false;
            }
        }
        else if (isActive())
        {
            onHide();
        }
    }

    public virtual void setParent(myUGUIObject parent, bool refreshDepth = true)
    {
    }

    public virtual void setAsLastSibling(bool refreshDepth = true)
    {
    }

    public virtual void setAsFirstSibling(bool refreshDepth = true)
    {
    }

    public bool isRootWindowObject()
    {
        return parent == null;
    }

    public void addWindowPool(WindowStructPoolBase pool)
    {
        poolList ??= new();
        if (!poolList.addUnique(pool))
        {
            logError("重复加入对象池");
        }
    }

    public LayoutScript getScript()
    {
        return script;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected T0 newObject<T0>(out T0 obj, myUGUIObject parent, string name, bool showError) where T0 : myUGUIObject, new()
    {
        return script.newObject(out obj, parent, name, showError);
    }

    protected T0 newObject<T0>(out T0 obj, myUGUIObject parent, string name) where T0 : myUGUIObject, new()
    {
        return script.newObject(out obj, parent, name, true);
    }

    protected void addChild(WindowObjectBase child)
    {
        childList ??= new();
        if (!childList.addUnique(child))
        {
            logError("重复加入子节点");
        }

        if (child is IDragViewLoop dragViewLoop)
        {
            dragViewLoopList ??= new();
            dragViewLoopList.Add(dragViewLoop);
        }
    }

    // 由于如果让应用层子类都去重写多个assignWindow就会显得很繁琐,而且会有重复代码
    // 所以应用层子类只需要重写assignWindowInternal,在这里写逻辑即可,然后会在assignWindow中调用assignWindowInternal
    protected virtual void assignWindowInternal()
    {
    }
}