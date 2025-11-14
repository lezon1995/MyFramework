using System.Collections.Generic;
using static UnityUtility;
using static CSharpUtility;
using static MathUtility;
using static FrameBaseUtility;

// 负责窗口对象池,UsedList是有序的
public class WindowStructPool<T> : WindowStructPoolBase where T : WindowObjectBase, IRecyclable
{
    protected HashSet<T> unused = new(); // 未使用列表
    protected List<T> usings = new(); // 正在使用的列表

    public WindowStructPool(IWindowObjectOwner parent) : base(parent)
    {
    }

    public override void destroy()
    {
        base.destroy();
        unuseAll();
        foreach (var item in unused)
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

    public List<T> getUsedList()
    {
        return usings;
    }

    public bool isUsed(T item)
    {
        return usings.Contains(item);
    }

    public void checkCapacity(int capacity)
    {
        int needCount = capacity - usings.Count;
        for (int i = 0; i < needCount; ++i)
        {
            newItem();
        }
    }

    // 将source从sourcePool中移动到当前池中,inUsed表示移动到当前池以后是处于正在使用的状态还是未使用状态
    public void moveItem(WindowStructPool<T> sourcePool, T source, bool inUsed, bool moveParent = true)
    {
        // 从原来的池中移除
        sourcePool.usings.Remove(source);
        sourcePool.unused.Remove(source);
        // 加入新的池中
        if (inUsed)
            usings.Add(source);
        else
            unused.Add(source);

        if (moveParent)
        {
            source.setParent(parent);
        }

        // 检查分配ID种子,确保后面池中的已分配ID一定小于分配ID种子
        assignIDSeed = getMax(source.getAssignID(), assignIDSeed);
    }

    public void newItem(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            newItem(parent);
        }
    }

    public T newItem(out T item)
    {
        return item = newItem(parent);
    }

    // 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.refreshUIDepth()来刷新深度
    public T newItem(myUGUIObject _parent = null)
    {
        if (!initialized)
        {
            logError("还未执行初始化,不能newItem");
            return null;
        }

        _parent ??= parent;
        T item;
        if (unused.Count > 0)
        {
            item = unused.popFirst();
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
        if (usings.Count == 0)
            return;

        foreach (T item in usings)
        {
            item.recycle();
            if (item.isActive())
                item.setActive(false);

            unused.Add(item);
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

        unused.Add(item);
        return true;
    }

    public void unuseIndex(int index)
    {
        unuseRange(index, 1);
    }

    // 回收一定下标范围的对象,count小于0表示回收从startIndex到结尾的所有对象
    public void unuseRange(int startIndex, int count = -1)
    {
        int usedCount = usings.Count;
        if (count < 0)
        {
            count = usedCount - startIndex;
        }
        else
        {
            clampMax(ref count, usedCount - startIndex);
        }

        for (int i = 0; i < count; ++i)
        {
            T item = usings[startIndex + i];
            item.recycle();
            if (item.isActive())
                item.setActive(false);

            unused.Add(item);
        }

        usings.RemoveRange(startIndex, count);
    }
}