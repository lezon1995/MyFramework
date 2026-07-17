using System;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains
{
    public enum Intent
    {
        NONE,
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
        SLEEP,
        STUN,
        UNKNOWN,

        ENEMY_HEAL, //敌人获得治疗
        ENEMY_SHIELD, //敌人获得护盾
        ENEMY_ATTACK, //敌人直接攻击玩家

        BRICK_HEALING_X, //治疗砖块
        
        BRICK_STAGE_INITIALIZATION,//关卡初始化

        BRICK_GENERATE_X, //生成砖块
        BRICK_MOVE_DOWN_X, //向下方移动砖块X格
        BRICK_MOVE_TO_CENTER_HORIZONTAL, //所有砖块以水平方向移动到中心
        BRICK_MOVE_TO_CENTER_VERTICAL, //所有砖块以竖直方向移动到中心
        BRICK_MOVE_TO_BORDER_TOP, //所有砖块移动到上边界
        BRICK_MOVE_TO_BORDER_BOT, //所有砖块移动到下边界
        BRICK_MOVE_TO_BORDER_LEFT, //所有砖块移动到左边界
        BRICK_MOVE_TO_BORDER_RIGHT, //所有砖块移动到右边界
        BRICK_SHUFFLE_POSITION_X, //X个砖块洗牌重新定位
        BRICK_SHUFFLE_POSITION_ALL, //所有砖块洗牌重新定位
        BRICK_KILL_X, //摧毁X个砖块
        BRICK_KILL_ALL, //摧毁全部砖块

        BRICK_EMPOWER_ATTACK_X, //赋能-进攻
        BRICK_EMPOWER_SHIELD_X, //赋能-护盾
        BRICK_EMPOWER_INVINCIBLE_X, //赋能-无敌
        BRICK_ENHANCE_HIT_EVASION_X, //强化-撞击闪避率
        BRICK_ENHANCE_SKILL_EVASION_X, //强化-技能闪避率
        BRICK_ENHANCE_HEALTH_X, //强化-生命

        BALL_DISARMED_BY_X, //缴械X个球
        BALL_DISARMED_TO_1, //缴械到1个球
        BALL_WEAKEN_HIT_ACCURACY, //弱化-撞击命中率
        BALL_WEAKEN_SKILL_ACCURACY, //弱化-技能命中率
    }

    public enum EnemyType
    {
        NORMAL,
        ELITE,
        BOSS
    }


    public abstract class AMonster : ACreature
    {
        public override bool isPlayer => false;
        const float DEATH_TIME = 1.8F;

        public static string[] MOVES;
        public static string[] DIALOG;


        public EnemyType type;

        Timer deathTimer;
        bool tintFadeOutCalled;

        protected override void OnAwake()
        {
            base.OnAwake();
            block = new MonsterBlock(this);
        }

        protected override void Initialization()
        {
            base.Initialization();
        }

        public override void onAcquire()
        {
            base.onAcquire();
        }

        public override void onRelease()
        {
            base.onRelease();
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            foreach (var p in powers)
                p.updateParticles();

            updateDeathAnimation(dt);
            // updateIntent(dt);
            // tint.update();
        }

        public override bool isDeadOrEscaped() => isDying || halfDead;

        public override void damage(DamageInfo info)
        {
            if (info.output > 0 && hasPower("IntangiblePlayer"))
                info.output = 1;

            int damageAmount = info.output;
            if (isDying)
                return;

            if (damageAmount < 0)
                damageAmount = 0;

            bool hadBlock = block.currentBlock != 0;
            bool weakenedToZero = damageAmount == 0;
            block.decrementBlock(ref damageAmount);

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
                if (damageAmount >= 99 && !Game.overkill)
                    Game.overkill = true;

                currentHealth = clamp(currentHealth - damageAmount, 0, maxHealth);

                // if (!probablyInstantKill)
                // ADungeon.effectList.Add(new StrikeEffect(this, hb.cX, hb.cY, damageAmount));

                // healthBarUpdatedEvent();
            }
            else if (!probablyInstantKill)
            {
                if (weakenedToZero && block.currentBlock == 0)
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
                // die();
                if (block.currentBlock > 0)
                {
                    block.loseBlock();
                    // ADungeon.effectList.Add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
                }
            }
        }

        public void initMoves()
        {
            // healthBarUpdatedEvent();
        }

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

                powers.Clear();
            }
        }

        public void usePreBattleAction()
        {
        }

        public void useUniversalPreBattleAction()
        {
            // actionManager.addToBot<DisplayMovesAction>().with(this);
            
            // if (ModHelper.isModEnabled("Lethality"))
            // actionManager.addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 3), 3));

            // foreach (AbstractBlight b in player.blights)
            // b.onCreateEnemy(this);

            // if (ModHelper.isModEnabled("Time Dilation") && id != "GiantHead")
            // actionManager.addToBot(new ApplyPowerAction(this, this, new SlowPower(this, 0)));
        }

        public void applyPowers()
        {
        }

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
                    Game.perfect++;
                }
            }

            music.silenceTempBgmInstantly();
            music.silenceBGMInstantly();
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

                if (Game.playtime <= 1200.0F)
                    UnlockTracker.unlockAchievement("SPEED_CLIMBER");

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
            sound.play("BOSS_VICTORY_STINGER");
            if (ADungeon.id == "TheEnding")
            {
                music.playTempBgmInstantly("STS_EndingStinger_v1.ogg", false);
            }
            else
            {
                switch (MathUtils.random(0, 3))
                {
                    case 0:
                        music.playTempBgmInstantly("STS_BossVictoryStinger_1_v3_MUSIC.ogg", false);
                        return;
                    case 1:
                        music.playTempBgmInstantly("STS_BossVictoryStinger_2_v3_MUSIC.ogg", false);
                        return;
                    case 2:
                        music.playTempBgmInstantly("STS_BossVictoryStinger_3_v3_MUSIC.ogg", false);
                        return;
                    case 3:
                        music.playTempBgmInstantly("STS_BossVictoryStinger_4_v3_MUSIC.ogg", false);
                        return;
                }

                logError("[ERROR] Attempted to play boss stinger but failed.");
            }
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

        public virtual void takeTurn()
        {
        }

        public virtual void takeMove(EnemyMoveInfo moveInfo)
        {
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