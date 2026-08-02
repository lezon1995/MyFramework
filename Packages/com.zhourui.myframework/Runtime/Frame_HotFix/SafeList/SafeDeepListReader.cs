using System;
using System.Collections.Generic;

// 深度安全列表的只读遍历辅助,搭配SafeDeepList使用,using释放
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