using System;
using System.Collections.Generic;

public struct SafeDeepDictionaryReader<K, V> : IDisposable
{
    private SafeDeepDictionary<K, V> safeList;
    public Dictionary<K, V> mList;

    public SafeDeepDictionaryReader(SafeDeepDictionary<K, V> list)
    {
        safeList = list;
        mList = list.startForeach();
    }

    public void Dispose()
    {
        safeList.endForeach(mList);
    }
}