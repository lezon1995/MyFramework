using System;
using System.Collections.Generic;
using Drawing;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public partial class Ball : MovableObject, IDamageable<Brick>, IReusable
{
    const float PHYSICS_CAST_DISTANCE = 100F;
    protected Comparison<RaycastHit2D> comparison;

    public int instanceID; //GameObject的instanceID，可以根据不同GameObject而变化
    protected Type type;
    public long guid; // Ball这个对象的guid，

    #region Stats

    public int maxHealth;
    public int minPhysicDamage, maxPhysicDamage;
    public int minMagicDamage, maxMagicDamage;
    public Stat speed = 6F;
    public float radius = 0.1F;
    public float dmgRate = 1F;
    public Stat crit = 0.1F;


    public float curHealth;
    public bool immuneToDamage;
    public bool invulnerable;

    public bool isPenetrable; //是否可穿透砖块
    public bool horizontalBorderTeleportable; //是否可在左右边界来回传送

    public void setHealth(float value)
    {
        curHealth = value;
    }

    public void setPenetrable(bool value)
    {
        isPenetrable = value;
    }

    public void setHorizontalBorderTeleportable(bool value)
    {
        horizontalBorderTeleportable = value;
    }

    #endregion

    protected List<Buff> buffs = new();
    public List<BallPower> powers = new();

    GameObject ballRenderer;

    Collider2D hitCollider;

    // TrailRenderer trailRenderer;
    SmoothTrail trailRenderer;

    APlayer player;
    public Brick collidingBrick;
    public Brick overlappingBrick;

    Action<Ball> onDead;

    Vector2 prePos, curPos, targetPos;
    Vector2 lastDirection;
    Vector2 direction;
    Vector2 hitNormal;
    public BallCounters counters;

    float movementDelta;
    float lastRadius;
    bool enabled;
    bool hasBeenCollided;
    public IHittable lastHittable;
    public bool isOverlappingBrick;

    public void setOnDead(Action<Ball> action) => onDead = action;
    public void setBallType(Type t) => type = t;
    public void setID(long id) => guid = id;
    public Type getType() => type;
    public long getGUID() => guid;

    public Ball()
    {
        comparison = Comparison;
    }

    public override void onCreate()
    {
        CLASS(out counters);
    }

    public override void init()
    {
        base.init();
        enableMoveInfo();

        addListeners();
    }

    public override void destroy()
    {
        base.destroy();

        removeListeners();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        instanceID = 0;
        type = null;
        guid = 0;
        onDead = null;
        // comparison = null; 不重置
        // buffs = null; 不重置
        removeAllPowers(); // 移除所有powers
        powers.Clear();
        prePos = curPos = targetPos = Vector2.zero;

        movementDelta = 0;
        direction = Vector2.zero;
        hitNormal = Vector2.zero;
        UN_CLASS(ref counters);
        player = null;
        collidingBrick = null;
        overlappingBrick = null;
        hitCollider = null;
        ballRenderer = null;
        trailRenderer = null;
        lastHittable = null;
        lastRadius = 0;
        lastDirection = default;
        enabled = false;
        hasBeenCollided = false;
        isOverlappingBrick = false;

        maxHealth = 0;
        minPhysicDamage = maxPhysicDamage = 0;
        minMagicDamage = maxMagicDamage = 0;
        speed = 0F;
        radius = 0F;
        dmgRate = 1F;
        crit = 0.1F;

        curHealth = 0F;
        immuneToDamage = false;
        invulnerable = false;
        isPenetrable = false;
        horizontalBorderTeleportable = false;
    }

    public void onAcquire()
    {
        this.addListener<OnBrickColliderChanged>();
    }

    public void onRelease()
    {
        lastHittable = null;
        prePos = curPos = targetPos = Vector2.zero;
        movementDelta = 0;
        direction = Vector2.zero;
        hitNormal = Vector2.zero;
        lastRadius = 0;
        lastDirection = default;
        enabled = false;
        hasBeenCollided = false;
        removeAllPowers();

        maxHealth = 0;
        minPhysicDamage = maxPhysicDamage = 0;
        minMagicDamage = maxMagicDamage = 0;
        speed = 0F;
        radius = 0F;
        dmgRate = 1F;
        crit = 0.1F;

        curHealth = 0F;
        immuneToDamage = false;
        invulnerable = false;
        isPenetrable = false;
        horizontalBorderTeleportable = false;

        this.removeListener<OnBrickColliderChanged>();

        UN_CLASS_LIST(buffs);
    }

    public void setPlayer(APlayer p) => player = p;
    public APlayer getPlayer() => player;

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        instanceID = obj.GetInstanceID();
        curPos = obj.transform.position;

        obj.find(out ballRenderer, "Renderer");
        obj.find(out trailRenderer);

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

        if (!enabled)
            return;

        float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        var p = Vector3.Lerp(prePos, curPos, t);
        setPosition(p);

        Draw.ingame.xy.Circle(p, radius, Color.red);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);

        if (!enabled)
            return;

        if (isVectorEqual(hitNormal, Vector2.zero))
            return;

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
            var validHit = onHitEnter(hitCollider, hitNormal);
            if (!validHit)
            {
                refreshHitInfo(true);
            }
        }
        else
        {
            if (collidingBrick)
            {
                if (circleIntersectRectangle(getCircle(), collidingBrick.getRect()))
                {
                    if (overlappingBrick != collidingBrick)
                    {
                        if (!isOverlappingBrick)
                        {
                            isOverlappingBrick = true;
                        }
                        else
                        {
                            //如果上一次的Overlapping还未结束，则提前结束上一次的Overlapping
                            var lastOverlappingBrick = overlappingBrick;
                            player.onBallEndOverlappingBrickOne(this, lastOverlappingBrick, true);
                        }

                        var noOverlappingBefore = overlappingBrick == null;
                        overlappingBrick = collidingBrick;
                        if (noOverlappingBefore)
                            player.onBallBeginOverlappingBrickAll(this, overlappingBrick);

                        player.onBallBeginOverlappingBrickOne(this, overlappingBrick);
                    }
                }
                else
                {
                    if (isOverlappingBrick)
                    {
                        isOverlappingBrick = false;
                        player.onBallEndOverlappingBrickOne(this, overlappingBrick, false);
                        player.onBallEndOverlappingBrickAll(this, overlappingBrick, false);
                        overlappingBrick = null;
                    }

                    collidingBrick = null;
                }
            }
        }
    }

    public void reflectBounce(Vector2 normal, bool fromBrick = false)
    {
        var reflectDir = Vector2.Reflect(direction, normal);
        player.onBallReflect(this, normal, fromBrick, ref reflectDir);
        setDirection(reflectDir);
        counters.reflect.count();
    }

    public Vector2 getDirection()
    {
        return direction;
    }

    public void setDirection(Vector2 dir, int exceptMask = 0)
    {
        lastDirection = direction;
        direction = dir.normalized;
        refreshHitInfo(true, exceptMask);
    }

    public void setShootDirection(Vector2 dir, bool checkBorderBot, int exceptMask = 0)
    {
        lastDirection = direction;
        direction = dir.normalized;
        refreshHitInfo(checkBorderBot, exceptMask);
    }

    public void setEnabled(bool b)
    {
        enabled = b;
    }

    protected void refreshHitInfo(bool checkBorderBot, int exceptMask = 0)
    {
        RaycastHit2D hit = default;

        int mask;
        if (checkBorderBot)
            mask = ALL_BORDER_LAYER_MASK;
        else
            mask = NON_BOT_BORDER_LAYER_MASK;

        mask |= BRICK_LAYER_MASK;
        mask &= ~exceptMask;

        // Issue 1: 先检测球是否已嵌入碰撞体，若是则尝试向反方向推出
        var overlapFilter = new ContactFilter2D();
        overlapFilter.SetLayerMask(mask);
        using var overlapList = new ListScope<Collider2D>(out var overlapColliders);
        int overlapCount = Physics2D.OverlapCircle(curPos, radius, overlapFilter, overlapColliders);
        if (overlapCount > 0)
        {
            // 先尝试沿运动方向的反方向推出
            Vector2 antiPushDir = direction != Vector2.zero ? -direction : Vector2.left;
            float pushDist = radius * 2f;
            Vector2 pushedPos = curPos + antiPushDir * pushDist;
            overlapCount = Physics2D.OverlapCircle(pushedPos, radius, overlapFilter, overlapColliders);
            if (overlapCount == 0)
            {
                curPos = pushedPos;
            }
            else
            {
                // 反方向不行，尝试正交方向
                Vector2 orthoDir = new Vector2(-direction.y, direction.x);
                pushedPos = curPos + orthoDir * pushDist;
                overlapCount = Physics2D.OverlapCircle(pushedPos, radius, overlapFilter, overlapColliders);
                if (overlapCount == 0)
                {
                    curPos = pushedPos;
                }
                else
                {
                    pushedPos = curPos - orthoDir * pushDist;
                    overlapCount = Physics2D.OverlapCircle(pushedPos, radius, overlapFilter, overlapColliders);
                    if (overlapCount == 0)
                    {
                        curPos = pushedPos;
                    }
                    // 所有方向都无法推出，保持原位，交给ensureNotOverlapping处理
                }
            }
        }

        if (isPenetrable)
        {
            // Issue 4: 每次新建filter，避免状态残留
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);

            using var a = new ListScope<Collider2D>(out var overlapColliders2);
            var overlapCount2 = Physics2D.OverlapCircle(curPos, radius, filter, overlapColliders2);
            if (overlapCount2 > 0)
            {
                filter.SetLayerMask(mask);
                using var _ = new ListScope<RaycastHit2D>(out var hits);
                var count = Physics2D.CircleCast(curPos, radius, direction, filter, hits, PHYSICS_CAST_DISTANCE);
                if (count > 0)
                {
                    hits.Sort(comparison);
                    for (var i = 0; i < count; i++)
                    {
                        hit = hits[i];
                        if (overlapColliders2.Contains(hit.collider))
                            continue;

                        var hitDir = hit.point - curPos;
                        if (Vector2.Dot(direction, hitDir) < 0)
                            continue;

                        break;
                    }
                }
            }
            else
            {
                hit = Physics2D.CircleCast(curPos, radius, direction, PHYSICS_CAST_DISTANCE, mask);
            }
        }
        else
        {
            hit = Physics2D.CircleCast(curPos, radius, direction, PHYSICS_CAST_DISTANCE, mask);
        }

        if (hit)
        {
            targetPos = hit.point + hit.normal * radius;
            hitNormal = hit.normal;
            hitCollider = hit.collider;
        }
        else
        {
            // Issue 7: 无命中时设置远端目标，防止球停住
            hitCollider = null;
            hitNormal = Vector2.zero;
            targetPos = curPos + direction * PHYSICS_CAST_DISTANCE;
        }
    }

    int Comparison(RaycastHit2D h1, RaycastHit2D h2)
    {
        var d1 = Vector2.Distance(curPos, h1.point);
        var d2 = Vector2.Distance(curPos, h2.point);
        return d1.CompareTo(d2);
    }

    public void setTeleportPosition(Vector2 pos, int exceptMask = 0)
    {
        prePos = curPos = pos;
        setPosition(pos);
        setDirection(direction, exceptMask);
        clearTrail();
    }

    void clearTrail()
    {
        trailRenderer.clearTrail();
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

    public Circle2 getCircle()
    {
        return new(curPos, radius);
    }

    void checkRadius()
    {
        if (isFloatEqual(lastRadius, radius))
            return;
        setRadius(radius);
    }

    public void setPhysicDamage(int min, int max)
    {
        minPhysicDamage = min;
        maxPhysicDamage = max;
    }

    public void setMagicDamage(int min, int max)
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

    public virtual Dmg getHitDmg(Brick brick, Vector2 normal)
    {
        var d = getPhysicDamage();
        var dmg = Dmg.physicDmg(d);
        dmg.setHitEffect();
        dmg.setDmgRate(dmgRate);
        dmg.setHitNormal(normal);
        if (randomHit(crit))
            dmg.setCrit();
        return dmg;
    }

    public virtual Dmg getSkillDmg(Brick brick)
    {
        var d = getMagicDamage();
        var dmg = Dmg.magicDmg(d);
        dmg.setSkillEffect();
        dmg.setDmgRate(dmgRate);
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

    public virtual bool computeDamageOutput(ref Dmg dmg, IDmgCalculator calculator = null)
    {
        if (invulnerable)
            return false;

        if (immuneToDamage)
            return false;

        calculator ??= DmgCalculator.Default;
        int actualDamage = 1;
        float damage = dmg.value;
        var totalDamage = damage;

        float rawBaseDamage = calculator.computeDamageAlgo(dmg.algo, totalDamage, curHealth, maxHealth);
        float rawCritDamage = calculator.computeDamageCrit(dmg, rawBaseDamage);
        var rawFinalDamage = calculator.computeDamageRate(dmg, rawCritDamage);

        dmg.setDamageRaw(rawFinalDamage);
        dmg.setDamageDealt(actualDamage);
        return actualDamage > 0;
    }

    public void damage(ref Dmg dmg, GameObject instigator, Brick source, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null)
    {
        if (!canTakeDamageThisFrame(out _))
            return;

        computeDamageOutput(ref dmg, calculator);

        //设置此次dmg实际造成的伤害，并通知伤害飘字显示
        {
            dmg.setDirection(direction);
        }

        // we decrease the character's health by the damage
        float preHealth = curHealth;
        setHealth(curHealth - dmg.damageDealt);
        // lastDamage = damageDealt;
        // lastDamageType = dmg.actualType;
        // lastDamageDirection = direction;

        eventRouter.trigger(new OnHit());

        //造成伤害后处理Source吸血，触发DoDmg
        {
            if (!dmg.isSelf)
            {
                source.eventRouter.trigger(new DoDmgBall(this, dmg));
            }
        }

        //检测是否死亡
        {
            if (curHealth <= 0)
            {
                curHealth = 0;
                var isLethal = kill();
                if (isLethal && !dmg.isSelf)
                    source.eventRouter.trigger(new DoKillBall(this, instigator));
            }
        }
    }

    public bool kill()
    {
        if (immuneToDamage)
            return false;

        setHealth(0);

        eventRouter.trigger(new OnBallDeath());

        onDead?.Invoke(this);

        return true;
    }

    public bool isDead()
    {
        return curHealth <= 0 && maxHealth > 0;
    }

    /*public void returnBall(Vector3 nextPosition)
    {
        setEnabled(false);
        Tween
            .Position(getTransform(), endValue: nextPosition, duration: 0.25f, ease: Ease.OutCubic)
            .OnComplete(this, ball =>
            {
                ballManager.releaseBall(ball);
            });
    }*/

    public void addBuff(Buff buff)
    {
        buffs.add(buff);
    }

    public T addPower<T>() where T : BallPower
    {
        var power = CLASS<BallPower>(typeof(T));
        power.with(this);
        powers.add(power);
        power.onGainPower(this);
        return power as T;
    }

    public void removeAllPowers()
    {
        for (var i = powers.Count - 1; i >= 0; i--)
        {
            var power = powers[i];
            power.onLosePower(this);
            powers.removeAt(i);
            UN_CLASS(power);
        }
    }

    public bool removePower<T>() where T : BallPower
    {
        for (var i = powers.Count - 1; i >= 0; i--)
        {
            if (powers[i] is T power)
            {
                power.onLosePower(this);
                powers.removeAt(i);
                UN_CLASS(power);
                return true;
            }
        }

        return false;
    }
}