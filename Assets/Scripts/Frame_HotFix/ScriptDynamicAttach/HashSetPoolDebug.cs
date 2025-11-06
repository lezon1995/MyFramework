using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// HashSet对象池的调试信息
public class HashSetPoolDebug : MonoBehaviour
{
    public List<string> persistentInuseList = new(); // 持久使用的列表
    public List<string> inuseList = new(); // 单帧使用的列表
    public List<string> unuseList = new(); // 未使用列表

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        persistentInuseList.Clear();
        foreach (var (key, hashSet) in mHashSetPool.getPersistentInusedList())
        {
            if (hashSet.Count == 0)
                continue;

            persistentInuseList.Add(key + ", 数量:" + IToS(hashSet.Count));
        }

        inuseList.Clear();
        foreach (var (key, hashSet) in mHashSetPool.getInusedList())
        {
            if (hashSet.Count == 0)
                continue;

            inuseList.Add(key + ", 数量:" + IToS(hashSet.Count));
        }

        unuseList.Clear();
        foreach (var (key, queue) in mHashSetPool.getUnusedList())
        {
            if (queue.Count == 0)
                continue;

            unuseList.Add(key + ", 数量:" + IToS(queue.Count));
        }
    }
}