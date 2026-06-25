using System;
using System.Collections.Generic;

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