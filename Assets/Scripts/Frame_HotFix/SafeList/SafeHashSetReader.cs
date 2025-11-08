using System;
using System.Collections.Generic;

public struct SafeHashSetReader<T> : IDisposable
{
    private SafeHashSet<T> safeList;
    public HashSet<T> mReadList;

    public SafeHashSetReader(SafeHashSet<T> list)
    {
        safeList = list;
        mReadList = list.startForeach();
    }

    public void Dispose()
    {
        safeList.endForeach();
    }
}