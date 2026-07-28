using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    public abstract partial class APlayer : ACreature
    {
        public override bool isPlayer => true;
        public abstract PlayerClass chosenClass { get; }

        public override int gold
        {
            get => wallet.Balance;
            set => wallet.SetBalance(value);
        }

        public bool isEndingTurn { get; set; }
        public bool viewingRelics;
        public int damagedThisCombat;
        public string title;
        public int cardsPlayedThisTurn;
        bool isHoveringCard;
        public ACard cardInUse { get; set; }
        public bool endTurnQueued { get; set; }

        public static List<string> customMods;
        public new List<PlayerPower> powers = new();
        public PlayerRenderer playerRenderer;
        public PlayerRecollectBallAbility playerRecollectBall;
        public new PlayerStats Stats => stats as PlayerStats;

        protected override void OnAwake()
        {
            base.OnAwake();

            TryGetComponent(out playerRenderer);
            playerRenderer.Awake();

            name = Game.playerName;
            title = getTitle(chosenClass);
            block = new PlayerBlock(this);
            initializeStarterRelics(chosenClass);
            loadPrefs();

            // if (ADungeon.ascensionLevel >= 11)
            // potionSlots--;

            // potions.Clear();
            // int i;
            // for (i = 0; i < potionSlots; i++)
            // potions.Add(new PotionSlot(i));
        }

        public override void onAcquire()
        {
            base.onAcquire();
            _controller2D.RegisterToVolumeManager();
        }

        public override void onRelease()
        {
            _controller2D.UnregisterToVolumeManager();
            ;
            base.onRelease();
        }

        public override void SetInputManager()
        {
            Input = InputManager.Instance;
            UpdateInputManagersInAbilities();
        }

        public abstract string getPortraitImageName();

        public virtual List<string> getStartingDeck()
        {
            return new List<string>();
        }

        public virtual List<string> getStartingRelics()
        {
            return new List<string>();
        }

        public abstract string getTitle(PlayerClass paramPlayerClass);
        public abstract string getAchievementKey();
        public abstract List<ACard> getCardPool(List<ACard> paramArrayList);

        public abstract ACard getStartCardForEvent();
        public abstract string getLeaderboardCharacterName();
        public abstract int getAscensionMaxHPLoss();
        public abstract Prefs getPrefs();
        public abstract void loadPrefs();
        public abstract int getUnlockedCardCount();
        public abstract int getSeenCardCount();
        public abstract int getCardCount();
        public abstract string getWinStreakKey();
        public abstract string getLeaderboardWinStreakKey();
        public abstract void doCharSelectScreenSelectEffect();
        public abstract string getCustomModeCharacterButtonSoundKey();

        // public abstract Texture getCustomModeCharacterButtonImage();

        // public abstract CharacterStrings getCharacterString();
        public abstract string getLocalizedCharacterName();
        public abstract void refreshCharStat();

        public string getSaveFilePath() => SaveAndContinue.getPlayerSavePath(chosenClass.ToString());

        public void initializeStarterDeck()
        {
            List<string> cards = new();
            bool addBaseCards = !ModHelper.isModEnabled("Draft") && !ModHelper.isModEnabled("Chimera") && !ModHelper.isModEnabled("SealedDeck") && !ModHelper.isModEnabled("Shiny") && !ModHelper.isModEnabled("Insanity");

            if (ModHelper.isModEnabled("Chimera"))
            {
                //masterDeck.addToTop(new Bash());
                //masterDeck.addToTop(new Survivor());
                //masterDeck.addToTop(new Zap());
                //masterDeck.addToTop(new Eruption());
                //masterDeck.addToTop(new Strike_Red());
                //masterDeck.addToTop(new Strike_Green());
                //masterDeck.addToTop(new Strike_Blue());
                //masterDeck.addToTop(new Defend_Red());
                //masterDeck.addToTop(new Defend_Green());
                //masterDeck.addToTop(new Defend_Watcher());
            }

            if (ModHelper.isModEnabled("Insanity"))
            {
                // for (int i = 0; i < 50; i++)
                // masterDeck.addToTop(ADungeon.returnRandomCard().makeCopy());
            }

            if (ModHelper.isModEnabled("Shiny"))
            {
                CardGroup group = ADungeon.getEachRare();
                // foreach (var c in group.group)
                //     masterDeck.addToTop(c);
            }

            if (addBaseCards)
            {
                foreach (string cardId in cards)
                {
                    // if (CardLibrary.getCard(chosenClass, cardId, out var card))
                    //     masterDeck.addToTop(card.makeCopy());
                }
            }

            // foreach (var c in masterDeck.group)
            //     UnlockTracker.markCardAsSeen(c.cardID);
        }

        protected void initializeStarterRelics(PlayerClass chosenClass)
        {
            var relics = getStartingRelics();
            if (ModHelper.isModEnabled("Cursed Run"))
            {
                relics.Clear();
                relics.Add("Cursed Key");
                relics.Add("Darkstone Periapt");
                relics.Add("Du-Vu Doll");
            }

            if (ModHelper.isModEnabled("ControlledChaos"))
                relics.Add("Frozen Eye");

            int index = 0;
            foreach (string s in relics)
            {
                var relic = RelicLibrary.getRelic(s);
                if (relic == null)
                    continue;

                relic.makeCopy().instantObtain(this, index, true);
                index++;
            }

            ADungeon.relicsToRemoveOnStart.AddRange(relics);
        }


        public void combatUpdate(float dt)
        {
            foreach (var power in powers)
                power.updateParticles();
        }

        public void combatFixedUpdate(float dt)
        {
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            updatePowers(dt);
        }

        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);
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

        public override void loseGold(int amount, PayType type = PayType.DEFAULT)
        {
            if (room is ShopRoom)
            {
                foreach (var relic in relics)
                    relic.onSpendGold();
            }

            if (room is not ShopRoom && (room).phase != RoomPhase.COMBAT)
                sound.play("EVENT_PURCHASE");

            if (amount > 0)
            {
                wallet.Pay(amount, type);

                foreach (var relic in relics)
                    relic.onLoseGold();
            }
            else
            {
                log("NEGATIVE MONEY???");
            }
        }

        public override void gainGold(int amount, EarnType type = EarnType.DEFAULT)
        {
            if (tryGetRelic("Ectoplasm", out var ectoplasm))
            {
                ectoplasm.flash();
                return;
            }

            if (amount > 0)
            {
                Game.goldGained += amount;
                wallet.Earn(amount, type);

                foreach (var relic in relics)
                    relic.onGainGold();
            }
            else
            {
                log("NEGATIVE MONEY???");
            }
        }

        static bool isCardQueued(ACard card) => actionManager.containsInCardQueue(card);

        void playCard()
        {
            // InputHelper.justClickedLeft = false;
            // isUsingClickDragControl = false;
        }

        public void playCard(ACard card, CardQueueItem item)
        {
            if (!actionManager.containsInCardQueue(card))
            {
                actionManager.addCardQueueItem(item);
            }
        }

        public void useCard(ACard c)
        {
            c.use();
            cardInUse = c;
        }

        public override void damage(DamageInfo info)
        {
            int dmg = info.output;
            bool hadBlock = block.currentBlock > 0;
            if (dmg < 0)
                dmg = 0;

            if (dmg > 1 && hasPower("IntangiblePlayer"))
                dmg = 1;

            block.decrementBlock(ref dmg);

            if (info.owner == this)
                foreach (var relic in relics)
                    dmg = relic.onAttackToChangeDamage(info, dmg);

            if (info.owner != null)
                foreach (var power in info.owner.powers)
                    dmg = power.onAttackToChangeDamage(info, dmg);

            foreach (var relic in relics)
                dmg = relic.onAttackedToChangeDamage(info, dmg);

            foreach (var power in powers)
                dmg = power.onAttackedToChangeDamage(info, dmg);

            if (info.owner == this)
                foreach (var relic in relics)
                    relic.onAttack(info, dmg, this);

            if (info.owner != null)
            {
                foreach (var power in info.owner.powers)
                    power.onAttack(info, dmg, this);

                foreach (var power in powers)
                    dmg = power.onAttacked(info, dmg);

                foreach (var relic in relics)
                    dmg = relic.onAttacked(info, dmg);
            }
            else
            {
                log("NO OWNER, DON'T TRIGGER POWERS");
            }

            foreach (var relic in relics)
                dmg = relic.onLoseHpLast(dmg);

            lastDamageTaken = Math.Min(dmg, currentHealth);
            if (dmg > 0)
            {
                foreach (var power in powers)
                    dmg = power.onLoseHp(dmg);

                foreach (var relic in relics)
                    relic.onLoseHp(dmg);

                foreach (var power in powers)
                    power.wasHPLost(info, dmg);

                foreach (var relic in relics)
                    relic.wasHPLost(dmg);

                if (info.owner != null)
                    foreach (var power in info.owner.powers)
                        power.onInflictDamage(info, dmg, this);

                if (info.type == DamageInfo.DamageType.HP_LOSS)
                    GameActionManager.hpLossThisCombat += dmg;

                GameActionManager.damageReceivedThisTurn += dmg;
                GameActionManager.damageReceivedThisCombat += dmg;
                currentHealth = clamp(currentHealth - dmg, 0, maxHealth);

                if (dmg > 0 && room.inCombat())
                {
                    // updateCardsOnDamage();
                    damagedThisCombat++;
                }

                //ADungeon.effectList.Add(new StrikeEffect(this, hb.cX, hb.cY, damageAmount));
                if (currentHealth < maxHealth / 4)
                {
                    // ADungeon.topLevelEffects.Add(new BorderFlashEffect(new Color(1.0F, 0.1F, 0.05F, 0.0F)));
                }

                if (currentHealth <= maxHealth / 2.0F && !isBloodied)
                {
                    isBloodied = true;
                    foreach (var relic in relics)
                        relic?.onBloodied();
                }

                if (currentHealth < 1)
                {
                    if (!hasRelic("Mark of the Bloom"))
                    {
                        /*if (hasPotion("FairyPotion"))
                        {
                            foreach (var p in potions)
                            {
                                if (p.ID == ("FairyPotion"))
                                {
                                    p.flash();
                                    currentHealth = 0;
                                    p.use(this);
                                    ADungeon.topPanel.destroyPotion(p.slot);
                                    return;
                                }
                            }
                        }
                        else */
                        if (tryGetRelic("Lizard Tail", out var relic))
                        {
                            if (relic.counter == -1)
                            {
                                currentHealth = 0;
                                relic.onTrigger();
                                return;
                            }
                        }
                    }

                    isDead = true;
                    // ADungeon.deathScreen = new DeathScreen(monsters);
                    currentHealth = 0;
                    if (block.currentBlock > 0)
                    {
                        block.loseBlock();
                        //ADungeon.effectList.Add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
                    }
                }
            }
            else if (block.currentBlock > 0)
            {
                //ADungeon.effectList.Add(new BlockedWordEffect(this, hb.cX, hb.cY, uiStrings.TEXT[0]));
            }
            else if (hadBlock)
            {
                //ADungeon.effectList.Add(new BlockedWordEffect(this, hb.cX, hb.cY, uiStrings.TEXT[0]));
                //ADungeon.effectList.Add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
            }
            else
            {
                //ADungeon.effectList.Add(new StrikeEffect(this, hb.cX, hb.cY, 0));
            }
        }

        public override bool isDeadOrEscaped()
        {
            return isDying || halfDead;
        }

        public void preBattlePrep()
        {
            actionManager.clear();
            damagedThisCombat = 0;
            cardsPlayedThisTurn = 0;
            isBloodied = currentHealth <= maxHealth / 2;
            GameActionManager.playerHpLastTurn = currentHealth;
            endTurnQueued = false;
            cardInUse = null;

            //初始化抽牌堆
            // drawPile.initializeDeck(masterDeck);

            powers.Clear();
            isEndingTurn = false;
            // if (ModHelper.isModEnabled("Lethality"))
            // actionManager.addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 3), 3));

            // if (ModHelper.isModEnabled("Terminal"))
            // actionManager.addToBot(new ApplyPowerAction(this, this, new PlatedArmorPower(this, 5), 5));

            room.monsters?.usePreBattleAction();

            actionManager.addToTop<WaitAction>().with(0.2F);
            applyPreCombatLogic();
        }

        public int getCircletCount()
        {
            int count = 0;
            int counterSum = 0;
            foreach (var relic in relics)
            {
                if (relic.relicId == "Circlet")
                {
                    count++;
                    counterSum += relic.counter;
                }
            }

            if (counterSum > 0)
                return counterSum;
            return count;
        }

        public void applyPreCombatLogic()
        {
            foreach (var relic in relics)
                relic.atPreBattle();
        }

        public void applyStartOfCombatLogic()
        {
            foreach (var relic in relics)
                relic.atBattleStart();
        }

        public void applyStartOfCombatPreDrawLogic()
        {
            foreach (var relic in relics)
                relic.atBattleStartPreDraw();
        }

        public void onVictory()
        {
            if (!isDying)
            {
                foreach (var relic in relics)
                    relic.onVictory();

                foreach (var power in powers)
                    power.onVictory();
            }

            damagedThisCombat = 0;
        }

        public enum PlayerClass
        {
            IRONCLAD,
            THE_SILENT,
            DEFECT,
            WATCHER
        }

        public void resetControllerValues()
        {
        }

        public override void applyEndOfTurnTriggers()
        {
            foreach (var power in powers)
                power.atEndOfTurn(true);
        }

        public void onPlayerTurnUpdate(float dt)
        {
            foreach (var relic in relics)
                relic.onPlayerTurnUpdate(this, dt);
        }

        public void onPlayerTurnBegin()
        {
            foreach (var relic in relics)
                relic.onPlayerTurnBegin(this);
        }

        public void onPlayerTurnEnd()
        {
            foreach (var relic in relics)
                relic.onPlayerTurnEnd(this);
        }

        public void onFightingPhaseEnd()
        {
            foreach (var relic in relics)
                relic.onFightingPhaseEnd(this);
        }

        public void onBallBeginOverlappingBrickAll(Ball ball, Brick brick)
        {
            foreach (var relic in relics)
                relic.onBallBeginOverlappingBrickAll(this, ball, brick);

            //log($"重叠All开始 start with {brick.getName()}");
        }

        public void onBallEndOverlappingBrickAll(Ball ball, Brick brick, bool prematurely)
        {
            foreach (var relic in relics)
                relic.onBallEndOverlappingBrickAll(this, ball, brick, prematurely);

            //log($"重叠All结束 end with {brick.getName()}");
        }

        public void onBallBeginOverlappingBrickOne(Ball ball, Brick brick)
        {
            foreach (var relic in relics)
                relic.onBallBeginOverlappingBrickOne(this, ball, brick);

            //log($"重叠One开始 start with {brick.getName()}");
        }

        public void onBallEndOverlappingBrickOne(Ball ball, Brick brick, bool prematurely)
        {
            ball.counters.penetrateBrick.count();

            foreach (var relic in relics)
                relic.onBallEndOverlappingBrickOne(this, ball, brick, prematurely);

            //log($"重叠One结束 end with {brick.getName()}");
        }

        public void onBallHitBorderBot(Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn)
        {
            foreach (var relic in relics)
                relic.onBallHitBorderBot(this, ball, border, normal, ref forceReturn);
        }

        public void onBallHitBorderTop(Ball ball, BorderTop border, ref Vector2 normal)
        {
            foreach (var relic in relics)
                relic.onBallHitBorderTop(this, ball, border, ref normal);
        }

        public void onBallHitBorderLeft(Ball ball, BorderLeft border, ref Vector2 normal)
        {
            foreach (var relic in relics)
                relic.onBallHitBorderLeft(this, ball, border, ref normal);
        }

        public void onBallHitBorderRight(Ball ball, BorderRight border, ref Vector2 normal)
        {
            foreach (var relic in relics)
                relic.onBallHitBorderRight(this, ball, border, ref normal);
        }

        public void onBallHitBrick(Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
        {
            foreach (var relic in relics)
                relic.onBallHitBrick(this, ball, brick, normal, ref triggerRegularHit);
        }

        public void onBallHitObstacle(Ball ball, Obstacle obstacle, ref Vector2 normal)
        {
            foreach (var relic in relics)
                relic.onBallHitObstacle(this, ball, obstacle, ref normal);
        }

        public void onBallReflect(Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
        {
            foreach (var relic in relics)
                relic.onBallReflect(this, ball, normal, fromBrick, ref reflectDir);
        }

        public void onBallKillBrick(Ball ball, Brick brick)
        {
            foreach (var relic in relics)
                relic.onBallKillBrick(this, ball, brick);
        }

        public bool equalWith(APlayer other)
        {
            return this ==  other;
        }
    }
}