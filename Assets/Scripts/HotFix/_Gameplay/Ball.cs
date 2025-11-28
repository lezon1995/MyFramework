using System;
using UnityEngine;

namespace MarbleHero;

public class Ball : MovableObject
{
    public int instanceId;
    protected Type mType; // 角色类型
    protected long mGUID; // 角色的唯一ID
    protected Action<GameObject, Ball> onObjectSet;


    Vector3 prePos;
    Vector3 curPos;
    Vector3 targetPos;
    float speed = 10F;
    float radius => mTransform.localScale.x;
    float movementDelta;
    Vector3 direction = new(1F, 0F, 2F);

    public void setOnObjectSet(Action<GameObject, Ball> action) => onObjectSet = action;
    public void setBallType(Type type) => mType = type;
    public void setID(long id) => mGUID = id;
    public Type getType() => mType;
    public long getGUID() => mGUID;

    public Vector3 getDirection()
    {
        return direction;
    }

    public void setDirection(Vector3 value)
    {
        direction = value;
    }

    public override void init()
    {
        base.init();

        enableMoveInfo();
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        instanceId = obj.GetInstanceID();
        onObjectSet?.Invoke(obj, this);
        curPos = obj.transform.position;
    }

    protected override void initComponents()
    {
        base.initComponents();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        var p = Vector3.Lerp(prePos, curPos, t);
        var delta = (p - mPosition).magnitude;
        var safePos = Vector3.MoveTowards(mPosition, targetPos, delta);
        setPosition(safePos);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);

        if (Physics.SphereCast(curPos, radius, direction, out var hit, 10F, BORDER_LAYER_MASK | BRICK_LAYER_MASK))
        {
            targetPos = hit.point;
        }

        prePos = curPos;
        movementDelta = speed * elapsedTime;
        curPos = Vector3.MoveTowards(mPosition, targetPos, movementDelta);
        if (curPos == targetPos)
        {
            var reflectDir = Vector3.Reflect(direction, hit.normal);
            var newDir = reflectDir.normalized;
            direction = newDir;
        }
    }
}