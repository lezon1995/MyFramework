using System;
using System.Collections.Generic;
using Drawing;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public partial class Brick : MovableObject
    , IDamageable
    , IDamageable<Ball>
    , IHittable
    , IReusable
{
    public override string ToString() => mName;

    protected BrickManager manager;
    public int instanceID;
    protected Type type; // 角色类型
    public long guid; // 角色的唯一ID

    #region Stats

    public float width, height;
    public int maxHealth = 10;
    public int maxHealthStack = 1;
    public float physicResist;
    public float magicResist;
    public float dodgeChance;

    public float getPhysicResist()
    {
        return physicResist;
    }

    #endregion

    public BrickRenderer brickRenderer;
    BrickCollider brickCollider;

    public List<BrickPower> powers = new();

    public int curHealth;
    public int curHealthStack;
    public bool immuneToDamage;
    public bool invulnerable;

    enum CoroutineState
    {
        None,
        DamageEnabled,
    }

    CoroutineState _coroutineState;
    float _coroutineTimeElapsed;
    float _invincibleTime;
    Timer killTimer;

    public void setBrickType(Type t) => type = t;
    public void setID(long id) => guid = id;
    public Type getType() => type;
    public long getGUID() => guid;

    public override void onCtor()
    {
        base.onCtor();
    }

    public override void init()
    {
        base.init();

        enableMoveInfo();
    }

    protected override void initComponents()
    {
        base.initComponents();
        addInitComponent(out brickRenderer, true);
        addInitComponent(out brickCollider, true);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        UN_CLASS_LIST(powers);
        manager = null;
        instanceID = 0;
        type = null;
        guid = 0;
        brickRenderer = null;
        brickCollider = null;

        width = 0F;
        height = 0F;
        maxHealth = 0;
        maxHealthStack = 0;
        physicResist = 0F;
        magicResist = 0F;
        dodgeChance = 0F;

        curHealth = 0;
        curHealthStack = 0;
        immuneToDamage = false;
        invulnerable = false;

        _coroutineState = default;
        _coroutineTimeElapsed = 0F;
        _invincibleTime = 0F;
        killTimer = 0F;
    }

    public void onAcquire()
    {
        brickCollider.setColliderEnabled(true);
        brickRenderer.setRendererActive(true);
        brickRenderer.playFadeIn();
    }

    public void onRelease()
    {
        UN_CLASS_LIST(powers);

        width = 0F;
        height = 0F;
        maxHealth = 0;
        maxHealthStack = 0;
        physicResist = 0F;
        magicResist = 0F;
        dodgeChance = 0F;


        curHealth = 0;
        curHealthStack = 0;
        immuneToDamage = false;
        invulnerable = false;

        _coroutineState = default;
        _coroutineTimeElapsed = 0F;
        _invincibleTime = 0F;
        killTimer = 0F;
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        instanceID = obj.GetInstanceID();

        if (isEditor())
        {
            var debug = getOrAddUnityComponent<BrickDebug>();
            debug.brick = this;
        }
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        if (killTimer)
        {
            if (killTimer.update(elapsedTime))
            {
                var e = new OnBrickDeathTotally(this);
                e.trigger(this);
            }
        }

        // Draw.ingame.xy.WireRectangle(getRect(), Color.red);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);

        switch (_coroutineState)
        {
            case CoroutineState.DamageEnabled:
                _coroutineTimeElapsed += elapsedTime;
                if (_coroutineTimeElapsed > _invincibleTime)
                {
                    _coroutineTimeElapsed = 0F;
                    invulnerable = false;
                    _coroutineState = CoroutineState.None;
                }

                break;
        }
    }

    public void setManager(BrickManager m)
    {
        manager = m;
    }

    public void setHealth(int value, bool changeColor = true)
    {
        curHealth = value;
        brickRenderer.refreshHealth(curHealth, maxHealth);
        if (changeColor)
        {
            // var sprite = manager.getBrickSpriteByHealth(value);
            // brickRenderer.setBrickSprite(sprite);
        }
    }

    public void setInitialHealth(int value)
    {
        curHealth = value;
        brickRenderer.refreshInitialHealth(curHealth, maxHealth);
        // var sprite = manager.getBrickSpriteByHealth(value);
        // brickRenderer.setBrickSprite(sprite);
    }

    public void setMaxHealth(int value)
    {
        maxHealth = value;
    }

    public virtual void setDamageDisabled()
    {
        invulnerable = true;
    }

    /// <summary>
    /// Allows the character to take damage
    /// </summary>
    public virtual void setDamageEnabled()
    {
        invulnerable = false;
    }

    public virtual bool canTakeDamageThisFrame(out ResistDamageType resistType)
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

        if (dodgeChance > 0 && randomHit(dodgeChance))
        {
            resistType = ResistDamageType.Dodged;
            return false;
        }

        resistType = ResistDamageType.None;
        return true;
    }

    /// <summary>
    /// Returns the damage this health should take after processing potential resistances
    /// </summary>
    public virtual bool computeDamageOutput(ref Dmg dmg, IDmgCalculator calculator = null)
    {
        if (invulnerable)
            return false;

        if (immuneToDamage)
            return false;

        calculator ??= DmgCalculator.Default;

        float damage = dmg.value;
        var totalDamage = damage;
        int actualDamage;
        float rawBaseDamage = calculator.computeDamageAlgo(dmg.algo, totalDamage, curHealth, maxHealth);
        float rawCritDamage = calculator.computeDamageCrit(dmg, rawBaseDamage);
        int rawFinalDamage = calculator.computeDamageRate(dmg, rawCritDamage);

        if (dmg.mix.on)
        {
            dmg.mix = calculator.computeDamageMix(dmg.mix, rawFinalDamage, physicResist, magicResist);
            actualDamage = dmg.mix.Sum();
        }
        else
        {
            actualDamage = calculator.computeDamageDefence(dmg.actualType, rawFinalDamage, physicResist, magicResist);
        }

        dmg.setDamageRaw(rawFinalDamage);
        dmg.setDamageDealt(actualDamage);
        return actualDamage > 0;
    }

    public virtual void takeDamage(ref Dmg dmg, GameObject instigator, Ball source, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null)
    {
        if (!canTakeDamageThisFrame(out _))
            return;

        computeDamageOutput(ref dmg, calculator);

        //设置此次dmg实际造成的伤害，并通知伤害飘字显示
        {
            dmg.setDirection(direction);

            if (dmg.damageDealt > 0)
            {
                new DmgTextEvent(dmg, getTransform()).trigger();
            }
        }

        foreach (var p in powers)
            p.onBeforeApplyDamage(this, source, ref dmg);

        if (dmg.triggerEffect)
        {
            //触发本次伤害所造成的攻击特效/技能特效
            if (dmg.hasHitEffect())
            {
                var e = new DoHitEffect(source, this);
                source.eventRouter.trigger(e);
                source.getPlayer().eventRouter.trigger(e);
            }

            if (dmg.hasSkillEffect())
            {
                var e = new DoSkillEffect(source, this);
                source.eventRouter.trigger(e);
                source.getPlayer().eventRouter.trigger(e);
            }
        }

        eventRouter.trigger(new OnHit());

        if (dmg.damageDealt > 0)
        {
            // we decrease the character's health by the damage
            int preHealth = curHealth;
            int health = clampMin((curHealth - dmg.damageDealt), 0);
            setHealth(health);
            // lastDamage = damageDealt;
            // lastDamageType = dmg.actualType;
            // lastDamageDirection = direction;

            // we prevent the character from colliding with Projectiles, Player and Enemies
            if (invincibleTime > 0)
            {
                setDamageDisabled();
                _coroutineTimeElapsed = 0F;
                _coroutineState = CoroutineState.DamageEnabled;
                _invincibleTime = invincibleTime;
            }

            // we trigger a damage taken event
            // MMDamageTakenEvent.Trigger(this, instigator, curHealth, damageDealt, preHealth);

            //造成伤害后处理Source吸血，触发DoDmg
            {
                var e = new DoDmgBrick(this, dmg);
                source.eventRouter.trigger(e);
                source.getPlayer().eventRouter.trigger(e);

                //造成伤害后，触发OnDmg
                eventRouter.trigger(new OnDmg(source, dmg));
            }

            // we play our feedback
            // if (FeedbackIsProportionalToDamage)
            //     DamageMMFeedbacks.Play(transform.position, damageDealt);
            // else
            //     DamageMMFeedbacks.Play(transform.position);

            // we update the health bar
            // UpdateHealthBar(true);

            brickRenderer.playFxDamage(direction);

            //检测是否死亡
            if (curHealth <= 0)
            {
                curHealth = 0;
                if (dmg.hasHitEffect())
                {
                    var e = new DoAttackKillEffect(source, this, instigator);
                    source.eventRouter.trigger(e);
                    source.getPlayer().eventRouter.trigger(e);
                }

                {
                    var e = new DoKillBrick(source, this, instigator);
                    source.eventRouter.trigger(e);
                    source.getPlayer().eventRouter.trigger(e);
                }

                kill();
                dmg.isLethal = true;
            }
        }
    }

    public virtual void heal(Heal heal)
    {
        if (heal.Healing > 0)
        {
            new HealTextEvent(heal, getTransform()).trigger();
        }

        eventRouter.trigger(new OnHeal());

        if (heal.Healing > 0)
        {
            // we decrease the character's health by the damage
            int preHealth = curHealth;
            int newHealth = curHealth + (int)heal.Healing;
            if (newHealth > maxHealth)
                setMaxHealth(newHealth);

            setHealth(newHealth);

            //造成伤害后处理Source吸血，触发DoDmg
            // {
            //     var e = new DoDmgBrick(this, dmg);
            //     source.eventRouter.trigger(e);
            //     source.getPlayer().eventRouter.trigger(e);
            //
            //     //造成伤害后，触发OnDmg
            //     eventRouter.trigger(new OnDmg(source, dmg));
            // }

            // we play our feedback
            // if (FeedbackIsProportionalToDamage)
            //     DamageMMFeedbacks.Play(transform.position, damageDealt);
            // else
            //     DamageMMFeedbacks.Play(transform.position);

            // we update the health bar
            // UpdateHealthBar(true);

            brickRenderer.playFxHeal();

            //检测是否回满血
            if (curHealth >= maxHealth)
            {
                curHealth = maxHealth;
                // if (dmg.hasHitEffect())
                // {
                //     var e = new DoAttackKillEffect(source, this, instigator);
                //     source.eventRouter.trigger(e);
                //     source.getPlayer().eventRouter.trigger(e);
                // }
                //
                // {
                //     var e = new DoKillBrick(source, this, instigator);
                //     source.eventRouter.trigger(e);
                //     source.getPlayer().eventRouter.trigger(e);
                // }
            }
        }
    }

    public void addBlock(int amount)
    {
        if (!tryGetPower<BrickBlockPower>(out var power))
        {
            power = addPower<BrickBlockPower>();
            power.with(this, amount);
        }
        else
        {
            power.addBlockAmount(amount);
        }
    }

    public void removeBlock(int amount)
    {
        if (tryGetPower<BrickBlockPower>(out var power))
        {
            power.removeBlockAmount(amount);
        }
    }

    public bool hasPower<T>()
    {
        for (var i = powers.Count - 1; i >= 0; i--)
        {
            if (powers[i] is T)
                return true;
        }

        return false;
    }

    public bool tryGetPower<T>(out T power) where T : BrickPower
    {
        for (var i = powers.Count - 1; i >= 0; i--)
        {
            if (powers[i] is T t)
            {
                power = t;
                return true;
            }
        }

        power = null;
        return false;
    }

    public T addPower<T>() where T : BrickPower
    {
        var power = CLASS<BrickPower>(typeof(T));
        powers.add(power);
        return power as T;
    }

    public bool removePower<T>() where T : BrickPower
    {
        for (var i = powers.Count - 1; i >= 0; i--)
        {
            if (powers[i] is T t)
            {
                powers.removeAt(i);
                UN_CLASS(t);
                return true;
            }
        }

        return false;
    }

    public virtual bool kill()
    {
        setHealth(0);

        // we prevent further damage
        setDamageDisabled();

        var e = new OnBrickDeath(this);
        e.trigger(this);
        e.trigger();

        brickCollider.setColliderEnabled(false);

        brickRenderer.playFxDead();
        brickRenderer.setHealthBar(false);

        killTimer = 1F;
        return true;
    }

    public bool isDead()
    {
        return curHealth <= 0 && maxHealth > 0;
    }

    public void setSize(Vector2 size)
    {
        setSize(size.x, size.y);
    }

    public void setSize(float w, float h)
    {
        width = w;
        height = h;
        brickRenderer.setSize(w, h);
        brickCollider.setSize(w, h);
    }

    public Rect getRect()
    {
        Rect rect = new(0, 0, width, height);
        rect.center = getWorldPosition();
        return rect;
    }

    public void setSortingOrder(int order)
    {
        brickRenderer.setSortingOrder(order);
    }
}