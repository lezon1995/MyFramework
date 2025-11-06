using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

[Serializable]
public class PrefabPoolDebugInfo
{
    public int InuseCount;
    public int UnuseCount;
    public string PrefabName;
    public string FileName;
}

// 从资源加载的物体池的调试信息
public class ObjectPoolDebug : MonoBehaviour
{
    public List<ObjectInfo> instances = new(); // 物体信息列表
    public List<PrefabPoolDebugInfo> prefabPoolInfo = new(); // 预设列表

    void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug || mPrefabPoolManager == null)
            return;

        instances.setRange(mPrefabPoolManager.getInstanceList().Values);

        prefabPoolInfo.Clear();
        foreach (var (key, pool) in mPrefabPoolManager.getPrefabPoolList().getMainList())
        {
            var info = new PrefabPoolDebugInfo();
            info.InuseCount = pool.getInuseCount();
            info.UnuseCount = pool.getUnuseCount();
            info.PrefabName = pool.getPrefab() != null ? pool.getPrefab().name : "null";
            info.FileName = pool.getFileName();
            prefabPoolInfo.Add(info);
        }
    }
}