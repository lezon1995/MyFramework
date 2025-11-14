using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseHotFix;
using static StringUtility;
using static CSharpUtility;
using static MathUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FileUtility;
using static FrameBaseUtility;

// 用于记录布局的注册信息
public struct LayoutRegisterInfo
{
    public Type type; // 布局脚本类型
    public bool inResource; // 布局是否在Resources中
    public LAYOUT_LIFE_CYCLE lifeCycle; // 布局的生命周期
    public LayoutScriptCallback callback; // 用于加载或者卸载后对脚本变量进行赋值
}

// 布局信息
public struct LayoutInfo
{
    public LAYOUT_ORDER orderType; // 显示顺序类型
    public Type type; // 布局脚本类型
    public string name; // 布局名字
    public bool isScene; // 是否为场景布局,场景布局不会挂在UGUIRoot下面
    public int renderOrder; // 显示顺序
}

// 布局管理器
public class LayoutManager : FrameSystem
{
    protected Dictionary<Type, LayoutRegisterInfo> layoutInfos = new(); // 布局注册信息列表
    protected SafeDictionary<Type, GameLayout> layouts = new(); // 所有布局的列表
    protected Dictionary<string, LayoutInfo> asyncLayouts = new(); // 正在异步加载的布局列表
    protected Dictionary<Type, string> layoutTypeToPath = new(); // 根据布局ID查找布局路径
    protected Dictionary<string, Type> layoutPathToType = new(); // 根据布局路径查找布局ID
    protected List<GameLayout> backBlurLayouts = new(); // 需要背景模糊的布局的列表
    protected COMLayoutManagerEscHide escHide; // Esc按键事件传递逻辑的组件
    protected myUGUICanvas root; // 所有UI的根节点
    protected bool useAnchor = true; // 是否启用锚点来自动调节窗口的大小和位置

    public LayoutManager()
    {
        // 在构造中获取UI根节点,确保其他组件能在任意时刻正常访问
        var uiRoot = getRootGameObject(UGUI_ROOT, true);
        root = LayoutScript.newUIObject<myUGUICanvas>(null, null, uiRoot);
    }

    public Canvas getUGUIRootComponent()
    {
        return root.getCanvas();
    }

    public myUGUICanvas getUIRoot()
    {
        return root;
    }

    public GameObject getRootObject()
    {
        return root?.getObject();
    }

    public void notifyLayoutRenderOrder()
    {
        escHide.notifyLayoutRenderOrder();
    }

    public void notifyLayoutVisible(bool visible, GameLayout layout)
    {
        escHide.notifyLayoutVisible(visible, layout);
        if (visible)
        {
            backBlurLayouts.addIf(layout, layout.isBlurBack());
            // 显示布局时,如果当前正在显示有背景模糊的布局,则需要判断当前布局是否需要模糊
            if (backBlurLayouts.Count > 0)
            {
                CmdLayoutManagerBackBlur.execute(backBlurLayouts, backBlurLayouts.Count > 0);
            }
        }
        else
        {
            backBlurLayouts.removeIf(layout, layout.isBlurBack());
            CmdLayoutManagerBackBlur.execute(backBlurLayouts, backBlurLayouts.Count > 0);
            // 布局在隐藏时都需要确认设置层为UI层
            setGameObjectLayer(layout.getRoot()?.getObject(), layout.getDefaultLayer());
        }
    }

    public void setUseAnchor(bool _useAnchor)
    {
        useAnchor = _useAnchor;
    }

    public bool isUseAnchor()
    {
        return useAnchor;
    }

    public override void update(float dt)
    {
        base.update(dt);
        using var a = new SafeDictionaryReader<Type, GameLayout>(layouts);
        foreach (var layout in a.mReadList.Values)
        {
            try
            {
                using var b = new ProfilerScope(layout.getName());
                layout.update(dt);
            }
            catch (Exception e)
            {
                logException(e, "界面:" + layout.getName());
            }
        }
    }

    public override void onDrawGizmos()
    {
        using var a = new SafeDictionaryReader<Type, GameLayout>(layouts);
        foreach (var layout in a.mReadList.Values)
            layout.onDrawGizmos();
    }

    public override void lateUpdate(float dt)
    {
        base.lateUpdate(dt);
        using var a = new SafeDictionaryReader<Type, GameLayout>(layouts);
        foreach (var layout in a.mReadList.Values)
        {
            try
            {
                layout.lateUpdate(dt);
            }
            catch (Exception e)
            {
                logException(e, "layout:" + layout.getName());
            }
        }
    }

