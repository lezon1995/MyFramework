using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static MathUtility;

// UGUIWindow的鼠标相关事件的逻辑
public class COMWindowUGUIInteractive : GameComponent
{
    protected Action<PointerEventData, GameObject> onMouseEnter; // 鼠标进入的回调,由UGUI触发
    protected Action<PointerEventData, GameObject> onMouseLeave; // 鼠标离开的回调,由UGUI触发
    protected Action<PointerEventData, GameObject> onMouseDown; // 鼠标按下的回调,由UGUI触发
    protected Action<PointerEventData, GameObject> onMouseUp; // 鼠标抬起的回调,由UGUI触发
    protected Action<PointerEventData, GameObject> onClick; // 鼠标点击的回调,由UGUI触发
    protected Action<Vector2, Vector3> onMouseMove; // 鼠标移动的回调,由UGUI触发,第一个参数是这一帧触点的移动量,第二个参数是触点当前的位置
    protected Action<Vector3> onMouseStay; // 鼠标在窗口内停止的回调,由UGUI触发,参数是触点当前的位置
    protected EventTriggerListener eventTriggerListener; // UGUI事件监听器,用于接收UGUI的事件
    protected PointerEventData mousePointer; // 鼠标在当前窗口按下时的触点信息

    public override void resetProperty()
    {
        base.resetProperty();
        onMouseEnter = null;
        onMouseLeave = null;
        onMouseDown = null;
        onMouseUp = null;
        onClick = null;
        onMouseMove = null;
        onMouseStay = null;
        eventTriggerListener = null;
        mousePointer = null;
    }

    public override void destroy()
    {
        base.destroy();
        if (eventTriggerListener)
        {
            eventTriggerListener.onClick -= onUGUIClick;
            eventTriggerListener.onDown -= onUGUIMouseDown;
            eventTriggerListener.onUp -= onUGUIMouseUp;
            eventTriggerListener.onEnter -= onUGUIMouseEnter;
            eventTriggerListener.onExit -= onUGUIMouseLeave;
        }

        mousePointer = null;
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (mousePointer != null)
        {
            // 此处应该获取touchID的移动量
            Vector3 delta = mousePointer.delta;
            if (!isVectorZero(delta))
            {
                onMouseMove?.Invoke(delta, mousePointer.position);
            }
            else
            {
                onMouseStay?.Invoke(mousePointer.position);
            }
        }
    }

    public void setUGUIClick(Action<PointerEventData, GameObject> callback)
    {
        checkEventTrigger();
        onClick = callback;
    }

    public void setUGUIMouseDown(Action<PointerEventData, GameObject> callback)
    {
        checkEventTrigger();
        onMouseDown = callback;
    }

    public void setUGUIMouseUp(Action<PointerEventData, GameObject> callback)
    {
        checkEventTrigger();
        onMouseUp = callback;
    }

    public void setUGUIMouseEnter(Action<PointerEventData, GameObject> callback)
    {
        checkEventTrigger();
        onMouseEnter = callback;
    }

    public void setUGUIMouseExit(Action<PointerEventData, GameObject> callback)
    {
        checkEventTrigger();
        onMouseLeave = callback;
    }

    public void setUGUIMouseMove(Action<Vector2, Vector3> callback)
    {
        checkEventTrigger();
        onMouseMove = callback;
    }

    public void setUGUIMouseStay(Action<Vector3> callback)
    {
        checkEventTrigger();
        onMouseStay = callback;
    }

    public void clearMousePointer()
    {
        mousePointer = null;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void checkEventTrigger()
    {
        if (eventTriggerListener)
            return;

        eventTriggerListener = ((myUGUIObject)owner).getOrAddUnityComponent<EventTriggerListener>();
        eventTriggerListener.onClick += onUGUIClick;
        eventTriggerListener.onDown += onUGUIMouseDown;
        eventTriggerListener.onUp += onUGUIMouseUp;
        eventTriggerListener.onEnter += onUGUIMouseEnter;
        eventTriggerListener.onExit += onUGUIMouseLeave;
    }

    protected void onUGUIMouseDown(PointerEventData eventData, GameObject go)
    {
        // 如果当前正在被按下,则不允许再响应按下事件,否则会影响正在进行的按下逻辑
        if (mousePointer != null)
            return;

        mousePointer = eventData;
        onMouseDown?.Invoke(eventData, go);
    }

    protected void onUGUIMouseUp(PointerEventData eventData, GameObject go)
    {
        // 不是来自于当前按下的触点的事件不需要处理
        if (mousePointer != eventData)
            return;

        mousePointer = null;
        onMouseUp?.Invoke(eventData, go);
    }

    protected void onUGUIClick(PointerEventData eventData, GameObject go)
    {
        onClick?.Invoke(eventData, go);
    }

    protected void onUGUIMouseEnter(PointerEventData eventData, GameObject go)
    {
        onMouseEnter?.Invoke(eventData, go);
    }

    protected void onUGUIMouseLeave(PointerEventData eventData, GameObject go)
    {
        onMouseLeave?.Invoke(eventData, go);
    }
}