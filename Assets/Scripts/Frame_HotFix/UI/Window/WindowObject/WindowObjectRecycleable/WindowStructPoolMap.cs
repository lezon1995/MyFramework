using System.Collections.Generic;
using static UnityUtility;
using static CSharpUtility;
using static FrameBaseUtility;

// 可通过Key索引的复杂窗口对象池
public class WindowStructPoolMap<Key, T> : WindowStructPoolBase where T : WindowObjectBase, IRecyclable
{
    protected Stack<T> unused = new(); // 未使用列表
    protected Dictionary<Key, T> usings = new(); // 正在使用的列表

    public WindowStructPoolMap(IWindowObjectOwner parent) : base(parent)
    {
    }

    public override void destroy()
    {
        base.destroy();
        unuseAll();
        foreach (T item in unused)
        {
            item.destroy();
        }

        unused.Clear();
    }

    public void init()
    {
        init(template.getParent(), typeof(T), true);
    }

    public void init(myUGUIObject parent)
    {
        init(parent, typeof(T), true);
    }

    public void init(bool newItemToLast)
    {
        init(template.getParent(), typeof(T), newItemToLast);
    }

    public void init1(myUGUIObject parent, bool newItemToLast)
    {
        init(parent, typeof(T), newItemToLast);
    }

    public bool hasKey(Key key)
    {
        return usings.ContainsKey(key);
    }

    public T getItem(Key key)
    {
        return usings.get(key);
    }

    public Dictionary<Key, T> getUsedList()
    {
        return usings;
    }

    public T newItem(Key key)
    {
        return newItem(parent, key);
    }

    // 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.refreshUIDepth()来刷新深度
    public T newItem(myUGUIObject parent, Key key)
    {
        if (!initialized)
        {
            logError("还未执行初始化,不能newItem");
            return null;
        }

        T item;
        if (unused.Count > 0)
        {
            item = unused.Pop();
            item.setParent(parent, false);
        }
        else
        {
            item = createInstance<T>(objectType, script);
            item.assignWindow(parent, template, isEditor() ? namePrefix + makeID() : namePrefix);
            item.init();
        }

        item.setAssignID(++assignIDSeed);
        item.reset();
        item.setActive(true);
        if (newItemMoveToLast)
        {
            item.setAsLastSibling(false);
        }

        usings.Add(key, item);
        return item;
    }

    public override void unuseAll()
    {
        foreach (T item in usings.Values)
        {
            item.recycle();
            if (item.isActive())
            {
                item.setActive(false);
            }

            unused.Push(item);
        }

        usings.Clear();
    }

    public bool unuseItem(Key key, bool showError = true)
    {
        if (key == null)
        {
            return false;
        }

        if (!usings.Remove(key, out T item))
        {
            if (showError)
            {
                logError("此窗口物体不属于当前窗口物体池,无法回收,key:" + key + ", type:" + typeof(T));
            }

            return false;
        }

        item.recycle();
        if (item.isActive())
        {
            item.setActive(false);
        }

        unused.Push(item);
        return true;
    }
}