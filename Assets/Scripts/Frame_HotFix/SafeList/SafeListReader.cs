using System;
using System.Collections.Generic;

public struct SafeListReader<T> : IDisposable
{
    SafeList<T> mSafeList;
    public List<T> mReadList;

    public SafeListReader(SafeList<T> list)
    {
        mSafeList = list;
        mReadList = list.startForeach();
    }

    public void Dispose()
    {
        mSafeList.endForeach();
    }
}