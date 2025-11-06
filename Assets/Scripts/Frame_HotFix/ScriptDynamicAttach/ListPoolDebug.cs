using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 列表池的调试信息
public class ListPoolDebug : MonoBehaviour
{
    public List<string> persistentInuseList = new(); // 持久使用的列表
    public List<string> inuseList = new(); // 单帧使用的列表
    public List<string> unuseList = new(); // 未使用的列表

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        persistentInuseList.Clear();
        foreach (var (key, hashSet) in mListPool.getPersistentInusedList())
        {
            if (hashSet.Count == 0)
                continue;

            persistentInuseList.Add(key + ", 数量:" + IToS(hashSet.Count));
        }

        inuseList.Clear();
        foreach (var (key, hashSet) in mListPool.getInusedList())
        {
            if (hashSet.Count == 0)
                continue;

            inuseList.Add(key + ", 数量:" + IToS(hashSet.Count));
        }

        unuseList.Clear();
        foreach (var (key, queue) in mListPool.getUnusedList())
        {
            if (queue.Count == 0)
                continue;

            unuseList.Add(key + ", 数量:" + IToS(queue.Count));
        }
    }
}