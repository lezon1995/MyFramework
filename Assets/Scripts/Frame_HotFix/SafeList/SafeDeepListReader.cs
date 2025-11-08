using System;
using System.Collections.Generic;

public struct SafeDeepListReader<T> : IDisposable
{
    private SafeDeepList<T> safeList;
    public List<T> mReadList;

    public SafeDeepListReader(SafeDeepList<T> list)
    {
        safeList = list;
        mReadList = list.startForeach();
    }

    public void Dispose()
    {
        safeList.endForeach(mReadList);
    }
}