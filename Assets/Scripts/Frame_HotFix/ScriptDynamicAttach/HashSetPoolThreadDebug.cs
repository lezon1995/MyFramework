using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线程安全的HashSet对象池的调试信息
public class HashSetPoolThreadDebug : MonoBehaviour
{
    public List<string> inuseList = new(); // 已使用列表
    public List<string> unuseList = new(); // 未使用列表

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        inuseList.Clear();
        unuseList.Clear();
        using (new ThreadLockScope(mHashSetPoolThread.getLock()))
        {
            foreach (var (key, hashSet) in mHashSetPoolThread.getInusedList())
            {
                if (hashSet.Count == 0)
                    continue;

                inuseList.Add(key + ", 数量:" + IToS(hashSet.Count));
            }

            foreach (var (key, queue) in mHashSetPoolThread.getUnusedList())
            {
                if (queue.Count == 0)
                    continue;

                unuseList.Add(key + ", 数量:" + IToS(queue.Count));
            }
        }
    }
}