using System.Collections.Generic;
using UnityEngine;
#if BYTE_DANCE
using TTSDK;
#endif
using UObject = UnityEngine.Object;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static CSharpUtility;
using static FrameBaseUtility;

// AssetBundle的信息,存储了AssetBundle中相关的所有数据
public class AssetBundleInfo : ClassObject
{
    protected Dictionary<string, AssetBundleInfo> children = new(); // 依赖自己的AssetBundle列表,即引用了自己的AssetBundle
    protected Dictionary<string, AssetBundleInfo> parents = new(); // 依赖的AssetBundle列表,即自己引用的AssetBundle,包含所有的直接和间接的依赖项
    protected Dictionary<UObject, AssetInfo> objectToAsset = new(); // 通过Object查找AssetInfo的列表
    protected Dictionary<string, AssetInfo> assetList = new(); // 资源包中的所有资源,初始化时就会填充此列表
    protected List<AssetBundleCallback> loadedCallbacks = new(); // 资源包加载完毕后的回调列表
    protected List<AssetBundleBytesCallback> downloadedCallbacks = new(); // 资源包下载完毕后的回调列表
    protected HashSet<AssetInfo> loadAsyncList = new(); // AssetBundle还未加载完时请求的异步加载的资源列表
    protected AssetBundle assetBundle; // 资源包内存镜像
    protected LOAD_STATE loadState = LOAD_STATE.NONE; // 资源包加载状态
    protected string bundleFileName; // 资源所在的AssetBundle名,相对于StreamingAsset,含后缀
    protected string bundleName; // 资源所在的AssetBundle名,相对于StreamingAsset,不含后缀
    protected float willUnloadTime = -1.0f; // 引用计数变为0时的计时,小于0表示还有引用,不会被卸载,大于等于0表示计数为0,即将在一定时间后卸载
    protected const float UNLOAD_DELAY_TIME = 5.0f; // 没有引用时延迟5秒卸载

    public AssetBundleInfo(string name)
    {
        bundleName = name;
        bundleFileName = bundleName + ASSET_BUNDLE_SUFFIX;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        children.Clear();
        parents.Clear();
        objectToAsset.Clear();
        assetList.Clear();
        loadedCallbacks.Clear();
        downloadedCallbacks.Clear();
        loadAsyncList.Clear();
        assetBundle = null;
        loadState = LOAD_STATE.NONE;
        // bundleName,bundleFileName不重置
        // bundleFileName = null;
        // bundleName = null;
        willUnloadTime = -1.0f;
    }

    public void update(float dt)
    {
        // 需要再次确认是否有引用
        if (tickTimerOnce(ref willUnloadTime, dt) && canUnload())
        {
            unload();
        }
    }

    // 卸载整个资源包
    public void unload()
    {
        if (res.getAssetBundleLoader().isDontUnloadAssetBundle(bundleFileName))
            return;

        if (assetBundle)
        {
            // 为true表示会卸载掉LoadAsset加载的资源,并不影响该资源实例化的物体
            // 只支持参数为true,如果是false,则是只卸载AssetBundle镜像,但是加载资源包中时会需要使用内存镜像
            // 其他资源包中的资源引用到此资源时,也会自动从此AssetBundle内存镜像中加载需要的资源
            // 所以卸载镜像,将会造成这些自动加载失败,仅在当前资源包内已经没有任何资源在使用了,并且
            // 其他资源包中的资源实例没有对当前资源包进行引用时才会卸载
#if BYTE_DANCE
			assetBundle.TTUnload(true);
#else
            assetBundle.Unload(true);
#endif
            assetBundle = null;
        }

        objectToAsset.Clear();
        foreach (var assetInfo in assetList.Values)
            assetInfo.clear();

        loadState = LOAD_STATE.NONE;
        // 通知依赖项,自己被卸载了
        foreach (var bundleInfo in parents.Values)
            bundleInfo.notifyChildUnload();
    }

