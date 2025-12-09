using System;
using TMPro;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public partial class Brick : MovableObject, IDamageable<Ball>
{
    public int instanceID;
    protected Type type; // 角色类型
    public long guid; // 角色的唯一ID

    #region Stats

    public float maxHealth = 10F;
    public int maxHealthStack = 1;
    public float physicResist;
    public float magicResist;
    public float dodgeChance;

    public float getPhysicResist()
    {
        return physicResist;
    }

    #endregion


    Action<GameObject, Brick> onObjectSet;
    Action<Brick> onDead;
    SpriteRenderer brickRenderer;
    TextMeshPro textHealth;

    public float curHealth;
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

    public void setOnObjectSet(Action<GameObject, Brick> action) => onObjectSet = action;
    public void setOnDead(Action<Brick> action) => onDead = action;
    public void setBrickType(Type t) => type = t;
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
        brickRenderer = null;
        textHealth = null;

        maxHealth = 0F;
        maxHealthStack = 0;
        physicResist = 0F;
        magicResist = 0F;
        dodgeChance = 0F;

        curHealth = 0F;
        curHealthStack = 0;
        immuneToDamage = false;
        invulnerable = false;

        _coroutineState = default;
        _coroutineTimeElapsed = 0F;
        _invincibleTime = 0F;
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        instanceID = obj.GetInstanceID();
        onObjectSet?.Invoke(obj, this);
        brickRenderer = getUnityComponentInChild<SpriteRenderer>(true);
        textHealth = getUnityComponentInChild<TextMeshPro>(true);

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

    public void setHealth(float value)
    {
        curHealth = value;
        textHealth.text = curHealth.ToString("F0");
    }

    public void setMaxHealth(float value)
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

        if (dmg.mix.on)
        {
            dmg.mix = calculator.computeDamageMix(dmg.mix, rawFinalDamage, physicResist, magicResist);
            actualDamage = dmg.mix.Sum();
        }
        else
        {
            actualDamage = calculator.computeDamageDefence(dmg.actualType, rawFinalDamage, physicResist, magicResist);
        }

        return actualDamage > 0;
    }

    public virtual void damage(Dmg dmg, GameObject instigator, Ball source, float invincibleTime = 0, Vector3 direction = default, IDmgCalculator calculator = null)
    {
        if (!canTakeDamageThisFrame(out _))
            return;

        dmg.setDmgRate(source.dmgRate);

        computeDamageOutput(ref dmg, out var damageDealt, out var damageRaw, calculator);

        //设置此次dmg实际造成的伤害，并通知伤害飘字显示
        {
            dmg.setDamageRaw(damageRaw);
            dmg.setDamageDealt(damageDealt);
            dmg.setDirection(direction);

            if ((int)dmg.damageDealt > 0)
            {
                new DmgTextEvent(dmg, getTransform()).trigger();
            }
        }

        //触发本次伤害所造成的攻击特效/技能特效
        if (!dmg.isSelf)
        {
            switch (dmg.effect)
            {
                case Dmg.Effects.Attack:
                    source.trigger(new DoAttackEffect(this));
                    break;
                case Dmg.Effects.Ability:
                    source.trigger(new DoAbilityEffect(this));
                    break;
            }
        }

        // we decrease the character's health by the damage
        float preHealth = curHealth;
        var health = clampMin(curHealth - damageDealt, 0F);
        setHealth(health);
        // lastDamage = damageDealt;
        // lastDamageType = dmg.actualType;
        // lastDamageDirection = direction;

        trigger(new OnHit());

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
            if (!dmg.isSelf)
            {
                source.trigger(new DoDmgBrick(this, dmg));
            }
        }

        //造成伤害后，触发OnDmg
        {
            if (!dmg.isSelf)
                trigger(new OnDmg(source, dmg));
        }

        // we play our feedback
        // if (FeedbackIsProportionalToDamage)
        //     DamageMMFeedbacks.Play(transform.position, damageDealt);
        // else
        //     DamageMMFeedbacks.Play(transform.position);

        // we update the health bar
        // UpdateHealthBar(true);

        //检测是否死亡
        {
            if (curHealth <= 0)
            {
                curHealth = 0;
                var isLethal = kill();
                if (isLethal && !dmg.isSelf)
                    source.trigger(new DoKillBrick(this, instigator));
            }
        }

        textHealth.text = curHealth.ToString("F0");
    }

    public virtual bool kill()
    {
        if (immuneToDamage)
            return false;

        setHealth(0);

        // we prevent further damage
        setDamageDisabled();

        trigger(new OnDeath());

        onDead?.Invoke(this);

        return true;
    }

    public bool isDead()
    {
        return curHealth <= 0 && maxHealth > 0;
    }
}