using System.Collections.Generic;
using static FrameBaseUtility;

public static class DictionaryExtension
{
    public static void set<K, V>(this IDictionary<K, V> dic, K key, V value)
    {
        if (isEditor() && !dic.ContainsKey(key))
        {
            logErrorBase("字典中不包含此key,无法set");
        }

        dic[key] = value;
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

    public static V getOrAddNew<K, V>(this IDictionary<K, V> map, K key) where V : new()
    {
        if (!map.TryGetValue(key, out V value))
        {
            value = new();
            map.Add(key, value);
        }

        return value;
    }

    public static V addNotNullKey<K, V>(this IDictionary<K, V> map, K key, V value)
    {
        if (key == null)
            return default;

        map.Add(key, value);
        return value;
    }

    public static void setRange<K, V>(this IDictionary<K, V> map, IDictionary<K, V> other)
    {
        map.Clear();
        if (other == null)
            return;

        foreach (var item in other)
        {
            map.Add(item.Key, item.Value);
        }
    }

    public static bool isEmpty<K, V>(this IDictionary<K, V> list)
    {
        return list == null || list.Count == 0;
    }
}