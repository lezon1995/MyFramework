using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// ClassPool调试信息
public class ClassPoolDebug : MonoBehaviour
{
    public List<string> persistentInuseList = new(); // 持久使用的对象列表
    public List<string> inuseList = new(); // 单帧使用的对象列表
    public List<string> unuseList = new(); // 未使用的对象列表

    public void Update()
    {
        if (GameEntry.getInstance() == null)
            return;

        if (!GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        persistentInuseList.Clear();
        foreach (var (key, hashSet) in mClassPool.getPersistentInusedList())
        {
            if (hashSet.Count == 0)
                continue;

            persistentInuseList.Add(key + ": 个数:" + IToS(hashSet.Count));
        }

        inuseList.Clear();
        foreach (var (key, hashSet) in mClassPool.getInusedList())
        {
            if (hashSet.Count == 0)
                continue;

            inuseList.Add(key + ": 个数:" + IToS(hashSet.Count));
        }

        unuseList.Clear();
        foreach (var (key, queue) in mClassPool.getUnusedList())
        {
            if (queue.Count == 0)
                continue;

            unuseList.Add(key + ": 个数:" + IToS(queue.Count));
        }
    }
}