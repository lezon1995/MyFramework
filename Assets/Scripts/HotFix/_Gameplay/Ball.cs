using System;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public class Ball : MovableObject
{
    public int instanceId;
    protected Type mType; // 角色类型
    public long mGUID; // 角色的唯一ID
    protected Action<GameObject, Ball> onObjectSet;

    public Vector2 prePos;
    public Vector2 curPos;
    public Vector2 targetPos;
    public float speed = 6F;
    public float radius = 0.1F;
    public float movementDelta;
    public Vector2 direction;
    public Vector2 hitNormal;
    public Collider2D hitCollider;
    public SpriteRenderer ballRenderer;

    public void setOnObjectSet(Action<GameObject, Ball> action) => onObjectSet = action;
    public void setBallType(Type type) => mType = type;
    public void setID(long id) => mGUID = id;
    public Type getType() => mType;
    public long getGUID() => mGUID;


    float lastRadius;
    Vector2 lastDirection;

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
        ballRenderer = getUnityComponentInChild<SpriteRenderer>(true);

        setRadius(radius);
        setDirection(new(1F, 2F));

        if (isEditor())
        {
            var debug = getOrAddUnityComponent<BallDebug>();
            debug.ball = this;
        }
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

        checkRadius();
        
        prePos = curPos;
        movementDelta = speed * elapsedTime;
        curPos = Vector2.MoveTowards(curPos, targetPos, movementDelta);
        var mid = (prePos + curPos) / 2F;
        Debug.DrawLine(prePos, mid, Color.red, 0.02F);
        Debug.DrawLine(mid, curPos, Color.green, 0.02F);
        Debug.DrawLine(curPos, targetPos, Color.white, 0.02F);
        if (curPos == targetPos)
        {
            var reflectDir = Vector2.Reflect(direction, hitNormal);
            var newDir = reflectDir.normalized;
            setDirection(newDir);
        }
    }

    public Vector2 getDirection()
    {
        return direction;
    }

    public void setDirection(Vector2 value)
    {
        lastDirection = direction;
        direction = value;
        var hit = Physics2D.CircleCast(curPos, radius, direction, 20F, BORDER_LAYER_MASK | BRICK_LAYER_MASK);
        if (hit)
        {
            targetPos = hit.point + hit.normal * radius;
            hitNormal = hit.normal;
            hitCollider = hit.collider;
        }
    }
    
    public void setSpeed(float value)
    {
        speed = value;
    }

    public void setRadius(float value)
    {
        lastRadius = radius;
        radius = value;
        var diameter = value * 2F;
        ballRenderer.transform.localScale = new(diameter, diameter, 1);
    }

    void checkRadius()
    {
        if (isFloatEqual(lastRadius, radius))
            return;
        setRadius(radius);
    }
}