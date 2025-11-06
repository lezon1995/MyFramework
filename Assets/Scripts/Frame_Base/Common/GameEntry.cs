using System;
using System.Collections;
using System.Net;
using System.Threading;
using UnityEngine;
using static FrameBaseUtility;
using static FrameBaseDefine;

// 游戏的入口
public class GameEntry : MonoBehaviour
{
    protected static GameEntry instance;
    public FrameworkParam frameworkParam;
    protected IFramework aot;
    protected IFramework hotfix;

    public virtual void Awake()
    {
        instance = this;
        ServicePointManager.DefaultConnectionLimit = 200;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Physics.simulationMode = SimulationMode.Script;
        // 每当Transform组件更改时是否自动将变换更改与物理系统同步
        Physics.autoSyncTransforms = true;
        AppDomain.CurrentDomain.UnhandledException += unhandledException;
        setMainThreadID(Thread.CurrentThread.ManagedThreadId);
        dumpSystem();

        var fullScreen = frameworkParam.windowMode;
        if (isEditor())
        {
            // 编辑器下固定全屏
            fullScreen = WINDOW_MODE.FULL_SCREEN;
        }
        else if (isWindows())
        {
            // windows下读取配置
            //// 设置为无边框窗口,只在Windows平台使用,由于无边框的需求非常少,所以此处不再实现,如有需求
            //if (fullScreen == WINDOW_MODE.NO_BOARD_WINDOW)
            //{
            //	// 无边框的设置有时候会失效,并且同样的设置,如果上一次设置失效后,即便恢复设置也同样会失效,也就是说本次的是否生效与上一次的结果有关
            //	// 当设置失效后,可以使用添加启动参数-popupwindow来实现无边框
            //	long curStyle = User32.GetWindowLong(User32.GetForegroundWindow(), GWL_STYLE);
            //	curStyle &= ~WS_BORDER;
            //	curStyle &= ~WS_DLGFRAME;
            //	User32.SetWindowLong(User32.GetForegroundWindow(), GWL_STYLE, curStyle);
            //}
        }
        else if (isAndroid() || isIOS())
        {
            // 移动平台下固定为全屏
            fullScreen = WINDOW_MODE.FULL_SCREEN;
        }
        else if (isWebGL())
        {
            fullScreen = WINDOW_MODE.FULL_SCREEN;
        }
        else if (isWeiXin())
        {
            fullScreen = WINDOW_MODE.FULL_SCREEN;
        }

        Vector2 windowSize;
        if (fullScreen == WINDOW_MODE.FULL_SCREEN)
        {
            windowSize.x = Screen.width;
            windowSize.y = Screen.height;
        }
        else
        {
            windowSize.x = frameworkParam.screenWidth;
            windowSize.y = frameworkParam.screenHeight;
        }

        bool fullMode = fullScreen is WINDOW_MODE.FULL_SCREEN or WINDOW_MODE.FULL_SCREEN_CUSTOM_RESOLUTION;
        setScreenSizeBase(new((int)windowSize.x, (int)windowSize.y), fullMode);
    }

    public void Update()
    {
        try
        {
            var dt = Time.deltaTime;
            aot?.update(dt);
            hotfix?.update(dt);
        }
        catch (Exception e)
        {
            logExceptionBase(e);
        }
    }

    public void FixedUpdate()
    {
        try
        {
            var dt = Time.fixedDeltaTime;
            aot?.fixedUpdate(dt);
            hotfix?.fixedUpdate(dt);
        }
        catch (Exception e)
        {
            logExceptionBase(e);
        }
    }

    public void LateUpdate()
    {
        try
        {
            var dt = Time.deltaTime;
            aot?.lateUpdate(dt);
            hotfix?.lateUpdate(dt);
        }
        catch (Exception e)
        {
            logExceptionBase(e);
        }
    }

    public void OnDrawGizmos()
    {
        try
        {
            aot?.drawGizmos();
            hotfix?.drawGizmos();
        }
        catch (Exception e)
        {
            logExceptionBase(e);
        }
    }

    public void OnApplicationFocus(bool focus)
    {
        aot?.onApplicationFocus(focus);
        hotfix?.onApplicationFocus(focus);
    }

    public void OnApplicationQuit()
    {
        aot?.onApplicationQuit();
        hotfix?.onApplicationQuit();
        aot = null;
        hotfix = null;
        logBase("程序退出完毕!");
    }

    protected void OnDestroy()
    {
        AppDomain.CurrentDomain.UnhandledException -= unhandledException;
    }

    public static GameEntry getInstance()
    {
        return instance;
    }

    public static GameObject getInstanceObject()
    {
        return instance.gameObject;
    }

    public void setFrameworkAOT(IFramework framework)
    {
        aot = framework;
    }

    public void setFrameworkHotFix(IFramework framework)
    {
        hotfix = framework;
    }

    public IFramework getFrameworkAOT()
    {
        return aot;
    }

    public IFramework getFrameworkHotFix()
    {
        return hotfix;
    }

    public static Coroutine startCoroutine(IEnumerator routine)
    {
        return getInstance().StartCoroutine(routine);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void unhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Debug.LogError(e.ExceptionObject.ToString());
    }

    protected void dumpSystem()
    {
        logBase("QualitySettings.currentLevel:" + QualitySettings.GetQualityLevel());
        logBase("QualitySettings.activeColorSpace:" + QualitySettings.activeColorSpace);
        logBase("Graphics.activeTier:" + Graphics.activeTier);
        logBase("SystemInfo.graphicsDeviceType:" + SystemInfo.graphicsDeviceType);
        logBase("SystemInfo.maxTextureSize:" + SystemInfo.maxTextureSize);
        logBase("SystemInfo.supportsInstancing:" + SystemInfo.supportsInstancing);
        logBase("SystemInfo.graphicsShaderLevel:" + SystemInfo.graphicsShaderLevel);
        logBase("PersistentDataPath:" + F_PERSISTENT_DATA_PATH);
        logBase("StreamingAssetPath:" + F_STREAMING_ASSETS_PATH);
        logBase("AssetPath:" + F_ASSETS_PATH);
    }
}