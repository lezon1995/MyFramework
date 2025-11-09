#if BYTE_DANCE
using TTSDK;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static StringUtility;
using static FrameBaseUtility;

// 资源管理器,管理所有资源的加载
public class ResourceManager : FrameSystem
{
    protected AssetDatabaseLoader assetDatabaseLoader = new(); // 通过AssetDataBase加载资源的加载器,只会在编辑器下使用
    protected AssetBundleLoader assetBundleLoader = new(); // 通过AssetBundle加载资源的加载器,打包后强制使用AssetBundle加载
    protected ResourcesLoader resourcesLoader = new(); // 通过Resources加载资源的加载器,Resources在编辑器或者打包后都会使用,用于加载Resources中的非热更资源
    protected List<UObjectCallback> unloadObjectCallback = new(); // 卸载某个单独资源的回调
    protected List<StringCallback> unloadPathCallback = new(); // 卸载目录中所有资源的回调,不会再次通知其中的单个资源
    protected LOAD_SOURCE loadSource; // 加载源,从AssetBundle加载还是从AssetDataBase加载
    protected static int downloadTimeout = 10; // 下载超时时间,秒

    public ResourceManager()
    {
        mCreateObject = true;
    }

    public override void init()
    {
        base.init();
        loadSource = isEditor() ? GameEntry.getInstance().frameworkParam.loadSource : LOAD_SOURCE.ASSET_BUNDLE;
        if (isEditor())
        {
            go.AddComponent<ResourcesManagerDebug>();
        }
    }

    public override void preInitAsync(Action callback)
    {
        if (loadSource != LOAD_SOURCE.ASSET_BUNDLE)
        {
            callback?.Invoke();
            return;
        }

        assetBundleLoader.initAssets(callback);
    }

    public bool isResourceInited()
    {
        if (loadSource == LOAD_SOURCE.ASSET_BUNDLE)
        {
            return assetBundleLoader.isInited();
        }

        return true;
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (loadSource == LOAD_SOURCE.ASSET_BUNDLE)
        {
            assetBundleLoader.update(dt);
        }
    }

    public override void destroy()
    {
        assetBundleLoader?.destroy();
        assetDatabaseLoader?.destroy();
        resourcesLoader?.destroy();
        base.destroy();
    }

    public void addUnloadObjectCallback(UObjectCallback callback)
    {
        unloadObjectCallback.Add(callback);
    }

    public void addUnloadPathCallback(StringCallback callback)
    {
        unloadPathCallback.Add(callback);
    }

    public void removeUnloadObjectCallback(UObjectCallback callback)
    {
        unloadObjectCallback.Remove(callback);
    }

    public void removeUnloadPathCallback(StringCallback callback)
    {
        unloadPathCallback.Remove(callback);
    }

    public AssetBundleLoader getAssetBundleLoader()
    {
        return assetBundleLoader;
    }

    public AssetDatabaseLoader getAssetDataBaseLoader()
    {
        return assetDatabaseLoader;
    }

    public ResourcesLoader getResourcesLoader()
    {
        return resourcesLoader;
    }

    public void setDownloadURL(string url)
    {
        assetBundleLoader.setDownloadURL(url);
    }

    public string getDownloadURL()
    {
        return assetBundleLoader.getDownloadURL();
    }

    public int getDownloadTimeout()
    {
        return downloadTimeout;
    }

    public void setDownloadTimeout(int timeout)
    {
        downloadTimeout = timeout;
    }

    // 卸载加载的资源,不是实例化出的物体,这里的泛型T是为了外部能传任意的类型的引用进来,而不是只能传ref UObject
    public bool unload<T>(ref T obj, bool showError = true) where T : UObject
    {
        if (obj == null)
        {
            return false;
        }

        foreach (UObjectCallback callback in unloadObjectCallback)
        {
            callback.Invoke(obj);
        }

        bool success = loadSource switch
        {
            LOAD_SOURCE.ASSET_DATABASE => assetDatabaseLoader.unloadResource(ref obj, showError),
            LOAD_SOURCE.ASSET_BUNDLE => assetBundleLoader.unloadAsset(ref obj, showError),
            _ => false
        };

        return success;
    }

