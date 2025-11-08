using System;
using System.Collections.Generic;

// 安全列表的只读遍历辅助,搭配SafeList使用,using释放
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