    public override void willDestroy()
    {
        mInputSystem?.unlistenKey(this);
        using var a = new SafeDictionaryReader<Type, GameLayout>(layouts);
        foreach (var layout in a.mReadList.Values)
            layout.destroy();

        layouts.clear();
        layoutTypeToPath.Clear();
        layoutPathToType.Clear();
        asyncLayouts.Clear();
        // 销毁UI摄像机
        mCameraManager?.destroyCamera(mCameraManager.getUICamera(), false);
        myUGUIObject.destroyWindowSingle(root, false);
        root = null;
        base.willDestroy();
    }

    public string getLayoutPathByType(Type type)
    {
        return layoutTypeToPath.get(type);
    }

    public Type getLayoutTypeByPath(string path)
    {
        return layoutPathToType.get(path);
    }

    public GameLayout getLayout(Type type)
    {
        return layouts.get(type);
    }

    public SafeDictionary<Type, GameLayout> getLayoutList()
    {
        return layouts;
    }

    public LayoutScript getScript(Type type)
    {
        return getLayout(type)?.getScript();
    }

    // 根据顺序类型,计算实际的渲染顺序
    public int generateRenderOrder(GameLayout exceptLayout, int renderOrder, LAYOUT_ORDER orderType)
    {
        switch (orderType)
        {
            case LAYOUT_ORDER.ALWAYS_TOP:
                if (renderOrder < ALWAYS_TOP_ORDER)
                    renderOrder += ALWAYS_TOP_ORDER;

                break;
            case LAYOUT_ORDER.ALWAYS_TOP_AUTO:
                renderOrder = getTopLayoutOrder(exceptLayout, true) + 1;
                break;
            case LAYOUT_ORDER.AUTO:
                renderOrder = getTopLayoutOrder(exceptLayout, false) + 1;
                break;
        }

        return renderOrder;
    }

    public GameLayout createLayout(LayoutInfo info)
    {
        if (layouts.tryGetValue(info.type, out var existLayout))
            return existLayout;

        if (isWebGL())
        {
            logError("webgl无法同步加载界面");
            return null;
        }

        string path = getLayoutPathByType(info.type);
        if (path.isEmpty())
        {
            logError("没有找到界面的注册信息:" + info.type);
        }

        info.name = getFileNameNoSuffixNoDir(path);
        string pathUnderResource = R_UI_PREFAB_PATH + path + ".prefab";
        GameObject prefab;
        if (layoutInfos.get(info.type).inResource)
            prefab = res.loadFromResources<GameObject>(pathUnderResource);
        else
            prefab = res.loadGameResource<GameObject>(pathUnderResource);

        return newLayout(info, prefab);
    }

    public void createLayoutAsync(LayoutInfo info, GameLayoutCallback callback)
    {
        if (layouts.tryGetValue(info.type, out var existLayout))
        {
            callback?.Invoke(existLayout);
            return;
        }

        string path = getLayoutPathByType(info.type);
        if (path.isEmpty())
        {
            logError("没有找到界面的注册信息:" + info.type);
        }

        info.name = getFileNameNoSuffixNoDir(path);
        string pathUnderResource = R_UI_PREFAB_PATH + path + ".prefab";
        asyncLayouts.Add(info.name, info);

        Action<GameObject> action = asset =>
        {
            if (asyncLayouts.Remove(asset.name, out var layoutInfo))
            {
                callback?.Invoke(newLayout(layoutInfo, asset));
            }
        };

        if (layoutInfos.get(info.type).inResource)
        {
            res.loadInResourceAsync(pathUnderResource, action);
        }
        else
        {
            res.loadGameResourceAsync(pathUnderResource, action);
        }
    }

    public void destroyLayout(Type type)
    {
        var layout = getLayout(type);
        if (layout == null)
            return;

        escHide.notifyLayoutDestroy(layout);
        layouts.remove(type);
        layout.destroy();
    }

    public LayoutScript createScript(GameLayout layout)
    {
        var info = layoutInfos.get(layout.getType());
        var script = createInstance<LayoutScript>(info.type);
        if (script == null)
        {
            logError("界面脚本未注册, Type:" + layout.getType());
            return null;
        }

        script.setLayout(layout);
        return script;
    }

    public void getAllLayoutBoxCollider(List<Collider> colliders)
    {
        colliders.Clear();
        foreach (var layout in layouts.getMainList().Values)
        {
            layout.getAllCollider(colliders, true);
        }
    }

    public void registerLayout(Type type, string name, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle, LayoutScriptCallback callback)
    {
        // 编辑器下检查文件是否存在
        if (isEditor() && !isFileExist(inResource ? (P_RESOURCES_UI_PREFAB_PATH + name + ".prefab") : (P_UI_PREFAB_PATH + name + ".prefab")))
        {
            logError("界面文件不存在:" + (inResource ? (P_RESOURCES_UI_PREFAB_PATH + name + ".prefab") : (P_UI_PREFAB_PATH + name + ".prefab")));
            return;
        }

        var info = new LayoutRegisterInfo();
        info.type = type;
        info.inResource = inResource;
        info.lifeCycle = lifeCycle;
        info.callback = callback;
        layoutTypeToPath.Add(type, name);
        layoutPathToType.Add(name, type);
        layoutInfos.Add(type, info);
    }

