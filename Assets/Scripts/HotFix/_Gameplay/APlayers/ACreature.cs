using System.Collections.Generic;
using UnityEngine;

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

        public int _healthMax;

        public virtual int maxHealth
        {
            get => _healthMax;
            set => _healthMax = value;
        }

        public int currentBlock;

        public List<APower> powers = new();
        public List<ARelic> relics = new();

        public override void setName(string name)
        {
            this.name = name;
            base.setName(name);
        }

        public virtual void createBrickGroup(int turnNum)
        {
        }

        public virtual void moveBrickGroup(int turnNum)
        {
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

        #region Block

        public void addBlock(int blockAmount)
        {
            float tmp = blockAmount;
            if (isPlayer)
            {
                foreach (var r in player.relics)
                    tmp = r.onPlayerGainedBlock(tmp);

                if (tmp > 0.0F)
                    foreach (var p in powers)
                        p.onGainedBlock(tmp);
            }

            bool effect = currentBlock == 0;
            foreach (var m in room.monsters.monsters)
            {
                foreach (var p in m.powers)
                    tmp = p.onPlayerGainedBlock(tmp);
            }

            currentBlock += floor(tmp);
            if (currentBlock >= 99 && isPlayer)
                UnlockTracker.unlockAchievement("IMPERVIOUS");

            if (currentBlock > 999)
                currentBlock = 999;

            if (currentBlock == 999)
                UnlockTracker.unlockAchievement("BARRICADED");

            if (effect && currentBlock > 0)
            {
                // gainBlockAnimation();
            }
            else if (blockAmount > 0)
            {
                // Color tmpCol = Settings.GOLD_COLOR.cpy();
                // tmpCol.a = blockTextColor.a;
                // blockTextColor = tmpCol;
                // blockScale = 5.0F;
            }
        }

        public void loseBlock(int amount, bool noAnimation)
        {
            bool effect = currentBlock != 0;
            currentBlock -= amount;
            if (currentBlock < 0)
                currentBlock = 0;

            if (currentBlock == 0 && effect)
            {
                // if (!noAnimation)
                // ADungeon.effectList.add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
            }
            else if (currentBlock > 0 && amount > 0)
            {
                Color tmp = Color.white;
                // tmp.a = blockTextColor.a;
                // blockTextColor = tmp;
                // blockScale = 5.0F;
            }
        }

        public void loseBlock() => loseBlock(currentBlock);
        public void loseBlock(bool noAnimation) => loseBlock(currentBlock, noAnimation);
        public void loseBlock(int amount) => loseBlock(amount, false);

        protected virtual void brokeBlock()
        {
            // ADungeon.effectList.add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
            // Game.sound.play("BLOCK_BREAK");
        }

        protected int decrementBlock(DamageInfo info, int damageAmount)
        {
            if (info.type != DamageInfo.DamageType.HP_LOSS && currentBlock > 0)
            {
                // Game.screenShake.shake(ScreenShake.ShakeIntensity.MED, ScreenShake.ShakeDur.SHORT, false);
                if (damageAmount > currentBlock)
                {
                    damageAmount -= currentBlock;
                    // if (Settings.SHOW_DMG_BLOCK)
                    // ADungeon.effectList.add(new BlockedNumberEffect(hb.cX, hb.cY + hb.height / 2.0F, Integer.toString(currentBlock)));
                    loseBlock();
                    brokeBlock();
                }
                else if (damageAmount == currentBlock)
                {
                    damageAmount = 0;
                    loseBlock();
                    brokeBlock();
                    // ADungeon.effectList.add(new BlockedWordEffect(this, hb.cX, hb.cY, TEXT[1]));
                }
                else
                {
                    // Game.sound.play("BLOCK_ATTACK");
                    loseBlock(damageAmount);
                    // for (int i = 0; i < 18; i++)
                    // ADungeon.effectList.add(new BlockImpactLineEffect(hb.cX, hb.cY));
                    // if (Settings.SHOW_DMG_BLOCK)
                    // ADungeon.effectList.add(new BlockedNumberEffect(hb.cX, hb.cY + hb.height / 2.0F, Integer.toString(damageAmount)));
                    damageAmount = 0;
                }
            }

            return damageAmount;
        }

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

        public void addPower(APower power)
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
                        if (p.type == APower.PowerType.BUFF)
                            buffCount++;
                    }

                    if (buffCount >= 10)
                        UnlockTracker.unlockAchievement("POWERFUL");
                }
            }
        }

        public APower getPower(string targetID)
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