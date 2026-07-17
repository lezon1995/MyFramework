using System;
using System.Collections.Generic;

public struct SafeListReader<T> : IDisposable
{
	private SafeList<T> mSafeList;
	public List<T> mReadList;
	public SafeListReader(SafeList<T> list)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
	}
	
	public SafeListReader(SafeList<T> list, out List<T> reader)
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