    // 卸载包中单个资源
    public bool unloadAsset(UObject obj)
    {
        if (!objectToAsset.Remove(obj, out AssetInfo info))
        {
            logError("object doesn't exist! name:" + obj.name + ", can not unload!");
            return false;
        }

        // 预设类型不真正进行卸载,否则在AssetBundle内存镜像重新加载之前,无法再次从AssetBundle加载此资源
        if (obj is GameObject or Component)
        {
            // UObject.DestroyImmediate(obj, true);
        }
        // 其他独立资源可以使用此方式卸载,使用Resources.UnloadAsset及时卸载资源
        // 可以减少Resources.UnloadUnusedAssets的耗时
        else
        {
            Resources.UnloadAsset(obj);
        }

        info.clear();
        if (canUnload())
        {
            willUnloadTime = UNLOAD_DELAY_TIME;
        }

        return true;
    }

    public Dictionary<string, AssetBundleInfo> getParents()
    {
        return parents;
    }

    public Dictionary<string, AssetBundleInfo> getChildren()
    {
        return children;
    }

    public Dictionary<string, AssetInfo> getAssetList()
    {
        return assetList;
    }

    public string getBundleName()
    {
        return bundleName;
    }

    public string getBundleFileName()
    {
        return bundleFileName;
    }

    public LOAD_STATE getLoadState()
    {
        return loadState;
    }

    public AssetBundle getAssetBundle()
    {
        return assetBundle;
    }

    public void setLoadState(LOAD_STATE state)
    {
        loadState = state;
    }

    public void addAssetName(string fileNameWithSuffix)
    {
        if (assetList.ContainsKey(fileNameWithSuffix))
        {
            logError("there is asset in asset bundle, asset : " + fileNameWithSuffix + ", asset bundle : " + bundleFileName);
            return;
        }

        AssetInfo info = assetList.add(fileNameWithSuffix, new());
        info.setAssetBundleInfo(this);
        info.setAssetName(fileNameWithSuffix);
    }

    public AssetInfo getAssetInfo(string fileNameWithSuffix)
    {
        return assetList.get(fileNameWithSuffix);
    }

    // 添加依赖项
    public void addParent(string dep)
    {
        parents.TryAdd(dep, null);
    }

    // 通知有其他的AssetBundle依赖了自己
    public void addChild(AssetBundleInfo other)
    {
        children.TryAdd(other.bundleName, other);
    }

    // 有一个引用了自己的AssetBundle被卸载了,尝试检查当前AssetBundle是否可以被卸载
    public void notifyChildUnload()
    {
        if (canUnload())
        {
            willUnloadTime = UNLOAD_DELAY_TIME;
        }
    }

    // 查找所有依赖项
    public void findAllDependence()
    {
        using var a = new ListScope<string>(out var tempList, parents.Keys);
        foreach (string depName in tempList)
        {
            var bundleInfo = res.getAssetBundleLoader().getAssetBundleInfo(depName);
            // 找到自己的父节点
            parents.set(depName, bundleInfo);
            // 并且通知父节点添加自己为子节点
            bundleInfo.addChild(this);
        }
    }

    // 所有依赖项是否都已经加载完成
    public bool isAllParentLoaded()
    {
        foreach (var bundleInfo in parents.Values)
        {
            if (bundleInfo.loadState != LOAD_STATE.LOADED)
            {
                return false;
            }
        }

        return true;
    }

    // 同步加载资源包
    public void loadAssetBundle()
    {
        if (isWebGL())
        {
            logError("webgl无法使用loadAssetBundle");
            return;
        }

        if (assetBundle)
            return;

        if (loadState != LOAD_STATE.NONE)
        {
            logError("资源包正在异步加载,无法开始同步加载." + bundleFileName);
            return;
        }

        // 先确保所有依赖项已经加载
        foreach (var bundleInfo in parents.Values)
            bundleInfo.loadAssetBundle();

        assetBundle = AssetBundle.LoadFromFile(availableReadPath(bundleFileName));
        if (assetBundle == null)
        {
            logError("can not load asset bundle : " + bundleFileName);
        }

        loadState = LOAD_STATE.LOADED;
        willUnloadTime = -1.0f;
    }

