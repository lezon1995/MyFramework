using System;
using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;
using static WidgetUtility;
using static FrameBase;
using static FrameBaseDefine;

// 用于表示一个布局
public class GameLayout
{
    protected Canvas canvas; // 布局Canvas
    protected Transform t; // 布局根节点
    protected GameObject prefab; // 布局预设,布局从该预设实例化
    protected Type type; // 布局的脚本类型
    protected int renderOrder; // 渲染顺序,越大则渲染优先级越高,不能小于0
    protected bool anchorApplied; // 是否已经完成了自适应的调整

    public virtual void assignWindow()
    {
    }

    public virtual void update(float dt)
    {
    }

    // 重置布局状态后,再根据当前游戏状态设置布局显示前的状态
    public virtual void onGameState()
    {
    }

    public virtual void onHide()
    {
    }

    public void close()
    {
        setVisible(false);
    }

    public void getUIComponent<T>(out T com, string name) where T : Component
    {
        com = getGameObject(name, canvas.gameObject).GetComponent<T>();
    }

    public static void getUIComponent<T>(out T com, Component parent, string name) where T : Component
    {
        com = getGameObject(name, parent.gameObject).GetComponent<T>();
    }

    public virtual void init()
    {
    }

    public void initLayout()
    {
        mLayoutManager.notifyLayoutChanged(this, true);

        // 初始化布局脚本
        canvas = getGameObject(type.ToString(), mLayoutManager.getUIRoot().gameObject).GetComponent<Canvas>();

        // 去除自带的锚点
        // 在unity2020中,不知道为什么实例化以后的RectTransform的大小会自动变为视图窗口大小,为了适配计算正确,这里需要重置一次
        var rectT = canvas.GetComponent<RectTransform>();
        rectT.anchorMin = Vector2.one * 0.5f;
        rectT.anchorMax = Vector2.one * 0.5f;
        setRectSize(rectT, new(STANDARD_WIDTH, STANDARD_HEIGHT));

        t = canvas.gameObject.transform;
        assignWindow();
        // 布局实例化完成,初始化之前,需要调用自适应组件的更新
        applyAnchor(canvas.gameObject, true, this);
        anchorApplied = true;
        init();
        // init后再次设置布局的渲染顺序,这样可以在此处刷新所有窗口的深度,因为是否刷新跟是否注册了碰撞体有关
        // 所以在assignWindow和init中不需要在创建窗口对象时刷新深度,这样会造成很大的性能浪费
        setRenderOrder(renderOrder);
        // 加载完布局后强制隐藏
        canvas.gameObject.SetActive(false);
    }

    public void updateLayout(float elapsedTime)
    {
        if (!isVisible())
            return;

        // 更新脚本逻辑
        update(elapsedTime);
    }

    public void destroy()
    {
        mLayoutManager.notifyLayoutChanged(this, false);
        destroyUnityObject(canvas.gameObject);
        canvas = null;
        t = null;
        res.unloadInResources(ref prefab);
    }

    public void setRenderOrder(int order)
    {
        renderOrder = order;
        if (renderOrder < 0)
        {
            logErrorBase("布局深度不能小于0,否则无法正确计算窗口深度");
            return;
        }

        if (canvas == null)
            return;

        canvas.sortingOrder = renderOrder;
    }

    public void setVisible(bool visible)
    {
        if (t == null)
            return;

        if (visible == isVisible())
            return;

        // 显示布局时立即显示
        if (visible)
        {
            canvas.gameObject.SetActive(true);
            onGameState();
        }
        // 隐藏布局时需要判断
        else
        {
            canvas.gameObject.SetActive(false);
            onHide();
        }
    }

    public Canvas getRoot()
    {
        return canvas;
    }

    public string getName()
    {
        return type.ToString();
    }

    public Type getType()
    {
        return type;
    }

    public bool isVisible()
    {
        return canvas && canvas.gameObject.activeInHierarchy;
    }

    public void setPrefab(GameObject p)
    {
        prefab = p;
    }

    public void setType(Type t)
    {
        type = t;
    }
}