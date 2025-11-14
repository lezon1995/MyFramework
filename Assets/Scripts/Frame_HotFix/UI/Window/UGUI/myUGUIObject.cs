using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityUtility;
using static MathUtility;
using static WidgetUtility;
using static CSharpUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// UGUI窗口的基类
public class myUGUIObject : Transformable, IMouseEventCollect
{
    protected static Comparison<Transform> _compareDescend = compareZDecending; // 避免GC的回调
    private static Comparison<myUGUIObject> _compareSiblingIndex = compareSiblingIndex; // 用于避免GC的委托
    private static bool allowDestroyWindow; // 是否允许销毁窗口,仅此类内部使用
    protected COMWindowInteractive interactive; // 鼠标键盘响应逻辑的组件
    protected COMWindowCollider collider; // 碰撞逻辑组件
    protected HashSet<myUGUIObject> childrenSet; // 子节点列表,用于查询是否有子节点
    protected List<myUGUIObject> childrenList; // 子节点列表,与GameObject的子节点顺序保持一致(已排序情况下),用于获取最后一个子节点
    protected GameLayout layout; // 所属布局
    protected myUGUIObject parent; // 父节点窗口
    protected Vector3 lastWorldScale; // 上一次设置的世界缩放值
    protected int uid; // 每个窗口的唯一ID
    protected bool destroyImmediately; // 销毁窗口时是否立即销毁
    protected bool receiveLayoutHide; // 布局隐藏时是否会通知此窗口,默认不通知
    protected bool childOrderSorted; // 子节点在列表中的顺序是否已经按照面板上的顺序排序了
    protected bool isNewObject; // 是否是从空的GameObject创建的,一般都是会确认已经存在了对应组件,而不是要动态添加组件
    protected COMWindowUGUIInteractive uiInteractive; // UGUI的鼠标键盘响应逻辑的组件
    protected RectTransform rectT; // UGUI的Transform

    public myUGUIObject()
    {
        uid = makeID();
        needUpdate = false; // 出于效率考虑,窗口默认不启用更新,只有部分窗口和使用组件进行变化时才自动启用更新
        destroyed = false; // 由于一般myUGUIObject不会使用对象池来管理,所以构造时就设置当前对象为有效
    }

    public virtual void init()
    {
        initComponents();
        layout?.registerUIObject(this);
        if (go.TryGetComponent<BoxCollider>(out var boxCollider))
        {
            getCOMCollider().setBoxCollider(boxCollider);
        }

        // 因为在使用UGUI时,原来的Transform会被替换为RectTransform,所以需要重新设置Transform组件
        if (!go.TryGetComponent(out rectT))
        {
            rectT = go.AddComponent<RectTransform>();
        }

        t = rectT;
        if (rectT == null)
        {
            if (t)
            {
                logError("Transform不是RectTransform,name:" + t.name);
            }
            else
            {
                logError("RectTransform为空");
            }
        }

        collider?.setColliderSize(rectT);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        uiInteractive = null;
        rectT = null;
    }

    public void onLayoutHide()
    {
        // 布局隐藏时需要将触点清除
        uiInteractive?.clearMousePointer();
    }

