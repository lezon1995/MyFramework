using System;
using System.Collections.Generic;

// 安全哈希集的只读遍历辅助,搭配SafeHashSet使用,using释放
public struct SafeHashSetReader<T> : IDisposable
{
	private SafeHashSet<T> mSafeList;
	public HashSet<T> mReadList;
	public SafeHashSetReader(SafeHashSet<T> list)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
	}
	public SafeHashSetReader(SafeHashSet<T> list, out HashSet<T> reader)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
		reader = mReadList;
	}
	public void Dispose()
	{
		mSafeList.endForeach();
	}
}