using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseUtility;
using static UnityUtility;
using static FrameUtility;
using static MathUtility;
using static TimeUtility;
using static FrameBaseHotFix;

// 最顶层的节点,也是游戏的入口,管理所有框架组件(管理器)
// [ObfuzIgnore]
public class GameFrameworkHotFix : IFramework
{
    protected Dictionary<string, FrameSystem> frames = new(); // 存储框架组件,用于查找
    protected Dictionary<string, Action<FrameSystem>> framesCallback = new(); // 用于通知框架系统创建或者销毁的回调

    protected List<FrameSystem> framesInit = new(); // 存储框架组件,用于初始化
    protected List<FrameSystem> framesUpdate = new(); // 存储框架组件,用于更新
    protected List<FrameSystem> framesDestroy = new(); // 存储框架组件,用于销毁

    protected ThreadTimeLock timeLock = new(15); // 用于主线程锁帧,与Application.targetFrameRate功能类似
    protected DateTime startTime; // 启动游戏时的时间
    protected DateTime curTime; // 记录当前时间
    protected Action OnApplicationQuitCallBack; // 程序退出的回调
    protected BoolCallback OnApplicationFocusCallBack; // 程序切换到前台或者切换到后台的回调
    protected float deltaTime; // 当前这一帧的消耗时间
    protected long frameIndex; // 当前帧下标
    protected int frameCount; // 当前已执行的帧数量
    protected int frameRate; // 当前设置的最大帧率
    protected int fps; // 当前帧率
    protected bool mResourceAvailable; // 资源是否已经可用
    protected bool destroyed; // 框架是否已经被销毁
    public static Action OnDestroy;
    public static Action<int, long, long, long, long> OnMemoryModifiedCheck;
    public static Func<string> OnPackageName;

    public static void startHotFix(Action callback)
    {
        var framework = new GameFrameworkHotFix();
        GameEntry.getInstance().setFrameworkHotFix(framework);
        framework.init(callback);
    }

    public DateTime StartTime => startTime;

    public DateTime getFrameStartTime()
    {
        return timeLock.getFrameStartTime();
    }

    public long getFrameIndex()
    {
        return frameIndex;
    }

    public void update(float dt)
    {
        ++frameIndex;
        ++frameCount;
        var now = DateTime.Now;
        if ((now - curTime).TotalMilliseconds >= 1000.0f)
        {
            fps = frameCount;
            frameCount = 0;
            curTime = now;
        }

        deltaTime = clampMax((float)(timeLock.update() * 0.001) * Time.timeScale, 0.3f);
        dt = deltaTime;
        setThisTimeMS(getNowTimeStampMS());
        if (framesUpdate == null)
            return;

        int count = framesUpdate.Count;
        for (int i = 0; i < count; ++i)
        {
            // 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
            if (framesUpdate == null)
                return;

            var com = framesUpdate[i];
            if (com.isValid())
            {
                using var a = new ProfilerScope(com.getName());
                com.update(dt);
            }
        }
    }

    public void fixedUpdate(float dt)
    {
        if (framesUpdate == null)
            return;

        int count = framesUpdate.Count;
        for (int i = 0; i < count; ++i)
        {
            // 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
            if (framesUpdate == null)
                return;

            var com = framesUpdate[i];
            if (com.isValid())
            {
                using var a = new ProfilerScope(com.getName());
                com.fixedUpdate(dt);
            }
        }
    }

    public void lateUpdate(float dt)
    {
        if (framesUpdate == null)
            return;

        int count = framesUpdate.Count;
        for (int i = 0; i < count; ++i)
        {
            // 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
            if (framesUpdate == null)
                return;

            var com = framesUpdate[i];
            if (com.isValid())
            {
                using var a = new ProfilerScope(com.getName());
                com.lateUpdate(dt);
            }
        }
    }

    public void drawGizmos()
    {
        if (framesUpdate == null)
            return;

        int count = framesUpdate.Count;
        for (int i = 0; i < count; ++i)
        {
            // 因为在更新过程中也可能销毁所有组件,所以需要每次循环都要判断
            if (framesUpdate == null)
                return;

            var com = framesUpdate[i];
            if (com.isValid())
            {
                com.onDrawGizmos();
            }
        }
    }

