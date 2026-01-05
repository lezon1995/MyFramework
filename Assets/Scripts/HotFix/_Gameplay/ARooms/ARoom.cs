using System;
using System.Collections.Generic;

namespace MarbleHero
{
    public enum RoomPhase
    {
        COMBAT,
        EVENT,
        COMPLETE,
        INCOMPLETE
    }

    public enum RoundResult
    {
        None,
        Lose,
        Win,
        Draw,
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

    public record struct OnBattleStart;

    public abstract partial class ARoom : IDisposable
    {
        const float END_TURN_WAIT_DURATION = 1.2F;
        const int BLIZZARD_POTION_MOD_AMT = 10;

        public abstract RoomType Type { get; }

        // public List<AbstractPotion> potions = new();
        public List<ARelic> relics = new();
        // public List<RewardItem> rewards = new();
        // public SoulGroup souls = new();
        public RoomPhase phase = RoomPhase.COMBAT;
        public AEvent evt;
        public MonsterGroup monsters { get; set; }
        Timer endBattleTimer;
        Timer rewardPopOutTimer = 1.0F;
        public Timer waitTimer;
        protected string mapSymbol;

        public bool isFightStarted;
        public bool isFightEnded;
        public RoundResult fightResult;
        public bool isTurnEnd;
        public bool isBattleOver;
        public bool cannotLose;
        public bool eliteTrigger;
        public static int blizzardPotionMod;
        public bool mugged, smoked;
        public bool combatEvent;
        public bool rewardAllowed = true;
        public bool rewardTime;
        public bool skipMonsterTurn { get; set; }
        public int baseRareCardChance = 3;
        public int baseUncommonCardChance = 37;
        public int rareCardChance = 3;
        public int uncommonCardChance = 37;

        public abstract void onPlayerEntry();

        protected virtual void onPlayerTurnStart(int turn)
        {
        }

        protected virtual void onPlayerTurnEnd()
        {
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

        protected virtual void onPlayerFightEnd(RoundResult result)
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
                //
                // if (Gdx.input.isKeyJustPressed(49))
                //     player.increaseMaxOrbSlots(1, true);
                //
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
                        if (actionManager.currentAction || !actionManager.isEmpty())
                            actionManager.update(dt);
                        else
                            finished = waitTimer.update(dt);

                        if (finished)
                            startBattle();
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
                            endTurn();
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

            player.update(dt);
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
                        if (actionManager.currentAction || !actionManager.isEmpty())
                            actionManager.fixedUpdate(dt);
                    }
                    else
                    {
                        if (!ADungeon.isScreenUp)
                            actionManager.fixedUpdate(dt);

                        if (ADungeon.screen != CurrentScreen.HAND_SELECT)
                            player.combatFixedUpdate(dt);
                    }

                    break;
                case RoomPhase.COMPLETE:
                    if (!ADungeon.isScreenUp)
                        actionManager.fixedUpdate(dt);
                    break;
                case RoomPhase.INCOMPLETE:
                    break;
            }

            player.fixedUpdate(dt);
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
                ADungeon.topLevelEffects.Add(CLASS<BattleStartEffect>());

            actionManager.addToBot(new GainEnergyAndEnableControlsAction(1));
            player.applyStartOfCombatPreDrawLogic();
            player.applyStartOfCombatLogic();
            // ADungeon.overlayMenu.showCombatPanels();

            startTurn();
            new OnBattleStart().trigger();
        }

        public void startTurn()
        {
            isTurnEnd = false;
            isFightEnded = false;

            GameActionManager.turn++;

            player.cardsPlayedThisTurn = 0;

            player.applyStartOfTurnRelics();
            player.applyStartOfTurnPowers();

            onPlayerTurnStart(GameActionManager.turn);

            actionManager.useNextCombatActions();

            skipMonsterTurn = false;
            actionManager.turnHasEnded = false;
            actionManager.cardsPlayedThisTurn.Clear();

            GameActionManager.totalDiscardedThisTurn = 0;
            GameActionManager.damageReceivedThisTurn = 0;

            if (!player.hasPower("Barricade") && !player.hasPower("Blur"))
            {
                if (player.hasRelic("Calipers"))
                    player.loseBlock(15);
                else
                    player.loseBlock();
            }

            if (!isBattleOver)
            {
                actionManager.addToBot(new PlayerStartTurnAction());
                // actionManager.addToBot(new DrawCardAction(player, player.gameHandSize));
                player.applyStartOfTurnPostDrawRelics();
                player.applyStartOfTurnPostDrawPowers();
                actionManager.addToBot(new EnableEndTurnButtonAction());
            }
        }

        void endTurn()
        {
            isTurnEnd = true;
            player.applyEndOfTurnTriggers();
            actionManager.addToBot(new ClearCardQueueAction());
            // actionManager.addToBot(new DiscardAtEndOfTurnAction());
            actionManager.addToBot(new EndAction(this));
            player.isEndingTurn = false;

            onPlayerTurnEnd();
        }

        public void startFight()
        {
            actionManager.addToBot(new WaitAction(1F));
            actionManager.addToBot(new FightStartAction());
        }

        public void checkFightingResult()
        {
            int myAlive = 0;
            int opAlive = 0;

            var result = RoundResult.None;
            if (myAlive == 0)
                result = RoundResult.Lose;
            else if (opAlive == 0)
                result = RoundResult.Win;

            if (result == RoundResult.None)
                return;

            endFight(result);
        }

        public void endFight(RoundResult result)
        {
            isFightStarted = false;
            isFightEnded = true;
            onPlayerFightEnd(result);
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
            // ADungeon.overlayMenu.hideCombatPanels();
        }

        public virtual void dropReward()
        {
        }

        public void spawnRelicAndObtain(float x, float y, ARelic relic)
        {
            if (relic.relicId == "Circlet" && player.hasRelic("Circlet"))
            {
                ARelic circ = player.getRelic("Circlet");
                circ.counter++;
                circ.flash();
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

            if (monsters != null)
                foreach (var m in monsters.monsters)
                    m.dispose();
        }

        class EndAction : AGameAction
        {
            ARoom room;
            public EndAction(ARoom r) => room = r;

            public override void update(float dt)
            {
                addToBot(new EndTurnAction());
                addToBot(new WaitAction(END_TURN_WAIT_DURATION));
                if (!room.skipMonsterTurn)
                    addToBot(new MonsterStartTurnAction());
                actionManager.monsterAttacksQueued = false;
                isDone = true;
            }
        }
    }
}