using System.Collections.Generic;
using UObject = UnityEngine.Object;

// 资源加载的信息,表示一个非AssetBundle的资源
public class ResourceLoadInfo
{
    protected List<AssetLoadDoneCallback> callbacks = new(); // 回调列表
    protected List<string> loadPaths = new(); // 用于回调传参的加载路径列表,实际上里面都是mResourceName
    protected UObject[] subAssets; // 子物体列表,比如图集中的所有Sprite
    protected UObject asset; // 资源物体
    protected string path; // 加载路径,也就是mResourcesName中的路径,不带文件名
    protected string resourceName; // GameResources下的相对路径,带后缀
    protected LOAD_STATE state = LOAD_STATE.NONE; // 加载状态

    public ResourceLoadInfo()
    {
    }

    public void addCallback(AssetLoadDoneCallback callback, string loadPath)
    {
        if (callback == null)
            return;

        callbacks.Add(callback);
        loadPaths.Add(loadPath);
    }

    public void callbackAll()
    {
        // 需要复制一份列表,避免回调期间又开始加载资源而造成逻辑错误
        var tempCallbackList = new List<AssetLoadDoneCallback>(callbacks);
        var tempLoadPath = new List<string>(loadPaths);
        callbacks.Clear();
        loadPaths.Clear();
        UObject tempObj = asset;
        UObject[] tempSubObjs = subAssets;
        int count = tempCallbackList.Count;
        for (int i = 0; i < count; ++i)
        {
            tempCallbackList[i](tempObj, tempSubObjs, null, tempLoadPath[i]);
        }
    }

    public UObject[] getSubObjects()
    {
        return subAssets;
    }

    public UObject getObject()
    {
        return asset;
    }

    public LOAD_STATE getState()
    {
        return state;
    }

    public string getPath()
    {
        return path;
    }

    public string getResourceName()
    {
        return resourceName;
    }

    public void setPath(string path)
    {
        this.path = path;
    }

    public void setResourceName(string name)
    {
        resourceName = name;
    }

    public void setObject(UObject obj)
    {
        asset = obj;
    }

    public void setSubObjects(UObject[] objs)
    {
        subAssets = objs;
    }

    public void setState(LOAD_STATE state)
    {
        this.state = state;
    }
}