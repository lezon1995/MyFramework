using System;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public partial class Ball : MovableObject, IDamageable<Brick>
{
    public int instanceID;
    protected Type type; // 角色类型
    public long guid; // 角色的唯一ID

    #region Stats

    public float maxHealth;
    public float minPhysicDamage, maxPhysicDamage;
    public float minMagicDamage, maxMagicDamage;
    public float speed = 6F;
    public float radius = 0.1F;
    public float dmgRate = 1F;


    public float curHealth;
    public bool immuneToDamage;
    public bool invulnerable;

    public void setHealth(float value)
    {
        curHealth = value;
    }

    #endregion

    Action<GameObject, Ball> onObjectSet;
    Action<Ball> onDead;

    Vector2 prePos, curPos, targetPos;
    Vector2 lastDirection;
    Vector2 direction;
    Vector2 hitNormal;

    float movementDelta;
    float lastRadius;

    Collider2D hitCollider;
    SpriteRenderer ballRenderer;

    public void setOnObjectSet(Action<GameObject, Ball> action) => onObjectSet = action;
    public void setOnDead(Action<Ball> action) => onDead = action;
    public void setBallType(Type t) => type = t;
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
        onObjectSet = null;
        onDead = null;
        prePos = curPos = targetPos = Vector2.zero;

        movementDelta = 0;
        direction = Vector2.zero;
        hitNormal = Vector2.zero;
        hitCollider = null;
        ballRenderer = null;
        lastRadius = 0;
        lastDirection = default;

        maxHealth = 0F;
        minPhysicDamage = maxPhysicDamage = 0F;
        minMagicDamage = maxMagicDamage = 0F;
        speed = 0F;
        radius = 0F;
        dmgRate = 1F;

        curHealth = 0F;
        immuneToDamage = false;
        invulnerable = false;
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        instanceID = obj.GetInstanceID();
        onObjectSet?.Invoke(obj, this);
        curPos = obj.transform.position;
        ballRenderer = getUnityComponentInChild<SpriteRenderer>(true);

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
            onHitEnter(hitCollider, hitNormal);
        }
    }

    void reflectBounce(Vector2 normal)
    {
        var reflectDir = Vector2.Reflect(direction, normal);
        var newDir = reflectDir.normalized;
        setDirection(newDir);
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

    public void setTeleportPosition(Vector2 pos)
    {
        prePos = curPos = pos;
        setPosition(pos);
        setDirection(direction);
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

    public void setPhysicDamage(float min, float max)
    {
        minPhysicDamage = min;
        maxPhysicDamage = max;
    }

    public void setMagicDamage(float min, float max)
    {
        minMagicDamage = min;
        maxMagicDamage = max;
    }

    public float getPhysicDamage()
    {
        var damage = randomFloat(minPhysicDamage, maxPhysicDamage);
        return damage;
    }

    public float getMagicDamage()
    {
        var damage = randomFloat(minMagicDamage, maxMagicDamage);
        return damage;
    }

    public virtual bool getSelfDamage(Brick brick, out float selfDamage)
    {
        selfDamage = 0F;
        return false;
    }

    public virtual Dmg getDmg(Brick brick)
    {
        var d = getPhysicDamage();
        var dmg = Dmg.physicDmg(d);
        return dmg;
    }

    public bool canTakeDamageThisFrame(out ResistDamageType resistType)
    {
        if (!isActive())
        {
            resistType = ResistDamageType.Disabled;
            return false;
        }

        // if the object is invulnerable, we do nothing and exit
        if (invulnerable)
        {
            resistType = ResistDamageType.Invulnerable;
            return false;
        }

        if (immuneToDamage)
        {
            resistType = ResistDamageType.ImmuneToDamage;
            return false;
        }

        // if we're already below zero, we do nothing and exit
        if (curHealth <= 0 && maxHealth > 0)
        {
            resistType = ResistDamageType.Dead;
            return false;
        }

        resistType = ResistDamageType.None;
        return true;
    }

    public virtual bool computeDamageOutput(ref Dmg dmg, out float actualDamage, out float rawFinalDamage, IDmgCalculator calculator = null)
    {
        calculator ??= DmgCalculator.Default;

        actualDamage = 0F;
        rawFinalDamage = 0F;
        if (invulnerable)
            return false;

        if (immuneToDamage)
            return false;

        float damage = dmg.value;
        var totalDamage = damage;

        float rawBaseDamage = calculator.computeDamageAlgo(dmg.algo, totalDamage, curHealth, maxHealth);
        float rawCritDamage = calculator.computeDamageCrit(dmg, rawBaseDamage);
        rawFinalDamage = calculator.computeDamageRate(dmg, rawCritDamage);

        return actualDamage > 0;
    }

    public void damage(Dmg dmg, GameObject instigator, Brick source, float invincibleTime = 0, Vector3 direction = default, IDmgCalculator calculator = null)
    {
        if (!canTakeDamageThisFrame(out _))
            return;

        computeDamageOutput(ref dmg, out var damageDealt, out var damageRaw, calculator);

        //设置此次dmg实际造成的伤害，并通知伤害飘字显示
        {
            dmg.setDamageRaw(damageRaw);
            dmg.setDamageDealt(damageDealt);
            dmg.setDirection(direction);
        }

        // we decrease the character's health by the damage
        float preHealth = curHealth;
        setHealth(curHealth - damageDealt);
        // lastDamage = damageDealt;
        // lastDamageType = dmg.actualType;
        // lastDamageDirection = direction;

        trigger(new OnHit());

        //造成伤害后处理Source吸血，触发DoDmg
        {
            if (!dmg.isSelf)
            {
                source.trigger(new DoDmgBall(this, dmg));
            }
        }

        //检测是否死亡
        {
            if (curHealth <= 0)
            {
                curHealth = 0;
                var isLethal = kill();
                if (isLethal && !dmg.isSelf)
                    source.trigger(new DoKillBall(this, instigator));
            }
        }
    }

    public bool kill()
    {
        if (immuneToDamage)
            return false;

        setHealth(0);

        trigger(new OnDeath());

        onDead?.Invoke(this);

        return true;
    }

    public bool isDead()
    {
        return curHealth <= 0 && maxHealth > 0;
    }
}