using System.Collections.Generic;
using static UnityUtility;
using static CSharpUtility;
using static FrameBaseUtility;

// 负责窗口对象池,效率稍微高一些,但是功能会比普通的WindowStructPool少一点,UsedList是无序的
public class WindowStructPoolUnOrder<T> : WindowStructPoolBase where T : WindowObjectBase, IRecyclable
{
    protected Queue<T> unused = new(); // 未使用列表
    protected HashSet<T> usings = new(); // 正在使用的列表

    public WindowStructPoolUnOrder(IWindowObjectOwner parent) : base(parent)
    {
    }

    public override void destroy()
    {
        base.destroy();
        unuseAll();
        foreach (T item in unused)
            item.destroy();

        unused.Clear();
    }

    public void init(bool newItemToLast = true)
    {
        init(template.getParent(), typeof(T), newItemToLast);
    }

    public void init(myUGUIObject parent, bool newItemToLast = true)
    {
        init(parent, typeof(T), newItemToLast);
    }

    public HashSet<T> getUsedList()
    {
        return usings;
    }

    public void checkCapacity(int capacity)
    {
        int needCount = capacity - usings.Count;
        for (int i = 0; i < needCount; ++i)
            newItem();
    }

    public void newItem(int count)
    {
        for (int i = 0; i < count; ++i)
            newItem(parent);
    }

    public T newItem(out T item)
    {
        return item = newItem(parent);
    }

    public T newItem()
    {
        return newItem(parent);
    }

    // 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.refreshUIDepth()来刷新深度
    public T newItem(myUGUIObject _parent)
    {
        if (!initialized)
        {
            logError("还未执行初始化,不能newItem");
            return null;
        }

        T item;
        if (unused.Count > 0)
        {
            item = unused.Dequeue();
            item.setParent(_parent, false);
        }
        else
        {
            item = createInstance<T>(objectType, script);
            item.assignWindow(_parent, template, isEditor() ? namePrefix + makeID() : namePrefix);
            item.init();
        }

        item.setAssignID(++assignIDSeed);
        item.reset();
        item.setActive(true);
        if (newItemMoveToLast)
        {
            item.setAsLastSibling(false);
        }

        usings.Add(item);
        return item;
    }

    public override void unuseAll()
    {
        foreach (T item in usings)
        {
            item.recycle();
            if (item.isActive())
                item.setActive(false);

            unused.Enqueue(item);
        }

        usings.Clear();
    }

    public bool unuseItem(T item, bool showError = true)
    {
        if (item == null)
            return false;

        if (item.GetType() != objectType)
        {
            logError("物体类型与池类型不一致,无法回收,物体类型:" + item.GetType() + ",池类型:" + objectType);
            return false;
        }

        if (item.getAssignID() <= 0)
        {
            logError("物体已经回收过了,无法重复回收,type:" + item.GetType());
            return false;
        }

        if (!usings.Remove(item))
        {
            if (showError)
                logError("此窗口物体不属于当前窗口物体池,无法回收,type:" + item.GetType());

            return false;
        }

        item.recycle();
        if (item.isActive())
            item.setActive(false);

        unused.Enqueue(item);
        return true;
    }
}