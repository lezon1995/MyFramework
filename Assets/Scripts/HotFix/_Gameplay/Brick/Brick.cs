using System;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public class Brick : MovableObject
{
    public int instanceId;
    protected Type mType; // 角色类型
    public long mGUID; // 角色的唯一ID
    protected Action<GameObject, Brick> onObjectSet;
    
    public SpriteRenderer brickRenderer;

    public void setOnObjectSet(Action<GameObject, Brick> action) => onObjectSet = action;
    public void setBrickType(Type type) => mType = type;
    public void setID(long id) => mGUID = id;
    public Type getType() => mType;
    public long getGUID() => mGUID;

    public override void init()
    {
        base.init();

        enableMoveInfo();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        instanceId = 0;
        mType = null;
        mGUID = 0;
        onObjectSet = null;
        brickRenderer = null;
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        instanceId = obj.GetInstanceID();
        onObjectSet?.Invoke(obj, this);
        brickRenderer = getUnityComponentInChild<SpriteRenderer>(true);

        if (isEditor())
        {
            var debug = getOrAddUnityComponent<BrickDebug>();
            debug.brick = this;
        }
    }

    protected override void initComponents()
    {
        base.initComponents();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
    }
}