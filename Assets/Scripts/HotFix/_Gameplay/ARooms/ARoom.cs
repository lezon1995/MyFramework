using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains
{
    public enum RoomPhase
    {
        COMBAT,
        EVENT,
        COMPLETE,
        INCOMPLETE
    }

    public enum RoomType
    {
        EMPTY,
        NEOW,
        SHOP,
        MONSTER,
        ELITE,
        REST,
        TREASURE,
        EVENT,
        BOSS,
        VICTORY,
        TRUE_VICTORY,
    }

    public enum RoomPhaseType
    {
        NONE,
        PREPARE,//战前准备阶段，通常是等个几秒介绍一下接下来可能出现的怪物
        BATTLE,//战斗阶段
        BATTLE_PASS_CLEANUP,//战后舞台清理阶段
        LEVEL_UP_REWARD,//升级奖励阶段
        SHOPPING,//购物阶段
        GAME_SETTLEMENT,//对局结算阶段
    }

    public record struct OnBattleStart;

    public abstract partial class ARoom
    {
        const int BLIZZARD_POTION_MOD_AMT = 10;

        public abstract RoomType Type { get; }
        public RoomPhaseType LastPhase { get; set; }
        public RoomPhaseType CurPhase { get; set; }
        public RoomPhaseType ToPhase { get; set; }
        public bool inPlayerTurn => CurPhase == RoomPhaseType.BATTLE;
        public APlayer Player => _player;

        // public List<AbstractPotion> potions = new();
        public List<ARelic> relics = new();

        // public List<RewardItem> rewards = new();
        // public SoulGroup souls = new();
        public RoomPhase phase = RoomPhase.COMBAT;
        public AEvent evt;
        public MonsterGroup monsters { get; set; }
        public Timer endBattleTimer;
        public Timer rewardPopOutTimer = 1.0F;
        public Timer waitTimer;
        protected string mapSymbol;
        protected APlayer _player;

        public bool isFightStarted;
        public bool isFightEnded;
        public bool isPlayerTurnEnd;
        public bool isEnemyTurnEnd;
        public bool isBattleOver;
        public bool cannotLose;
        public bool eliteTrigger;
        public static int blizzardPotionMod;
        public bool mugged, smoked;
        public bool combatEvent;
        public bool rewardAllowed = true;
        public bool rewardTime;
        public bool skipMonsterTurn { get; set; }
        public bool isEndingTurn { get; set; }
        public int baseRareCardChance = 3;
        public int baseUncommonCardChance = 37;
        public int rareCardChance = 3;
        public int uncommonCardChance = 37;

        public virtual void onRoomInitialize()
        {
        }

        public virtual void onPlayerEntry(APlayer p)
        {
            _player = p;
            log($"onPlayerEntry Room {GetType().Name}");
        }

        public virtual void onPlayerExit()
        {
            log($"onPlayerExit Room {GetType().Name}");
        }
        
        public virtual void onRoomUninitialize()
        {
        }

        protected virtual void changePhase(RoomPhaseType type)
        {
            LastPhase = CurPhase;
            CurPhase = type;
        }

        protected virtual void onPlayerTurnStart(int turn)
        {
            log($"onPlayerTurnStart {GetType().Name}");
        }

        protected virtual void onPlayerTurnEnd()
        {
            log($"onPlayerTurnEnd {GetType().Name}");
        }

        protected virtual void onEnemyTurnStart(int turn)
        {
            log($"onEnemyTurnStart {GetType().Name}");
        }

        protected virtual void onEnemyTurnEnd()
        {
            log($"onEnemyTurnEnd {GetType().Name}");
        }

        protected virtual void onCombatUpdate(float dt)
        {
        }

        protected virtual void onCombatFixedUpdate(float dt)
        {
        }

        public virtual void onCombatFightStart()
        {
        }

        protected virtual void onFightPhaseEnd()
        {
        }

        protected virtual void onPlayerCompletedRewardGold()
        {
        }

        protected virtual int onPlayerCompletedGetPotionChance() => 0;

        public virtual void applyEmeraldEliteBuff()
        {
        }

        public string getMapSymbol() => mapSymbol;
        public void setMapSymbol(string newSymbol) => mapSymbol = newSymbol;
        public virtual CardRarity getCardRarity(int roll) => getCardRarity(roll, true);

        protected virtual CardRarity getCardRarity(int roll, bool useAlternation)
        {
            rareCardChance = baseRareCardChance;
            uncommonCardChance = baseUncommonCardChance;
            if (useAlternation)
                alterCardRarityProbabilities();

            if (roll < rareCardChance)
            {
                if (roll >= baseRareCardChance)
                {
                    foreach (var r in player.relics)
                    {
                        if (r.changeRareCardRewardChance(baseRareCardChance) > baseRareCardChance)
                            r.flash();
                    }
                }

                return CardRarity.Rare;
            }

            if (roll < rareCardChance + uncommonCardChance)
            {
                if (roll >= baseRareCardChance + baseUncommonCardChance)
                {
                    foreach (var r in player.relics)
                    {
                        if (r.changeUncommonCardRewardChance(baseUncommonCardChance) > baseUncommonCardChance)
                            r.flash();
                    }
                }

                return CardRarity.Uncommon;
            }

            return CardRarity.Common;
        }

        public void alterCardRarityProbabilities()
        {
            foreach (var r in player.relics)
                rareCardChance = r.changeRareCardRewardChance(rareCardChance);

            foreach (var r in player.relics)
                uncommonCardChance = r.changeUncommonCardRewardChance(uncommonCardChance);
        }

        public void updateObjects(float dt)
        {
            // souls.update(dt);

            // for (Iterator<AbstractPotion> iterator = potions.iterator(); iterator.hasNext();)
            // {
            //     AbstractPotion tmpPotion = iterator.next();
            //     tmpPotion.update();
            //     if (tmpPotion.isObtained)
            //         iterator.remove();
            // }
            //

            for (var i = relics.Count - 1; i >= 0; i--)
            {
                var relic = relics[i];
                relic.update(dt);
                if (relic.isDone)
                {
                    relics.RemoveAt(i);
                }
            }
        }

        public virtual void update(float dt)
        {
            // if (!ADungeon.isScreenUp && InputHelper.pressedEscape && ADungeon.overlayMenu.cancelButton.current_x == CancelButton.HIDE_X)
            // ADungeon.settingsScreen.open();

            if (Settings.isDebug)
            {
                // if (InputHelper.justClickedRight)
                // {
                //     player.obtainPotion(new BlessingOfTheForge());
                //     ADungeon.scene.randomizeScene();
                // }

                if (DevInputActionSet.gainGold.isJustPressed())
                    player.gainGold(100);
            }

            switch (phase)
            {
                case RoomPhase.EVENT:
                    evt?.updateDialog();
                    break;
                case RoomPhase.COMBAT:
                    onCombatUpdate(dt);
                    monsters?.update(dt);
                    if (waitTimer)
                    {
                        bool finished = false;
                        if (actionManager.currentAction || actionManager.anyAction())
                            actionManager.update(dt);
                        else
                            finished = waitTimer.update(dt);

                        if (finished)
                        {
                            // startBattle();
                            startGame();
                        }
                    }
                    else
                    {
                        // if (Settings.isDebug && DevInputActionSet.drawCard.isJustPressed())
                        // actionManager.addToTop(new DrawCardAction(player, 1));

                        if (!ADungeon.isScreenUp)
                        {
                            actionManager.update(dt);

                            // if (monsters is {anyAlive: true} && player.currentHealth > 0)
                            // player.updateInput(dt);
                        }

                        if (ADungeon.screen != CurrentScreen.HAND_SELECT)
                            player.combatUpdate(dt);

                        if (player.isEndingTurn)
                            endPlayerTurn();

                        if (isEndingTurn)
                            endEnemyTurn();
                        
                        if (CurPhase != ToPhase)
                        {
                            changePhase(ToPhase);
                        }
                    }

                    if (isBattleOver && actionManager.isEmpty())
                    {
                        skipMonsterTurn = false;
                        if (endBattleTimer.update(dt))
                            collectBattleReward();
                    }

                    monsters?.updateAnimations(dt);
                    break;
                case RoomPhase.COMPLETE:
                    if (!ADungeon.isScreenUp)
                    {
                        actionManager.update(dt);
                        evt?.updateDialog();
                        if (actionManager.isEmpty() && !ADungeon.isFadingOut)
                        {
                            if (rewardPopOutTimer.update(dt))
                            {
                                if (evt == null)
                                {
                                    // ADungeon.overlayMenu.proceedButton.show();
                                    break;
                                }

                                // if (evt is not AbstractImageEvent && !evt.hasFocus)
                                // ADungeon.overlayMenu.proceedButton.show();
                            }
                        }
                    }

                    break;
                case RoomPhase.INCOMPLETE:
                    break;
                default:
                    break;
            }

            player.OnUpdate(dt);
        }

        public virtual void fixedUpdate(float dt)
        {
            switch (phase)
            {
                case RoomPhase.COMBAT:
                    onCombatFixedUpdate(dt);
                    monsters?.update(dt);
                    if (waitTimer)
                    {
                        effectManager.fixedUpdateLogic(dt);
                        if (actionManager.currentAction || actionManager.anyAction())
                            actionManager.fixedUpdate(dt);
                    }
                    else
                    {
                        if (!ADungeon.isScreenUp)
                        {
                            effectManager.fixedUpdateLogic(dt);
                            actionManager.fixedUpdate(dt);
                        }

                        if (ADungeon.screen != CurrentScreen.HAND_SELECT)
                            player.combatFixedUpdate(dt);
                    }

                    break;
                case RoomPhase.COMPLETE:
                    if (!ADungeon.isScreenUp)
                    {
                        effectManager.fixedUpdateLogic(dt);
                        actionManager.fixedUpdate(dt);
                    }

                    break;
                case RoomPhase.INCOMPLETE:
                    break;
            }

            player.OnFixedUpdate(dt);
        }

        public void completeRoom()
        {
            phase = RoomPhase.COMPLETE;
        }

        void collectBattleReward()
        {
            completeRoom();
            if (room is not MonsterRoomBoss || _dungeon is not TheBeyond || Settings.isEndless)
                sound.play("VICTORY");

            endBattleTimer.kill();
            onPlayerCompletedRewardGold();

            if (room is not MonsterRoomBoss || _dungeon is not TheBeyond and not TheEnding || Settings.isEndless)
            {
                if (!ADungeon.loading_post_combat)
                {
                    dropReward();
                    addPotionToRewards();
                }

                saveBattle();
            }
        }

        void saveBattle()
        {
            int card_seed_before_roll = ADungeon.cardRng.counter;
            // int card_randomizer_before_roll = ADungeon.cardBlizzRandomizer;
            if (rewardAllowed)
            {
                // if (mugged)
                // ADungeon.combatRewardScreen.openCombat(TEXT[0]);
                // else if (smoked)
                // ADungeon.combatRewardScreen.openCombat(TEXT[1], true);
                // else
                // ADungeon.combatRewardScreen.open();

                if (!Game.loadingSave && !ADungeon.loading_post_combat)
                {
                    var saveFile = new SaveFile(SaveType.POST_COMBAT)
                    {
                        card_seed_count = card_seed_before_roll,
                        // card_random_seed_randomizer = card_randomizer_before_roll
                    };

                    if (combatEvent)
                        saveFile.event_seed_count--;

                    SaveAndContinue.save(saveFile);
                    // ADungeon.effectList.Add(new GameSavedEffect());
                }
                else
                {
                    Game.loadingSave = false;
                }

                ADungeon.loading_post_combat = false;
            }
        }

        void startBattle()
        {
            actionManager.turnHasEnded = true;
            if (!ADungeon.isScreenUp)
                effectManager.addRender<BattleStartEffect>();

            actionManager.addToBot<GainEnergyAndEnableControlsAction>().with(1);
            player.applyStartOfCombatPreDrawLogic();
            player.applyStartOfCombatLogic();
            ADungeon.overlayMenu.showCombatPanels();

            startGameTurn();
            new OnBattleStart().trigger();
        }

        public void startGame()
        {
            effectManager.addRender<GameStartEffect>();
            ToPhase = RoomPhaseType.PREPARE;
        }

        public void enter_SelectCharacter()
        {
        }
        public void enter_SelectWeapon()
        {
        }
        public void enter_SelectDifficulty()
        {
        }
        public void enter_Prepare()
        {
        }
        public void enter_Battle()
        {
        }
        public void enter_BattlePassCleanup()
        {
        }
        public void enter_LevelUpReward()
        {
        }
        public void enter_Shopping()
        {
        }
        public void enter_GameSettlement()
        {
        }
        public void endGame()
        {
        }

        public void startGameTurn()
        {
            GameActionManager.turn.increment();
            GameActionManager.turnScore = 0;

            // startPlayerTurn();
            startEnemyTurn();
        }

        public void startPlayerTurn()
        {
            isPlayerTurnEnd = false;
            isFightEnded = false;

            player.cardsPlayedThisTurn = 0;
            player.applyStartOfTurnRelics();
            player.applyStartOfTurnPowers();

            onPlayerTurnStart(GameActionManager.turn);

            actionManager.useNextCombatActions();

            skipMonsterTurn = false;
            actionManager.turnHasEnded = false;

            GameActionManager.totalDiscardedThisTurn = 0;
            GameActionManager.damageReceivedThisTurn = 0;

            if (!isBattleOver)
            {
                actionManager.addToBot<PlayerStartTurnAction>();
                // actionManager.addToBot(new DrawCardAction(player, player.gameHandSize));
                player.applyStartOfTurnPostDrawRelics();
                player.applyStartOfTurnPostDrawPowers();
                actionManager.addToBot<EnableEndTurnButtonAction>();
            }
        }

        void endPlayerTurn()
        {
            isPlayerTurnEnd = true;
            player.applyEndOfTurnTriggers();
            actionManager.addToBot<ClearCardQueueAction>();
            // actionManager.addToBot(new DiscardAtEndOfTurnAction());
            actionManager.addToBot<EndPlayerTurnAction>().with(this);
            player.isEndingTurn = false;

            onPlayerTurnEnd();
        }

        public void startEnemyTurn()
        {
            isEnemyTurnEnd = false;
            onEnemyTurnStart(GameActionManager.turn);
            actionManager.addToBot<StartEnemyTurnAction>().with(room);
        }

        public void endEnemyTurn()
        {
            isEnemyTurnEnd = true;
            onEnemyTurnEnd();
        }

        public void startFight()
        {
            actionManager.addToBot<FightStartAction>();
        }

        public void checkFightingOver()
        {
            if (player.BallManagement.Instance.anyActiveBall())
                return;

            endFight();
        }

        public void endFight()
        {
            isFightStarted = false;
            isFightEnded = true;
            onFightPhaseEnd();
        }

        public void endBattle()
        {
            isBattleOver = true;
            if (player.currentHealth == 1)
                UnlockTracker.unlockAchievement("SHRUG_IT_OFF");

            player.onVictory();
            endBattleTimer = 0.25F;

            if (!smoked)
            {
                if (GameActionManager.damageReceivedThisCombat - GameActionManager.hpLossThisCombat <= 0 && this is MonsterRoomElite)
                    Game.champion++;
            }

            metricData.addEncounterData();
            actionManager.clear();
            // player.inSingleTargetMode = false;
            player.resetControllerValues();
            ADungeon.overlayMenu.hideCombatPanels();
        }

        public virtual void dropReward()
        {
        }

        public void spawnRelicAndObtain(float x, float y, ARelic relic)
        {
            if (relic.relicId == "Circlet" && player.tryGetRelic("Circlet", out var circlet))
            {
                circlet.counter++;
                circlet.flash();
            }
            else
            {
                relic.spawn(x, y);
                relics.Add(relic);
                relic.obtain();
                relic.isObtained = true;
                relic.isAnimating = false;
                relic.isDone = false;
                relic.flash();
            }
        }


        public void applyEndOfTurnRelics()
        {
            foreach (var r in player.relics)
                r.onPlayerEndTurn();

            // foreach (var b in player.blights)
            //     b.onPlayerEndTurn();
        }

        public void applyEndOfTurnPreCardPowers()
        {
            foreach (var p in player.powers)
                p.atEndOfTurnPreEndTurnCards(true);
        }

        public void addRelicToRewards(RelicTier tier)
        {
            // rewards.Add(new RewardItem(ADungeon.returnRandomRelic(tier)));
        }

        /*public void addSapphireKey(RewardItem item)
        {
            rewards.Add(new RewardItem(item, RewardType.SAPPHIRE_KEY));
        }*/

        /*
        public void removeOneRelicFromRewards()
        {
            for (Iterator<RewardItem> i = rewards.iterator(); i.hasNext();)
            {
                RewardItem rewardItem = i.next();
                if (rewardItem.type == RewardType.RELIC)
                {
                    i.remove();
                    if (i.hasNext() && rewardItem.relicLink == i.next())
                        i.remove();
                    break;
                }
            }
        }
        */

        public void addNoncampRelicToRewards(RelicTier tier)
        {
            // rewards.Add(new RewardItem(ADungeon.returnRandomNonCampfireRelic(tier)));
        }

        public void addRelicToRewards(ARelic relic)
        {
            // rewards.Add(new RewardItem(relic));
        }

        // public void addPotionToRewards(AbstractPotion potion)
        // {
        //     rewards.Add(new RewardItem(potion));
        // }

        public void addCardToRewards()
        {
            // RewardItem cardReward = new RewardItem();
            // if (cardReward.cards.Count > 0)
            // rewards.Add(cardReward);
        }

        void addPotionToRewards()
        {
            int chance = onPlayerCompletedGetPotionChance();

            if (player.hasRelic("White Beast Statue"))
                chance = 100;

            // if (rewards.Count >= 4)
            //     chance = 0;

            if (ADungeon.potionRng.random(0, 99) < chance || Settings.isDebug)
            {
                metricData.potions_floor_spawned.Add(ADungeon.floorNum);
                // rewards.Add(new RewardItem(ADungeon.returnRandomPotion()));
                blizzardPotionMod -= BLIZZARD_POTION_MOD_AMT;
            }
            else
            {
                blizzardPotionMod += BLIZZARD_POTION_MOD_AMT;
            }
        }

        public void addGoldToRewards(int gold)
        {
            // foreach (var i in rewards)
            {
                // if (i.type == RewardType.GOLD)
                // {
                //     i.incrementGold(gold);
                //     return;
                // }
            }

            // rewards.Add(new RewardItem(gold));
        }

        public void addStolenGoldToRewards(int gold)
        {
            // foreach (var i in rewards)
            {
                // if (i.type == RewardType.STOLEN_GOLD)
                // {
                //     i.incrementGold(gold);
                //     return;
                // }
            }

            // rewards.Add(new RewardItem(gold, true));
        }

        public bool isCompleted() => phase == RoomPhase.COMPLETE;
        public bool inCombat() => phase == RoomPhase.COMBAT;

        public bool isBattleEnding()
        {
            if (isBattleOver)
                return true;

            if (monsters != null)
                return monsters.areMonstersBasicallyDead;

            return false;
        }

        public void clearEvent()
        {
            if (evt != null)
            {
                // evt.imageEventText.clear();
                // evt.roomEventText.clear();
            }
        }

        /*public void addCardReward(RewardItem rewardItem)
        {
            if (rewardItem.cards.Count > 0)
                rewards.Add(rewardItem);
        }*/

        public virtual void Dispose()
        {
            evt?.Dispose();

            // if (monsters != null)
                // foreach (var m in monsters.monsters)
                    // m.dispose();
        }

        public virtual void getAllBricks(ref List<Brick> list)
        {
        }
        
        public static implicit operator bool(ARoom self) => self != null;
    }
}