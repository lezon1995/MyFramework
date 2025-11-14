using System;
using System.Collections.Generic;
using static FrameUtility;
using static MathUtility;

// 布局加载时的一些信息
public class LayoutLoadInfo : ClassObject
{
    public GameLayout layout; // 加载的布局
    public Type type; // 布局类型
    public int renderOrder; // 布局渲染顺序
    public LAYOUT_ORDER orderType; // 布局渲染顺序类型
    public bool isScene; // 是否为场景UI

    public override void resetProperty()
    {
        base.resetProperty();
        type = null;
        renderOrder = 0;
        orderType = LAYOUT_ORDER.ALWAYS_TOP;
        layout = null;
        isScene = false;
    }
}

// 用于批量异步加载布局,封装一些通用的逻辑,需要通过LayoutLoadGroup.create来创建,会自动回收
public class LayoutLoadGroup : ClassObject
{
    protected Dictionary<Type, LayoutLoadInfo> loadInfos = new(); // 要加载的布局列表
    protected Action onLoaded; // 所有布局加载完成时的回调
    protected GameLayoutCallback onLoading; // 单个布局加载完成时的回调
    protected int loadedCount; // 加载完成数量
    protected bool autoDestroy; // 加载完成后是否自动销毁

    public override void resetProperty()
    {
        base.resetProperty();
        loadInfos.Clear();
        onLoaded = null;
        onLoading = null;
        loadedCount = 0;
        autoDestroy = false;
    }

    public override void destroy()
    {
        base.destroy();
        UN_CLASS_LIST(loadInfos);
    }

    public static LayoutLoadGroup create(bool autoDestroy = true)
    {
        var obj = CLASS<LayoutLoadGroup>();
        obj.autoDestroy = autoDestroy;
        return obj;
    }

    public CustomAsyncOperation startLoad(Action loadedCallback, GameLayoutCallback loadingCallback = null)
    {
        CustomAsyncOperation op = new();
        onLoaded = loadedCallback;
        onLoading = loadingCallback;
        foreach (var item in loadInfos)
        {
            if (item.Value.isScene)
            {
                LT.LOAD_SCENE_ASYNC_HIDE(item.Key, item.Value.renderOrder, (layout) =>
                {
                    onLayoutLoaded(layout, op);
                });
            }
            else
            {
                LT.LOAD_ASYNC_HIDE(item.Key, item.Value.renderOrder, item.Value.orderType, (layout) =>
                {
                    onLayoutLoaded(layout, op);
                });
            }
        }

        return op;
    }

    public void addLayout(Type type, int order = 0, LAYOUT_ORDER orderType = LAYOUT_ORDER.AUTO)
    {
        LayoutLoadInfo info = loadInfos.addClass(type);
        info.type = type;
        info.renderOrder = order;
        info.orderType = orderType;
        info.isScene = false;
    }

    public void addSceneUI(Type type, int order = 0)
    {
        LayoutLoadInfo info = loadInfos.addClass(type);
        info.type = type;
        info.renderOrder = order;
        info.orderType = LAYOUT_ORDER.FIXED;
        info.isScene = true;
    }

    public void addLayout<T>(int order = 0, LAYOUT_ORDER orderType = LAYOUT_ORDER.AUTO) where T : LayoutScript
    {
        addLayout(typeof(T), order, orderType);
    }

    public void addSceneUI<T>(int order = 0) where T : LayoutScript
    {
        addSceneUI(typeof(T), order);
    }

    public float getProgress()
    {
        return divide(loadedCount, loadInfos.Count);
    }

    public bool isAllLoaded()
    {
        return loadedCount == loadInfos.Count;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onLayoutLoaded(GameLayout layout, CustomAsyncOperation op)
    {
        if (!loadInfos.TryGetValue(layout.getType(), out LayoutLoadInfo info))
            return;

        info.layout = layout;
        ++loadedCount;
        onLoading?.Invoke(layout);
        if (loadedCount < loadInfos.Count)
            return;

        delayCall(() =>
        {
            op.setFinish();
            Action temp = onLoaded;
            if (autoDestroy)
            {
                UN_CLASS(this);
            }

            temp?.Invoke();
        });
    }
}