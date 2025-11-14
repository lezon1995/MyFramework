using System;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static CSharpUtility;
using static StringUtility;

// 可移动物体,表示一个3D物体
public class MovableObject : Transformable, IMouseEventCollect
{
    protected COMMovableObjectInteractive interactive; // 交互组件
    protected COMMovableObjectMoveInfo moveInfo; // 移动信息组件
    protected int objectId; // 物体的客户端ID
    protected bool selfCreatedObject; // 是否已经由MovableObject自己创建一个GameObject作为节点

    public MovableObject()
    {
        objectId = makeID();
    }

    public override void destroy()
    {
        // 自动创建的GameObject需要在此处自动销毁
        destroySelfCreateObject();
        mGlobalTouchSystem?.unregisterCollider(this);
        base.destroy();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        interactive = null;
        moveInfo = null;
        selfCreatedObject = false;
        // objectId不重置
        // objectId = 0;
    }

    // mObject需要外部自己创建以及销毁,内部只是引用,不会管理其生命周期
    // 且此函数需要在init之前调用,这样的话就能检测到setObject是否与mAutoCreateObject冲突,而且初始化也能够正常执行
    public override void setObject(GameObject obj)
    {
        // 如果是当前类自动创建的GameObject设置为空了,而且设置了一个不同的节点(无论是否为空),则取消此标记
        if (go != obj)
        {
            destroySelfCreateObject();
        }

        if (obj == null)
        {
            selfCreateObject();
        }
        else
        {
            base.setObject(obj);
        }
    }

    public virtual void init()
    {
        // 自动创建GameObject
        if (go == null)
        {
            selfCreateObject();
        }

        initComponents();
    }

    // 让MovableObject自己创建一个GameObject作为自己的节点,同时销毁对象时会将此GameObject也销毁
    public void selfCreateObject(string name = null, GameObject parent = null)
    {
        if (parent == null)
        {
            parent = mGameObjectPool.getObject();
        }

        setObject(mGameObjectPool.newObject(name, parent));
        selfCreatedObject = true;
    }

    // get
    //------------------------------------------------------------------------------------------------------------------------------
    public Vector3 getPhysicsSpeed()
    {
        if (moveInfo == null)
        {
            logError("未启用移动信息组件,无法获取速度");
            return Vector3.zero;
        }

        return moveInfo.getPhysicsSpeed();
    }

    public Vector3 getPhysicsAcceleration()
    {
        if (moveInfo == null)
        {
            logError("未启用移动信息组件,无法获取加速度");
            return Vector3.zero;
        }

        return moveInfo.getPhysicsAcceleration();
    }

    public bool hasMovedDuringFrame()
    {
        if (moveInfo == null)
        {
            logError("未启用移动信息组件,无法获取");
            return false;
        }

        return moveInfo.hasMovedDuringFrame();
    }

    public bool isEnableFixedUpdate()
    {
        return moveInfo != null && moveInfo.isEnableFixedUpdate();
    }

    public Vector3 getMoveSpeedVector()
    {
        if (moveInfo == null)
        {
            logError("未启用移动信息组件,无法获取");
            return Vector3.zero;
        }

        return moveInfo.getMoveSpeedVector();
    }

    public Vector3 getLastSpeedVector()
    {
        if (moveInfo == null)
        {
            logError("未启用移动信息组件,无法获取");
            return Vector3.zero;
        }

        return moveInfo.getLastSpeedVector();
    }

    public Vector3 getLastPosition()
    {
        if (moveInfo == null)
        {
            logError("未启用移动信息组件,无法获取");
            return Vector3.zero;
        }

        return moveInfo.getLastPosition();
    }

    public int getObjectID()
    {
        return objectId;
    }

    // 可移动物体没有固定深度,只在实时检测时根据相交点来判断深度
    public virtual UIDepth getDepth()
    {
        return null;
    }

    public virtual bool isHandleInput()
    {
        return interactive != null && interactive.isHandleInput();
    }

    public virtual bool isReceiveScreenMouse()
    {
        return interactive != null && interactive.isReceiveScreenMouse();
    }

    public virtual bool isPassRay()
    {
        return interactive == null || interactive.isPassRay();
    }

    public virtual bool isPassDragEvent()
    {
        return !isDragable() || (interactive != null && interactive.isPassDragEvent());
    }

    public virtual bool isMouseHovered()
    {
        return interactive != null && interactive.isMouseHovered();
    }

    public virtual bool isDragable()
    {
        return getActiveComponent<COMMovableObjectDrag>() != null;
    }

    public int getClickSound()
    {
        return interactive?.getClickSound() ?? 0;
    }

    public string getDescription()
    {
        return EMPTY;
    }

