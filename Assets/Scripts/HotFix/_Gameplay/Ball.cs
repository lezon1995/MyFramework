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
    float speed = 6F;
    float radius => mTransform.localScale.x / 2F;
    float movementDelta;
    Vector3 direction;
    Vector3 hitNormal;
    Collider hitCollider;

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
        if (Physics.SphereCast(curPos, radius, direction, out var hit, 20F, BORDER_LAYER_MASK | BRICK_LAYER_MASK))
        {
            targetPos = hit.point + hit.normal * radius;
            hitNormal = hit.normal;
            hitCollider = hit.collider;
        }
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
        setDirection(new(1F, 0F, 2F));
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
        setPosition(p);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);

        prePos = curPos;
        movementDelta = speed * elapsedTime;
        curPos = Vector3.MoveTowards(curPos, targetPos, movementDelta);
        var mid = (prePos + curPos) / 2F;
        Debug.DrawLine(prePos, mid, Color.red, 0.02F);
        Debug.DrawLine(mid, curPos, Color.green, 0.02F);
        Debug.DrawLine(curPos, targetPos, Color.white, 0.02F);
        if (curPos == targetPos)
        {
            hitCollider.GetComponent<MeshRenderer>().enabled = !hitCollider.GetComponent<MeshRenderer>().enabled;
            
            var reflectDir = Vector3.Reflect(direction, hitNormal);
            var newDir = reflectDir.normalized;
            setDirection(newDir);
        }
    }
}