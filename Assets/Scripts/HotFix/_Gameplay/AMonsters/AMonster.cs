using System;
using System.Collections.Generic;

namespace MarbleHero
{
    [Serializable]
    public struct EnemyMoveInfo
    {
        public int nextMove;
        public Intent intent;
        public int baseDamage;
        public int multiplier;
        public bool isMultiDamage;

        public EnemyMoveInfo(int _nextMove, Intent _intent, int _intentBaseDmg, int _multiplier, bool _isMultiDamage)
        {
            nextMove = _nextMove;
            intent = _intent;
            baseDamage = _intentBaseDmg;
            multiplier = _multiplier;
            isMultiDamage = _isMultiDamage;
        }
    }

    public enum Intent
    {
        ATTACK,
        ATTACK_BUFF,
        ATTACK_DEBUFF,
        ATTACK_DEFEND,
        BUFF,
        DEBUFF,
        STRONG_DEBUFF,
        DEBUG,
        DEFEND,
        DEFEND_DEBUFF,
        DEFEND_BUFF,
        ESCAPE,
        MAGIC,
        NONE,
        SLEEP,
        STUN,
        UNKNOWN
    }

    public enum EnemyType
    {
        NORMAL,
        ELITE,
        BOSS
    }

    public record struct OnOpPlayerHealthChanged;

    public record struct OnOpPlayerIntentCreated;

    public record struct OnOpPlayerTakeTurn;

    public partial class AMonster : ACreature
    {
        const float DEATH_TIME = 1.8F;
        const float ESCAPE_TIME = 3.0F;

        public Timer deathTimer;
        public Timer escapeTimer;
        public bool tintFadeOutCalled;
        public bool escaped;
        public bool isEscapeNext;
        public EnemyType type;
        float hoverTimer;
        public List<DamageInfo> damageList = new();

        public Intent intent = Intent.DEBUG;
        public Intent tipIntent = Intent.DEBUG;

        int intentDmg = -1;
        int intentBaseDmg = -1;
        int intentMultiAmt;

        bool isMultiDmg;

        public EnemyMoveInfo move;
        public List<int> moveHistory = new();
        protected Dictionary<int, string> moveSet = new();
        public int nextMove = -1;
        public string moveName;

        protected List<IDisposable> disposables = new();
        public static string[] MOVES;

        public static string[] DIALOG;

        public override int currentHealth
        {
            get => _health;
            set
            {
                if (_health == value)
                    return;
                _health = value;
                new OnOpPlayerHealthChanged().trigger();
            }
        }

        public AMonster(string _name, string _id, int _maxHealth, bool ignoreBlights = false)
        {
            isPlayer = false;
            name = _name;
            id = _id;
            _healthMax = _maxHealth;
            if (ModHelper.isModEnabled("MonsterHunter"))
                _health = (int)(_health * 1.5F);

            _health = _maxHealth;
            currentBlock = 0;
            refreshIntentHbLocation();
        }


        public void refreshIntentHbLocation()
        {
            // intentHb.move(hb.cX + intentOffsetX, hb.cY + hb_h / 2.0F + INTENT_HB_W / 2.0F);
        }

        public void update(float dt)
        {
            foreach (var p in powers)
                p.updateParticles();

            updateDeathAnimation(dt);
            updateEscapeAnimation(dt);
            // updateIntent(dt);
            // tint.update();
        }

        public override void heal(ref int healAmount)
        {
            if (isDying)
                return;

            foreach (var p in powers)
                healAmount = p.onHeal(healAmount);

            currentHealth = clamp(currentHealth + healAmount, 0, maxHealth);

            if (healAmount > 0)
            {
                // ADungeon.effectList.Add(new HealEffect(hb.cX - animX, hb.cY, healAmount));
                // healthBarUpdatedEvent();
            }
        }

        public override bool isDeadOrEscaped() => isDying || halfDead || isEscaping;

