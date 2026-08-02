using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityUtility;

// 3D场景的实例
public class SceneInstance : DelayCmdWatcher
{
    protected FloatCallback onLoading; // 加载中回调
    protected Action onLoaded; // 加载完成回调
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
        onLoading = null;
        onLoaded = null;
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

    public virtual void update(float dt)
    {
    }

    public virtual void lateUpdate(float dt)
    {
    }

    // 不要直接调用SceneInstance的setActive,应该使用SceneSystem的showScene或者hideScene
    public void setActive(bool active)
    {
        if (root && root.activeSelf != active)
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
        onLoaded = callback;
    }

    public void setLoadingCallback(FloatCallback callback)
    {
        onLoading = callback;
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

    public bool isActive()
    {
        return root && root.activeSelf;
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
        onLoading?.Invoke(percent);
    }

    public void callLoaded()
    {
        onLoaded?.Invoke();
    }
}