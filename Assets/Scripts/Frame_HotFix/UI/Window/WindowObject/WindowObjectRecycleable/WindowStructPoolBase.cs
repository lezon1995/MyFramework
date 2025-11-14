using System;

// 窗口对象池基类
public abstract class WindowStructPoolBase
{
    protected WindowObjectBase ownerObject; // 如果是WindowObject中创建的对象池,则会存储此WindowObject
    protected LayoutScript script; // 所属的布局脚本
    protected myUGUIObject parent; // 创建节点时默认的父节点
    protected myUGUIObject template; // 创建节点时使用的模板
    protected string namePrefix; // 创建物体的名字前缀
    protected Type objectType; // 物体类型
    protected static long assignIDSeed; // 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
    protected bool newItemMoveToLast; // 新创建的物体是否需要放到父节点的最后,也就是是否在意其渲染顺序
    protected bool initialized; // 是否已经初始化

    protected WindowStructPoolBase(IWindowObjectOwner parent)
    {
        switch (parent)
        {
            case WindowObjectBase objBase:
                script = objBase.getScript();
                ownerObject = objBase;
                break;
            case LayoutScript _script:
                script = _script;
                ownerObject = null;
                break;
        }

        ownerObject?.addWindowPool(this);
        script.addWindowStructPool(this);
        newItemMoveToLast = true;
    }

    public virtual void destroy()
    {
    }

    public void init(myUGUIObject _parent, Type _objectType, bool newItemToLast = true)
    {
        parent = _parent;
        newItemMoveToLast = newItemToLast;
        namePrefix = template?.getName();
        objectType = _objectType;
        template?.setActive(false);
        initialized = true;
    }

    public void assignTemplate(myUGUIObject parent, string name)
    {
        script.newObject(out myUGUIObject obj, parent, name);
        template = obj;
    }

    public void assignTemplate<T>(myUGUIObject parent, string name) where T : myUGUIObject, new()
    {
        script.newObject(out T obj, parent, name);
        template = obj;
    }

    public void assignTemplate(string name)
    {
        script.newObject(out myUGUIObject obj, name);
        template = obj;
    }

    public void assignTemplate(myUGUIObject _template)
    {
        template = _template;
    }

    public myUGUIObject getInUseParent()
    {
        return parent;
    }

    public myUGUIObject getTemplate()
    {
        return template;
    }

    public void setItemPreName(string preName)
    {
        namePrefix = preName;
    }

    public virtual void unuseAll()
    {
    }

    public bool isRootPool()
    {
        return ownerObject == null;
    }
}