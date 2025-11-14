using System;
using UnityEngine;
using System.Collections.Generic;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameUtility;
using static FrameDefine;

// Window的鼠标相关事件的逻辑
public class COMWindowInteractive : GameComponent
{
    protected List<LongPressData> longPressList = new(); // 长按事件列表,可同时设置不同时长的长按回调事件
    protected Action onDoubleClick; // 双击回调,由GlobalTouchSystem驱动
    protected ClickCallback onDoubleClickDetail; // 双击回调,由GlobalTouchSystem驱动,带当前触点坐标
    protected Action onPreClick; // 单击的预回调,单击时会首先调用此回调,由GlobalTouchSystem驱动
    protected ClickCallback onPreClickDetail; // 单击的预回调,单击时会首先调用此回调,由GlobalTouchSystem驱动,带当前触点坐标
    protected OnReceiveDrag _onReceiveDrag; // 接收到有物体拖到当前窗口时的回调,由GlobalTouchSystem驱动
    protected OnDragHover _onDragHover; // 有物体拖拽悬停到当前窗口时的回调,由GlobalTouchSystem驱动
    protected Action onClick; // 单击回调,在预回调之后调用,由GlobalTouchSystem驱动
    protected ClickCallback onClickDetail; // 单击回调,在预回调之后调用,由GlobalTouchSystem驱动,带当前触点坐标
    protected BoolCallback onHover; // 悬停回调,由GlobalTouchSystem驱动
    protected HoverCallback onHoverDetail; // 悬停回调,由GlobalTouchSystem驱动,带当前触点坐标
    protected BoolCallback onPress; // 按下时回调,由GlobalTouchSystem驱动
    protected PressCallback onPressDetail; // 按下时回调,由GlobalTouchSystem驱动,带当前触点坐标
    protected OnScreenMouseUp _onScreenMouseUp; // 屏幕上鼠标抬起的回调,无论鼠标在哪儿,由GlobalTouchSystem驱动
    protected OnMouseEnter _onMouseEnter; // 鼠标进入时的回调,由GlobalTouchSystem驱动
    protected OnMouseLeave _onMouseLeave; // 鼠标离开时的回调,由GlobalTouchSystem驱动
    protected OnMouseMove _onMouseMove; // 鼠标移动的回调,由GlobalTouchSystem驱动
    protected Vector3IntCallback _onMouseStay; // 鼠标静止在当前窗口内的回调,由GlobalTouchSystem驱动
    protected Vector3IntCallback _onMouseDown; // 鼠标按下的回调,由GlobalTouchSystem驱动
    protected Vector3IntCallback _onMouseUp; // 鼠标抬起的回调,由GlobalTouchSystem驱动
    protected DateTime lastClickTime; // 上一次点击操作的时间,用于双击检测
    protected DateTime mouseDownTime; // 鼠标按下时的时间点
    protected Vector3 mouseDownPosition; // 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
    protected UIDepth depth; // UI深度,深度越大,渲染越靠前,越先接收到输入事件
    protected float longPressLengthThreshold = -1.0f; // 小于0表示不判断鼠标移动对长按检测的影响
    protected float pressedTime = -1.0f; // 小于0表示未计时,大于等于0表示正在计时长按操作,防止长时间按下时总会每隔指定时间调用一次回调
    protected int downTouchID; // 在此窗口下按下的触点ID
    protected int clickSound; // 点击时播放的音效ID,由于按钮音效播放的操作较多,所以统一到此处实现最基本的按钮点击音效播放
    protected bool depthOverAllChild; // 计算深度时是否将深度设置为所有子节点之上,实际调整的是mExtraDepth
    protected bool mouseHovered; // 当前鼠标是否悬停在窗口上
    protected bool pressing; // 鼠标当前是否在窗口中处于按下状态,鼠标离开窗口时认为鼠标不在按下状态
    protected bool passRay = true; // 当存在且注册了碰撞体时是否允许射线穿透
    protected bool colliderForClick = true; // 窗口上的碰撞体是否是用于鼠标点击的
    protected bool allowGenerateDepth = true; // 是否允许为当前窗口以及所有子节点计算深度,此变量只是计算深度的条件之一,一般由外部设置
    protected bool passDragEvent; // 是否将开始拖拽的事件穿透下去,使自己的下层也能够同时响应拖拽

