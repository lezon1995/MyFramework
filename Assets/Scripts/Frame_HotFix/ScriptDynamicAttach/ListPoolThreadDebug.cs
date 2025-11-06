using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线程安全的对象池列表
public class ListPoolThreadDebug : MonoBehaviour
{
    public List<string> inuseList = new(); // 已使用列表
    public List<string> unuseList = new(); // 未使用列表

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        inuseList.Clear();
        unuseList.Clear();
        using (new ThreadLockScope(mListPoolThread.getLock()))
        {
            foreach (var (key, hashSet) in mListPoolThread.getInusedList())
            {
                if (hashSet.Count == 0)
                    continue;

                inuseList.Add(key + ", 数量:" + IToS(hashSet.Count));
            }

            foreach (var (key, queue) in mListPoolThread.getUnusedList())
            {
                if (queue.Count == 0)
                    continue;

                unuseList.Add(key + ", 数量:" + IToS(queue.Count));
            }
        }
    }
}