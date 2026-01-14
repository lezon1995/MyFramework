using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public enum PowerType
{
    BUFF,
    DEBUFF
}

public abstract class APower : ClassObject, IComparable<APower>
{
    protected static float POWER_STACK_FONT_SCALE = 8.0F;
    static float FONT_LERP = 10.0F;
    static float FONT_SNAP_THRESHOLD = 0.05F;

    protected float fontScale = 1.0F;

    List<AGameEffect> effect = new();
    public ACreature owner;
    public string name;
    public string description;
    public string ID;
    public int amount = -1;
    public int priority = 5;
    public PowerType type;
    protected bool isTurnBased;
    public bool isPostActionPower;
    public bool canGoNegative;
    public static string[] DESCRIPTIONS;

    public override void resetProperty()
    {
        base.resetProperty();
        
        fontScale = 0;
        UN_CLASS_LIST(effect);
        owner = null;
        name = null;
        description = null;
        ID = null;
        amount = 0;
        priority = 0;
        type = default;
        isTurnBased = false;
        isPostActionPower = false;
        canGoNegative = false;
    }

    public static void initialize()
    {
        // atlas = new TextureAtlas(Gdx.files. internal ("powers/powers.atlas"));
    }

    protected void loadRegion(string fileName)
    {
        // region48 = atlas.findRegion("48/" + fileName);
        // region128 = atlas.findRegion("128/" + fileName);
    }

    public string toString()
    {
        return "[" + name + "]: " + description;
    }

    public void playApplyPowerSfx()
    {
        int roll = MathUtils.random(0, 2);
        if (type == PowerType.BUFF)
        {
            if (roll == 0)
                sound.play("BUFF_1");
            else if (roll == 1)
                sound.play("BUFF_2");
            else
                sound.play("BUFF_3");
        }
        else
        {
            if (roll == 0)
                sound.play("DEBUFF_1");
            else if (roll == 1)
                sound.play("DEBUFF_2");
            else
                sound.play("DEBUFF_3");
        }
    }

    public void updateParticles()
    {
    }

    public void update(float dt)
    {
        updateFlash(dt);
        updateFontScale();
        updateColor();
    }

    void updateFlash(float dt)
    {
        for (var i = 0; i < effect.Count;)
        {
            var e = effect[i];
            e.update(dt);
            if (e.isDone)
                effect.RemoveAt(i);
            else
                i++;
        }
    }

    void updateColor()
    {
        // if (color.a != 1.0F)
        // color.a = MathHelper.fadeLerpSnap(color.a, 1.0F);
    }

    void updateFontScale()
    {
        if (fontScale != 1.0F)
        {
            fontScale = lerp(fontScale, 1.0F, Time.deltaTime * FONT_LERP);
            if (fontScale - 1.0F < FONT_SNAP_THRESHOLD)
                fontScale = 1.0F;
        }
    }

    public virtual void updateDescription()
    {
    }

    public virtual void stackPower(int stackAmount)
    {
        if (amount == -1)
        {
            log(name + " does not stack");
            return;
        }

        fontScale = POWER_STACK_FONT_SCALE;
        amount += stackAmount;
    }

    public virtual void reducePower(int reduceAmount)
    {
        if (amount - reduceAmount <= 0)
        {
            fontScale = POWER_STACK_FONT_SCALE;
            amount = 0;
        }
        else
        {
            fontScale = POWER_STACK_FONT_SCALE;
            amount -= reduceAmount;
        }
    }

    public virtual string getHoverMessage()
    {
        return name + ":\n" + description;
    }

    public virtual float atDamageGive(float damage, DamageInfo.DamageType type) => damage;
    public virtual float atDamageFinalGive(float damage, DamageInfo.DamageType type) => damage;
    public virtual float atDamageFinalReceive(float damage, DamageInfo.DamageType type) => damage;
    public virtual float atDamageReceive(float damage, DamageInfo.DamageType damageType) => damage;
    public virtual float atDamageGive(float damage, DamageInfo.DamageType type, ACard card) => atDamageGive(damage, type);
    public virtual float atDamageFinalGive(float damage, DamageInfo.DamageType type, ACard card) => atDamageFinalGive(damage, type);
    public virtual float atDamageFinalReceive(float damage, DamageInfo.DamageType type, ACard card) => atDamageFinalReceive(damage, type);
    public virtual float atDamageReceive(float damage, DamageInfo.DamageType damageType, ACard card) => atDamageReceive(damage, damageType);

    public virtual void atStartOfTurn()
    {
    }

    public virtual void duringTurn()
    {
    }

    public virtual void atStartOfTurnPostDraw()
    {
    }

    public virtual void atEndOfTurn(bool isPlayer)
    {
    }

    public virtual void atEndOfTurnPreEndTurnCards(bool isPlayer)
    {
    }

    public virtual void atEndOfRound()
    {
    }

    public virtual void onScry()
    {
    }

    public virtual void onDamageAllEnemies(int[] damage)
    {
    }

    public virtual int onHeal(int healAmount) => healAmount;

    public virtual int onAttacked(DamageInfo Info, int damageAmount)
    {
        return damageAmount;
    }

    public virtual void onAttack(DamageInfo Info, int damageAmount, ACreature target)
    {
    }

    public virtual int onAttackedToChangeDamage(DamageInfo Info, int damageAmount) => damageAmount;

    public virtual int onAttackToChangeDamage(DamageInfo Info, int damageAmount) => damageAmount;

    public virtual void onInflictDamage(DamageInfo Info, int damageAmount, ACreature target)
    {
    }

    public virtual void onPlayCard(ACard card, ACreature m)
    {
    }

    public virtual void wasHPLost(DamageInfo Info, int damageAmount)
    {
    }

    public virtual void onSpecificTrigger()
    {
    }

    public virtual void triggerMarks(ACard card)
    {
    }

    public virtual void onDeath()
    {
    }

    public virtual float modifyBlock(float blockAmount) => blockAmount;

    public virtual float modifyBlock(float blockAmount, ACard card)
    {
        return modifyBlock(blockAmount);
    }

    public virtual float modifyBlockLast(float blockAmount) => blockAmount;

    public virtual void onGainedBlock(float blockAmount)
    {
    }


    public virtual void onGainCharge(int chargeAmount)
    {
    }

    public virtual void onRemove()
    {
    }

    public virtual void onEnergyRecharge()
    {
    }

    public virtual void onDrawOrDiscard()
    {
    }

    public virtual void onAfterCardPlayed(ACard usedCard)
    {
    }

    public virtual void onInitialApplication()
    {
    }

    public int CompareTo(APower other) => priority.CompareTo(other.priority);

    public void flash()
    {
        // effect.add(new GainPowerEffect(this));
        // ADungeon.effectList.add(new FlashPowerEffect(this));
    }

    public void flashWithoutSound()
    {
        // effect.add(new SilentGainPowerEffect(this));
        // ADungeon.effectList.add(new FlashPowerEffect(this));
    }

    public virtual void onApplyPower(APower power, ACreature target, ACreature source)
    {
    }

    public Dictionary<string, object> getLocStrings()
    {
        Dictionary<string, object> powerData = new();
        powerData.Add("name", name);
        powerData.Add("description", DESCRIPTIONS);
        return powerData;
    }

    public virtual int onLoseHp(int damageAmount)
    {
        return damageAmount;
    }

    public virtual void onVictory()
    {
    }

    public virtual bool canPlayCard(ACard card)
    {
        return true;
    }
}