    // 异步加载所有依赖项,确认依赖项即将加载或者已加载
    public void loadParentAsync()
    {
        foreach (var bundleInfo in parents.Values)
            bundleInfo.loadAssetBundleAsync(null);
    }

    public void checkAssetBundleDependenceLoaded()
    {
        // 先确保所有依赖项已经加载
        foreach (var bundleInfo in parents.Values)
            bundleInfo.checkAssetBundleDependenceLoaded();

        if (loadState == LOAD_STATE.NONE)
        {
            loadAssetBundle();
        }
    }

    // 异步加载资源包
    public void loadAssetBundleAsync(AssetBundleCallback callback)
    {
        willUnloadTime = -1.0f;
        // 加载完毕,直接调用回调
        if (loadState == LOAD_STATE.LOADED)
        {
            callback?.Invoke(this);
            return;
        }

        // 还未加载完成时,则加入等待列表
        loadedCallbacks.addNotNull(callback);
        // 如果还未开始加载,则加载资源包
        if (loadState == LOAD_STATE.NONE)
        {
            // 先确保所有依赖项已经加载
            loadParentAsync();
            loadState = LOAD_STATE.WAIT_FOR_LOAD;
            // 通知AssetBundleLoader请求异步加载AssetBundle,只在真正开始异步加载时才标记为正在加载状态,此处只是加入等待列表
            res.getAssetBundleLoader().requestLoadAssetBundle(this);
        }
    }

    // 同步加载资源
    public T loadAsset<T>(string fileNameWithSuffix) where T : UObject
    {
        willUnloadTime = -1.0f;
        // 如果AssetBundle还没有加载,则先加载AssetBundle
        if (loadState != LOAD_STATE.LOADED)
        {
            loadAssetBundle();
        }

        AssetInfo info = assetList.get(fileNameWithSuffix);
        T asset = info.loadAsset<T>();
        if (asset != null)
        {
            objectToAsset.TryAdd(asset, info);
            res.getAssetBundleLoader().notifyAssetLoaded(asset, this);
        }

        return asset;
    }

    // 同步加载资源的子集
    public UObject[] loadSubAssets(string fileNameWithSuffix, out UObject mainAsset)
    {
        willUnloadTime = -1.0f;
        // 如果AssetBundle还没有加载,则先加载AssetBundle
        if (loadState != LOAD_STATE.LOADED)
        {
            loadAssetBundle();
        }

        AssetInfo info = assetList.get(fileNameWithSuffix);
        UObject[] objs = info.loadAsset();
        UObject asset = objs.get(0);
        mainAsset = asset;
        if (asset != null)
        {
            objectToAsset.TryAdd(asset, info);
            res.getAssetBundleLoader().notifyAssetLoaded(asset, this);
        }

        return objs;
    }

    // 同步加载所有子集
    public void loadAllSubAssets()
    {
        foreach (AssetInfo assetInfo in assetList.Values)
        {
            // 确认是否正常加载完成,如果当前资源包已经卸载,则无法完成加载资源
            if (loadState != LOAD_STATE.NONE)
            {
                assetInfo.loadAsset();
                UObject asset = assetInfo.getAsset();
                if (asset != null)
                {
                    objectToAsset.TryAdd(asset, assetInfo);
                    res.getAssetBundleLoader().notifyAssetLoaded(asset, this);
                }
            }

            assetInfo.callbackAll();
        }
    }