    public void onApplicationFocus(bool focus)
    {
        OnApplicationFocusCallBack?.Invoke(focus);
    }

    public void onApplicationQuit()
    {
        OnApplicationQuitCallBack?.Invoke();
        destroy();
    }

    public void registerOnApplicationQuit(Action action)
    {
        OnApplicationQuitCallBack += action;
    }

    public void unregisterOnApplicationQuit(Action action)
    {
        OnApplicationQuitCallBack -= action;
    }

    public void registerOnApplicationFocus(BoolCallback action)
    {
        OnApplicationFocusCallBack += action;
    }

    public void unregisterOnApplicationFocus(BoolCallback action)
    {
        OnApplicationFocusCallBack -= action;
    }

    // 当资源更新完毕后,由外部进行调用
    public void resourceAvailable()
    {
        mResourceAvailable = true;
        foreach (var frame in framesInit)
            frame.resourceAvailable();
    }

    public void setFrameRate(int rate)
    {
        if (rate <= 0)
        {
            logError("帧率不能小于等于0");
            return;
        }

        frameRate = rate;
        Application.targetFrameRate = frameRate;
    }

    public void resetFrameRate()
    {
        setFrameRate(GameEntry.getInstance().frameworkParam.defaultFrameRate);
    }

    public bool isResourceAvailable()
    {
        return mResourceAvailable;
    }

    public bool isDestroy()
    {
        return destroyed;
    }

    public void destroy()
    {
        destroyed = true;
        if (framesDestroy == null)
            return;

        OnDestroy?.Invoke();
        foreach (var frame in framesInit)
            frame?.willDestroy();

        foreach (var frame in framesInit)
        {
            if (frame)
            {
                frame.destroy();
                framesCallback.Remove(frame.getName(), out var callback);
                callback?.Invoke(null);
            }
        }

        framesInit.Clear();
        framesUpdate.Clear();
        framesDestroy.Clear();
        frames.Clear();

        framesInit = null;
        framesUpdate = null;
        framesDestroy = null;
        frames = null;
    }

    public int getFPS()
    {
        return fps;
    }

    public void destroyComponent<T>(ref T com) where T : FrameSystem
    {
        int count = framesUpdate.Count;
        for (int i = 0; i < count; ++i)
        {
            if (framesInit[i] == com)
                framesInit[i] = null;

            if (framesUpdate[i] == com)
                framesUpdate[i] = null;

            if (framesDestroy[i] == com)
                framesDestroy[i] = null;
        }

        string name = com.getName();
        frames.Remove(name);
        com.destroy();
        com = null;
        framesCallback.Remove(name, out var callback);
        callback?.Invoke(null);
    }

    public T registerFrameSystem<T>(Action<T> callback, int initOrder = -1, int updateOrder = -1, int destroyOrder = -1) where T : FrameSystem, new()
    {
        Type type = typeof(T);
        log("注册系统:" + type + ", owner:" + GetType());
        var com = new T();
        string name = type.Assembly.FullName.rangeToFirst(',') + "_" + type;
        com.setName(name);
        com.setInitOrder(initOrder == -1 ? frames.Count : initOrder);
        com.setUpdateOrder(updateOrder == -1 ? frames.Count : updateOrder);
        com.setDestroyOrder(destroyOrder == -1 ? frames.Count : destroyOrder);
        frames.Add(name, com);
        framesInit.Add(com);
        framesUpdate.Add(com);
        framesDestroy.Add(com);
        framesCallback.Add(name, t => callback?.Invoke(t as T));
        callback?.Invoke(com);
        return com;
    }

    public void sortList()
    {
        framesInit.Sort(FrameSystem.compareInit);
        framesUpdate.Sort(FrameSystem.compareUpdate);
        framesDestroy.Sort(FrameSystem.compareDestroy);
    }

