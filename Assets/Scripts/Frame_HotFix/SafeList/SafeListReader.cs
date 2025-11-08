using System;
using System.Collections.Generic;

public struct SafeListReader<T> : IDisposable
{
    SafeList<T> safeList;
    public List<T> mReadList;

    public SafeListReader(SafeList<T> list)
    {
        safeList = list;
        mReadList = list.startForeach();
    }

    public SafeListReader(SafeList<T> list, out List<T> readList)
    {
        safeList = list;
        mReadList = list.startForeach();
        readList = mReadList;
    }

    public void Dispose()
    {
        safeList.endForeach();
    }
}