        protected override void brokeBlock()
        {
            foreach (var r in player.relics)
                r.onBlockBroken(this);

            base.brokeBlock();
        }

        public override void damage(DamageInfo info)
        {
            if (info.output > 0 && hasPower("IntangiblePlayer"))
                info.output = 1;

            int damageAmount = info.output;
            if (isDying || isEscaping)
                return;

            if (damageAmount < 0)
                damageAmount = 0;

            bool hadBlock = currentBlock != 0;
            bool weakenedToZero = damageAmount == 0;
            damageAmount = decrementBlock(info, damageAmount);

            if (info.owner == player)
                foreach (var r in player.relics)
                    damageAmount = r.onAttackToChangeDamage(info, damageAmount);

            if (info.owner != null)
                foreach (var p in info.owner.powers)
                    damageAmount = p.onAttackToChangeDamage(info, damageAmount);

            foreach (var p in powers)
                damageAmount = p.onAttackedToChangeDamage(info, damageAmount);

            if (info.owner == player)
                foreach (var r in player.relics)
                    r.onAttack(info, damageAmount, this);

            foreach (var p in powers)
                p.wasHPLost(info, damageAmount);

            if (info.owner != null)
                foreach (var p in info.owner.powers)
                    p.onAttack(info, damageAmount, this);

            foreach (var p in powers)
                damageAmount = p.onAttacked(info, damageAmount);

            lastDamageTaken = Math.Min(damageAmount, currentHealth);
            bool probablyInstantKill = (currentHealth == 0);
            if (damageAmount > 0)
            {
                // if (damageAmount >= 99 && !Game.overkill)
                    // Game.overkill = true;

                currentHealth = clamp(currentHealth - damageAmount, 0, maxHealth);

                // if (!probablyInstantKill)
                // ADungeon.effectList.Add(new StrikeEffect(this, hb.cX, hb.cY, damageAmount));

                // healthBarUpdatedEvent();
            }
            else if (!probablyInstantKill)
            {
                if (weakenedToZero && currentBlock == 0)
                {
                    if (hadBlock)
                    {
                        //ADungeon.effectList.Add(new BlockedWordEffect(this, hb.cX, hb.cY, TEXT[30]));
                    }
                    else
                    {
                        //ADungeon.effectList.Add(new StrikeEffect(this, hb.cX, hb.cY, 0));
                    }
                }
                else if (Settings.SHOW_DMG_BLOCK)
                {
                    //ADungeon.effectList.Add(new BlockedWordEffect(this, hb.cX, hb.cY, TEXT[30]));
                }
            }

            if (currentHealth <= 0)
            {
                die();
                if (monsters is { areMonstersBasicallyDead: true })
                {
                    actionManager.cleanCardQueue();
                    // ADungeon.effectList.Add(new DeckPoofEffect(64.0F * Settings.scale, 64.0F * Settings.scale, true));
                    // ADungeon.effectList.Add(new DeckPoofEffect(Settings.WIDTH - 64.0F * Settings.scale, 64.0F * Settings.scale, false));
                    // ADungeon.overlayMenu.hideCombatPanels();
                }

                if (currentBlock > 0)
                {
                    loseBlock();
                    // ADungeon.effectList.Add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
                }
            }
        }

        public void init()
        {
            rollMove();
            // healthBarUpdatedEvent();
        }

        protected void setHp(int minHp, int maxHp)
        {
            _health = ADungeon.monsterHpRng.random(minHp, maxHp);
            if (ModHelper.isModEnabled("MonsterHunter"))
                _health = (int)(_health * 1.5F);
            _healthMax = _health;
        }

        protected void setHp(int hp) => setHp(hp, hp);

        void updateDeathAnimation(float dt)
        {
            bool finished = false;
            if (isDying)
            {
                finished = deathTimer.update(dt);
                if (deathTimer < DEATH_TIME && !tintFadeOutCalled)
                {
                    tintFadeOutCalled = true;
                    // tint.fadeOut();
                }
            }

            if (finished)
            {
                isDead = true;
                if (monsters is { areMonstersDead: true } && !room.isBattleOver && !room.cannotLose)
                    room.endBattle();

                dispose();
                powers.Clear();
            }
        }

