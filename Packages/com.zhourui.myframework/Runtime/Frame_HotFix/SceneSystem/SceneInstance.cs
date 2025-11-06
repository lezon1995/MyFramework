using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityUtility;

// 3D场景的实例
public class SceneInstance : DelayCmdWatcher
{
    protected FloatCallback loadingCallback; // 加载中回调
    protected Action loadedCallback; // 加载完成回调
    protected GameObject root; // 场景根节点,每个场景都应该添加一个名称格式固定的根节点,场景名_Root
    protected Scene scene; // Unity场景实例
    protected Type type; // 类型
    protected string name; // 场景名
    protected bool activeLoaded; // 加载完毕后是否立即显示
    protected bool mainScene; // 是否为主场景
    protected bool initialized; // 是否已经执行了初始化
    protected LOAD_STATE state; // 加载状态

    public SceneInstance()
    {
        state = LOAD_STATE.NONE;
    }

    public virtual void init()
    {
        if (initialized)
            return;

        findShaders(root);
        initialized = true;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        loadingCallback = null;
        loadedCallback = null;
        root = null;
        scene = default;
        type = null;
        name = null;
        activeLoaded = false;
        mainScene = false;
        initialized = false;
        state = LOAD_STATE.NONE;
    }

    public override void destroy()
    {
        base.destroy();
        initialized = false;
    }

    public virtual void onShow()
    {
    }

    public virtual void onHide()
    {
    }

    public virtual void update(float elapsedTime)
    {
    }

    public virtual void lateUpdate(float elapsedTime)
    {
    }

    // 不要直接调用SceneInstance的setActive,应该使用SceneSystem的showScene或者hideScene
    public void setActive(bool active)
    {
        if (root != null && root.activeSelf != active)
        {
            root.SetActive(active);
        }
    }

    public void setType(Type t)
    {
        type = t;
    }

    public void setName(string n)
    {
        name = n;
    }

    public void setState(LOAD_STATE s)
    {
        state = s;
    }

    public void setActiveLoaded(bool active)
    {
        activeLoaded = active;
    }

    public void setLoadedCallback(Action callback)
    {
        loadedCallback = callback;
    }

    public void setLoadingCallback(FloatCallback callback)
    {
        loadingCallback = callback;
    }

    public void setScene(Scene s)
    {
        scene = s;
    }

    public void setRoot(GameObject r)
    {
        root = r;
    }

    public void setMainScene(bool main)
    {
        mainScene = main;
    }

    public Type getType()
    {
        return type;
    }

    public bool getActive()
    {
        return root != null && root.activeSelf;
    }

    public GameObject getRoot()
    {
        return root;
    }

    public string getName()
    {
        return name;
    }

    public LOAD_STATE getState()
    {
        return state;
    }

    public bool isActiveLoaded()
    {
        return activeLoaded;
    }

    public bool isInited()
    {
        return initialized;
    }

    public bool isMainScene()
    {
        return mainScene;
    }

    public Scene getScene()
    {
        return scene;
    }

    public void callLoading(float percent)
    {
        loadingCallback?.Invoke(percent);
    }

    public void callLoaded()
    {
        loadedCallback?.Invoke();
    }
}