    public COMWindowInteractive()
    {
        lastClickTime = DateTime.Now;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        longPressList.Clear();
        onDoubleClick = null;
        onDoubleClickDetail = null;
        onPreClick = null;
        onPreClickDetail = null;
        _onReceiveDrag = null;
        _onDragHover = null;
        onClick = null;
        onClickDetail = null;
        onHover = null;
        onHoverDetail = null;
        onPress = null;
        onPressDetail = null;
        _onScreenMouseUp = null;
        _onMouseEnter = null;
        _onMouseLeave = null;
        _onMouseMove = null;
        _onMouseStay = null;
        _onMouseDown = null;
        _onMouseUp = null;
        lastClickTime = DateTime.Now;
        mouseDownTime = DateTime.Now;
        mouseDownPosition = Vector3.zero;
        depth = null;
        longPressLengthThreshold = -1.0f;
        pressedTime = -1.0f;
        downTouchID = 0;
        clickSound = 0;
        depthOverAllChild = false;
        mouseHovered = false;
        pressing = false;
        passRay = true;
        colliderForClick = true;
        allowGenerateDepth = true;
        passDragEvent = false;
    }

    public override void update(float dt)
    {
        base.update(dt);
        // 长按检测,mPressedTime小于0表示长按计时无效
        if (pressing && (longPressLengthThreshold < 0.0f || lengthLess(mouseDownPosition - mInputSystem.getTouchPoint(downTouchID).getCurPosition(), longPressLengthThreshold)))
        {
            pressedTime += dt;
        }
        else
        {
            pressedTime = -1.0f;
        }

        if (pressing)
        {
            foreach (LongPressData item in longPressList)
            {
                item?.update(pressedTime);
            }
        }

        // 因为外部移除长按事件时只是将列表中对象设置为空,这是为了避免在遍历中移除而出错,所以此处需要再检查有没有已经被移除的长按事件
        for (int i = 0; i < longPressList.Count; ++i)
        {
            if (longPressList[i] == null)
            {
                longPressList.RemoveAt(i--);
            }
        }
    }

    public OnScreenMouseUp getOnScreenMouseUp()
    {
        return _onScreenMouseUp;
    }

    public bool isPassRay()
    {
        return passRay;
    }

    public bool isPassDragEvent()
    {
        return passDragEvent;
    }

    public bool isMouseHovered()
    {
        return mouseHovered;
    }

    public float getLongPressLengthThreshold()
    {
        return longPressLengthThreshold;
    }

    public bool isColliderForClick()
    {
        return colliderForClick;
    }

    public UIDepth getDepth()
    {
        return depth ??= new();
    }

    public bool isDepthOverAllChild()
    {
        return depthOverAllChild;
    }

    public bool isAllowGenerateDepth()
    {
        return allowGenerateDepth;
    }

    public int getClickSound()
    {
        return clickSound;
    }

    public void setDepth(UIDepth parentDepth, int orderInParent)
    {
        depth ??= new();
        depth.setDepthValue(parentDepth, orderInParent, depthOverAllChild);
    }

    public void setDepthOverAllChild(bool depthOver)
    {
        depthOverAllChild = depthOver;
    }

    public void setAllowGenerateDepth(bool allowGenerate)
    {
        allowGenerateDepth = allowGenerate;
    }

    public void setPassRay(bool passRay)
    {
        this.passRay = passRay;
    }

    public void setPassDragEvent(bool pass)
    {
        passDragEvent = pass;
    }

    public void setLongPressLengthThreshold(float threshold)
    {
        longPressLengthThreshold = threshold;
    }

