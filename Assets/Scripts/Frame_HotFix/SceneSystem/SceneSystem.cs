using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityUtility;
using static FrameBaseHotFix;
using static FileUtility;
using static FrameBaseUtility;
using static StringUtility;
using static CSharpUtility;
using static FrameUtility;

// 3D场景管理器,管理unity场景资源
public class SceneSystem : FrameSystem
{
    // 场景注册信息
    protected class RegisterInfo
    {
        public string name; // 场景名
        public string path; // 场景路径
        public Type type; // 场景逻辑类的类型
        public SceneScriptCallback callback; // 用于给场景脚本对象赋值
    }

    // 场景脚本类型与场景注册信息的映射,允许多个相似的场景共用同一个场景脚本
    protected Dictionary<Type, List<RegisterInfo>> scriptMappings = new();
    protected Dictionary<string, RegisterInfo> sceneRegisterInfos = new(); // 场景注册信息
    protected Dictionary<string, SceneInstance> scenes = new(); // 已经加载的所有场景

    public override void destroy()
    {
        base.destroy();
        foreach (string sceneName in scenes.Keys)
        {
            unloadSceneOnly(sceneName);
        }

        scenes.Clear();
    }

    public override void update(float dt)
    {
        base.update(dt);
        foreach (var scene in scenes.Values)
        {
            if (scene.isActive())
            {
                scene.update(dt);
            }
        }
    }

    public override void lateUpdate(float dt)
    {
        base.lateUpdate(dt);
        foreach (var scene in scenes.Values)
        {
            if (scene.isActive())
            {
                scene.lateUpdate(dt);
            }
        }
    }

    // filePath是场景文件所在目录,不含场景名,最好该目录下包含所有只在这个场景使用的资源
    public void registerScene(Type type, string name, string filePath, SceneScriptCallback callback)
    {
        // 路径需要以/结尾
        validPath(ref filePath);
        RegisterInfo info = sceneRegisterInfos.add(name, new());
        info.name = name;
        info.path = filePath;
        info.type = type;
        info.callback = callback;
        scriptMappings.getOrAddNew(type).Add(info);
    }

    public string getScenePath(string name)
    {
        return sceneRegisterInfos.get(name)?.path ?? EMPTY;
    }

    public T getScene<T>(string name) where T : SceneInstance
    {
        return scenes.get(name) as T;
    }

    public int getScriptMappingCount(Type classType)
    {
        return scriptMappings.get(classType).count();
    }

    public void setMainScene(string name)
    {
        if (!scenes.TryGetValue(name, out var scene))
            return;

        SceneManager.SetActiveScene(scene.getScene());
    }

    public void hideScene(string name)
    {
        if (!scenes.TryGetValue(name, out var scene))
            return;

        scene.setActive(false);
        scene.onHide();
    }

    public void showScene(string name, bool hideOther = true, bool mainScene = true)
    {
        if (!scenes.TryGetValue(name, out var scene))
            return;

        // 如果需要隐藏其他场景,则遍历所有场景设置可见性
        if (hideOther)
        {
            foreach (var (sceneName, sceneInstance) in scenes)
            {
                sceneInstance.setActive(sceneInstance == scene);
                if (sceneInstance == scene)
                {
                    sceneInstance.onShow();
                }
                else
                {
                    sceneInstance.onHide();
                }
            }
        }
        // 不隐藏其他场景则只是简单的将指定场景显示
        else
        {
            scene.setActive(true);
            scene.onShow();
        }

        if (mainScene)
        {
            setMainScene(name);
        }
    }

