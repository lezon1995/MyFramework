using System;
using System.Collections.Generic;

// 安全哈希集的只读遍历辅助,搭配SafeHashSet使用,using释放
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