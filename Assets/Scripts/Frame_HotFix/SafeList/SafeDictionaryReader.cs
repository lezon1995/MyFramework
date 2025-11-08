using System;
using System.Collections.Generic;

public struct SafeDictionaryReader<K, V> : IDisposable
{
    SafeDictionary<K, V> safeList;
    public Dictionary<K, V> mReadList;

    public SafeDictionaryReader(SafeDictionary<K, V> list)
    {
        safeList = list;
        mReadList = list.startForeach();
    }

    public void Dispose()
    {
        safeList.endForeach();
    }
}