using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 资源管理器调试信息
public class ResourcesManagerDebug : MonoBehaviour
{
    public List<string> loadedAssetBundleListKeys = new(); // 已加载的AssetBundle列表Key
    public List<AssetBundleDebug> loadedAssetBundleListValues = new(); // 已加载的AssetBundle列表Value

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        loadedAssetBundleListKeys.Clear();
        loadedAssetBundleListValues.Clear();
        var dict = mResourceManager.getAssetBundleLoader().getAssetBundleInfoList();
        foreach (var (key, info) in dict)
        {
            if (info.getLoadState() != LOAD_STATE.LOADED)
                continue;

            loadedAssetBundleListKeys.Add(key);
            AssetBundleDebug bundleDebug = new(info.getBundleName());
            bundleDebug.mAssetList.setRange(info.getAssetList().Values);
            bundleDebug.mParentBundles.setRange(info.getParents().Keys);
            bundleDebug.mChildBundles.setRange(info.getChildren().Keys);
            loadedAssetBundleListValues.Add(bundleDebug);
        }
    }
}