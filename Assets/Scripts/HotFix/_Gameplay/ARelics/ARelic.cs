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

        protected APlayer _player { get; set; }
        public RelicDef def { get; set; }
        public RelicItem item { get; set; }

        public void setRelicDef(RelicDef d)
        {
            def = d;
            onRelicDefSet(d);
        }

        protected virtual void onRelicDefSet(RelicDef d) { }

        public void setRelicItem(RelicItem d) => item = d;
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
            name = relicStrings.NAME;
            flavorText = relicStrings.FLAVOR;
            this.tier = tier;
            landingSFX = sfx;
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

        public void onPlayCard(ACard c) { }

        public void onPreviewObtainCard(ACard c) { }

        public void onObtainCard(ACard c) { }

        public virtual void onPickupGold(int amount) { }

        public void onGainGold() { }

        public void onLoseGold() { }

        public void onSpendGold() { }

        public virtual void onEquip(APlayer p)
        {
            _player = p;
            foreach (var mod in def.PlayerStatMods.safe())
            {
                if (p.GetStat(mod.stat, out var stat))
                {
                    var modKey = Guid.NewGuid().ToString();
                    if (!mod.BonusFlat.isZero())
                    {
                        stat.BonusFlat.AddFlat(mod.BonusFlat, modKey);
                    }
                    else if (!mod.BonusPct.isZero())
                    {
                        stat.BonusPct.AddFlat(mod.BonusPct, modKey);
                    }
                    else if (!mod.BonusRatio.isZero())
                    {
                        stat.BonusRatio.AddFlat(mod.BonusRatio, modKey);
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
                        else if (!mod.BonusRatio.isZero())
                        {
                            stat.BonusRatio.RemoveMod(modKey);
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

        public void atPreBattle() { }

        public void atBattleStart() { }

        public void onSpawnMonster(AMonster monster) { }

        public void atBattleStartPreDraw() { }

        public void atTurnStart() { }

        public void atTurnStartPostDraw() { }

        public void onPlayerEndTurn() { }

        public virtual void onShootBall(APlayer p, Ball ball) { }
        public virtual void onEnterBloodied(APlayer p) { }
        public virtual void onExitBloodied(APlayer p) { }

        public void onManualDiscard() { }

        public void onVictory() { }

        public void onMonsterDeath(AMonster m) { }

        public void onBlockBroken(ACreature m) { }

        public void onPlayerGainBlock(ref int blockAmount) { }

        public virtual void onBeforeHeal(ref int healAmount) { }
        public virtual void onAfterHeal(APlayer p, Heal heal) { }

        public void onMeditate() { }

        public void onEnergyRecharge() { }

        public void beforeEnergyPrep() { }

        public void onRest() { }

        public void onRitual() { }

        public void onEnterRestRoom() { }

        public void onRefreshHand() { }

        public void onShuffle() { }

        public void onSmith() { }

        public void onAttack(DamageInfo info, int damageAmount, ACreature target) { }

        public void onAttacked(DamageInfo info, ref int damageAmount) { }

        public void onAttackedToChangeDamage(DamageInfo info, ref int damageAmount) { }

        public void onAttackToChangeDamage(DamageInfo info, ref int damageAmount) { }

        public void onExhaust(ACard card) { }

        public void onTrigger() { }

        public void onTrigger(ACreature target) { }

        public bool checkTrigger() => false;

        public void onEnterRoom(ARoom room) { }

        public void justEnteredRoom(ARoom room) { }

        public void onCardDraw(ACard card) { }

        public void onChestOpen(bool bossChest) { }

        public void onChestOpenAfter(bool bossChest) { }

        public void onDrawOrDiscard() { }

        public void onMasterDeckChange() { }

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
            return builder.toString();
        }

        public override string ToString() => name;

        public int CompareTo(ARelic arg0) => string.Compare(name, arg0.name, StringComparison.Ordinal);

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

        public void onUsePotion() { }

        // public void onChangeStance(AbstractStance prevStance, AbstractStance newStance)
        // {
        // }

        public void onLoseHp(int damageAmount) { }
        public void onLoseHpLast(ref int damageAmount) { }
        public void wasHPLost(int damageAmount) { }

        public virtual void onPlayerTurnUpdate(APlayer p, float dt) { }

        public virtual void onBallBeginOverlappingBrickAll(APlayer p, Ball ball, Brick brick) { }
        public virtual void onBallEndOverlappingBrickAll(APlayer p, Ball ball, Brick brick, bool prematurely) { }
        public virtual void onBallBeginOverlappingBrickOne(APlayer p, Ball ball, Brick brick) { }
        public virtual void onBallEndOverlappingBrickOne(APlayer p, Ball ball, Brick brick, bool prematurely) { }

        public virtual void onBallBeginOverlappingPlayerAll(APlayer p, Ball ball, APlayer p2) { }
        public virtual void onBallEndOverlappingPlayerAll(APlayer p, Ball ball, APlayer p2, bool prematurely) { }
        public virtual void onBallBeginOverlappingPlayerOne(APlayer p, Ball ball, APlayer p2) { }
        public virtual void onBallEndOverlappingPlayerOne(APlayer p, Ball ball, APlayer p2, bool prematurely) { }

        public virtual void onPlayerTurnBegin(APlayer p) { }
        public virtual void onPlayerTurnEnd(APlayer p) { }
        public virtual void onFightingPhaseEnd(APlayer p) { }

        public virtual ARelic makeCopy()
        {
            var instance = Activator.CreateInstance(GetType());
            return (ARelic)instance;
        }

        public virtual void onBallHitBorder(APlayer p, Ball ball, Border border) { }
        public virtual void onBallHitBorderBot(APlayer p, Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn) { }
        public virtual void onBallHitBorderTop(APlayer p, Ball ball, BorderTop border, ref Vector2 normal) { }
        public virtual void onBallHitBorderLeft(APlayer p, Ball ball, BorderLeft border, ref Vector2 normal) { }
        public virtual void onBallHitBorderRight(APlayer p, Ball ball, BorderRight border, ref Vector2 normal) { }
        public virtual void onBallImpactHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit) { }
        public virtual void onBallImpactHitPlayer(APlayer p, Ball ball, APlayer p2, Vector2 normal) { }
        public virtual void onBallSkillHitBrick(APlayer p, Ball ball, Brick brick) { }
        public virtual void onBallImpactCrit(APlayer p, Ball ball, Brick brick, in Dmg dmg) { }
        public virtual void onBallSkillCrit(APlayer p, Ball ball, Brick brick) { }
        public virtual void onBallHitObstacle(APlayer p, Ball ball, Obstacle obstacle, ref Vector2 normal) { }
        public virtual void onBallEnterPlayer(APlayer p, Ball ball, APlayer p2, ref Vector2 normal, ref bool needReflect) { }
        public virtual void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir) { }
        public virtual void onBallKillBrick(APlayer p, Ball ball, Brick brick) { }
        public virtual void onKillBrick(APlayer p, Brick brick) { }
        public virtual void onKillBrickTotally(APlayer p, Brick brick) { }
        public virtual void onBallCauseImpactDamage(APlayer p, Ball ball, Brick brick, in Dmg dmg) { }
        public virtual void onBallCauseSkillDamage(APlayer p, Ball ball, Brick brick, in Dmg dmg) { }
        public virtual void onDropCoin(APlayer p, Coin coin) { }
        public virtual void onCauseDotDamage(APlayer p, Brick brick, in Dmg dmg) { }
        public virtual void onPlayerStartDash(APlayer p, Vector3 start, Vector3 end) { }
        public virtual void onPlayerStopDash(APlayer p) { }
        public virtual void onPlayerDashing(APlayer p, Vector3 pos) { }
        public virtual void onShield(APlayer p, int amount) { }
        public virtual void onBallDoHitEffect(APlayer p, Ball ball, DoHitEffect e, float delay) { }
        public virtual void onPlayerDashHitBall(APlayer p, Ball ball, Vector3 pos, Vector3 dir) { }
        public virtual void onPlayerHealthChanged(APlayer p, int curHealth) { }
        public virtual void onPlayerBeforeHandleImpactDamage(APlayer p, Ball ball, Brick brick, ref Dmg dmg) { }
        public virtual void onPlayerBeforeHandleSkillDamage(APlayer p, Ball ball, Brick brick, ref Dmg dmg) { }
        public virtual void onBallDeath(APlayer p, Ball ball) { }
        public virtual void onTakeDamage(APlayer p, in Dmg dmg) { }
        public virtual void onBallGetImpactDmg(APlayer p, Ball ball, ref Dmg dmg) { }
        public virtual void onBallGetSkillDmg(APlayer p, Ball ball, ref Dmg dmg) { }
    }
}