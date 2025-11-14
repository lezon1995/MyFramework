using System;
using UnityEngine;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameDefine;

// 物体的鼠标事件逻辑组件
public class COMMovableObjectInteractive : GameComponent
{
    protected Action onDoubleClick; // 双击回调,由GlobalTouchSystem驱动
    protected ClickCallback onDoubleClickDetail; // 双击回调,由GlobalTouchSystem驱动,带当前触点坐标
    protected Action onPreClick; // 单击的预回调,单击时会首先调用此回调,由GlobalTouchSystem驱动
    protected ClickCallback onPreClickDetail; // 单击的预回调,单击时会首先调用此回调,由GlobalTouchSystem驱动,带当前触点坐标
    protected Action onClick; // 鼠标点击物体的回调
    protected ClickCallback onClickDetail; // 鼠标点击物体的回调,带当前触点坐标
    protected BoolCallback onHover; // 鼠标悬停在物体上的持续回调
    protected HoverCallback onHoverDetail; // 鼠标悬停在物体上的持续回调,带当前触点坐标
    protected BoolCallback onPress; // 鼠标在物体上处于按下状态的持续回调
    protected PressCallback onPressDetail; // 鼠标在物体上处于按下状态的持续回调,带当前触点坐标
    protected OnScreenMouseUp _onScreenMouseUp; // 鼠标在任意位置抬起的回调
    protected OnMouseEnter _onMouseEnter; // 鼠标进入物体的回调
    protected OnMouseLeave _onMouseLeave; // 鼠标离开物体的回调
    protected Vector3IntCallback _onMouseDown; // 鼠标在物体上按下的回调
    protected OnMouseMove _onMouseMove; // 鼠标在物体上移动的回调
    protected Vector3IntCallback _onMouseUp; // 鼠标在物体上抬起的回调
    protected Vector3 mouseDownPosition; // 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
    protected DateTime lastClickTime; // 上一次点击操作的时间,用于双击检测
    protected DateTime mouseDownTime; // 鼠标按下时的时间点
    protected int clickSound; // 点击时播放的音效ID,由于音效播放的操作较多,所以统一到此处实现最基本的点击音效播放
    protected bool mouseHovered; // 鼠标当前是否悬停在物体上
    protected bool handleInput; // 是否接收鼠标输入事件
    protected bool passDragEvent; // 是否将开始拖拽的事件穿透下去,使自己的下层也能够同时响应拖拽
    protected bool passRay; // 是否允许射线穿透

    public COMMovableObjectInteractive()
    {
        handleInput = true;
        passRay = true;
        lastClickTime = DateTime.Now;
        mouseDownTime = DateTime.Now;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        onDoubleClick = null;
        onDoubleClickDetail = null;
        onPreClick = null;
        onPreClickDetail = null;
        onClick = null;
        onClickDetail = null;
        onHover = null;
        onHoverDetail = null;
        onPress = null;
        onPressDetail = null;
        _onScreenMouseUp = null;
        _onMouseEnter = null;
        _onMouseLeave = null;
        _onMouseDown = null;
        _onMouseMove = null;
        _onMouseUp = null;
        mouseDownPosition = Vector3.zero;
        lastClickTime = DateTime.Now;
        mouseDownTime = DateTime.Now;
        clickSound = 0;
        mouseHovered = false;
        handleInput = true;
        passDragEvent = false;
        passRay = true;
    }

    public bool isHandleInput()
    {
        return handleInput;
    }

    public bool isReceiveScreenMouse()
    {
        return _onScreenMouseUp != null;
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

    public int getClickSound()
    {
        return clickSound;
    }

    public void setPassRay(bool passRay)
    {
        this.passRay = passRay;
    }

    public void setHandleInput(bool handleInput)
    {
        this.handleInput = handleInput;
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

    public void setOnScreenMouseUp(OnScreenMouseUp callback)
    {
        _onScreenMouseUp = callback;
    }

    public void setDoubleClickCallback(Action callback)
    {
        onDoubleClick = callback;
    }

    public void setDoubleClickDetailCallback(ClickCallback callback)
    {
        onDoubleClickDetail = callback;
    }

    public void setPreClickCallback(Action callback)
    {
        onPreClick = callback;
    }

    public void setPreClickDetailCallback(ClickCallback callback)
    {
        onPreClickDetail = callback;
    }

    public void setClickSound(int sound)
    {
        clickSound = sound;
    }

    public void onMouseEnter(Vector3 mousePos, int touchID)
    {
        var obj = owner as MovableObject;
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
        var obj = owner as MovableObject;
        if (mouseHovered)
        {
            mouseHovered = false;
            onHover?.Invoke(false);
            onHoverDetail?.Invoke(obj, mousePos, false);
        }

        _onMouseLeave?.Invoke(obj, mousePos, touchID);
    }

    // 鼠标左键在窗口内按下
    public void onMouseDown(Vector3 mousePos, int touchID)
    {
        mouseDownPosition = mousePos;
        mouseDownTime = DateTime.Now;
        onPress?.Invoke(true);
        var obj = owner as MovableObject;
        onPressDetail?.Invoke(obj, mousePos, true);
        _onMouseDown?.Invoke(mousePos, touchID);

        // 如果是触屏的触点,则触点在当前物体内按下时,认为时开始悬停
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
        var obj = owner as MovableObject;
        onPress?.Invoke(false);
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

            // 双击回调
            if ((DateTime.Now - lastClickTime).TotalSeconds < DOUBLE_CLICK_TIME)
            {
                onDoubleClick?.Invoke();
                onDoubleClickDetail?.Invoke(obj, mousePos);
            }

            lastClickTime = DateTime.Now;
        }

        _onMouseUp?.Invoke(mousePos, touchID);

        // 如果是触屏的触点,则触点在当前物体内抬起时,认为已经取消悬停
        TouchPoint touch = mInputSystem.getTouchPoint(touchID);
        if ((touch == null || !touch.isMouse()) && mouseHovered)
        {
            mouseHovered = false;
            onHover?.Invoke(false);
            onHoverDetail?.Invoke(obj, mousePos, false);
        }
    }

    // 鼠标在窗口内,并且有移动
    public void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
    {
        _onMouseMove?.Invoke(mousePos, moveDelta, moveTime, touchID);
    }

    public void onMouseStay(Vector3 mousePos, int touchID)
    {
    }

    public void onScreenMouseDown(Vector3 mousePos, int touchID)
    {
    }

    // 鼠标在屏幕上抬起
    public void onScreenMouseUp(Vector3 mousePos, int touchID)
    {
        _onScreenMouseUp?.Invoke(owner as MovableObject, mousePos, touchID);
    }

    public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent)
    {
    }

    public void onDragHovered(IMouseEventCollect dragObj, Vector3 mousePos, bool hover)
    {
    }

    public void onMultiTouchStart(Vector3 touch0, Vector3 touch1)
    {
    }

    public void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1)
    {
    }

    public void onMultiTouchEnd()
    {
    }
}