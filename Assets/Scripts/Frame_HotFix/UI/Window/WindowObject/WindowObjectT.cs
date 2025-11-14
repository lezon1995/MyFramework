using UnityEngine;
using static UnityUtility;
using static FrameDefine;

// 用于固定数量类,不能用于回收复用窗口
// 通常只是用于已经在预设中创建好的窗口,创建对象时不会创建新的节点,也可以选择克隆到指定父节点下
public class WindowObjectT<T> : WindowObjectBase where T : myUGUIObject, new()
{
    protected T root; // 根节点
    protected bool rootIsFromClone; // 根节点是否是克隆来的
    protected bool changePositionAsInvisible; // 是否使用移动位置来代替隐藏

    public WindowObjectT(IWindowObjectOwner parent) : base(parent)
    {
    }

    // 对象真正从内存中销毁时调用
    public override void destroy()
    {
        base.destroy();
        if (rootIsFromClone && root != null)
        {
            LayoutScript.destroyCloned(root);
            root = null;
        }
    }

    // 在parent下根据template克隆一个物体作为Root,设置名字为name
    public void assignWindow(myUGUIObject parent, myUGUIObject template, string name)
    {
        rootIsFromClone = true;
        script.cloneObject(out root, parent, template, name);
        assignWindowInternal();
    }

    // 使用itemRoot作为Root
    public void assignWindow(myUGUIObject itemRoot)
    {
        root = itemRoot as T;
        assignWindowInternal();
    }

    // 在指定的父节点下获取一个物体,将parent下名字为name的节点作为Root
    public void assignWindow(myUGUIObject parent, string name)
    {
        newObject(out root, parent, name);
        assignWindowInternal();
    }

    public override bool isActive()
    {
        return root?.isActiveInHierarchy() ?? false;
    }

    public override void setActive(bool visible)
    {
        base.setActive(visible);
        if (changePositionAsInvisible)
        {
            if (!visible)
            {
                root.setPosition(FAR_POSITION);
            }
        }
        else
        {
            root.setActive(visible);
        }
    }

    public virtual void setPosition(Vector3 pos)
    {
        root.setPosition(pos);
    }

    public T getRoot()
    {
        return root;
    }

    public Vector3 getPosition()
    {
        return root.getPosition();
    }

    public Vector2 getSize()
    {
        return root.getWindowSize();
    }

    public int getSibling()
    {
        checkRoot();
        return root.getSibling();
    }

    public bool setSibling(int index, bool refreshDepth = true)
    {
        checkRoot();
        return root.setSibling(index, refreshDepth);
    }

    public override void setAsFirstSibling(bool refreshDepth = true)
    {
        checkRoot();
        root.setAsFirstSibling(refreshDepth);
    }

    public override void setAsLastSibling(bool refreshDepth = true)
    {
        checkRoot();
        root.setAsLastSibling(refreshDepth);
    }

    public override void setParent(myUGUIObject parent, bool refreshDepth = true)
    {
        checkRoot();
        root.setParent(parent, refreshDepth);
    }

    public bool isVisible()
    {
        checkRoot();
        return root.isActiveInHierarchy();
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void checkRoot()
    {
        if (root == null)
        {
            logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
        }
    }

    protected T0 newObject<T0>(out T0 obj, string name) where T0 : myUGUIObject, new()
    {
        return newObject(out obj, root, name, true);
    }

    protected T0 newObject<T0>(out T0 obj, string name, bool showError) where T0 : myUGUIObject, new()
    {
        return newObject(out obj, root, name, showError);
    }
}