    // 卸载从Resources中加载的资源
    public bool unloadInResources<T>(ref T obj, bool showError = true) where T : UObject
    {
        return resourcesLoader.unloadResource(ref obj, showError);
    }

    // 卸载指定目录中的所有资源,path为GameResources下的相对路径
    public void unloadPath(string path)
    {
        removeEndSlash(ref path);
        foreach (StringCallback callback in unloadPathCallback)
        {
            callback.Invoke(path);
        }

        switch (loadSource)
        {
            case LOAD_SOURCE.ASSET_DATABASE:
                assetDatabaseLoader.unloadPath(path);
                break;
            case LOAD_SOURCE.ASSET_BUNDLE:
                assetBundleLoader.unloadPath(path);
                break;
        }
    }

    // 卸载Resources指定目录中的所有资源
    public void unloadPathInResources(string path)
    {
        removeEndSlash(ref path);
        resourcesLoader.unloadPath(path);
    }

    // 指定卸载资源包,StreamingAssets/平台名下的路径,不带后缀
    public void unloadAssetBundle(string bundleName)
    {
        // 只有从AssetBundle加载才能卸载AssetBundle
        if (loadSource == LOAD_SOURCE.ASSET_BUNDLE)
        {
            assetBundleLoader.unloadAssetBundle(bundleName);
        }
    }

    // 指定资源是否已经加载,name是GameResources下的相对路径,带后缀
    public bool isGameResourceLoaded<T>(string name) where T : UObject
    {
        checkRelativePath(name);
        bool ret = loadSource switch
        {
            LOAD_SOURCE.ASSET_DATABASE => assetDatabaseLoader.isResourceLoaded(name),
            LOAD_SOURCE.ASSET_BUNDLE => assetBundleLoader.isAssetLoaded<T>(name),
            _ => false
        };

        return ret;
    }

    // 在Resources中的指定资源是否已经加载,带后缀
    public bool isInResourceLoaded<T>(string name) where T : UObject
    {
        checkRelativePath(name);
        return resourcesLoader.isResourceLoaded(removeSuffix(name));
    }

    // 获得资源,如果没有加载,则获取不到,使用频率可能比较低,name是GameResources下的相对路径,带后缀
    public T getGameResource<T>(string name, bool errorIfNull = true) where T : UObject
    {
        checkRelativePath(name);
        T res = loadSource switch
        {
            LOAD_SOURCE.ASSET_DATABASE => assetDatabaseLoader.getResource(name) as T,
            LOAD_SOURCE.ASSET_BUNDLE => assetBundleLoader.getAsset<T>(name),
            _ => null
        };

        if (res == null && errorIfNull)
        {
            logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
        }

        return res;
    }

    // 强制在Resources中获得资源,如果未加载,则无法获取,name是Resources下的相对路径,带后缀
    public T getInResource<T>(string name, bool errorIfNull = true) where T : UObject
    {
        checkRelativePath(name);
        T res = resourcesLoader.getResource(removeSuffix(name)) as T;
        if (res == null && errorIfNull)
        {
            logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
        }

        return res;
    }

    // 检查指定资源包的依赖项是否已经加载,如果没有会强制加载,一般来说用不上
    // 不会出现还在被其他资源包依赖就已经被卸载的情况,因为卸载的时候会检查是否有被其他资源包依赖,除非是手动强制卸载
    public void checkAssetBundleDependenceLoaded(string bundleName)
    {
        if (loadSource == LOAD_SOURCE.ASSET_BUNDLE)
        {
            assetBundleLoader.checkAssetBundleDependenceLoaded(bundleName);
        }
    }

