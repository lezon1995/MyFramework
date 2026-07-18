using System;
using System.Collections.Generic;

public struct SafeDictionaryReader<Key, Value> : IDisposable
{
	private SafeDictionary<Key, Value> mSafeList;
	public Dictionary<Key, Value> mReadList;
	public SafeDictionaryReader(SafeDictionary<Key, Value> list)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
	}
	
	public SafeDictionaryReader(SafeDictionary<Key, Value> list, out Dictionary<Key, Value> reader)
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