    public void setClickSound(int sound)
    {
        clickSound = sound;
    }

    public void setPreClickCallback(Action callback)
    {
        onPreClick = callback;
    }

    public void setPreClickDetailCallback(ClickCallback callback)
    {
        onPreClickDetail = callback;
    }

    public void setClickCallback(Action callback)
    {
        onClick = callback;
    }

    public void setClickDetailCallback(ClickCallback callback)
    {
        onClickDetail = callback;
    }

    public void setHoverCallback(BoolCallback callback)
    {
        onHover = callback;
    }

    public void setHoverDetailCallback(HoverCallback callback)
    {
        onHoverDetail = callback;
    }

    public void setPressCallback(BoolCallback callback)
    {
        onPress = callback;
    }

    public void setPressDetailCallback(PressCallback callback)
    {
        onPressDetail = callback;
    }

    public void setDoubleClickCallback(Action callback)
    {
        onDoubleClick = callback;
    }

    public void setDoubleClickDetailCallback(ClickCallback callback)
    {
        onDoubleClickDetail = callback;
    }

    public void setOnReceiveDrag(OnReceiveDrag callback)
    {
        _onReceiveDrag = callback;
    }

    public void setOnDragHover(OnDragHover callback)
    {
        _onDragHover = callback;
    }

    public void setOnMouseEnter(OnMouseEnter callback)
    {
        _onMouseEnter = callback;
    }

    public void setOnMouseLeave(OnMouseLeave callback)
    {
        _onMouseLeave = callback;
    }

    public void setOnMouseDown(Vector3IntCallback callback)
    {
        _onMouseDown = callback;
    }

    public void setOnMouseUp(Vector3IntCallback callback)
    {
        _onMouseUp = callback;
    }

    public void setOnMouseMove(OnMouseMove callback)
    {
        _onMouseMove = callback;
    }

    public void setOnMouseStay(Vector3IntCallback callback)
    {
        _onMouseStay = callback;
    }

    public void setOnScreenMouseUp(OnScreenMouseUp callback)
    {
        _onScreenMouseUp = callback;
    }

    public void setColliderForClick(bool forClick)
    {
        colliderForClick = forClick;
    }

    public void addLongPress(Action callback, float pressTime, FloatCallback pressingCallback = null)
    {
        // 先判断是否已经有此长按回调了
        foreach (LongPressData item in longPressList)
        {
            if (item.onLongPressed == callback)
            {
                return;
            }
        }

        CLASS(out LongPressData data);
        data.onLongPressed = callback;
        data.onLongPressing = pressingCallback;
        data.longPressTime = pressTime;
        longPressList.Add(data);
    }

    public void removeLongPress(Action callback)
    {
        foreach (LongPressData data in longPressList)
        {
            if (data.onLongPressed == callback)
            {
                UN_CLASS(data);
                break;
            }
        }
    }

    public void clearLongPress()
    {
        UN_CLASS_LIST(longPressList);
    }

    public void onMouseEnter(Vector3 mousePos, int touchID)
    {
        var obj = owner as myUGUIObject;
        if (!mouseHovered)
        {
            mouseHovered = true;
            onHover?.Invoke(true);
            onHoverDetail?.Invoke(obj, mousePos, true);
        }

        _onMouseEnter?.Invoke(obj, mousePos, touchID);
        _onMouseMove?.Invoke(mousePos, Vector3.zero, 0.0f, touchID);
    }

    public void onMouseLeave(Vector3 mousePos, int touchID)
    {
        var obj = owner as myUGUIObject;
        if (mouseHovered)
        {
            mouseHovered = false;
            onHover?.Invoke(false);
            onHoverDetail?.Invoke(obj, mousePos, false);
        }

        pressing = false;
        pressedTime = -1.0f;
        _onMouseLeave?.Invoke(obj, mousePos, touchID);
        foreach (LongPressData data in longPressList)
        {
            data.onLongPressing?.Invoke(0.0f);
        }
    }