    public void onMemoryModified(int flag, long param0, long param1, long param2, long param3)
    {
        OnMemoryModifiedCheck?.Invoke(flag, param0, param1, param2, param3);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void preInitAsync(Action callback)
    {
        int count = 0;
        foreach (var item in framesInit)
        {
            item.preInitAsync(() =>
            {
                if (++count == framesInit.count())
                {
                    callback?.Invoke();
                }
            });
        }
    }

    protected void init(Action callback)
    {
        using var _ = new ProfilerScope(0);
        // 通过代码添加接受java日志的节点
        getOrAddComponent<UnityAndroidLog>(findOrCreateRootGameObject("UnityLog"));
        mGameFrameworkHotFix = this;
        destroyed = false;
        startTime = DateTime.Now;
        setFrameRate(GameEntry.getInstance().frameworkParam.defaultFrameRate);

        if (!isEditor() && isDevelopment())
        {
            // 跟引擎自带的dev的调试控制台功能重合了,所以不再使用
            getOrAddComponent<ConsoleToScreen>(GameEntry.getInstanceObject());
        }

        // 设置默认的日志等级
        setLogLevel(LOG_LEVEL.FORCE);

        registerFrameSystem<AndroidPluginManager>(null);
        registerFrameSystem<AndroidAssetLoader>(null);
        registerFrameSystem<AndroidMainClass>(null);
        AndroidPluginManager.initAndroidPlugin(OnPackageName?.Invoke());
        AndroidAssetLoader.initJava(AndroidPluginManager.getPackageName() + ".AssetLoader");
        AndroidMainClass.initJava(AndroidPluginManager.getPackageName() + ".MainClass");
        log("start game hotfix!");
        log("当前平台:" + getPlatformName());

        try
        {
            var startTime = DateTime.Now;
            initFrameSystem();
            recoverCrossParam();
            log("start消耗时间:" + (int)(DateTime.Now - startTime).TotalMilliseconds);
            // 根据设置的顺序对列表进行排序
            sortList();

            foreach (var frame in framesInit)
            {
                try
                {
                    var start = DateTime.Now;
                    frame.init();
                    log(frame.getName() + "初始化消耗时间:" + (int)(DateTime.Now - start).TotalMilliseconds);
                }
                catch (Exception e)
                {
                    logException(e, "init failed! :" + frame.getName());
                }
            }

            foreach (var frame in framesInit)
            {
                try
                {
                    frame.lateInit();
                }
                catch (Exception e)
                {
                    logException(e, "lateInit failed! :" + frame.getName());
                }
            }
        }
        catch (Exception e)
        {
            logException(e, "init failed! " + (e.InnerException?.Message ?? "empty"));
        }

        curTime = DateTime.Now;

        // 先执行所有的preInitAsync
        preInitAsync(() =>
        {
            // 再执行所有的initAsync,因为部分initAsync会依赖于preInitAsync
            int count = 0;
            foreach (var item in framesInit)
            {
                item.initAsync(() =>
                {
                    if (++count == framesInit.count())
                    {
                        resourceAvailable();
                        callback?.Invoke();
                    }
                });
            }
        });
    }

    protected void initFrameSystem()
    {
        registerFrameSystem<ResourceManager>(com => res = com, -1, 3000, 3000); // 资源管理器的需要最先初始化,并且是最后被销毁,作为最后的资源清理
        registerFrameSystem<TimeManager>(com => mTimeManager = com);
        registerFrameSystem<GlobalCmdReceiver>(com => mGlobalCmdReceiver = com);
        registerFrameSystem<SQLiteManager>(com => mSQLiteManager = com);
        registerFrameSystem<CommandSystem>(com => mCommandSystem = com, -1, -1, 2001); // 命令系统在大部分管理器都销毁完毕后再销毁
        registerFrameSystem<InputSystem>(com => mInputSystem = com); // 输入系统应该早点更新,需要更新输入的状态,以便后续的系统组件中使用
        registerFrameSystem<KeyMappingSystem>(com => mKeyMappingSystem = com); // 输入映射系统需要在输入系统之后
        registerFrameSystem<GlobalTouchSystem>(com => mGlobalTouchSystem = com);
        registerFrameSystem<TweenerManager>(com => mTweenerManager = com);
        registerFrameSystem<CharacterManager>(com => mCharacterManager = com);
        registerFrameSystem<AudioManager>(com => mAudioManager = com);
        registerFrameSystem<GameSceneManager>(com => mGameSceneManager = com, -1, -1, 0); // 在退出程序时,需要先执行流程的退出,然后才能执行其他系统的销毁
        registerFrameSystem<KeyFrameManager>(com => mKeyFrameManager = com);
        registerFrameSystem<DllImportSystem>(com => mDllImportSystem = com);
        registerFrameSystem<ShaderManager>(com => mShaderManager = com);
        registerFrameSystem<CameraManager>(com => mCameraManager = com);
        registerFrameSystem<SceneSystem>(com => mSceneSystem = com);
        registerFrameSystem<GamePluginManager>(com => mGamePluginManager = com);
        registerFrameSystem<ClassPool>(com => mClassPool = com, -1, -1, 3101);
        registerFrameSystem<ClassPoolThread>(com => mClassPoolThread = com, -1, -1, 3102);
        registerFrameSystem<ListPool>(com => mListPool = com, -1, -1, 3103);
        registerFrameSystem<ListPoolThread>(com => mListPoolThread = com, -1, -1, 3104);
        registerFrameSystem<HashSetPool>(com => mHashSetPool = com, -1, -1, 3104);
        registerFrameSystem<HashSetPoolThread>(com => mHashSetPoolThread = com, -1, -1, 3105);
        registerFrameSystem<DictionaryPool>(com => mDictionaryPool = com, -1, -1, 3106);
        registerFrameSystem<DictionaryPoolThread>(com => mDictionaryPoolThread = com, -1, -1, 3107);
        registerFrameSystem<ArrayPool>(com => mArrayPool = com, -1, -1, 3108);
        registerFrameSystem<ArrayPoolThread>(com => mArrayPoolThread = com, -1, -1, 3109);
        registerFrameSystem<ByteArrayPool>(com => mByteArrayPool = com, -1, -1, 3110);
        registerFrameSystem<ByteArrayPoolThread>(com => mByteArrayPoolThread = com, -1, -1, 3111);
        registerFrameSystem<MovableObjectManager>(com => mMovableObjectManager = com);
        registerFrameSystem<EffectManager>(com => mEffectManager = com);
        registerFrameSystem<AtlasManager>(com => mAtlasManager = com);
        registerFrameSystem<NetPacketFactory>(com => mNetPacketFactory = com);
        registerFrameSystem<PathKeyframeManager>(com => mPathKeyframeManager = com);
        registerFrameSystem<EventSystem>(com => mEventSystem = com);
        registerFrameSystem<StateManager>(com => mStateManager = com);
        registerFrameSystem<NetPacketTypeManager>(com => mNetPacketTypeManager = com);
        registerFrameSystem<GameObjectPool>(com => mGameObjectPool = com);
        registerFrameSystem<ExcelManager>(com => mExcelManager = com);
        registerFrameSystem<RedPointSystem>(com => mRedPointSystem = com);
        registerFrameSystem<AssetVersionSystem>(com => mAssetVersionSystem = com);
        registerFrameSystem<GlobalKeyProcess>(com => mGlobalKeyProcess = com);
        registerFrameSystem<LocalizationManager>(com => mLocalizationManager = com);
        registerFrameSystem<AsyncTaskGroupManager>(com => mAsyncTaskGroupManager = com);
        registerFrameSystem<GoogleLogin>(com => mGoogleLogin = com);
        registerFrameSystem<AppleLogin>(com => mAppleLogin = com);
        registerFrameSystem<ScreenOrientationSystem>(com => mScreenOrientationSystem = com);
        registerFrameSystem<WaitingManager>(com => mWaitingManager = com);
        registerFrameSystem<UndoManager>(com => mUndoManager = com);
        registerFrameSystem<AndroidPurchasing>(com => mAndroidPurchasing = com);
        registerFrameSystem<PurchasingSystem>(com => mPurchasingSystem = com);
        registerFrameSystem<AvatarRenderer>(com => mAvatarRenderer = com);
        registerFrameSystem<LayoutManager>(com => mLayoutManager = com, 1000, 1000, -1); // 布局管理器也需要在最后更新,确保所有游戏逻辑都更新完毕后,再更新界面
        registerFrameSystem<PrefabPoolManager>(com => mPrefabPoolManager = com, 2000, 2000, 2000); // 物体管理器最后注册,销毁所有缓存的资源对象
    }
}