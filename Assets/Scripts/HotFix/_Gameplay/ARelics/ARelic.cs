using System;
using System.Collections.Generic;
using UniStats;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    [Serializable]
    public abstract partial class ARelic : IComparable<ARelic>
    {
        public string name;
        public string relicId;
        public string description;
        public string flavorText;
        public int cost;
        public int counter = -1;
        public RelicTier tier;
        LandingSound landingSFX;
        float rotation;
        string assetURL;

        public APlayer _player { get; set; }
        public RelicDef def { get; set; }

        public void setDef(RelicDef d) => def = d;
        Dictionary<Character.Stat, string> modKeys;

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
            name = relicStrings.NAME;
            flavorText = relicStrings.FLAVOR;
            this.tier = tier;
            landingSFX = sfx;
            assetURL = "images/relics/" + imgName;
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
            _player = p;
            foreach (var mod in def.PlayerStatMods.safe())
            {
                if (p.GetStat(mod.stat, out var stat))
                {
                    string modKey = null;
                    if (!mod.BonusFlat.isZero())
                    {
                        modKey = stat.BonusFlat.AddFlat(mod.BonusFlat, name: Guid.NewGuid().ToString());
                    }
                    else if (!mod.BonusPct.isZero())
                    {
                        modKey = stat.BonusPct.AddFlat(mod.BonusPct, name: Guid.NewGuid().ToString());
                    }

                    modKeys ??= DictionaryPool<Character.Stat, string>.Get();
                    modKeys[mod.stat] = modKey;
                }
            }
        }

        public virtual void onUnequip(APlayer p)
        {
            foreach (var mod in def.PlayerStatMods.safe())
            {
                if (p.GetStat(mod.stat, out var stat))
                {
                    if (modKeys == null)
                        break;

                    if (modKeys.TryGetValue(mod.stat, out var modKey))
                    {
                        if (!mod.BonusFlat.isZero())
                        {
                            stat.BonusFlat.RemoveMod(modKey);
                        }
                        else if (!mod.BonusPct.isZero())
                        {
                            stat.BonusPct.RemoveMod(modKey);
                        }
                    }
                }
            }

            if (modKeys != null)
            {
                DictionaryPool<Character.Stat, string>.Release(modKeys);
            }

            if (_player == p)
                _player = null;
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

        public virtual void onEnterBloodied()
        {
        }

        public virtual void onExitBloodied()
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

        public void onPlayerGainBlock(ref int blockAmount)
        {
        }

        public void onPlayerHeal(ref int healAmount)
        {
        }

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

        public void onAttacked(DamageInfo info, ref int damageAmount)
        {
        }

        public void onAttackedToChangeDamage(DamageInfo info, ref int damageAmount)
        {
        }

        public void onAttackToChangeDamage(DamageInfo info, ref int damageAmount)
        {
        }

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

        public void onLoseHpLast(ref int damageAmount)
        {
        }

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

        public virtual ARelic makeCopy()
        {
            var instance = Activator.CreateInstance(GetType());
            return (ARelic)instance;
        }

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