        void updateEscapeAnimation(float dt)
        {
            bool finished = false;
            if (escapeTimer)
            {
                finished = escapeTimer.update(dt);
            }

            if (finished)
            {
                escaped = true;
                if (monsters is { areMonstersDead: true } && !room.isBattleOver && !room.cannotLose)
                    room.endBattle();
            }
        }

        public void dispose()
        {
            // if (img != null)
            // {
            //     logger.Info("Disposed monster img asset");
            //     img.dispose();
            //     img = null;
            // }
            //
            // foreach (var d in disposables)
            // {
            //     logger.Info("Disposed extra monster assets");
            //     d.dispose();
            // }
            //
            // if (atlas != null)
            // {
            //     atlas.dispose();
            //     atlas = null;
            //     logger.Info("Disposed Texture: " + name);
            // }
        }

        public void escapeNext()
        {
            isEscapeNext = true;
        }

        public void deathReact()
        {
        }

        public void escape()
        {
            // hideHealthBar();
            isEscaping = true;
            // escapeTimer = ESCAPE_TIME;
        }

        public void die() => die(true);

        public void die(bool triggerRelics)
        {
            if (isDying) 
                return;

            isDying = true;
            if (currentHealth <= 0 && triggerRelics)
            {
                foreach (var p in powers)
                    p.onDeath();
            }

            if (triggerRelics)
            {
                foreach (var r in player.relics)
                    r.onMonsterDeath(this);
            }

            if (monsters is { areMonstersBasicallyDead: true })
            {
                // ADungeon.overlayMenu.endTurnButton.disable();
            }

            currentHealth = 0;

            if (!Settings.FAST_MODE)
                deathTimer += DEATH_TIME;
            else
                deathTimer++;

            // StatsScreen.incrementEnemySlain();
        }

        public void usePreBattleAction()
        {
        }

        public void useUniversalPreBattleAction()
        {
            // if (ModHelper.isModEnabled("Lethality"))
            // actionManager.addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 3), 3));

            // foreach (AbstractBlight b in player.blights)
            // b.onCreateEnemy(this);

            // if (ModHelper.isModEnabled("Time Dilation") && id != "GiantHead")
            // actionManager.addToBot(new ApplyPowerAction(this, this, new SlowPower(this, 0)));
        }

        void calculateDamage(int dmg)
        {
            var target = player;
            float tmp = dmg;
            if (Settings.isEndless)
            {
                // float mod = player.getBlight("DeadlyEnemies").effectFloat();
                // tmp *= mod;
            }

            foreach (var p in powers)
                tmp = p.atDamageGive(tmp, DamageInfo.DamageType.NORMAL);

            foreach (var p in target.powers)
                tmp = p.atDamageReceive(tmp, DamageInfo.DamageType.NORMAL);

            foreach (var p in powers)
                tmp = p.atDamageFinalGive(tmp, DamageInfo.DamageType.NORMAL);

            foreach (var p in target.powers)
                tmp = p.atDamageFinalReceive(tmp, DamageInfo.DamageType.NORMAL);

            dmg = floor(tmp);
            if (dmg < 0)
                dmg = 0;

            intentDmg = dmg;
        }

        public void applyPowers()
        {
            // if (canApplyBackAttack && !hasPower("BackAttack"))
            // actionManager.addToTop(new ApplyPowerAction(this, null, new BackAttackPower(this)));

            foreach (DamageInfo dmg in damageList)
            {
                dmg.applyPowers(this, player);
            }

            if (move.baseDamage > -1)
                calculateDamage(move.baseDamage);

            // intentImg = getIntentImg();
            // updateIntentTip();
        }

