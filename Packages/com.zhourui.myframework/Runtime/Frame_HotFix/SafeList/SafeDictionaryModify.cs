// 安全字典的修改信息,修改的数据,以及修改操作类型
public struct SafeDictionaryModify<K, V>
{
    public V Value; // 数据的Value
    public K Key; // 数据的Key
    public bool IsAdd; // 是否为添加操作

    public SafeDictionaryModify(K key)
    {
        Key = key;
        Value = default;
        IsAdd = false;
    }

    public SafeDictionaryModify(K key, V value)
    {
        Key = key;
        Value = value;
        IsAdd = true;
    }
}