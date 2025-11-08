using System.Collections.Generic;
using System.Linq;
using static FrameBaseUtility;
using static FrameUtility;
using static UnityUtility;

public static class DictionaryExtension
{
    public static IDictionary<K, V> safe<K, V>(this IDictionary<K, V> dic)
    {
        return dic ?? new MyEmptyDictionary<K, V>();
    }

    public static void addOrSet<K, V>(this IDictionary<K, V> dic, K key, V value)
    {
        dic[key] = value;
    }

    public static void set<K, V>(this IDictionary<K, V> dic, K key, V value)
    {
        if (isEditor() && !dic.ContainsKey(key))
        {
            logError("字典中不包含此key,无法set");
        }

        dic[key] = value;
    }

    // 添加或者更新值,并且返回旧的值
    public static V replace<K, V>(this IDictionary<K, V> dic, K key, V value)
    {
        dic.TryGetValue(key, out V curValue);
        dic[key] = value;
        return curValue;
    }

    // 等效于CollectionExtensions.GetValueOrDefault
    public static V get<K, V>(this IDictionary<K, V> map, K key, V defaultValue)
    {
        return map != null && map.TryGetValue(key, out V value) ? value : defaultValue;
    }

    // 等效于CollectionExtensions.GetValueOrDefault
    public static V get<K, V>(this IDictionary<K, V> map, K key)
    {
        if (map == null)
            return default;

        map.TryGetValue(key, out V value);
        return value;
    }

    public static V add<K, V>(this IDictionary<K, V> map, K key, V value)
    {
        map.Add(key, value);
        return value;
    }

    public static void add<K, V>(this IDictionary<K, V> map, KeyValuePair<K, V> pair)
    {
        map.Add(pair.Key, pair.Value);
    }

    public static V addClass<K, V>(this IDictionary<K, V> map, K key) where V : ClassObject, new()
    {
        return map.add(key, CLASS<V>());
    }

    public static V addNotNullKey<K, V>(this IDictionary<K, V> map, K key, V value)
    {
        if (key == null)
        {
            return default;
        }

        map.Add(key, value);
        return value;
    }

    public static V addNotNullValue<K, V>(this IDictionary<K, V> map, K key, V value)
    {
        if (value == null)
        {
            return default;
        }

        map.Add(key, value);
        return value;
    }

    public static void setRange<K, V>(this IDictionary<K, V> map, IDictionary<K, V> other)
    {
        map.Clear();
        if (other == null)
        {
            return;
        }

        foreach (var item in other)
        {
            map.Add(item.Key, item.Value);
        }
    }

    public static void addRange<K, V>(this IDictionary<K, V> map, IDictionary<K, V> other)
    {
        if (other == null)
        {
            return;
        }

        foreach (var item in other)
        {
            map.Add(item.Key, item.Value);
        }
    }

    public static void addOrIncreaseValue<K>(this IDictionary<K, int> dic, K key, int increase)
    {
        if (dic.TryGetValue(key, out int curValue))
        {
            dic[key] = curValue + increase;
        }
        else
        {
            dic.Add(key, increase);
        }
    }

    public static void addOrIncreaseValue<K>(this IDictionary<K, float> dic, K key, float increase)
    {
        if (dic.TryGetValue(key, out float curValue))
        {
            dic[key] = curValue + increase;
        }
        else
        {
            dic.Add(key, increase);
        }
    }

    public static void setAllValue<K, V>(this IDictionary<K, V> map, V value)
    {
        using var a = new ListScope<K>(out var temp, map.Keys);
        foreach (K item in temp)
        {
            map[item] = value;
        }
    }

    public static Dictionary<T0, T1> getOrAddListPersist<K, T0, T1>(this IDictionary<K, Dictionary<T0, T1>> map, K key)
    {
        if (!map.TryGetValue(key, out var value))
        {
            DIC_PERSIST(out value);
            map.Add(key, value);
        }

        return value;
    }

    public static T getOrAddClass<K, T>(this IDictionary<K, T> map, K key) where T : ClassObject, new()
    {
        if (!map.TryGetValue(key, out T value))
        {
            map.Add(key, CLASS(out value));
        }

        return value;
    }

    // 返回值表示是否获取到了列表中已经存在的值
    public static bool getOrAddClass<K, T>(this IDictionary<K, T> map, K key, out T value) where T : ClassObject, new()
    {
        if (!map.TryGetValue(key, out value))
        {
            map.Add(key, CLASS(out value));
            return false;
        }

        return true;
    }

    // 返回值表示是否为列表中已经存在的对象出来的对象
    public static bool getOrAddNew<K, V>(this IDictionary<K, V> map, K key, out V value) where V : new()
    {
        if (!map.TryGetValue(key, out value))
        {
            value = new();
            map.Add(key, value);
            return false;
        }

        return true;
    }

    public static V getOrAddNew<K, V>(this IDictionary<K, V> map, K key) where V : new()
    {
        if (!map.TryGetValue(key, out V value))
        {
            value = new();
            map.Add(key, value);
        }

        return value;
    }

    public static List<T> getOrAddListPersist<K, T>(this IDictionary<K, List<T>> map, K key)
    {
        if (!map.TryGetValue(key, out var value))
        {
            LIST_PERSIST(out value);
            map.Add(key, value);
        }

        return value;
    }

    public static HashSet<T> getOrAddListPersist<K, T>(this IDictionary<K, HashSet<T>> map, K key)
    {
        if (!map.TryGetValue(key, out var value))
        {
            SET_PERSIST(out value);
            map.Add(key, value);
        }

        return value;
    }

    public static void remove<K, T>(this IDictionary<K, T> map, K key0, K key1)
    {
        map.Remove(key0);
        map.Remove(key1);
    }

    public static void remove<K, T>(this IDictionary<K, T> map, K key0, K key1, K key2)
    {
        map.Remove(key0);
        map.Remove(key1);
        map.Remove(key2);
    }

    public static void remove<K, T>(this IDictionary<K, T> map, K key0, K key1, K key2, K key3)
    {
        map.Remove(key0);
        map.Remove(key1);
        map.Remove(key2);
        map.Remove(key3);
    }

    public static void remove<K, T>(this IDictionary<K, T> map, K key0, K key1, K key2, K key3, K key4)
    {
        map.Remove(key0);
        map.Remove(key1);
        map.Remove(key2);
        map.Remove(key3);
        map.Remove(key4);
    }

    public static bool isEmpty<K, V>(this IDictionary<K, V> list)
    {
        return list == null || list.Count == 0;
    }

    public static V firstValue<K, V>(this IDictionary<K, V> list)
    {
        if (list.count() == 0)
        {
            return default;
        }

        return list.First().Value;
    }

    public static K firstKey<K, V>(this IDictionary<K, V> list)
    {
        if (list.count() == 0)
        {
            return default;
        }

        return list.First().Key;
    }
}