    // 同步预加载资源包,一般不需要调用,只有需要预加载时才会用到
    public void preloadAssetBundle(string bundleName)
    {
        // 只有从AssetBundle加载时才能加载AssetBundle
        if (loadSource == LOAD_SOURCE.ASSET_BUNDLE)
        {
            assetBundleLoader.loadAssetBundle(bundleName, null);
        }
    }

    // 异步预加载资源包,一般不需要调用,只有需要预加载时才会用到
    public void preloadAssetBundleAsync(string bundleName, AssetBundleCallback callback)
    {
        switch (loadSource)
        {
            case LOAD_SOURCE.ASSET_DATABASE:
                // 从Resource加载不能加载AssetBundle
                callback?.Invoke(null);
                break;
            case LOAD_SOURCE.ASSET_BUNDLE:
                assetBundleLoader.loadAssetBundleAsync(bundleName, callback);
                break;
        }
    }

    // 同步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
    public T loadGameResource<T>(string name, bool errorIfNull = true) where T : UObject
    {
        using var a = new ProfilerScope(0);
        checkRelativePath(name);
        T res = loadSource switch
        {
            LOAD_SOURCE.ASSET_DATABASE => assetDatabaseLoader.loadResource<T>(name),
            LOAD_SOURCE.ASSET_BUNDLE => assetBundleLoader.loadAsset<T>(name),
            _ => null
        };

        if (res == null && errorIfNull)
        {
            logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
        }

        return res;
    }

    // 强制从Resources中同步加载指定资源,name是Resources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
    public T loadInResource<T>(string name, bool errorIfNull = true) where T : UObject
    {
        checkRelativePath(name);
        T res = resourcesLoader.loadResource<T>(removeSuffix(name));
        if (res == null && errorIfNull)
        {
            logError("can not find resource : " + name);
        }

        return res;
    }

    // 同步加载资源的子资源,一般是图集才会有子资源,或者是fbx
    public UObject[] loadSubGameResource<T>(string name, out UObject mainAsset, bool errorIfNull = true) where T : UObject
    {
        using var a = new ProfilerScope(0);
        checkRelativePath(name);
        mainAsset = null;
        UObject[] res = loadSource switch
        {
            LOAD_SOURCE.ASSET_DATABASE => assetDatabaseLoader.loadSubResource<T>(name, out mainAsset),
            LOAD_SOURCE.ASSET_BUNDLE => assetBundleLoader.loadSubAsset<T>(name, out mainAsset),
            _ => null
        };

        if (res == null && errorIfNull)
        {
            logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
        }

        return res;
    }

    // 强制从Resources中同步加载资源的子资源,一般是图集才会有子资源,或者是fbx
    public UObject[] loadSubInResource<T>(string name, out UObject mainAsset, bool errorIfNull = true) where T : UObject
    {
        checkRelativePath(name);
        UObject[] res = resourcesLoader.loadSubResource<T>(removeSuffix(name), out mainAsset);
        if (res == null && errorIfNull)
        {
            logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
        }

        return res;
    }