    // 目前只支持异步加载,因为SceneManager.LoadScene并不是真正地同步加载
    // 该方法只能保证在这一帧结束后场景能加载完毕,但是函数返回后场景并没有加载完毕
    public CustomAsyncOperation loadSceneAsync(string sceneName, bool active, bool mainScene, Action loadedCallback, FloatCallback loadingCallback = null)
    {
        var op = new CustomAsyncOperation();
        // 如果场景已经加载,则直接返回
        if (scenes.ContainsKey(sceneName))
        {
            showScene(sceneName, false, mainScene);
            if (loadingCallback != null || loadedCallback != null)
            {
                delayCall(() =>
                {
                    loadingCallback?.Invoke(1.0f);
                    loadedCallback?.Invoke();
                    op.setFinish();
                });
            }
        }
        else
        {
            SceneInstance scene = scenes.add(sceneName, createScene(sceneName));
            scene.setState(LOAD_STATE.NONE);
            scene.setActiveLoaded(active);
            scene.setMainScene(mainScene);
            scene.setLoadingCallback(loadingCallback);
            scene.setLoadedCallback(loadedCallback);
            // scenePath + sceneName表示场景文件AssetBundle的路径,包含文件名
            mResourceManager.preloadAssetBundleAsync(getScenePath(sceneName) + sceneName, (AssetBundleInfo bundle) =>
            {
                GameEntry.startCoroutine(loadSceneCoroutine(scene, op));
            });
        }

        return op;
    }

    // unloadPath表示是否将场景所属文件夹的所有资源卸载
    public void unloadScene(string name, bool unloadPath = true)
    {
        // 销毁场景,并且从列表中移除
        unloadSceneOnly(name);
        scenes.Remove(name);
        if (unloadPath)
        {
            mResourceManager?.unloadPath(sceneRegisterInfos.get(name).path);
        }
    }

    // 卸载除了dontUnloadSceneName以外的其他场景,初始默认场景除外
    public void unloadOtherScene(string dontUnloadSceneName, bool unloadPath = true)
    {
        using var a = new ListScope<string>(out var tempList, scenes.Keys);
        foreach (string sceneName in tempList)
        {
            if (sceneName != dontUnloadSceneName)
            {
                unloadScene(sceneName, unloadPath);
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected IEnumerator loadSceneCoroutine(SceneInstance scene, CustomAsyncOperation op)
    {
        scene.setState(LOAD_STATE.LOADING);
        // 所有场景都只能使用叠加的方式来加载,方便场景管理器来管理所有场景的加载和卸载
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene.getName(), LoadSceneMode.Additive);
        // allowSceneActivation指定了加载场景时是否需要调用场景中所有脚本的Awake和Start,以及贴图材质的引用等等
        operation.allowSceneActivation = true;
        while (true)
        {
            scene.callLoading(operation.progress);
            yield return null;
            // 当allowSceneActivation为true时,加载到progress为1时停止,并且isDone为true,scene.isLoaded为true
            // 当allowSceneActivation为false时,加载到progress为0.9时就停止,并且isDone为false, scene.isLoaded为false
            // 当场景被激活时isDone变为true,progress也为1,scene.isLoaded为true
            if (operation.isDone || operation.progress >= 1.0f)
            {
                break;
            }
        }

        // 首先获得场景
        scene.setScene(SceneManager.GetSceneByName(scene.getName()));
        // 获得了场景根节点才能使场景显示或隐藏,为了尽量避免此处查找节点错误,所以不能使用容易重名的名字
        scene.setRoot(getRootGameObject(scene.getName() + "_Root", true));
        // 加载完毕后就立即初始化
        scene.init();
        if (scene.isActiveLoaded())
        {
            showScene(scene.getName(), false, scene.isMainScene());
        }
        else
        {
            hideScene(scene.getName());
        }

        scene.setState(LOAD_STATE.LOADED);
        try
        {
            scene.callLoading(1.0f);
            scene.callLoaded();
        }
        catch (Exception e)
        {
            logException(e);
        }

        op.setFinish();
    }

    protected SceneInstance createScene(string sceneName)
    {
        if (!sceneRegisterInfos.TryGetValue(sceneName, out RegisterInfo info))
        {
            logError("scene :" + sceneName + " is not registered!");
            return null;
        }

        var scene = createInstance<SceneInstance>(info.type);
        scene.setName(sceneName);
        scene.setType(info.type);
        notifySceneChanged(scene, true);
        return scene;
    }

    // 只销毁场景,不从列表移除
    protected void unloadSceneOnly(string name)
    {
        if (!scenes.TryGetValue(name, out var scene))
            return;

        notifySceneChanged(scene, false);
        scene.destroy();
        SceneManager.UnloadSceneAsync(name);
    }

    protected void notifySceneChanged(SceneInstance scene, bool isLoad)
    {
        sceneRegisterInfos.get(scene.getName())?.callback?.Invoke(isLoad ? scene : null);
    }
}