    // 获取已注册的布局数量,而不是已加载的布局数量
    public int getLayoutCount()
    {
        return layoutTypeToPath.Count;
    }

    // 获取当前已经显示的布局中最上层布局的渲染深度,但是不包括始终在最上层的布局
    public int getTopLayoutOrder(GameLayout exceptLayout, bool alwaysTop)
    {
        int maxOrder = 0;
        foreach (var layout in layouts.getMainList().Values)
        {
            if (exceptLayout == layout)
                continue;

            if (!layout.isVisible())
                continue;

            var order = layout.getRenderOrderType();
            bool curIsAlwaysTop = order is LAYOUT_ORDER.ALWAYS_TOP or LAYOUT_ORDER.ALWAYS_TOP_AUTO;
            if (alwaysTop != curIsAlwaysTop)
                continue;

            maxOrder = getMax(maxOrder, layout.getRenderOrder());
        }

        // 如果没有始终在最上层的布局,则需要确保渲染顺序最低不能小于指定值
        if (alwaysTop && maxOrder == 0)
        {
            maxOrder += ALWAYS_TOP_ORDER;
        }

        return maxOrder;
    }

    // 卸载所有非常驻的布局
    public void unloadAllPartLayout()
    {
        using var a = new SafeDictionaryReader<Type, GameLayout>(layouts);
        foreach (var type in a.mReadList.Keys)
        {
            var registerInfo = layoutInfos.get(type);
            if (registerInfo.lifeCycle == LAYOUT_LIFE_CYCLE.PART_USE)
            {
                LT.UNLOAD(type);
            }
        }
    }

    public void notifyLayoutChanged(GameLayout layout)
    {
        var registerInfo = layoutInfos.get(layout.getType());
        registerInfo.callback?.Invoke(layout.getScript());
    }

    // 方便调用的布局注册函数
    public static void registerLayoutResPart<T>(Action<T> callback = null) where T : LayoutScript
    {
        registerLayout<T>(true, LAYOUT_LIFE_CYCLE.PART_USE, script => callback?.Invoke(script as T));
    }

    public static void registerLayoutResAlways<T>(Action<T> callback = null) where T : LayoutScript
    {
        registerLayout<T>(true, LAYOUT_LIFE_CYCLE.PERSIST, script => callback?.Invoke(script as T));
    }

    public static void registerLayout<T>(bool inResource, LAYOUT_LIFE_CYCLE lifeCycle, LayoutScriptCallback callback = null) where T : LayoutScript
    {
        mLayoutManager.registerLayout(typeof(T), typeof(T).ToString(), inResource, lifeCycle, callback);
    }

    public static void registerLayout<T>(Action<T> callback) where T : LayoutScript
    {
        registerLayout(typeof(T).ToString(), false, LAYOUT_LIFE_CYCLE.PART_USE, callback);
    }

    public static void registerLayoutPersist<T>(Action<T> callback) where T : LayoutScript
    {
        registerLayout(typeof(T).ToString(), false, LAYOUT_LIFE_CYCLE.PERSIST, callback);
    }

    public static void registerLayout<T>(string name, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle, Action<T> callback) where T : LayoutScript
    {
        mLayoutManager.registerLayout(typeof(T), name, inResource, lifeCycle, script => callback?.Invoke(script as T));
    }

    public void testAllLayout()
    {
        if (!isEditor())
            return;

        foreach (var type in layoutInfos.Keys)
        {
            if (!layouts.containsKey(type))
            {
                LT.LOAD_SHOW(type);
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected override void initComponents()
    {
        base.initComponents();
        addInitComponent(out escHide, true);
    }

    protected GameLayout newLayout(LayoutInfo info, GameObject prefab)
    {
        myUGUIObject layoutParent = info.isScene ? null : getUIRoot();
        GameObject layoutObj = instantiatePrefab(layoutParent?.getObject(), prefab, info.name, true);
        var layout = new GameLayout();
        layout.setPrefab(prefab);
        layout.setType(info.type);
        layout.setName(info.name);
        layout.setParent(layoutParent);
        layout.setOrderType(info.orderType);
        var order = generateRenderOrder(layout, info.renderOrder, info.orderType);
        layout.setRenderOrder(order);
        layout.setInResources(layoutInfos.get(info.type).inResource);
        layout.init();
        if (layout.getRoot().getObject() != layoutObj)
        {
            logError("布局的根节点不是实例化出来的节点,请确保运行前UI根节点下没有与布局同名的节点");
        }

        layouts.add(info.type, layout);
        return layout;
    }
}