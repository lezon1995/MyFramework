using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero
{
    public record struct OnMyPlayerHealthChanged;
    public abstract partial class APlayer : ACreature
    {
        public PlayerClass chosenClass;
        public int startingMaxHP;

        public bool isEndingTurn { get; set; }
        public bool viewingRelics;

        public bool inspectMode;

        public static int poisonKillCount;
        public int damagedThisCombat;
        public string title;

        public int cardsPlayedThisTurn;
        bool isHoveringCard;
        public bool isHoveringDropZone;
        float hoverStartLine;
        bool passedHesitationLine;
        public ACard cardInUse { get; set; }
        public bool endTurnQueued { get; set; }
        
        public override int currentHealth
        {
            get => _health;
            set
            {
                if (_health == value)
                    return;
                _health = value;
                new OnMyPlayerHealthChanged().Trigger();
            }
        }

        public override Team team => Team.Player;
        public static List<string> customMods;

        protected APlayer(string playerName)
        {
            player = this;
            name = playerName;
            var setClass = PlayerClass.IRONCLAD;
            title = getTitle(setClass);
            chosenClass = setClass;
            isPlayer = true;
            initializeStarterRelics(setClass);
            loadPrefs();

            // if (ADungeon.ascensionLevel >= 11)
            // potionSlots--;

            // potions.Clear();
            // int i;
            // for (i = 0; i < potionSlots; i++)
            // potions.Add(new PotionSlot(i));
        }

        public abstract string getPortraitImageName();

        public virtual List<string> getStartingDeck()
        {
            return null;
        }

        public List<string> getStartingRelics()
        {
            return null;
        }

        public abstract CharSelectInfo getLoadout();
        public abstract string getTitle(PlayerClass paramPlayerClass);
        public abstract string getAchievementKey();
        public abstract List<ACard> getCardPool(List<ACard> paramArrayList);

        public abstract ACard getStartCardForEvent();

        public abstract Color getCardTrailColor();
        public abstract string getLeaderboardCharacterName();

        // public abstract Texture getEnergyImage();
        public abstract int getAscensionMaxHPLoss();
        public abstract Prefs getPrefs();
        public abstract void loadPrefs();
        public abstract CharStat getCharStat();
        public abstract int getUnlockedCardCount();
        public abstract int getSeenCardCount();
        public abstract int getCardCount();
        public abstract bool saveFileExists();
        public abstract string getWinStreakKey();
        public abstract string getLeaderboardWinStreakKey();
        public abstract void doCharSelectScreenSelectEffect();
        public abstract string getCustomModeCharacterButtonSoundKey();

        // public abstract Texture getCustomModeCharacterButtonImage();

        // public abstract CharacterStrings getCharacterString();
        public abstract string getLocalizedCharacterName();
        public abstract void refreshCharStat();

        public abstract APlayer newInstance();

        // public abstract TextureAtlas.AtlasRegion getOrb();
        public abstract string getSpireHeartText();

        // public abstract Color getSlashAttackColor();
        public abstract AttackEffect[] getSpireHeartSlashEffect();
        public abstract string getVampireText();

        public string getSaveFilePath() => SaveAndContinue.getPlayerSavePath(chosenClass.ToString());

        public void dispose()
        {
        }

        protected void initializeClass(string imgUrl, string shoulder2ImgUrl, string shouldImgUrl, string corpseImgUrl, CharSelectInfo info, EnergyManager energy)
        {
            // if (imgUrl != null)
            //     img = ImageMaster.loadImage(imgUrl);
            // if (img != null)
            //     atlas = null;
            // shoulderImg = ImageMaster.loadImage(shouldImgUrl);
            // shoulder2Img = ImageMaster.loadImage(shoulder2ImgUrl);
            // corpseImg = ImageMaster.loadImage(corpseImgUrl);

            // if (Settings.isMobile)
                // hb_w *= 1.17F;
            _healthMax = info.maxHp;
            startingMaxHP = maxHealth;
            _health = info.currentHp;
            // masterMaxOrbs = info.maxOrbs;
            gold = info.gold;
            displayGold = gold;
        }

        public void initializeStarterDeck()
        {
            List<string> cards = Data.StartingDeck;
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
                for (int i = 0; i < 50; i++)
                    masterDeck.addToTop(ADungeon.returnRandomCard().makeCopy());
            }

            if (ModHelper.isModEnabled("Shiny"))
            {
                CardGroup group = ADungeon.getEachRare();
                foreach (var c in group.group)
                    masterDeck.addToTop(c);
            }

            if (addBaseCards)
            {
                foreach (string cardId in cards)
                {
                    if (CardLibrary.getCard(chosenClass, cardId, out var card))
                        masterDeck.addToTop(card.makeCopy());
                }
            }

            foreach (var c in masterDeck.group)
                UnlockTracker.markCardAsSeen(c.cardID);
        }

        protected void initializeStarterRelics(PlayerClass chosenClass)
        {
            List<string> relics = getStartingRelics();
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

                relic.makeCopy().instantObtain(index, false);
                index++;
            }

            ADungeon.relicsToRemoveOnStart.AddRange(relics);
        }


        public void combatUpdate(float dt)
        {
            cardInUse?.update(dt);

            foreach (var power in powers)
                power.updateParticles();
        }

        public void combatFixedUpdate(float dt)
        {
        }

        public void update(float dt)
        {
            updatePowers(dt);
        }

        public void fixedUpdate(float dt)
        {
        }


        public override void loseGold(int amount)
        {
            if (room is ShopRoom)
            {
                foreach (var relic in relics)
                    relic.onSpendGold();
            }

            // if (room is not ShopRoom && (room).phase != RoomPhase.COMBAT)
            // Game.sound.play("EVENT_PURCHASE");

            if (amount > 0)
            {
                gold -= amount;
                if (gold < 0)
                    gold = 0;

                foreach (var relic in relics)
                    relic.onLoseGold();
            }
            else
            {
                log("NEGATIVE MONEY???");
            }
        }

        public override void gainGold(int amount)
        {
            if (tryGetRelic("Ectoplasm", out var ectoplasm))
            {
                ectoplasm.flash();
                return;
            }

            if (amount <= 0)
            {
                log("NEGATIVE MONEY???");
            }
            else
            {
                Game.goldGained += amount;
                gold += amount;

                foreach (var relic in relics)
                    relic.onGainGold();
            }
        }

        static bool isCardQueued(ACard card) => actionManager.containsInCardQueue(card);

        void playCard()
        {
            InputHelper.justClickedLeft = false;
            isUsingClickDragControl = false;
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
            bool hadBlock = currentBlock > 0;
            if (dmg < 0)
                dmg = 0;

            if (dmg > 1 && hasPower("IntangiblePlayer"))
                dmg = 1;

            dmg = decrementBlock(info, dmg);

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
                    updateCardsOnDamage();
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
                        if (hasPotion("FairyPotion"))
                        {
                            // foreach (var p in potions)
                            // {
                            //     if (p.ID == ("FairyPotion"))
                            //     {
                            //         p.flash();
                            //         currentHealth = 0;
                            //         p.use(this);
                            //         ADungeon.topPanel.destroyPotion(p.slot);
                            //         return;
                            //     }
                            // }
                        }
                        else if (tryGetRelic("Lizard Tail", out var relic))
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
                    if (currentBlock > 0)
                    {
                        loseBlock();
                        //ADungeon.effectList.Add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
                    }
                }
            }
            else if (currentBlock > 0)
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

        public override bool isDeadOrEscaped() => isDying || halfDead;

        public override void heal(ref int healAmount, bool showEffect)
        {
            if (Settings.isEndless && player.hasBlight("FullBelly"))
            {
                healAmount /= 2;
                if (healAmount < 1)
                    healAmount = 1;
            }

            base.heal(ref healAmount, showEffect);
        }

        public override void heal(ref int healAmount)
        {
            base.heal(ref healAmount);
            if (currentHealth > maxHealth / 2.0F && isBloodied)
            {
                isBloodied = false;
                foreach (var relic in relics)
                    relic.onNotBloodied();
            }
        }

        public void preBattlePrep()
        {
            actionManager.clear();
            damagedThisCombat = 0;
            cardsPlayedThisTurn = 0;
            isBloodied = currentHealth <= maxHealth / 2;
            poisonKillCount = 0;
            GameActionManager.playerHpLastTurn = currentHealth;
            endTurnQueued = false;
            isHoveringDropZone = false;
            cardInUse = null;

            //初始化抽牌堆
            drawPile.initializeDeck(masterDeck);

            powers.Clear();
            isEndingTurn = false;
            // if (ModHelper.isModEnabled("Lethality"))
            // actionManager.addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 3), 3));

            // if (ModHelper.isModEnabled("Terminal"))
            // actionManager.addToBot(new ApplyPowerAction(this, this, new PlatedArmorPower(this, 5), 5));

            room.monsters?.usePreBattleAction();

            actionManager.addToTop(new WaitAction(1.0F));
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
    }
}