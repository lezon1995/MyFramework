using System;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public partial class Nexus : MovableObject
{
    public int instanceID;
    protected Type type; // 角色类型
    public long guid; // 角色的唯一ID

    #region Stats

    public float maxHealth;
    public float physicDamage;
    public float magicDamage;
    public float radius = 0.1F;

    public float curHealth;
    public bool immuneToDamage;
    public bool invulnerable;

    public void setHealth(float value)
    {
        curHealth = value;
    }

    #endregion

    SpriteRenderer renderer;

    public void setNexusType(Type t) => type = t;
    public void setID(long id) => guid = id;
    public Type getType() => type;
    public long getGUID() => guid;


    public override void init()
    {
        base.init();

        enableMoveInfo();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        instanceID = 0;
        type = null;
        guid = 0;
        renderer = null;

        maxHealth = 0F;
        physicDamage = 0F;
        magicDamage = 0F;
        radius = 0F;

        curHealth = 0F;
        immuneToDamage = false;
        invulnerable = false;
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        instanceID = obj.GetInstanceID();
        renderer = getUnityComponentInChild<SpriteRenderer>(true);

        if (isEditor())
        {
            var debug = getOrAddUnityComponent<NexusDebug>();
            debug.nexus = this;
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

    public bool isDead()
    {
        return curHealth <= 0 && maxHealth > 0;
    }
}