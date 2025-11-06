using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
// 由于在增删操作时可能会出现key相同但是value不相同的情况,甚至value的GetHashCode被重写导致即使GetHashCode相同,实例也不同的情况
// 所以不再进行增量同步,而是按顺序记录所有的操作,使之完全与主列表同步
public class SafeDictionary<Key, Value> : ClassObject
{
    protected List<SafeDictionaryModify<Key, Value>> modifyList = new(); // 记录操作的列表
    protected Dictionary<Key, Value> updateDict = new(); // 用于遍历更新的列表
    protected Dictionary<Key, Value> mainDict = new(); // 用于存储实时数据的列表
    protected string lastFileName; // 上一次开始遍历时的文件名
    protected bool isIterating; // 当前是否正在遍历中

    public override void resetProperty()
    {
        base.resetProperty();
        modifyList.Clear();
        updateDict.Clear();
        mainDict.Clear();
        lastFileName = null;
        isIterating = false;
    }

    // 获取用于更新的列表,会自动从主列表同步,遍历结束时需要调用endForeach,一般使用SafeDictionaryReader来安全遍历
    public Dictionary<Key, Value> startForeach(string fileName = null)
    {
        if (isIterating)
        {
            logError("当前列表正在遍历中,无法再次开始遍历, 上一次开始遍历的地方:" + (lastFileName ?? "") + ", 当前遍历的地方:" + fileName);
            return null;
        }

        lastFileName = fileName;
        isIterating = true;

        // 获取更新列表前,先同步主列表到更新列表,为了避免当列表过大时每次同步量太大
        // 所以单独使用了添加列表和移除列表,用来存储主列表的添加和移除的元素
        int mainCount = mainDict.Count;
        // 如果主列表为空,则直接清空即可
        if (mainCount == 0)
        {
            updateDict.Clear();
        }
        else
        {
            // 操作记录较少,则根据操作进行增删
            if (modifyList.Count < mainCount)
            {
                foreach (var modify in modifyList)
                {
                    if (modify.IsAdd)
                    {
                        updateDict.Add(modify.Key, modify.Value);
                    }
                    else
                    {
                        updateDict.Remove(modify.Key);
                    }
                }
            }
            // 主列表元素较少,则直接同步主列表到更新列表
            else
            {
                updateDict.setRange(mainDict);
            }
        }

        if (updateDict.Count != mainDict.Count)
        {
            logError("同步失败");
        }

        modifyList.Clear();
        return updateDict;
    }

    public void endForeach()
    {
        isIterating = false;
    }

    // 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
    // 如果确保在遍历过程中不会对列表进行修改,则可以使用MainList
    // 如果可能会对列表进行修改,则应该使用startForeach
    public Dictionary<Key, Value> getMainList()
    {
        return mainDict;
    }

    public bool tryGetValue(Key key, out Value value)
    {
        return mainDict.TryGetValue(key, out value);
    }

    public Value get(Key key)
    {
        return mainDict.get(key);
    }

    public bool containsKey(Key key)
    {
        return mainDict.ContainsKey(key);
    }

    public bool containsValue(Value value)
    {
        return mainDict.ContainsValue(value);
    }

    public int count()
    {
        return mainDict.Count;
    }

    // 因为只能保证开始遍历时mUpdateList与mMainList一致,但是遍历结束后两个列表可能就不一致了,所以即使没有正在遍历时,也只能是记录操作,而不是直接修改mUpdateList
    public bool add(Key key, Value value)
    {
        if (!mainDict.TryAdd(key, value))
        {
            return false;
        }

        modifyList.Add(new(key, value));
        return true;
    }

    public bool remove(Key key)
    {
        if (!mainDict.Remove(key))
        {
            return false;
        }

        modifyList.Add(new(key));
        return true;
    }

    // 清空所有数据
    public void clear()
    {
        if (isIterating)
        {
            foreach (var key in mainDict.Keys)
            {
                modifyList.Add(new(key));
            }
        }
        else
        {
            modifyList.Clear();
            updateDict.Clear();
        }

        mainDict.Clear();
    }
}

public static class SafeDictionaryExtension
{
    public static T0 addClass<T0, Key>(this SafeDictionary<Key, T0> list, Key key) where T0 : ClassObject, new()
    {
        CLASS(out T0 value);
        list.add(key, value);
        return value;
    }
}