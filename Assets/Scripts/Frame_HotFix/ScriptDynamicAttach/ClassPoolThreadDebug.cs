using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线性安全的对象池调试信息
public class ClassPoolThreadDebug : MonoBehaviour
{
    public List<string> types = new(); // 类型信息列表

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        using (new ThreadLockScope(mClassPoolThread.getLock()))
        {
            types.Clear();
            foreach (var (key, value) in mClassPoolThread.getPoolList())
            {
                using var a = new MyStringBuilderScope(out var builder);
                builder.append(key.ToString());
                if (value.getInusedList().Count > 0)
                {
                    builder.append(", 已使用:", IToS(value.getInusedList().Count));
                }

                if (value.getUnusedList().Count > 0)
                {
                    builder.append(", 未使用:", IToS(value.getUnusedList().Count));
                }

                types.Add(builder.ToString());
            }
        }
    }
}