    // 鼠标左键在窗口内按下
    public void onMouseDown(Vector3 mousePos, int touchID)
    {
        pressing = true;
        pressedTime = 0.0f;
        mouseDownPosition = mousePos;
        mouseDownTime = DateTime.Now;
        downTouchID = touchID;
        onPress?.Invoke(true);
        var obj = owner as myUGUIObject;
        onPressDetail?.Invoke(obj, mousePos, true);
        _onMouseDown?.Invoke(mousePos, touchID);
        foreach (LongPressData data in longPressList)
        {
            data?.onLongPressing?.Invoke(0.0f);
            data?.reset();
        }

        // 如果是触屏的触点,则触点在当前窗口内按下时,认为时开始悬停
        TouchPoint touch = mInputSystem.getTouchPoint(touchID);
        if ((touch == null || !touch.isMouse()) && !mouseHovered)
        {
            mouseHovered = true;
            onHover?.Invoke(true);
            onHoverDetail?.Invoke(obj, mousePos, true);
        }
    }

    // 鼠标左键在窗口内放开
    public void onMouseUp(Vector3 mousePos, int touchID)
    {
        pressing = false;
        pressedTime = -1.0f;
        onPress?.Invoke(false);
        var obj = owner as myUGUIObject;
        onPressDetail?.Invoke(obj, mousePos, false);
        if (lengthLess(mouseDownPosition - mousePos, CLICK_LENGTH) &&
            (DateTime.Now - mouseDownTime).TotalSeconds < CLICK_TIME)
        {
            onPreClick?.Invoke();
            onPreClickDetail?.Invoke(obj, mousePos);
            onClick?.Invoke();
            onClickDetail?.Invoke(obj, mousePos);
            if (clickSound > 0)
            {
                AT.SOUND_2D(clickSound);
            }

            if ((DateTime.Now - lastClickTime).TotalSeconds < DOUBLE_CLICK_TIME)
            {
                onDoubleClick?.Invoke();
                onDoubleClickDetail?.Invoke(obj, mousePos);
            }

            lastClickTime = DateTime.Now;
        }

        _onMouseUp?.Invoke(mousePos, touchID);
        foreach (LongPressData data in longPressList)
        {
            data.onLongPressing?.Invoke(0.0f);
        }

        // 如果是触屏的触点,则触点在当前窗口内抬起时,认为已经取消悬停
        TouchPoint touch = mInputSystem.getTouchPoint(touchID);
        if ((touch == null || !touch.isMouse()) && mouseHovered)
        {
            mouseHovered = false;
            onHover?.Invoke(false);
            onHoverDetail?.Invoke(obj, mousePos, false);
        }
    }

    // 触点在移动,此时触点是按下状态,且按下瞬间在窗口范围内
    public void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
    {
        _onMouseMove?.Invoke(mousePos, moveDelta, moveTime, touchID);
    }

    // 触点没有移动,此时触点是按下状态,且按下瞬间在窗口范围内
    public void onMouseStay(Vector3 mousePos, int touchID)
    {
        _onMouseStay?.Invoke(mousePos, touchID);
    }

    // 鼠标在屏幕上抬起
    public void onScreenMouseUp(Vector3 mousePos, int touchID)
    {
        pressing = false;
        pressedTime = -1.0f;
        _onScreenMouseUp?.Invoke(owner as myUGUIObject, mousePos, touchID);
    }

    // 鼠标在屏幕上按下
    public void onScreenMouseDown(Vector3 mousePos, int touchID)
    {
    }

    // 有物体拖动到了当前窗口上
    public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent)
    {
        if (_onReceiveDrag != null)
        {
            continueEvent = false;
            _onReceiveDrag(dragObj, mousePos, ref continueEvent);
        }
    }

    // 有物体拖动到了当前窗口上
    public void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover)
    {
        _onDragHover?.Invoke(dragObj, mousePos, hover);
    }
    //------------------------------------------------------------------------------------------------------------------------------
}