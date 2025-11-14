using UnityEngine;
using static MathUtility;

// 物体的移动信息
public class COMMovableObjectMoveInfo : GameComponent
{
    protected Vector3 lastPhysicsSpeed; // FixedUpdate中上一帧的移动速度
    protected Vector3 lastPhysicsPosition; // 上一帧FixedUpdate中的位置
    protected Vector3 physicsAcceleration; // FixedUpdate中的加速度
    protected Vector3 physicsSpeed; // FixedUpdate中的移动速度
    protected Vector3 curFramePosition; // 当前位置
    protected Vector3 moveSpeed; // 当前移动速度向量,根据上一帧的位置和当前位置以及时间计算出来的实时速度
    protected Vector3 lastSpeed; // 上一帧的移动速度向量
    protected Vector3 lastPosition; // 上一帧的位置
    protected float realtimeMoveSpeed; // 当前实时移动速率
    protected bool enableFixedUpdate; // 是否启用FixedUpdate来计算Physics相关属性
    protected bool movedDuringFrame; // 角色在这一帧内是否移动过
    protected bool _hasLastPosition; // mLastPosition是否有效

    public COMMovableObjectMoveInfo()
    {
        enableFixedUpdate = true;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        lastPhysicsSpeed = Vector3.zero;
        lastPhysicsPosition = Vector3.zero;
        physicsAcceleration = Vector3.zero;
        physicsSpeed = Vector3.zero;
        curFramePosition = Vector3.zero;
        moveSpeed = Vector3.zero;
        lastSpeed = Vector3.zero;
        lastPosition = Vector3.zero;
        realtimeMoveSpeed = 0.0f;
        enableFixedUpdate = true;
        movedDuringFrame = false;
        _hasLastPosition = false;
    }

    public override void update(float dt)
    {
        base.update(dt);
        var movableObject = owner as MovableObject;
        if (movableObject.isDestroy())
            return;

        if (dt > 0.0f)
        {
            curFramePosition = movableObject.getPosition();
            moveSpeed = _hasLastPosition ? divide(curFramePosition - lastPosition, dt) : Vector3.zero;
            realtimeMoveSpeed = getLength(moveSpeed);
            movedDuringFrame = !isVectorEqual(lastPosition, curFramePosition) && _hasLastPosition;
            lastPosition = curFramePosition;
            lastSpeed = moveSpeed;
            _hasLastPosition = true;
        }
    }

    public override void fixedUpdate(float dt)
    {
        if (!enableFixedUpdate)
            return;

        base.fixedUpdate(dt);
        var movableObject = owner as MovableObject;
        Vector3 curPos = movableObject.getPosition();
        physicsSpeed = divide(curPos - lastPhysicsPosition, dt);
        lastPhysicsPosition = curPos;
        physicsAcceleration = divide(physicsSpeed - lastPhysicsSpeed, dt);
        lastPhysicsSpeed = physicsSpeed;
    }

    public Vector3 getPhysicsSpeed()
    {
        return physicsSpeed;
    }

    public Vector3 getPhysicsAcceleration()
    {
        return physicsAcceleration;
    }

    public bool hasMovedDuringFrame()
    {
        return movedDuringFrame;
    }

    public bool isEnableFixedUpdate()
    {
        return enableFixedUpdate;
    }

    public Vector3 getMoveSpeedVector()
    {
        return moveSpeed;
    }

    public Vector3 getLastSpeedVector()
    {
        return lastSpeed;
    }

    public Vector3 getLastPosition()
    {
        return lastPosition;
    }

    public bool hasLastPosition()
    {
        return _hasLastPosition;
    }
}