using System.Collections.Generic;

namespace MarbleHero
{
    public abstract partial class ACreature : MovableObject
    {
        public string name;
        public string id;
        public bool isPlayer;
        public bool isBloodied;

        public int gold;
        public int displayGold;
        public bool isDying;
        public bool isDead;
        public bool halfDead;
        public bool isEscaping;

        float healthHideTimer;
        public int lastDamageTaken;
        public int _health;

        public virtual int currentHealth
        {
            get => _health;
            set => _health = value;
        }

        public float currentHealthPct => (float)_health / _healthMax;

        public int _healthMax;

        public virtual int maxHealth
        {
            get => _healthMax;
            set => _healthMax = value;
        }

        public ABlock block;

        public List<CreaturePower> powers = new();
        public List<ARelic> relics = new();

        public override void setName(string name)
        {
            this.name = name;
            base.setName(name);
        }

        #region MaxHp

        public void increaseMaxHp(int amount, bool showEffect)
        {
            if (amount < 0)
                log("Why are we decreasing health with increaseMaxHealth()?");
            maxHealth += amount;
            // ADungeon.effectsQueue.add(new TextAboveCreatureEffect(hb.cX - animX, hb.cY, TEXT[2] + amount, Settings.GREEN_TEXT_COLOR));
            heal(ref amount, true);
            // healthBarUpdatedEvent();
        }

        public void decreaseMaxHealth(int amount)
        {
            if (amount < 0)
                log("Why are we increasing health with decreaseMaxHealth()?");

            maxHealth -= amount;
            if (maxHealth <= 1)
                maxHealth = 1;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            // healthBarUpdatedEvent();
        }

        #endregion

        #region Damage & Heal

        public virtual void damage(DamageInfo info)
        {
        }

        public virtual void heal(ref int healAmount, bool showEffect)
        {
            if (isDying)
                return;

            foreach (var r in player.relics)
            {
                if (isPlayer)
                    healAmount = r.onPlayerHeal(healAmount);
            }

            foreach (var p in powers)
                healAmount = p.onHeal(healAmount);

            currentHealth += healAmount;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            if (currentHealth > maxHealth / 2.0F && isBloodied)
            {
                isBloodied = false;
                foreach (var r in player.relics)
                    r.onNotBloodied();
            }

            if (healAmount > 0)
            {
                if (showEffect && isPlayer)
                {
                    // ADungeon.topPanel.panelHealEffect();
                    // ADungeon.effectsQueue.add(new HealEffect(hb.cX - animX, hb.cY, healAmount));
                }

                // healthBarUpdatedEvent();
            }
        }

        public virtual void heal(ref int amount) => heal(ref amount, true);

        #endregion

        #region Events

        public void applyStartOfTurnPowers()
        {
            foreach (var p in powers)
                p.atStartOfTurn();
        }

        public void applyTurnPowers()
        {
            foreach (var p in powers)
                p.duringTurn();
        }

        public void applyStartOfTurnPostDrawPowers()
        {
            foreach (var p in powers)
                p.atStartOfTurnPostDraw();
        }

        public virtual void applyEndOfTurnTriggers()
        {
        }

        #endregion

        #region Powers

        public void updatePowers(float dt)
        {
            for (int i = 0; i < powers.Count; i++)
                powers[i].update(dt);
        }

        public void addPower(CreaturePower power)
        {
            bool hasBuffAlready = false;
            foreach (var p in powers)
            {
                if (p.ID == power.ID)
                {
                    p.stackPower(power.amount);
                    p.updateDescription();
                    hasBuffAlready = true;
                }
            }

            if (!hasBuffAlready)
            {
                powers.Add(power);
                if (isPlayer)
                {
                    int buffCount = 0;
                    foreach (var p in powers)
                    {
                        if (p.type == PowerType.BUFF)
                            buffCount++;
                    }

                    if (buffCount >= 10)
                        UnlockTracker.unlockAchievement("POWERFUL");
                }
            }
        }

        public CreaturePower getPower(string targetID)
        {
            foreach (var p in powers)
            {
                if (p.ID == targetID)
                    return p;
            }

            return null;
        }

        public bool hasPower(string targetID)
        {
            foreach (var p in powers)
            {
                if (p.ID == targetID)
                    return true;
            }

            return false;
        }

        public bool TryGetPower(string targetID, out APower result)
        {
            foreach (var p in powers)
            {
                if (p.ID == targetID)
                {
                    result = p;
                    return true;
                }
            }

            result = null;
            return false;
        }

        #endregion

        #region Gold

        public virtual void loseGold(int amount)
        {
            if (amount > 0)
            {
                gold -= amount;
                if (gold < 0)
                    gold = 0;
            }
            else
            {
                log("NEGATIVE MONEY???");
            }
        }

        public virtual void gainGold(int amount)
        {
            if (amount < 0)
            {
                log("NEGATIVE MONEY???");
            }
            else
            {
                gold += amount;
            }
        }

        #endregion

        public sealed override void update(float elapsedTime)
        {
            base.update(elapsedTime);
        }

        public sealed override void fixedUpdate(float elapsedTime)
        {
            base.fixedUpdate(elapsedTime);
        }

        public virtual void doUpdate(float dt) => update(dt);

        public virtual void doFixedUpdate(float dt) => fixedUpdate(dt);

        public void addRelic(ARelic relic)
        {
            relic.owner = this;
            relics.Add(relic);
        }

        public void setRelic(int slot, ARelic relic)
        {
            relic.owner = this;
            relics[slot] = relic;
        }

        public virtual bool isDeadOrEscaped() => isDying || halfDead;

        public static implicit operator bool(ACreature self) => self != null;
    }
}