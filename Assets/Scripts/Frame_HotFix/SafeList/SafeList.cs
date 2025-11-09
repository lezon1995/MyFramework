using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseUtility;


// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeList<T> : ClassObject
{
    // 因为即使继承了IEquatable,也会因为本身是带T模板的,无法在重写的Equals中完全避免装箱和拆箱,所以不继承IEquatable
    // 而且实际使用时也不会调用此类型的比较函数
    protected struct Modify
    {
        public T value;
        public bool isAdd;
        public int removeIndex;

        public Modify(T v, bool add, int idx)
        {
            value = v;
            isAdd = add;
            removeIndex = idx;
        }
    }

    protected List<Modify> modifies = new(); // 记录操作的列表,按顺序存储所有的操作
    protected List<T> updates = new(); // 用于遍历更新的列表
    protected List<T> list = new(); // 用于存储实时数据的列表
    protected string lastFileName; // 上一次开始遍历时的文件名
    protected bool isIterating; // 当前是否正在遍历中

    public override void resetProperty()
    {
        base.resetProperty();
        modifies.Clear();
        updates.Clear();
        list.Clear();
        lastFileName = null;
        isIterating = false;
    }

    // 获取用于更新的列表,会自动从主列表同步,遍历结束时需要调用endForeach
    // 搭配SafeListScope使用,using var a = new SafeListScope<T>(safeList);然后遍历a.mReadList
    public List<T> startForeach(string fileName = null)
    {
        if (isIterating)
        {
            logError("当前列表正在遍历中,无法再次开始遍历, 上一次开始遍历的地方:" + (lastFileName ?? string.Empty) + ", 当前遍历的地方:" + fileName);
            return null;
        }

        lastFileName = fileName;
        isIterating = true;

        // 获取更新列表前,先同步主列表到更新列表,为了避免当列表过大时每次同步量太大
        // 所以单独使用了添加列表和移除列表,用来存储主列表的添加和移除的元素
        int count = list.Count;
        if (count == 0)
        {
            // 主列表为空,则直接清空即可
            updates.Clear();
        }
        else
        {
            // 操作记录较少,则根据操作进行增删
            if (modifies.Count < count)
            {
                for (var i = 0; i < modifies.Count; i++)
                {
                    var m = modifies[i];
                    if (m.isAdd)
                    {
                        updates.Add(m.value);
                    }
                    else
                    {
                        if (isEditor() && !EqualityComparer<T>.Default.Equals(m.value, updates[m.removeIndex]))
                        {
                            logError("同步列表数据错误");
                        }

                        updates.RemoveAt(m.removeIndex);
                    }
                }
            }
            // 主列表元素较少,则直接同步主列表到更新列表
            else
            {
                updates.setRange(list);
            }
        }

        if (updates.Count != list.Count)
        {
            logError("同步失败");
        }

        modifies.Clear();
        return updates;
    }

    public void endForeach()
    {
        isIterating = false;
    }

    public bool isForeaching()
    {
        return isIterating;
    }

    // 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
    // 如果确保在遍历过程中不会对列表进行修改,则可以使用MainList
    // 如果可能会对列表进行修改,则应该使用startForeach
    public List<T> getMainList()
    {
        return list;
    }

    public bool contains(T value) => list.Contains(value);
    public T get(int index) => list[index];
    public int count() => list.Count;
    public T find(Predicate<T> predicate) => list.Find(predicate);

    // 因为只能保证开始遍历时mUpdateList与mMainList一致
    // 但是遍历结束后两个列表可能就不一致了
    // 所以即使没有正在遍历时,也只能是记录操作,而不是直接修改mUpdateList
    public T add(T value)
    {
        list.Add(value);
        modifies.Add(new(value, true, -1));
        return value;
    }

    public void addUnique(T value)
    {
        if (contains(value))
            return;

        add(value);
    }

    public void addNotNull(T value)
    {
        if (value == null)
            return;

        add(value);
    }

    public void addRange(IList<T> _list)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            var item = _list[i];
            list.Add(item);
            modifies.Add(new(item, true, -1));
        }
    }

    public void setRange(IList<T> _list)
    {
        clear();
        addRange(_list);
    }

    public bool remove(T item)
    {
        int index = list.IndexOf(item);
        if (index < 0 || index >= list.Count)
            return false;

        list.RemoveAt(index);
        modifies.Add(new(item, false, index));
        return true;
    }

    public void removeAt(int index)
    {
        if (index < 0 || index >= list.Count)
            return;

        var item = list.removeAt(index);
        modifies.Add(new(item, false, index));
    }

    // 清空所有数据
    public void clear()
    {
        if (isIterating)
        {
            int count = list.Count;
            for (int i = 0; i < count; ++i)
            {
                modifies.Add(new(list[i], false, i));
            }
        }
        else
        {
            modifies.Clear();
            updates.Clear();
        }

        list.Clear();
    }

    public SafeListReader<T> safeReader(out List<T> reader)
    {
        return new(this, out reader);
    }
}

public static class SafeListExtension
{
    public static T0 addClass<T0>(this SafeList<T0> list) where T0 : ClassObject, new()
    {
        CLASS(out T0 value);
        list.add(value);
        return value;
    }
}