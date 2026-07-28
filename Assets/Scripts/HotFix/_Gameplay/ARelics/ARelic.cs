using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    [Serializable]
    public abstract partial class ARelic : IComparable<ARelic>
    {
        public string name;
        public string relicId;
        public bool energyBased;
        public bool isUsedUp;
        public bool grayscale;
        public string description;
        public string flavorText = "missing";
        public int cost;
        public int counter = -1;
        public RelicTier tier;

        public bool isSeen;
        public float scale = Settings.scale;
        protected bool pulse;

        static float FLASH_ANIM_TIME = 2.0F;
        static float DEFAULT_ANIM_SCALE = 4.0F;
        public bool isDone;
        public bool isAnimating;
        public bool isObtained;
        LandingSound landingSFX;
        static float OBTAIN_SPEED = 6.0F;
        static float OBTAIN_THRESHOLD = 0.5F;
        float rotation;
        public bool discarded;
        string assetURL;

        public APlayer owner { get; set; }

        public enum LandingSound
        {
            CLINK,
            FLAT,
            HEAVY,
            MAGICAL,
            SOLID
        }

        protected ARelic(string setId, string imgName, RelicTier tier, LandingSound sfx)
        {
            relicId = setId;
            relicStrings = languagePack.getRelicStrings(relicId);
            DESCRIPTIONS = relicStrings.DESCRIPTIONS;
            imgUrl = imgName;
            // ImageMaster.loadRelicImg(setId, imgName);
            // img = ImageMaster.getRelicImg(setId);
            // outlineImg = ImageMaster.getRelicOutlineImg(setId);
            name = relicStrings.NAME;
            description = getUpdatedDescription();
            flavorText = relicStrings.FLAVOR;
            this.tier = tier;
            landingSFX = sfx;
            assetURL = "images/relics/" + imgName;
            // tips.Add(new PowerTip(name, description));
            // initializeTips();
        }

        public void usedUp()
        {
            grayscale = true;
            isUsedUp = true;
            // description = MSG[2];
            // tips.Clear();
            // tips.Add(new PowerTip(name, description));
            // initializeTips();
        }

        public void spawn(float x, float y)
        {
            // if (room is not ShopRoom)
            // ADungeon.effectsQueue.Add(new SmokePuffEffect(x, y));
            isAnimating = true;
            isObtained = false;
            if (tier == RelicTier.BOSS)
            {
                glowTimer = 0.0F;
            }
        }

        public int getPrice()
        {
            return tier switch
            {
                RelicTier.DEPRECATED => 300,
                RelicTier.STARTER => 150,
                RelicTier.COMMON => 250,
                RelicTier.UNCOMMON => 300,
                RelicTier.RARE => 150,
                RelicTier.SPECIAL => 400,
                RelicTier.BOSS => 999,
                RelicTier.SHOP => -1,
                _ => -1
            };
        }

        public void reorganizeObtain(APlayer p, int slot, bool callOnEquip, int relicAmount)
        {
            isDone = true;
            isObtained = true;
            p.relics.Add(this);
            if (callOnEquip)
            {
                onEquip(p);
                relicTip();
            }

            UnlockTracker.markRelicAsSeen(relicId);
        }

        public void instantObtain(APlayer player, int slot, bool callOnEquip)
        {
            if (relicId == "Circlet" && player.tryGetRelic("Circlet", out var relic))
            {
                relic.counter++;
                relic.flash();
                isDone = true;
                isObtained = true;
                discarded = true;
            }
            else
            {
                isDone = true;
                isObtained = true;
                if (slot >= player.relics.Count)
                    player.addRelic(this);
                else
                    player.setRelic(slot, this);

                if (callOnEquip)
                {
                    onEquip(player);
                    relicTip();
                }

                UnlockTracker.markRelicAsSeen(relicId);
                getUpdatedDescription();
                ADungeon.overlayMenu?.relics?.refresh(player.relics);
            }
        }

        public void instantObtain()
        {
            if (relicId == "Circlet" && player.tryGetRelic("Circlet", out var relic))
            {
                relic.counter++;
                relic.flash();
            }
            else
            {
                playLandingSFX();
                isDone = true;
                isObtained = true;
                flash();
                player.addRelic(this);
                // hb.move(currentX, currentY);
                onEquip(player);
                relicTip();
                UnlockTracker.markRelicAsSeen(relicId);
            }

            // if (ADungeon.topPanel != null)
            // ADungeon.topPanel.adjustRelicHbs();
        }

        public void obtain()
        {
            if (relicId == "Circlet" && player.hasRelic("Circlet"))
            {
                ARelic circ = player.getRelic("Circlet");
                circ.counter++;
                circ.flash();
            }
            else
            {
                player.addRelic(this);
                relicTip();
                UnlockTracker.markRelicAsSeen(relicId);
            }
        }

        public int getColumn() => player.relics.IndexOf(this);

        public void relicTip()
        {
            if (TipTracker.relicCounter < 20)
            {
                TipTracker.relicCounter++;
                if (TipTracker.relicCounter >= 1 && !TipTracker.tips["RELIC_TIP"])
                {
                    // ADungeon.ftue = new FtueTip(LABEL[0], MSG[0], 360.0F * Settings.scale, 760.0F * Settings.scale, FtueTip.TipType.RELIC);
                    TipTracker.neverShowAgain("RELIC_TIP");
                }
            }
        }

        public void setCounter(int counter)
        {
            this.counter = counter;
        }

        public void bossObtainLogic()
        {
            if (relicId != ("HolyWater") && relicId != ("Black Blood") && relicId != ("Ring of the Serpent") && relicId != ("FrozenCore"))
                obtain();
            isObtained = true;
        }

        public void onPlayCard(ACard c)
        {
        }

        public void onPreviewObtainCard(ACard c)
        {
        }

        public void onObtainCard(ACard c)
        {
        }

        public void onGainGold()
        {
        }

        public void onLoseGold()
        {
        }

        public void onSpendGold()
        {
        }

        public virtual void onEquip(APlayer p)
        {
        }

        public virtual void onUnequip(APlayer p)
        {
        }

        public void atPreBattle()
        {
        }

        public void atBattleStart()
        {
        }

        public void onSpawnMonster(AMonster monster)
        {
        }

        public void atBattleStartPreDraw()
        {
        }

        public void atTurnStart()
        {
        }

        public void atTurnStartPostDraw()
        {
        }

        public void onPlayerEndTurn()
        {
        }

        public virtual void onShootBall(Ball ball)
        {
        }

        public void onBloodied()
        {
        }

        public void onNotBloodied()
        {
        }

        public void onManualDiscard()
        {
        }

        public void onVictory()
        {
        }

        public void onMonsterDeath(AMonster m)
        {
        }

        public void onBlockBroken(ACreature m)
        {
        }

        public int onPlayerGainBlock(int blockAmount) => blockAmount;

        public int onPlayerGainedBlock(float blockAmount) => MathUtils.floor(blockAmount);

        public int onPlayerHeal(int healAmount) => healAmount;

        public void onMeditate()
        {
        }

        public void onEnergyRecharge()
        {
        }

        public void beforeEnergyPrep()
        {
        }

        public void onRest()
        {
        }

        public void onRitual()
        {
        }

        public void onEnterRestRoom()
        {
        }

        public void onRefreshHand()
        {
        }

        public void onShuffle()
        {
        }

        public void onSmith()
        {
        }

        public void onAttack(DamageInfo info, int damageAmount, ACreature target)
        {
        }

        public int onAttacked(DamageInfo info, int damageAmount) => damageAmount;

        public int onAttackedToChangeDamage(DamageInfo info, int damageAmount) => damageAmount;

        public int onAttackToChangeDamage(DamageInfo info, int damageAmount) => damageAmount;

        public void onExhaust(ACard card)
        {
        }

        public void onTrigger()
        {
        }

        public void onTrigger(ACreature target)
        {
        }

        public bool checkTrigger() => false;

        public void onEnterRoom(ARoom room)
        {
        }

        public void justEnteredRoom(ARoom room)
        {
        }

        public void onCardDraw(ACard card)
        {
        }

        public void onChestOpen(bool bossChest)
        {
        }

        public void onChestOpenAfter(bool bossChest)
        {
        }

        public void onDrawOrDiscard()
        {
        }

        public void onMasterDeckChange()
        {
        }

        public float atDamageModify(float damage, ACard c) => damage;

        public int changeNumberOfCardsInReward(int numberOfCards) => numberOfCards;

        public int changeRareCardRewardChance(int rareCardChance) => rareCardChance;

        public int changeUncommonCardRewardChance(int uncommonCardChance) => uncommonCardChance;

        public bool canPlay(ACard card) => true;

        public static string gameDataUploadHeader()
        {
            GameDataStringBuilder builder = new GameDataStringBuilder();
            builder.addFieldData("name");
            builder.addFieldData("relicID");
            builder.addFieldData("color");
            builder.addFieldData("description");
            builder.addFieldData("flavorText");
            builder.addFieldData("cost");
            builder.addFieldData("tier");
            builder.addFieldData("assetURL");
            return builder.toString();
        }

        public string gameDataUploadData(string color)
        {
            GameDataStringBuilder builder = new GameDataStringBuilder();
            builder.addFieldData(name);
            builder.addFieldData(relicId);
            builder.addFieldData(color);
            builder.addFieldData(description);
            builder.addFieldData(flavorText);
            builder.addFieldData(cost);
            builder.addFieldData(tier.ToString());
            builder.addFieldData(assetURL);
            return builder.toString();
        }

        public override string ToString() => name;

        public int CompareTo(ARelic arg0) => string.Compare(name, arg0.name, StringComparison.Ordinal);

        public string getAssetURL() => assetURL;

        public Dictionary<string, object> getLocStrings()
        {
            Dictionary<string, object> relicData = new()
            {
                { "name", name },
                { "description", description }
            };
            return relicData;
        }

        public bool canSpawn() => true;

        public void onUsePotion()
        {
        }

        // public void onChangeStance(AbstractStance prevStance, AbstractStance newStance)
        // {
        // }

        public void onLoseHp(int damageAmount)
        {
        }

        public int onLoseHpLast(int damageAmount) => damageAmount;

        public void wasHPLost(int damageAmount)
        {
        }

        public virtual void onPlayerTurnUpdate(APlayer p, float dt)
        {
        }

        public virtual void onBallBeginOverlappingBrickAll(APlayer p, Ball ball, Brick brick)
        {
        }

        public virtual void onBallEndOverlappingBrickAll(APlayer p, Ball ball, Brick brick, bool prematurely)
        {
        }
        public virtual void onBallBeginOverlappingBrickOne(APlayer p, Ball ball, Brick brick)
        {
        }

        public virtual void onBallEndOverlappingBrickOne(APlayer p, Ball ball, Brick brick, bool prematurely)
        {
        }

        public virtual void onPlayerTurnBegin(APlayer p)
        {
        }

        public virtual void onPlayerTurnEnd(APlayer p)
        {
        }

        public virtual void onFightingPhaseEnd(APlayer p)
        {
        }

        public abstract ARelic makeCopy();

        public virtual void onBallHitBorderBot(APlayer p, Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn)
        {
        }

        public virtual void onBallHitBorderTop(APlayer p, Ball ball, BorderTop border, ref Vector2 normal)
        {
        }
        public virtual void onBallHitBorderLeft(APlayer p, Ball ball, BorderLeft border, ref Vector2 normal)
        {
        }
        public virtual void onBallHitBorderRight(APlayer p, Ball ball, BorderRight border, ref Vector2 normal)
        {
        }

        public virtual void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
        {
        }

        public virtual void onBallHitObstacle(APlayer p, Ball ball, Obstacle obstacle, ref Vector2 normal)
        {
        }

        public virtual void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
        {
        }

        public virtual void onBallKillBrick(APlayer p, Ball ball, Brick brick)
        {
        }
    }
}