    // 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
    public CustomAsyncOperation loadGameResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, bool errorIfNull = true) where T : UObject
    {
        using var a = new ProfilerScope(0);
        checkRelativePath(name);
        return loadSource switch
        {
            LOAD_SOURCE.ASSET_DATABASE => assetDatabaseLoader.loadResourcesAsync<T>(name, doneCallback),
            LOAD_SOURCE.ASSET_BUNDLE => assetBundleLoader.loadAssetAsync<T>(name, errorIfNull, doneCallback),
            _ => null
        };
    }

    // 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
    public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<T, string> doneCallback, bool errorIfNull = true) where T : UObject
    {
        return loadGameResourceAsync<T>(name, (asset, assets, bytes, loadPath) =>
        {
            doneCallback?.Invoke(asset as T, loadPath);
        }, errorIfNull);
    }

    // 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
    // 在relatedObj生命周期内加载资源,如果完成加载后relatedObj已经被销毁,则会自动卸载资源并且不会调用回调
    public CustomAsyncOperation loadGameResourceAsyncSafe<T>(ClassObject relatedObj, string name, Action<T, string> doneCallback, bool errorIfNull = true) where T : UObject
    {
        long assignID = relatedObj.id;
        return loadGameResourceAsync<T>(name, (asset, assets, bytes, loadPath) =>
        {
            if (assignID != relatedObj.id)
            {
                unload(ref asset);
                return;
            }

            doneCallback?.Invoke(asset as T, loadPath);
        }, errorIfNull);
    }

    // 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
    public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<T> doneCallback, bool errorIfNull = true) where T : UObject
    {
        return loadGameResourceAsync<T>(name, (asset, assets, bytes, loadPath) =>
        {
            doneCallback?.Invoke(asset as T);
        }, errorIfNull);
    }

    // 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
    // 在relatedObj生命周期内加载资源,如果完成加载后relatedObj已经被销毁,则会自动卸载资源并且不会调用回调
    public CustomAsyncOperation loadGameResourceAsyncSafe<T>(ClassObject relatedObj, string name, Action<T> doneCallback, bool errorIfNull = true) where T : UObject
    {
        long assignID = relatedObj.id;
        return loadGameResourceAsync<T>(name, (asset, assets, bytes, loadPath) =>
        {
            if (assignID != relatedObj.id)
                return;

            doneCallback?.Invoke(asset as T);
        }, errorIfNull);
    }

    // 强制在Resource中异步加载资源,name是Resources下的相对路径,带后缀,errorIfNull表示当找不到资源时是否报错提示
    public CustomAsyncOperation loadInResourceAsync<T>(string name, Action<T> doneCallback) where T : UObject
    {
        return loadInResourceAsync<T>(name, (asset, assets, bytes, loadPath) =>
        {
            doneCallback?.Invoke(asset as T);
        });
    }

    // 强制在Resource中异步加载资源,name是Resources下的相对路径,带后缀,errorIfNull表示当找不到资源时是否报错提示
    public CustomAsyncOperation loadInResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback) where T : UObject
    {
        checkRelativePath(name);
        return resourcesLoader.loadResourcesAsync<T>(removeSuffix(name), doneCallback);
    }

    // 仅下载一个资源,下载后会写入本地文件,并且更新本地文件信息列表,fileName为带后缀,GameResources下的相对路径
    public void downloadGameResource(string name, BytesCallback callback)
    {
        checkRelativePath(name);
        if (loadSource == LOAD_SOURCE.ASSET_BUNDLE)
        {
            assetBundleLoader.downloadAsset(name, callback);
        }
    }

    // 根据一个URL加载资源,一般都是一个网络资源
    public static void loadAssetsFromUrl<T>(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
    {
        GameEntry.startCoroutine(loadAssetsUrl(url, typeof(T), callback, downloadingCallback));
    }

    // 根据一个URL加载资源,一般都是一个网络资源
    public static void loadAssetsFromUrl<T>(string url, BytesCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
    {
        GameEntry.startCoroutine(loadAssetsUrl(url, typeof(T), (asset, assets, bytes, loadPath) =>
        {
            callback?.Invoke(bytes);
        }, downloadingCallback));
    }

    // 根据一个URL加载资源,一般都是一个网络资源
    public static void loadAssetsFromUrl<T>(string url, Action<T> callback, DownloadCallback downloadingCallback = null) where T : UObject
    {
        GameEntry.startCoroutine(loadAssetsUrl(url, typeof(T), (asset, assets, bytes, loadPath) =>
        {
            callback?.Invoke(asset as T);
        }, downloadingCallback));
    }

    // 根据一个URL加载资源,一般都是一个网络资源
    public static void loadAssetsFromUrl<T>(string url, Action<T, string> callback, DownloadCallback downloadingCallback = null) where T : UObject
    {
        GameEntry.startCoroutine(loadAssetsUrl(url, typeof(T), (asset, assets, bytes, loadPath) =>
        {
            callback?.Invoke(asset as T, loadPath);
        }, downloadingCallback));
    }

    // 根据一个URL加载资源,一般都是一个网络资源
    public static void loadAssetsFromUrl(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null)
    {
        GameEntry.startCoroutine(loadAssetsUrl(url, null, callback, downloadingCallback));
    }

    // 根据一个URL加载资源,一般都是一个网络资源
    public static void loadAssetsFromUrl(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
    {
        GameEntry.startCoroutine(loadAssetsUrl(url, null, (asset, assets, bytes, loadPath) =>
        {
            callback?.Invoke(bytes);
        }, downloadingCallback));
    }

    // 根据一个URL加载资源,一般都是一个网络资源
    public static void loadAssetsFromUrl(string url, BytesStringCallback callback, DownloadCallback downloadingCallback = null)
    {
        GameEntry.startCoroutine(loadAssetsUrl(url, null, (asset, assets, bytes, loadPath) =>
        {
            callback?.Invoke(bytes, loadPath);
        }, downloadingCallback));
    }

    // 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
    public static IEnumerator loadAssetsFromUrlWaiting<T>(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
    {
        return loadAssetsUrl(url, typeof(T), callback, downloadingCallback);
    }

    // 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
    public static IEnumerator loadAssetsFromUrlWaiting<T>(string url, Action<T> callback, DownloadCallback downloadingCallback = null) where T : UObject
    {
        return loadAssetsUrl(url, typeof(T), (asset, assets, bytes, loadPath) =>
        {
            callback?.Invoke(asset as T);
        }, downloadingCallback);
    }

    // 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
    public static IEnumerator loadAssetsFromUrlWaiting(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null)
    {
        return loadAssetsUrl(url, null, callback, downloadingCallback);
    }

    // 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
    public static IEnumerator loadAssetsFromUrlWaiting(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
    {
        return loadAssetsUrl(url, null, (asset, assets, bytes, loadPath) =>
        {
            callback?.Invoke(bytes);
        }, downloadingCallback);
    }

    // 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
    public static IEnumerator loadAssetsUrl(string url, Type assetsType, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback)
    {
        log("开始下载: " + url);
        if (assetsType == typeof(AudioClip))
        {
            yield return loadAudioClipWithURL(url, callback);
        }
        else if (assetsType == typeof(Texture2D) || assetsType == typeof(Texture))
        {
            yield return loadTextureWithURL(url, callback);
        }
        else if (assetsType == typeof(AssetBundle))
        {
            yield return loadAssetBundleWithURL(url, callback);
        }
        else
        {
            yield return loadFileWithURL(url, callback, downloadingCallback);
        }
    }

    // 根据一个URL加载AssetBundle,可在协程中等待
    public static IEnumerator loadAssetBundleWithURL(string url, AssetLoadDoneCallback callback)
    {
        float timer = 0.0f;
        ulong lastDownloaded = 0;
#if BYTE_DANCE
		using var www = TTAssetBundle.GetAssetBundle(url);
#else
        using var www = UnityWebRequestAssetBundle.GetAssetBundle(url);
#endif
        www.SendWebRequest();
        while (!www.isDone)
        {
            if (www.downloadedBytes > lastDownloaded)
            {
                lastDownloaded = www.downloadedBytes;
                timer = 0.0f;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= downloadTimeout)
                {
                    log("下载超时");
                    break;
                }
            }

            if (isEditor() || isDevelopment())
            {
                log("当前计时:" + timer);
                log("下载中,www.downloadedBytes:" + www.downloadedBytes + ", www.downloadProgress:" + www.downloadProgress);
            }

            yield return null;
        }

        try
        {
            if (www.error != null)
            {
                log("下载失败 : " + url + ", info : " + www.error);
                callback?.Invoke(null, null, null, url);
            }
            else
            {
                log("下载成功:" + url);
#if BYTE_DANCE
				AssetBundle assetBundle = (www.downloadHandler as DownloadHandlerTTAssetBundle).assetBundle;
#else
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(www);
#endif
                assetBundle.name = url;
                callback?.Invoke(assetBundle, null, null, url);
            }
        }
        catch (Exception e)
        {
            logException(e);
        }
    }

    public static IEnumerator loadAudioClipWithURL(string url, AssetLoadDoneCallback callback)
    {
        using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
        www.timeout = downloadTimeout;
        yield return www.SendWebRequest();
        try
        {
            if (www.error != null)
            {
                log("下载失败 : " + url + ", info : " + www.error);
                callback?.Invoke(null, null, null, url);
            }
            else
            {
                log("下载成功:" + url);
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = url;
                callback?.Invoke(clip, null, www.downloadHandler.data, url);
            }
        }
        catch (Exception e)
        {
            logException(e);
        }
    }

    public static IEnumerator loadTextureWithURL(string url, AssetLoadDoneCallback callback)
    {
        using var www = UnityWebRequestTexture.GetTexture(url);
        www.timeout = downloadTimeout;
        yield return www.SendWebRequest();
        try
        {
            if (www.error != null)
            {
                log("下载失败 : " + url + ", info : " + www.error);
                callback?.Invoke(null, null, null, url);
            }
            else
            {
                log("下载成功:" + url);
                Texture2D tex = DownloadHandlerTexture.GetContent(www);
                tex.name = url;
                callback?.Invoke(tex, null, www.downloadHandler.data, url);
            }
        }
        catch (Exception e)
        {
            logException(e);
        }
    }

    public static IEnumerator loadFileWithURL(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback)
    {
        float timer = 0.0f;
        ulong lastDownloaded = 0;
        using var www = UnityWebRequest.Get(url);
        www.timeout = 0;
        www.SendWebRequest();
        DateTime startTime = DateTime.Now;
        while (!www.isDone)
        {
            // 累计每秒下载的字节数,计算下载速度
            int downloadDelta = 0;
            if (www.downloadedBytes > lastDownloaded)
            {
                lastDownloaded = www.downloadedBytes;
                downloadDelta = (int)(www.downloadedBytes - lastDownloaded);
                timer = 0.0f;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= downloadTimeout)
                {
                    log("下载超时");
                    break;
                }
            }

            double deltaTimeMillis = (DateTime.Now - startTime).TotalMilliseconds;
            downloadingCallback?.Invoke(www.downloadedBytes, downloadDelta, deltaTimeMillis, www.downloadProgress);
            yield return null;
        }

        try
        {
            if (www.error != null || www.downloadHandler?.data == null)
            {
                log("下载失败 : " + url + ", info : " + www.error);
                callback?.Invoke(null, null, null, url);
            }
            else
            {
                log("下载成功:" + url + ", size:" + www.downloadHandler.data.Length);
                callback?.Invoke(null, null, www.downloadHandler.data, url);
            }
        }
        catch (Exception e)
        {
            logException(e);
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    // 检查路径的合法性,需要带后缀,且需要是相对于GameResources的路径
    protected static void checkRelativePath(string path)
    {
        // 需要带后缀
        if (!path.Contains('.'))
        {
            logError("资源文件名需要带后缀:" + path);
            return;
        }

        // 不能是绝对路径
        if (path.startWith(FrameBaseDefine.F_ASSETS_PATH))
        {
            logError("不能传入绝对路径:" + path);
            return;
        }

        // 不能是以Assets或者Assets/GameResources开头的相对路径
        if (path.startWith(FrameDefine.P_GAME_RESOURCES_PATH) || path.startWith(FrameBaseDefine.ASSETS))
        {
            logError("不能是以Assets或者Assets/GameResources开头的相对路径:" + path);
            return;
        }
    }
}