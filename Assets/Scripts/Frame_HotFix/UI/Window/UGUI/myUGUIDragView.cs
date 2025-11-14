using System;
using UnityEngine;
using static UnityUtility;
using static MathUtility;

// 可拖拽滑动的窗口,类似于ScrollView
// 一般父节点是一个viewport
public class myUGUIDragView : myUGUIObject
{
    protected COMWindowDragView dragView; // 拖拽滑动组件
    protected bool initialized;

    public myUGUIDragView()
    {
        needUpdate = true;
    }

    public void initDragView()
    {
        initDragView(DRAG_DIRECTION.VERTICAL, toRadian(45.0f), false, true, true);
    }

    public void initDragView(DRAG_DIRECTION direction)
    {
        initDragView(direction, toRadian(45.0f), false, true, true);
    }

    public void initDragView(DRAG_DIRECTION direction, float angleThresholdRadian)
    {
        initDragView(direction, angleThresholdRadian, false, true, true);
    }

    // angleThresholdRadian表示拖拽方向与允许拖拽方向的夹角绝对值最大值,弧度制
    // clampInner为true表示DragView只能在父节点的区域内滑动,父节点区域小于DragView区域时不能滑动
    // clampInner为false表示DragView只能在父节点的区域外滑动,父节点区域大于DragView区域时不能滑动
    // 一般情况下作为滑动列表时可填false
    // allowDragOnlyOverParentSize表示是否只有大小超过父节点时才能拖拽,当前节点没有超过父节点时不允许拖拽
    // clampInRange为true表示拖拽时始终限制在正常范围内
    public void initDragView(DRAG_DIRECTION direction, float angleThresholdRadian, bool clampInner, bool allowDragOnlyOverParentSize, bool clampInRange)
    {
        // 这里直接获取父节点作为viewport
        layout.getScript().bindPassOnlyParent(parent);
        registerCollider(true);
        setDepthOverAllChild(true);
        setDragDirection(direction);
        setDragAngleThreshold(angleThresholdRadian);
        setClampInner(clampInner);
        setAllowDragOnlyOverParentSize(allowDragOnlyOverParentSize);
        setClampInRange(clampInRange);
        initialized = true;
    }

    public override bool isReceiveScreenMouse()
    {
        return true;
    }

    public myUGUIObject getViewport()
    {
        return parent;
    }

    // 显式调用调整窗口位置
    public void autoClampPosition()
    {
        dragView.autoClampPosition();
    }

    public void autoResetPosition()
    {
        dragView.autoResetPosition();
    }

    public override void onMouseDown(Vector3 mousePos, int touchID)
    {
        base.onMouseDown(mousePos, touchID);
        dragView.onMouseDown(mousePos, touchID);
        if (!initialized)
        {
            logError("COMWindowDragView组件未初始化,是否忘记调用了myUGUIDragView的initDragView?");
        }
    }

    // 鼠标在屏幕上抬起
    public override void onScreenMouseUp(Vector3 mousePos, int touchID)
    {
        base.onScreenMouseUp(mousePos, touchID);
        dragView.onScreenMouseUp();
    }

    public override void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
    {
        base.onMouseMove(mousePos, moveDelta, moveTime, touchID);
        dragView.onMouseMove(mousePos, moveDelta, moveTime, touchID);
    }

    public override void onMouseStay(Vector3 mousePos, int touchID)
    {
        base.onMouseStay(mousePos, touchID);
        dragView.onMouseStay(touchID);
    }

    public override void setWindowSize(Vector2 size)
    {
        base.setWindowSize(size);
        collider?.setColliderSize(getWindowSize(true));
        dragView.onWindowSizeChange();
    }

    public void setAlignTopOrLeft(bool alignTopOrLeft)
    {
        dragView.setAlignTopOrLeft(alignTopOrLeft);
    }

    // 当DragView的父节点的大小改变时,需要调用该函数重新计算可拖动的范围
    public void notifyParentSizeChange()
    {
        dragView.notifyWindowParentSizeChange();
    }

    public void stopMoving()
    {
        dragView.stopMoving();
    }

    public void setClampInner(bool inner)
    {
        dragView.setClampInner(inner);
    }

    public void setDragDirection(DRAG_DIRECTION direction)
    {
        dragView.setDragDirection(direction);
    }

    public void setMaxRelativePos(Vector3 max)
    {
        dragView.setMaxRelativePos(max);
    }

    public void setMinRelativePos(Vector3 min)
    {
        dragView.setMinRelativePos(min);
    }

    public void setMoveSpeedScale(float scale)
    {
        dragView.setMoveSpeedScale(scale);
    }

    public void setDragViewStartCallback(OnDragViewStartCallback callback)
    {
        dragView.setDragViewStartCallback(callback);
    }

    public void setDragingCallback(Action draging)
    {
        dragView.setDragingCallback(draging);
    }

    public void setReleaseDragCallback(Action releaseDrag)
    {
        dragView.setReleaseDragCallback(releaseDrag);
    }

    public void setPositionChangeCallback(Action positionChange)
    {
        dragView.setPositionChangeCallback(positionChange);
    }

    public void setClampType(CLAMP_TYPE clampType)
    {
        dragView.setClampType(clampType);
    }

    public void setClampInRange(bool clampInRange)
    {
        dragView.setClampInRange(clampInRange);
    }

    public void setAllowDragOnlyOverParentSize(bool dragOnly)
    {
        dragView.setAllowDragOnlyOverParentSize(dragOnly);
    }

    public void setAutoMoveToEdge(bool autoMove)
    {
        dragView.setAutoMoveToEdge(autoMove);
    }

    public void setAttenuateFactor(float value)
    {
        dragView.setAttenuateFactor(value);
    }

    public void setDragLengthThreshold(float value)
    {
        dragView.setDragLengthThreshold(value);
    }

    public void setDragAngleThreshold(float radian)
    {
        dragView.setDragAngleThreshold(radian);
    }

    public void setAutoClampSpeed(float speed)
    {
        dragView.setAutoClampSpeed(speed);
    }

    public DRAG_DIRECTION getDragDirection()
    {
        return dragView.getDragDirection();
    }

    public Vector3 getMaxRelativePos()
    {
        return dragView.getMaxRelativePos();
    }

    public Vector3 getMinRelativePos()
    {
        return dragView.getMinRelativePos();
    }

    public bool isClampInner()
    {
        return dragView.isClampInner();
    }

    public bool isDragging()
    {
        return dragView.isDraging();
    }

    public bool isAllowDragOnlyOverParentSize()
    {
        return dragView.isAllowDragOnlyOverParentSize();
    }

    public COMWindowDragView getDragViewComponent()
    {
        return dragView;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected override void initComponents()
    {
        base.initComponents();
        addInitComponent(out dragView, true);
    }
}