    public bool hasLastPosition()
    {
        return moveInfo != null && moveInfo.hasLastPosition();
    }

    public COMMovableObjectMoveInfo getCOMMoveInfo()
    {
        return moveInfo;
    }

    // set
    //------------------------------------------------------------------------------------------------------------------------------
    public virtual void setPassRay(bool passRay)
    {
        getCOMInteractive().setPassRay(passRay);
    }

    public virtual void setHandleInput(bool handleInput)
    {
        getCOMInteractive().setHandleInput(handleInput);
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

    public virtual void setClickCallback(Action callback)
    {
        getCOMInteractive().setClickCallback(callback);
    }

    public virtual void setClickDetailCallback(ClickCallback callback)
    {
        getCOMInteractive().setClickDetailCallback(callback);
    }

    public virtual void setHoverCallback(BoolCallback callback)
    {
        getCOMInteractive().setHoverCallback(callback);
    }

    public virtual void setHoverDetailCallback(HoverCallback callback)
    {
        getCOMInteractive().setHoverDetailCallback(callback);
    }

    public virtual void setPressCallback(BoolCallback callback)
    {
        getCOMInteractive().setPressCallback(callback);
    }

    public virtual void setPressDetailCallback(PressCallback callback)
    {
        getCOMInteractive().setPressDetailCallback(callback);
    }

    public void setOnScreenMouseUp(OnScreenMouseUp callback)
    {
        getCOMInteractive().setOnScreenMouseUp(callback);
    }

    public void setDoubleClickCallback(Action callback)
    {
        getCOMInteractive().setDoubleClickCallback(callback);
    }

    public void setDoubleClickDetailCallback(ClickCallback callback)
    {
        getCOMInteractive().setDoubleClickDetailCallback(callback);
    }

    public void setPreClickCallback(Action callback)
    {
        getCOMInteractive().setPreClickCallback(callback);
    }

    public void setPreClickDetailCallback(ClickCallback callback)
    {
        getCOMInteractive().setPreClickDetailCallback(callback);
    }

    public void setClickSound(int sound)
    {
        getCOMInteractive().setClickSound(sound);
    }

    public virtual void onMouseEnter(Vector3 mousePos, int touchID)
    {
        getCOMInteractive().onMouseEnter(mousePos, touchID);
    }

    public virtual void onMouseLeave(Vector3 mousePos, int touchID)
    {
        getCOMInteractive().onMouseLeave(mousePos, touchID);
    }

    // 鼠标左键在窗口内按下
    public virtual void onMouseDown(Vector3 mousePos, int touchID)
    {
        getCOMInteractive().onMouseDown(mousePos, touchID);
    }

    // 鼠标左键在窗口内放开
    public virtual void onMouseUp(Vector3 mousePos, int touchID)
    {
        getCOMInteractive().onMouseUp(mousePos, touchID);
    }

    // 鼠标在窗口内,并且有移动
    public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
    {
        getCOMInteractive().onMouseMove(mousePos, moveDelta, moveTime, touchID);
    }

    public virtual void onMouseStay(Vector3 mousePos, int touchID)
    {
        getCOMInteractive().onMouseStay(mousePos, touchID);
    }

    public virtual void onScreenMouseDown(Vector3 mousePos, int touchID)
    {
        getCOMInteractive().onScreenMouseDown(mousePos, touchID);
    }

    // 鼠标在屏幕上抬起
    public virtual void onScreenMouseUp(Vector3 mousePos, int touchID)
    {
        getCOMInteractive().onScreenMouseUp(mousePos, touchID);
    }

    public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent)
    {
        getCOMInteractive().onReceiveDrag(dragObj, mousePos, ref continueEvent);
    }

    public virtual void onDragHovered(IMouseEventCollect dragObj, Vector3 mousePos, bool hover)
    {
        getCOMInteractive().onDragHovered(dragObj, mousePos, hover);
    }

    public virtual void onMultiTouchStart(Vector3 touch0, Vector3 touch1)
    {
        getCOMInteractive().onMultiTouchStart(touch0, touch1);
    }

    public virtual void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1)
    {
        getCOMInteractive().onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1);
    }

    public virtual void onMultiTouchEnd()
    {
        getCOMInteractive().onMultiTouchEnd();
    }

    public void enableMoveInfo()
    {
        moveInfo ??= addComponent<COMMovableObjectMoveInfo>(true);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected COMMovableObjectInteractive getCOMInteractive()
    {
        return interactive ??= addComponent<COMMovableObjectInteractive>(false);
    }

    protected void destroySelfCreateObject()
    {
        if (go != null && selfCreatedObject)
        {
            selfCreatedObject = false;
            mGameObjectPool?.destroyObject(go, true);
            go = null;
        }
    }
}