        public void removeSurroundedPower()
        {
            // if (hasPower("BackAttack"))
            // actionManager.addToTop(new RemoveSpecificPowerAction(this, null, "BackAttack"));
        }

        public void changeState(string stateName)
        {
        }

        public void addToBot(AGameAction action) => actionManager.addToBot(action);

        public void addToTop(AGameAction action) => actionManager.addToTop(action);

        protected void onBossVictoryLogic()
        {
            if (Settings.FAST_MODE)
                deathTimer += 0.7F;
            else
                deathTimer += 1.5F;

            // ADungeon.scene.fadeInAmbiance();
            if (room.evt == null)
            {
                // ADungeon.bossCount++;
                // StatsScreen.incrementBossSlain();
                if (GameActionManager.turn <= 1)
                    UnlockTracker.unlockAchievement("YOU_ARE_NOTHING");

                if (GameActionManager.damageReceivedThisCombat - GameActionManager.hpLossThisCombat <= 0)
                {
                    UnlockTracker.unlockAchievement("PERFECT");
                    // Game.perfect++;
                }
            }

            // music.silenceTempBgmInstantly();
            // music.silenceBGMInstantly();
            playBossStinger();

            // foreach (AbstractBlight b in player.blights)
            // b.onBossDefeat();
        }

        protected void onFinalBossVictoryLogic()
        {
            if (ADungeon.ascensionLevel >= 20 && ADungeon.bossList.Count == 2)
                return;

            if (!Settings.isEndless)
            {
                // if (!Settings.isFinalActAvailable || !Settings.hasRubyKey || !Settings.hasEmeraldKey || !Settings.hasSapphireKey)
                    // Game.stopClock = true;

                // if (Game.playtime <= 1200.0F)
                    // UnlockTracker.unlockAchievement("SPEED_CLIMBER");

                // if (player.masterDeck.Count <= 5)
                    // UnlockTracker.unlockAchievement("MINIMALIST");

                bool commonSenseUnlocked = true;
                // foreach (var c in player.masterDeck.group)
                // {
                //     if (c.rarity is CardRarity.Uncommon or CardRarity.Rare)
                //     {
                //         commonSenseUnlocked = false;
                //         break;
                //     }
                // }

                if (commonSenseUnlocked)
                    UnlockTracker.unlockAchievement("COMMON_SENSE");

                if (player.relics.Count == 1)
                    UnlockTracker.unlockAchievement("ONE_RELIC");

                if (Settings.isDailyRun && !Settings.seedSet)
                    UnlockTracker.unlockLuckyDay();
            }
        }

        public static void playBossStinger()
        {
            // Game.sound.play("BOSS_VICTORY_STINGER");
            // if (ADungeon.id.equals("TheEnding"))
            // {
            //     music.playTempBgmInstantly("STS_EndingStinger_v1.ogg", false);
            // }
            // else
            // {
            //     switch (MathUtils.random(0, 3))
            //     {
            //         case 0:
            //             music.playTempBgmInstantly("STS_BossVictoryStinger_1_v3_MUSIC.ogg", false);
            //             return;
            //         case 1:
            //             music.playTempBgmInstantly("STS_BossVictoryStinger_2_v3_MUSIC.ogg", false);
            //             return;
            //         case 2:
            //             music.playTempBgmInstantly("STS_BossVictoryStinger_3_v3_MUSIC.ogg", false);
            //             return;
            //         case 3:
            //             music.playTempBgmInstantly("STS_BossVictoryStinger_4_v3_MUSIC.ogg", false);
            //             return;
            //     }
            //
            //     logger.Info("[ERROR] Attempted to play boss stinger but failed.");
            // }
        }

        public Dictionary<string, object> getLocStrings()
        {
            Dictionary<string, object> data = new()
            {
                { "name", name },
                { "moves", MOVES },
                { "dialogs", DIALOG }
            };
            return data;
        }

        public int getIntentDmg() => intentDmg;

        public int getIntentBaseDmg() => intentBaseDmg;

