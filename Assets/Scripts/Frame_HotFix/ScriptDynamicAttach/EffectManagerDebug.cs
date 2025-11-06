using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 特效管理器调试信息
public class EffectManagerDebug : MonoBehaviour
{
    public List<GameObject> effects = new(); // 特效列表

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        effects.Clear();
        var list = mEffectManager.getEffectList().getMainList();
        foreach (GameEffect item in list)
        {
            effects.Add(item.getObject());
        }
    }
}