    // 异步加载资源
    public CustomAsyncOperation loadAssetAsync(string fileNameWithSuffix, AssetLoadDoneCallback callback, string loadPath)
    {
        willUnloadTime = -1.0f;
        CustomAsyncOperation op = new();
        AssetInfo info = assetList.get(fileNameWithSuffix);
        info.addCallback((UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
        {
            callback?.Invoke(asset, assets, bytes, loadPath);
            op.setFinish();
        }, loadPath);

        // 如果资源包已经加载,则可以直接异步加载资源
        if (loadState == LOAD_STATE.LOADED)
        {
            info.loadAssetAsync();
        }
        // 如果当前资源包还未加载完毕,则需要等待资源包加载完以后才能加载资源
        else
        {
            // AssetBundle未加载完成时,记录下需要异步加载的资源名,等待AssetBundle加载完毕后再加载Asset
            loadAsyncList.Add(info);
            // 还没有开始加载则开始加载AssetBundle
            if (loadState == LOAD_STATE.NONE)
            {
                loadAssetBundleAsync(null);
            }
        }

        return op;
    }

    // 资源异步加载完成
    public void notifyAssetLoaded(string fileNameWithSuffix, UObject[] assets)
    {
        AssetInfo assetInfo = assetList.get(fileNameWithSuffix);
        // 确认是否正常加载完成,如果当前资源包已经卸载,则无法完成加载资源
        if (loadState != LOAD_STATE.NONE)
        {
            assetInfo.setSubAssets(assets);
            UObject asset = assetInfo.getAsset();
            if (asset != null)
            {
                objectToAsset.TryAdd(asset, assetInfo);
                res.getAssetBundleLoader().notifyAssetLoaded(asset, this);
            }
        }

        assetInfo.callbackAll();
    }

    // 资源包异步加载完成
    public void notifyAssetBundleAsyncLoaded(AssetBundle assetBundle)
    {
        this.assetBundle = assetBundle;
        if (loadState != LOAD_STATE.NONE)
        {
            loadState = LOAD_STATE.LOADED;
            // 异步加载请求的资源
            foreach (AssetInfo item in loadAsyncList)
            {
                assetList.get(item.getAssetName()).loadAssetAsync();
            }
        }
        // 加载状态为已卸载,表示在异步加载过程中,资源包被卸载掉了
        else
        {
            logWarning("资源包异步加载完成,但是异步加载过程中被卸载");
            unload();
        }

        loadAsyncList.Clear();

        using var a = new ListScope<AssetBundleCallback>(out var callbacks);
        foreach (AssetBundleCallback callback in callbacks.move(loadedCallbacks))
        {
            callback(this);
        }
    }

    public void addDownloadCallback(AssetBundleBytesCallback callback)
    {
        downloadedCallbacks.addNotNull(callback);
    }

    public void notifyAssetBundleDownloaded(byte[] bytes)
    {
        using var a = new ListScope<AssetBundleBytesCallback>(out var callbacks);
        foreach (AssetBundleBytesCallback call in callbacks.move(downloadedCallbacks))
        {
            call(this, bytes);
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    // 尝试卸载AssetBundle,卸载需要满足两个条件
    // 当前AssetBundle内的所有资源已经没有正在使用
    // 已经没有其他的正在使用的AssetBundle引用了自己
    protected bool canUnload()
    {
        if (loadState != LOAD_STATE.LOADED)
        {
            return false;
        }

        // 如果资源包的资源已经没有在使用中,则卸载当前资源包
        foreach (AssetInfo item in assetList.Values)
        {
            if (item.getLoadState() != LOAD_STATE.NONE)
            {
                return false;
            }
        }

        // 如果已经没有资源被引用了,则卸载AssetBundle
        // 当前已经没有正在使用的AssetBundle引用了自己时才可以卸载
        foreach (AssetBundleInfo item in children.Values)
        {
            if (item.getLoadState() != LOAD_STATE.NONE)
            {
                return false;
            }
        }

        return true;
    }
}