        public void setIntentBaseDmg(int amount) => intentBaseDmg = amount;

        public virtual void takeTurn()
        {
        }

        protected virtual void getMove(int paramInt)
        {
        }

        public void createIntent()
        {
            intent = move.intent;
            nextMove = move.nextMove;
            intentBaseDmg = move.baseDamage;
            if (move.baseDamage > -1)
            {
                calculateDamage(intentBaseDmg);
                if (move.isMultiDamage)
                {
                    intentMultiAmt = move.multiplier;
                    isMultiDmg = true;
                }
                else
                {
                    intentMultiAmt = -1;
                    isMultiDmg = false;
                }
            }

            // intentImg = getIntentImg();
            // intentBg = getIntentBg();
            tipIntent = intent;
            // intentAlpha = 0.0F;
            // intentAlphaTarget = 1.0F;
            // intentParticleTimer = 0.5F;
            // updateIntentTip();

            new OnOpPlayerIntentCreated().trigger();
            onIntentCreated(move);
        }

        protected virtual void onIntentCreated(EnemyMoveInfo m)
        {
            
        }
        
        public void setMove(string moveName, int nextMove, Intent intent, int baseDamage, int multiplier, bool isMultiDamage)
        {
            this.moveName = moveName;
            if (nextMove != -1)
                moveHistory.Add(nextMove);
            move = new(nextMove, intent, baseDamage, multiplier, isMultiDamage);
        }

        public void setMove(int nextMove, Intent intent, int baseDamage, int multiplier, bool isMultiDamage)
        {
            setMove(null, nextMove, intent, baseDamage, multiplier, isMultiDamage);
        }

        public void setMove(int nextMove, Intent intent, int baseDamage)
        {
            setMove(null, nextMove, intent, baseDamage, 0, false);
        }

        public void setMove(string moveName, int nextMove, Intent intent, int baseDamage)
        {
            setMove(moveName, nextMove, intent, baseDamage, 0, false);
        }

        public void setMove(string moveName, int nextMove, Intent intent)
        {
            switch (intent)
            {
                case Intent.ATTACK:
                case Intent.ATTACK_BUFF:
                case Intent.ATTACK_DEFEND:
                case Intent.ATTACK_DEBUFF:
                    // for (int i = 0; i < 8; i++)
                    // {
                    //     ADungeon.effectsQueue.Add(new TextAboveCreatureEffect(
                    //         MathUtils.random(Settings.WIDTH * 0.25F, Settings.WIDTH * 0.75F),
                    //         MathUtils.random(Settings.HEIGHT * 0.25F, Settings.HEIGHT * 0.75F), "ENEMY MOVE " + moveName + " IS SET INCORRECTLY! REPORT TO DEV", Color.red));
                    // }

                    log("ENEMY MOVE " + moveName + " IS SET INCORRECTLY! REPORT TO DEV");
                    break;
            }

            setMove(moveName, nextMove, intent, -1, 0, false);
        }

        public void setMove(int nextMove, Intent intent)
        {
            setMove(null, nextMove, intent, -1, 0, false);
        }

        public void rollMove()
        {
            getMove(ADungeon.aiRng.random(99));
        }

        protected bool lastMove(int move)
        {
            return moveHistory.Count switch
            {
                0 => false,
                _ => moveHistory[^1] == move
            };
        }

        protected bool lastMoveBefore(int move)
        {
            return moveHistory.Count switch
            {
                0 or < 2 => false,
                _ => moveHistory[^2] == move
            };
        }

        protected bool lastTwoMoves(int move)
        {
            if (moveHistory.Count < 2)
                return false;

            return moveHistory[^1] == move && moveHistory[^2] == move;
        }

        public override void applyEndOfTurnTriggers()
        {
            foreach (var power in powers)
            {
                power.atEndOfTurnPreEndTurnCards(false);
                power.atEndOfTurn(false);
            }
        }
    }
}