    // 将当前窗口的顶部对齐父节点的顶部
    public void alignParentTop()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowTopInParent(uiObj.getWindowTop());
    }

    // 将当前窗口的顶部中心对齐父节点的顶部中心
    public void alignParentTopCenter()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowTopInParent(uiObj.getWindowTop());
        setWindowInParentCenterX();
    }

    // 将当前窗口的底部对齐父节点的底部
    public void alignParentBottom()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowBottomInParent(uiObj.getWindowBottom());
    }

    // 将当前窗口的底部中心对齐父节点的底部中心
    public void alignParentBottomCenter()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowBottomInParent(uiObj.getWindowBottom());
        setWindowInParentCenterX();
    }

    // 将当前窗口的左边界对齐父节点的左边界
    public void alignParentLeft()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowLeftInParent(uiObj.getWindowLeft());
    }

    // 将当前窗口的左边界中心对齐父节点的左边界中心
    public void alignParentLeftCenter()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowLeftInParent(uiObj.getWindowLeft());
        setWindowInParentCenterY();
    }

    // 将当前窗口的右边界对齐父节点的右边界
    public void alignParentRight()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowRightInParent(uiObj.getWindowRight());
    }

    // 将当前窗口的右边界中心对齐父节点的右边界中心
    public void alignParentRightCenter()
    {
        if (parent is not { } uiObj)
        {
            logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
            return;
        }

        setWindowRightInParent(uiObj.getWindowRight());
        setWindowInParentCenterY();
    }

    // 设置窗口在父节点中横向居中
    public void setWindowInParentCenterX()
    {
        setPositionX(0.0f);
    }

    // 设置窗口在父节点中纵向居中
    public void setWindowInParentCenterY()
    {
        setPositionY(0.0f);
    }

    // 设置窗口左边界在父节点中的X坐标
    public void setWindowLeftInParent(float leftInParent)
    {
        setPositionX(leftInParent - getWindowLeft());
    }

    // 设置窗口右边界在父节点中的X坐标
    public void setWindowRightInParent(float rightInParent)
    {
        setPositionX(rightInParent - getWindowRight());
    }

    // 设置窗口顶部在父节点中的Y坐标
    public void setWindowTopInParent(float topInParent)
    {
        setPositionY(topInParent - getWindowTop());
    }

    // 设置窗口底部在父节点中的Y坐标
    public void setWindowBottomInParent(float bottomInParent)
    {
        setPositionY(bottomInParent - getWindowBottom());
    }

    // 获得窗口左边界在父窗口中的X坐标
    public float getWindowLeftInParent()
    {
        return getPosition().x + getWindowLeft();
    }

    // 获得窗口右边界在父窗口中的X坐标
    public float getWindowRightInParent()
    {
        return getPosition().x + getWindowRight();
    }

    // 获得窗口顶部在父窗口中的Y坐标
    public float getWindowTopInParent()
    {
        return getPosition().y + getWindowTop();
    }

    // 获得窗口底部在父窗口中的Y坐标
    public float getWindowBottomInParent()
    {
        return getPosition().y + getWindowBottom();
    }

    // 获得窗口顶部在窗口中的相对于窗口pivot的Y坐标
    public float getWindowTop()
    {
        return getWindowSize().y * (1.0f - getPivot().y);
    }

    // 获得窗口底部在窗口中的相对于窗口pivot的Y坐标
    public float getWindowBottom()
    {
        return -getWindowSize().y * getPivot().y;
    }

    // 获得窗口左边界在窗口中的相对于窗口pivot的X坐标
    public float getWindowLeft()
    {
        return -getWindowSize().x * getPivot().x;
    }

    // 获得窗口右边界在窗口中的相对于窗口pivot的X坐标
    public float getWindowRight()
    {
        return getWindowSize().x * (1.0f - getPivot().x);
    }

    // 获取不考虑中心点偏移的坐标,也就是固定获取窗口中心的坐标
    // 由于pivot的影响,Transform.localPosition获得的坐标并不一定等于窗口中心的坐标
    public Vector3 getPositionNoPivot()
    {
        return WidgetUtility.getPositionNoPivot(rectT);
    }

    public Vector2 getPivot()
    {
        return rectT.pivot;
    }

    public void setPivot(Vector2 pivot)
    {
        rectT.pivot = pivot;
    }

    public RectTransform getRectTransform()
    {
        return rectT;
    }

    public void setWindowWidth(float width)
    {
        if (isFloatEqual(rectT.rect.size.x, width))
            return;

        // 还是需要调用setWindowSize,需要触发一些虚函数的调用
        setWindowSize(replaceX(getWindowSize(), width));
    }

    public void setWindowHeight(float height)
    {
        if (isFloatEqual(rectT.rect.size.y, height))
            return;

        // 还是需要调用setWindowSize,需要触发一些虚函数的调用
        setWindowSize(replaceY(getWindowSize(), height));
    }

    public virtual void setWindowSize(Vector2 size)
    {
        if (isVectorEqual(rectT.rect.size, size))
            return;

        setRectSize(rectT, size);
        ensureColliderSize();
    }

    public virtual Vector2 getWindowSize(bool transformed = false)
    {
        Vector2 windowSize = rectT.rect.size;
        if (transformed)
        {
            windowSize = multiVector2(windowSize, getWorldScale());
        }

        return windowSize;
    }

    public virtual void setAlpha(float alpha, bool fadeChild)
    {
        if (fadeChild)
        {
            setUGUIChildAlpha(go, alpha);
        }
    }

    public virtual void cloneFrom(myUGUIObject obj)
    {
        if (obj.GetType() != GetType())
        {
            logError("type is different, can not clone!, this:" + GetType() + ", source:" + obj.GetType());
            return;
        }

        setPosition(obj.getPosition());
        setRotation(obj.getRotationQuaternion());
        setScale(obj.getScale());
    }

    public void refreshChildDepthByPositionZ()
    {
        // z值越大的子节点越靠后
        using var a = new ListScope<Transform>(out var tempList);
        int childCount = getChildCount();
        for (int i = 0; i < childCount; ++i)
        {
            tempList.Add(t.GetChild(i));
        }

        quickSort(tempList, _compareDescend);
        int count = tempList.Count;
        for (int i = 0; i < count; ++i)
        {
            tempList[i].SetSiblingIndex(i);
        }
    }

    public void setUGUIClick(Action<PointerEventData, GameObject> callback)
    {
        getCOMUGUIInteractive().setUGUIClick(callback);
    }

    public void setUGUIMouseDown(Action<PointerEventData, GameObject> callback)
    {
        getCOMUGUIInteractive().setUGUIMouseDown(callback);
        // 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
        receiveLayoutHide = true;
    }

    public void setUGUIMouseUp(Action<PointerEventData, GameObject> callback)
    {
        getCOMUGUIInteractive().setUGUIMouseUp(callback);
        // 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
        receiveLayoutHide = true;
    }

    public void setUGUIMouseEnter(Action<PointerEventData, GameObject> callback)
    {
        getCOMUGUIInteractive().setUGUIMouseEnter(callback);
    }

    public void setUGUIMouseExit(Action<PointerEventData, GameObject> callback)
    {
        getCOMUGUIInteractive().setUGUIMouseExit(callback);
    }

    public void setUGUIMouseMove(Action<Vector2, Vector3> callback)
    {
        getCOMUGUIInteractive().setUGUIMouseMove(callback);
        // 如果设置了要监听鼠标移动,则需要激活当前窗口
        needUpdate = true;
    }

    public void setUGUIMouseStay(Action<Vector3> callback)
    {
        getCOMUGUIInteractive().setUGUIMouseStay(callback);
        needUpdate = true;
    }

    public override void destroy()
    {
        if (!allowDestroyWindow)
            logError("can not call window's destroy()! use destroyWindow(myUGUIObject window, bool destroyReally) instead");

        base.destroy();
        destroyed = true;
    }

    public override void setActive(bool active)
    {
        if (active == isActive())
            return;

        base.setActive(active);
        mGlobalTouchSystem?.notifyWindowActiveChanged();
    }

    public static void collectChild<T>(myUGUIObject window, List<T> list) where T : myUGUIObject
    {
        list.addNotNull(window as T);
        foreach (myUGUIObject item in window.childrenList.safe())
        {
            collectChild(item, list);
        }
    }

    public static void destroyWindow(myUGUIObject window, bool destroyReally)
    {
        if (window == null)
            return;

        // 先销毁所有子节点,因为遍历中会修改子节点列表,所以需要复制一个列表
        if (!window.childrenList.isEmpty())
        {
            using var a = new ListScope<myUGUIObject>(out var childList, window.childrenList);
            foreach (myUGUIObject item in childList)
            {
                destroyWindow(item, destroyReally);
            }
        }

        window.getParent()?.removeChild(window);
        // 再销毁自己
        destroyWindowSingle(window, destroyReally);
    }

    public static void destroyWindowSingle(myUGUIObject window, bool destroyReally)
    {
        mGlobalTouchSystem?.unregisterCollider(window);
        if (window is IInputField inputField)
        {
            mInputSystem?.unregisteInputField(inputField);
        }

        window.layout?.unregisterUIObject(window);
        GameObject go = window.getObject();
        allowDestroyWindow = true;
        window.destroy();
        allowDestroyWindow = false;
        window.layout = null;
        if (destroyReally)
        {
            destroyUnityObject(go, window.destroyImmediately);
        }
    }

    public void removeChild(myUGUIObject child)
    {
        if (childrenSet != null && childrenSet.Remove(child))
        {
            childrenList?.Remove(child);
        }
    }

    public override void update(float dt)
    {
        base.update(dt);

        ensureColliderSize();

        // 检测世界缩放值是否有变化
        if (!isVectorEqual(lastWorldScale, getWorldScale()))
        {
            onWorldScaleChanged(lastWorldScale);
            lastWorldScale = getWorldScale();
        }
    }

    public override Collider getCollider(bool addIfNotExist = false)
    {
        var collider = tryGetUnityComponent<Collider>();
        // 由于Collider无法直接添加到GameObject上,所以只能默认添加BoxCollider
        if (addIfNotExist && collider == null)
        {
            collider = getOrAddUnityComponent<BoxCollider>();
            getCOMCollider().setBoxCollider((BoxCollider)collider);
            // 新加的碰撞盒需要设置大小
            ensureColliderSize();
        }

        return collider;
    }

    public void setAsLastSibling(bool refreshUIDepth = true)
    {
        t.SetAsLastSibling();
        childOrderSorted = false;
        if (refreshUIDepth)
        {
            layout.refreshUIDepth(parent, true);
        }
    }

    public void setAsFirstSibling(bool refreshUIDepth = true)
    {
        t.SetAsFirstSibling();
        childOrderSorted = false;
        if (refreshUIDepth)
        {
            layout.refreshUIDepth(parent, true);
        }
    }

    public int getSibling()
    {
        return t.GetSiblingIndex();
    }

    public bool setSibling(int index, bool refreshUIDepth = true)
    {
        if (t.GetSiblingIndex() == index)
            return false;

        t.SetSiblingIndex(index);
        childOrderSorted = false;
        if (refreshUIDepth)
        {
            layout.refreshUIDepth(parent, true);
        }

        return true;
    }

    // 当自适应更新完以后调用
    public virtual void notifyAnchorApply()
    {
    }

    // 获取描述,UI则返回所处布局名
    public string getDescription()
    {
        return layout?.getName();
    }

    // get
    //------------------------------------------------------------------------------------------------------------------------------
    public int getID()
    {
        return uid;
    }

    public GameLayout getLayout()
    {
        return layout;
    }

    public List<myUGUIObject> getChildList()
    {
        return childrenList;
    }

    public virtual bool isReceiveScreenMouse()
    {
        return interactive?.getOnScreenMouseUp() != null;
    }

    public myUGUIObject getParent()
    {
        return parent;
    }

    public override float getAlpha()
    {
        return 1.0f;
    }

    public virtual Color getColor()
    {
        return Color.white;
    }

    public UIDepth getDepth()
    {
        return getCOMInteractive().getDepth();
    }

    public virtual bool isHandleInput()
    {
        return collider != null && collider.isHandleInput();
    }

    public virtual bool isPassRay()
    {
        return interactive == null || interactive.isPassRay();
    }

    public virtual bool isPassDragEvent()
    {
        return !isDragable() || (interactive != null && interactive.isPassDragEvent());
    }

    public virtual bool isDragable()
    {
        return getActiveComponent<COMWindowDrag>() != null;
    }

    public bool isMouseHovered()
    {
        return interactive != null && interactive.isMouseHovered();
    }

    public int getClickSound()
    {
        return interactive?.getClickSound() ?? 0;
    }

    public bool isDepthOverAllChild()
    {
        return interactive != null && interactive.isDepthOverAllChild();
    }

    public float getLongPressLengthThreshold()
    {
        return interactive?.getLongPressLengthThreshold() ?? -1.0f;
    }

    public bool isReceiveLayoutHide()
    {
        return receiveLayoutHide;
    }

    public bool isColliderForClick()
    {
        return interactive != null && interactive.isColliderForClick();
    }

    public bool isAllowGenerateDepth()
    {
        return interactive == null || interactive.isAllowGenerateDepth();
    }

    // 是否可以计算深度,与mAllowGenerateDepth类似,都是计算深度的其中一个条件,只不过这个可以由子类重写
    public virtual bool canGenerateDepth()
    {
        return true;
    }

    public virtual bool isCulled()
    {
        return false;
    }

    public override bool raycastSelf(ref Ray ray, out RaycastHit hit, float maxDistance)
    {
        if (collider == null)
        {
            hit = new();
            return false;
        }

        return collider.raycast(ref ray, out hit, maxDistance);
    }

    public virtual float getFillPercent()
    {
        logError("can not get window fill percent with myUGUIObject");
        return 1.0f;
    }

    // 递归返回最后一个子节点,如果没有子节点,则返回空
    public myUGUIObject getLastChild()
    {
        if (childrenList.isEmpty())
            return null;

        if (childrenList.Count > 1 && !childOrderSorted)
        {
            logError("子节点没有被排序,无法获得正确的最后一个子节点");
        }

        myUGUIObject lastChild = childrenList[^1];
        if (lastChild.childrenList.Count == 0)
            return lastChild;

        return lastChild.getLastChild();
    }

    // set
    //------------------------------------------------------------------------------------------------------------------------------
    public void setDepthOverAllChild(bool depthOver)
    {
        getCOMInteractive().setDepthOverAllChild(depthOver);
    }

    public void setDestroyImmediately(bool immediately)
    {
        destroyImmediately = immediately;
    }

    public void setAllowGenerateDepth(bool allowGenerate)
    {
        getCOMInteractive().setAllowGenerateDepth(allowGenerate);
    }

    public virtual void setColor(Color color)
    {
    }

    public virtual void setFillPercent(float percent)
    {
        logError("can not set window fill percent with myUGUIObject");
    }

    public void setPassRay(bool passRay)
    {
        getCOMInteractive().setPassRay(passRay);
    }

    public void setPassDragEvent(bool pass)
    {
        getCOMInteractive().setPassDragEvent(pass);
    }

    public void setDepth(UIDepth parentDepth, int orderInParent)
    {
        getCOMInteractive().setDepth(parentDepth, orderInParent);
    }

    public void setLongPressLengthThreshold(float threshold)
    {
        getCOMInteractive().setLongPressLengthThreshold(threshold);
    }

    // 自己调用的callback,仅在启用自定义输入系统时生效
    public void setPreClickCallback(Action callback)
    {
        getCOMInteractive().setPreClickCallback(callback);
    }

    public void setPreClickDetailCallback(ClickCallback callback)
    {
        getCOMInteractive().setPreClickDetailCallback(callback);
    }

    public void setClickCallback(Action callback)
    {
        getCOMInteractive().setClickCallback(callback);
    }

    public void setClickDetailCallback(ClickCallback callback)
    {
        getCOMInteractive().setClickDetailCallback(callback);
    }

    public void setHoverCallback(BoolCallback callback)
    {
        getCOMInteractive().setHoverCallback(callback);
    }

    public void setHoverDetailCallback(HoverCallback callback)
    {
        getCOMInteractive().setHoverDetailCallback(callback);
    }

    public void setPressCallback(BoolCallback callback)
    {
        getCOMInteractive().setPressCallback(callback);
    }

    public void setPressDetailCallback(PressCallback callback)
    {
        getCOMInteractive().setPressDetailCallback(callback);
    }

    public void setDoubleClickCallback(Action callback)
    {
        getCOMInteractive().setDoubleClickCallback(callback);
    }

    public void setDoubleClickDetailCallback(ClickCallback callback)
    {
        getCOMInteractive().setDoubleClickDetailCallback(callback);
    }

    public void setOnReceiveDrag(OnReceiveDrag callback)
    {
        getCOMInteractive().setOnReceiveDrag(callback);
    }

    public void setOnDragHover(OnDragHover callback)
    {
        getCOMInteractive().setOnDragHover(callback);
    }

    public void setOnMouseEnter(OnMouseEnter callback)
    {
        getCOMInteractive().setOnMouseEnter(callback);
    }

    public void setOnMouseLeave(OnMouseLeave callback)
    {
        getCOMInteractive().setOnMouseLeave(callback);
    }

    public void setOnMouseDown(Vector3IntCallback callback)
    {
        getCOMInteractive().setOnMouseDown(callback);
    }

    public void setOnMouseUp(Vector3IntCallback callback)
    {
        getCOMInteractive().setOnMouseUp(callback);
    }

    public void setOnMouseMove(OnMouseMove callback)
    {
        getCOMInteractive().setOnMouseMove(callback);
    }

    public void setOnMouseStay(Vector3IntCallback callback)
    {
        getCOMInteractive().setOnMouseStay(callback);
    }

    public void setOnScreenMouseUp(OnScreenMouseUp callback)
    {
        getCOMInteractive().setOnScreenMouseUp(callback);
    }

    public void setLayout(GameLayout l)
    {
        layout = l;
    }

    public void setReceiveLayoutHide(bool receive)
    {
        receiveLayoutHide = receive;
    }

    public void setColliderForClick(bool forClick)
    {
        getCOMInteractive().setColliderForClick(forClick);
    }

    public void setClickSound(int sound)
    {
        getCOMInteractive().setClickSound(sound);
    }

    public override void setObject(GameObject go)
    {
        setName(go.name);
        base.setObject(go);
        if (isEditor())
        {
            // 由于物体可能使克隆出来的,所以如果已经添加了调试组件,直接获取即可
            getOrAddUnityComponent<WindowDebug>().setWindow(this);
        }
    }

    public void setParent(myUGUIObject p, bool refreshDepth)
    {
        if (parent == p)
            return;

        // 从原来的父节点上移除
        parent?.removeChild(this);
        // 设置新的父节点
        parent = p;
        if (parent != null)
        {
            // 先设置Transform父节点,因为在addChild中会用到Transform的GetSiblingIndex
            if (t.parent != parent.getTransform())
            {
                t.SetParent(parent.getTransform());
            }

            parent.addChild(this, refreshDepth);
        }
    }

    public virtual void setHandleInput(bool enable)
    {
        collider?.enableCollider(enable);
    }

    public void addLongPress(Action callback, float pressTime, FloatCallback pressingCallback = null)
    {
        getCOMInteractive().addLongPress(callback, pressTime, pressingCallback);
    }

    public void removeLongPress(Action callback)
    {
        interactive?.removeLongPress(callback);
    }

    public void clearLongPress()
    {
        interactive?.clearLongPress();
    }

    // callback
    //------------------------------------------------------------------------------------------------------------------------------
    public virtual void onMouseEnter(Vector3 mousePos, int touchID)
    {
        interactive?.onMouseEnter(mousePos, touchID);
    }

    public virtual void onMouseLeave(Vector3 mousePos, int touchID)
    {
        interactive?.onMouseLeave(mousePos, touchID);
    }

    // 鼠标左键在窗口内按下
    public virtual void onMouseDown(Vector3 mousePos, int touchID)
    {
        interactive?.onMouseDown(mousePos, touchID);
    }

    // 鼠标左键在窗口内放开
    public virtual void onMouseUp(Vector3 mousePos, int touchID)
    {
        interactive?.onMouseUp(mousePos, touchID);
    }

    // 触点在移动,此时触点是按下状态,且按下瞬间在窗口范围内
    public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
    {
        interactive?.onMouseMove(mousePos, moveDelta, moveTime, touchID);
    }

    // 触点没有移动,此时触点是按下状态,且按下瞬间在窗口范围内
    public virtual void onMouseStay(Vector3 mousePos, int touchID)
    {
        interactive?.onMouseStay(mousePos, touchID);
    }

    // 鼠标在屏幕上抬起
    public virtual void onScreenMouseUp(Vector3 mousePos, int touchID)
    {
        interactive?.onScreenMouseUp(mousePos, touchID);
    }

    // 鼠标在屏幕上按下
    public virtual void onScreenMouseDown(Vector3 mousePos, int touchID)
    {
        interactive?.onScreenMouseDown(mousePos, touchID);
    }

    // 有物体拖动到了当前窗口上
    public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent)
    {
        interactive?.onReceiveDrag(dragObj, mousePos, ref continueEvent);
    }

    // 有物体拖动到了当前窗口上
    public virtual void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover)
    {
        interactive?.onDragHoverd(dragObj, mousePos, hover);
    }

    public void sortChild()
    {
        if (childrenList.count() <= 1)
            return;

        childOrderSorted = true;
        quickSort(childrenList, _compareSiblingIndex);
    }

    // registerEvent,这些函数只是用于简化注册碰撞体的操作
    public void registerCollider(Action clickCallback, Action preClick, bool passRay = false)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickCallback(clickCallback);
        setPreClickCallback(preClick);
        setPassRay(passRay);
        setNeedUpdate(true);
    }

    // 用于接收GlobalTouchSystem处理的输入事件
    public void registerCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, bool passRay)
    {
        mGlobalTouchSystem.registerCollider(this);
        setObjectCallback(clickCallback, hoverCallback, pressCallback);
        setPassRay(passRay);
        // 由碰撞体的窗口都需要启用更新,以便可以保证窗口大小与碰撞体大小一致
        setNeedUpdate(true);
    }

    public void registerCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, GameCamera camera)
    {
        mGlobalTouchSystem.registerCollider(this, camera);
        setObjectCallback(clickCallback, hoverCallback, pressCallback);
        setPassRay(false);
        setNeedUpdate(true);
    }

    public void registerCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback)
    {
        mGlobalTouchSystem.registerCollider(this);
        setObjectCallback(clickCallback, hoverCallback, pressCallback);
        setPassRay(false);
        setNeedUpdate(true);
    }

    public void registerCollider(Action clickCallback, bool passRay)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickCallback(clickCallback);
        setPassRay(passRay);
        setNeedUpdate(true);
    }

    public void registerCollider(ClickCallback clickCallback, bool passRay)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickDetailCallback(clickCallback);
        setPassRay(passRay);
        setNeedUpdate(true);
    }

    public void registerCollider(Action clickCallback, int clickSound)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickCallback(clickCallback);
        setPassRay(false);
        setNeedUpdate(true);
        setClickSound(clickSound);
    }

    public void registerCollider(Action clickCallback, bool passRay, int clickSound)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickCallback(clickCallback);
        setPassRay(passRay);
        setNeedUpdate(true);
        setClickSound(clickSound);
    }

    public void registerCollider(ClickCallback clickCallback, int clickSound)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickDetailCallback(clickCallback);
        setPassRay(false);
        setNeedUpdate(true);
        setClickSound(clickSound);
    }

    public void registerCollider(Action clickCallback)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickCallback(clickCallback);
        setPassRay(false);
        setNeedUpdate(true);
    }

    public void registerCollider(Action clickCallback, GameCamera camera)
    {
        mGlobalTouchSystem.registerCollider(this, camera);
        setClickCallback(clickCallback);
        setPassRay(false);
        setNeedUpdate(true);
    }

    public void registerCollider(ClickCallback clickCallback)
    {
        mGlobalTouchSystem.registerCollider(this);
        setClickDetailCallback(clickCallback);
        setPassRay(false);
        setNeedUpdate(true);
    }

    public void registerCollider(ClickCallback clickCallback, GameCamera camera)
    {
        setClickDetailCallback(clickCallback);
        mGlobalTouchSystem.registerCollider(this, camera);
        setPassRay(false);
        setNeedUpdate(true);
    }

    public void registerCollider(bool passRay)
    {
        mGlobalTouchSystem.registerCollider(this);
        setPassRay(passRay);
        setNeedUpdate(true);
    }

    public void registerCollider()
    {
        mGlobalTouchSystem.registerCollider(this);
        setPassRay(false);
        setNeedUpdate(true);
    }

    public void unregisterCollider()
    {
        mGlobalTouchSystem?.unregisterCollider(this);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected static int compareZDecending(Transform a, Transform b)
    {
        return (int)sign(b.localPosition.z - a.localPosition.z);
    }

    protected virtual void ensureColliderSize()
    {
        // 确保RectTransform和BoxCollider一样大
        collider?.setColliderSize(rectT);
    }

    protected COMWindowUGUIInteractive getCOMUGUIInteractive()
    {
        return uiInteractive ??= addComponent<COMWindowUGUIInteractive>(true);
    }

    protected virtual void onWorldScaleChanged(Vector2 lastWorldScale)
    {
    }

    protected void addChild(myUGUIObject child, bool refreshDepth)
    {
        childrenSet ??= new();
        if (!childrenSet.Add(child))
            return;

        childrenList ??= new();
        childrenList.Add(child);
        childOrderSorted = false;
        if (refreshDepth)
        {
            layout.refreshUIDepth(this, false);
        }
    }

    protected static int compareSiblingIndex(myUGUIObject child0, myUGUIObject child1)
    {
        return sign(child0.getTransform().GetSiblingIndex() - child1.getTransform().GetSiblingIndex());
    }

    protected COMWindowInteractive getCOMInteractive()
    {
        return interactive ??= addComponent<COMWindowInteractive>(true);
    }

    protected COMWindowCollider getCOMCollider()
    {
        return collider ??= addComponent<COMWindowCollider>(true);
    }

    protected void setObjectCallback(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback)
    {
        setClickCallback(clickCallback);
        setPressCallback(pressCallback);
        setHoverCallback(hoverCallback);
    }
}