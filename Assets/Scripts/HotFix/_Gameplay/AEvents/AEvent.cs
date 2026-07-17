using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public abstract class AEvent : IDisposable
    {
        protected Texture img;
        // public RoomEventDialog roomEventText = new RoomEventDialog();
        // public GenericEventDialog imageEventText = new GenericEventDialog();
        protected float drawX;
        protected float drawY;
        protected float imgWidth;
        protected float imgHeight;
        protected Color imgColor = Color.white;
        // protected Hitbox hb = null;
        public float panelAlpha;
        public bool hideAlpha = false;
        public bool hasFocus;
        protected string body = null;
        public Timer waitTimer = 1.5F;
        protected bool waitForInput = false;
        public bool hasDialog = false;
        protected int screenNum;
        public static EventType type = EventType.IMAGE;
        public static string NAME;
        public static string[] DESCRIPTIONS;
        public static string[] OPTIONS;
        public bool combatTime = false;
        public bool noCardsInRewards = false;
        public List<int> optionsSelected = new();

        public enum EventType
        {
            TEXT,
            IMAGE,
            ROOM
        }

        protected AEvent()
        {
            type = EventType.ROOM;
            if (Settings.FAST_MODE)
                waitTimer = 0.1F;
        }

        protected void initializeImage(string imgUrl, float x, float y)
        {
            // img = ImageMaster.loadImage(imgUrl);
            drawX = x;
            drawY = y;
            // imgWidth = img.getWidth() * Settings.xScale;
            // imgHeight = img.getHeight() * Settings.scale;
        }

        public virtual void onEnterRoom()
        {
        }

        public void enterCombat()
        {
            // roomEventText.clear();
            room.phase = RoomPhase.COMBAT;
            room.monsters.init();
            room.waitTimer = MonsterRoom.COMBAT_WAIT_TIME;
            player.preBattlePrep();
            hasFocus = false;
            // roomEventText.hide();
        }

        protected abstract void buttonEffect(int paramInt);

        public void updateDialog()
        {
            // imageEventText.update();
            // roomEventText.update();
        }

        public virtual void update(float dt)
        {
            if (waitTimer)
            {
                if (waitTimer.update(dt) && hasDialog)
                {
                    // roomEventText.show(body);
                    waitTimer.kill();
                }
            }
            else if (room.phase != RoomPhase.COMBAT && !hideAlpha)
            {
                panelAlpha = MathHelper.fadeLerpSnap(panelAlpha, 0.66F, dt);
            }
            else
            {
                panelAlpha = MathHelper.fadeLerpSnap(panelAlpha, 0.0F, dt);
            }

            // if (!RoomEventDialog.waitForInput)
                // buttonEffect(roomEventText.getSelectedOption());
        }

        public void logInput(int buttonPressed)
        {
            optionsSelected.Add(buttonPressed);
        }

        protected void openMap()
        {
            room.completeRoom();
            new OpenMapPanel().trigger();
            // ADungeon.dungeonMapScreen.open(false);
        }

        /*public void render(SpriteBatch sb)
        {
            if (img != null)
            {
                sb.setColor(imgColor);
                sb.draw(img, drawX, drawY, imgWidth, imgHeight);
            }

            if (hb != null)
            {
                hb.render(sb);
                if (img != null && hb.hovered)
                {
                    sb.setBlendFunction(770, 1);
                    sb.setColor(new Color(1.0F, 1.0F, 1.0F, 0.3F));
                    sb.draw(img, drawX, drawY, imgWidth, imgHeight);
                    sb.setBlendFunction(770, 771);
                }
            }
        }

        public void renderText(SpriteBatch sb)
        {
            roomEventText.render(sb);
            imageEventText.render(sb);
        }

        public void renderRoomEventPanel(SpriteBatch sb)
        {
            sb.setColor(new Color(0.0F, 0.0F, 0.0F, panelAlpha));
            sb.draw(ImageMaster.EVENT_ROOM_PANEL, 0.0F, Settings.HEIGHT - 475.0F * Settings.scale, Settings.WIDTH, 300.0F * Settings.scale);
        }

        public void showProceedScreen(string bodyText)
        {
            roomEventText.updateBodyText(bodyText);
            roomEventText.updateDialogOption(0, "[ #bProceed ]");
            roomEventText.clearRemainingOptions();
            screenNum = 99;
        }

        public void renderAboveTopPanel(SpriteBatch sb)
        {
        }*/

        public void reopen()
        {
        }

        public void postCombatLoad()
        {
            room.phase = RoomPhase.COMBAT;
            room.isBattleOver = true;
            room.monsters = MonsterHelper.getEncounter("Colosseum Nobs");
            hasFocus = false;
            // GenericEventDialog.hide();
            // ADungeon.rs = ADungeon.RenderScene.NORMAL;
        }

        public static void logMetric(string eventName, string playerChoice, List<string> cardsObtained, List<string> cardsRemoved, List<string> cardsTransformed, List<string> cardsUpgraded, List<string> relicsObtained, List<string> potionsObtained, List<string> relicsLost, int damageTaken, int damageHealed, int hpLoss, int hpGain, int goldGain, int goldLoss)
        {
            Dictionary<string, object> choice = new()
            {
                { "event_name", eventName },
                { "player_choice", playerChoice },
                { "floor", ADungeon.floorNum },
                { "cards_obtained", cardsObtained },
                { "cards_removed", cardsRemoved },
                { "cards_transformed", cardsTransformed },
                { "cards_upgraded", cardsUpgraded },
                { "relics_obtained", relicsObtained },
                { "potions_obtained", potionsObtained },
                { "relics_lost", relicsLost },
                { "damage_taken", damageTaken },
                { "damage_healed", damageHealed },
                { "max_hp_loss", hpLoss },
                { "max_hp_gain", hpGain },
                { "gold_gain", goldGain },
                { "gold_loss", goldLoss }
            };
            metricData.event_choices.Add(choice);
        }

        public static void logMetricTransformCardsAtCost(string eventName, string playerChoice, List<string> cardsTransformed, List<string> cardsObtained, int cost)
        {
            logMetric(eventName, playerChoice, cardsObtained, null, cardsTransformed, null, null, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricRemoveCardsAtCost(string eventName, string playerChoice, List<string> cardsRemoved, int cost)
        {
            logMetric(eventName, playerChoice, null, cardsRemoved, null, null, null, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricRemoveCards(string eventName, string playerChoice, List<string> cardsRemoved)
        {
            logMetricRemoveCardsAtCost(eventName, playerChoice, cardsRemoved, 0);
        }

        public static void logMetricObtainCardsLoseMapHP(string eventName, string playerChoice, List<string> cardsObtained, int maxHPLoss)
        {
            logMetric(eventName, playerChoice, cardsObtained, null, null, null, null, null, null, 0, 0, maxHPLoss, 0, 0, 0);
        }

        public static void logMetricObtainCardsLoseRelic(string eventName, string playerChoice, List<string> cardsObtained, ARelic relicLost)
        {
            List<string> tempList2 = new() { relicLost.relicId };
            logMetric(eventName, playerChoice, cardsObtained, null, null, null, null, null, tempList2, 0, 0, 0, 0, 0, 0);
        }

        public static void logMetricObtainCards(string eventName, string playerChoice, List<string> cardsObtained)
        {
            logMetricObtainCardsLoseMapHP(eventName, playerChoice, cardsObtained, 0);
        }

        public static void logMetricUpgradeCardsAtCost(string eventName, string playerChoice, List<string> cardsUpgraded, int cost)
        {
            logMetric(eventName, playerChoice, null, null, null, cardsUpgraded, null, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricUpgradeCards(string eventName, string playerChoice, List<string> cardsUpgraded)
        {
            logMetricUpgradeCardsAtCost(eventName, playerChoice, cardsUpgraded, 0);
        }

        public static void logMetricTransformCards(string eventName, string playerChoice, List<string> cardsTransformed, List<string> cardsObtained)
        {
            logMetricTransformCardsAtCost(eventName, playerChoice, cardsTransformed, cardsObtained, 0);
        }

        public static void logMetricGainGoldAndDamage(string eventName, string playerChoice, int gold, int damage)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, damage, 0, 0, 0, gold, 0);
        }

        public static void logMetricGainGoldAndRelic(string eventName, string playerChoice, ARelic relicGained, int gold)
        {
            List<string> tempList2 = new() { relicGained.relicId };
            logMetric(eventName, playerChoice, null, null, null, null, tempList2, null, null, 0, 0, 0, 0, gold, 0);
        }

        public static void logMetricGainGoldAndLoseRelic(string eventName, string playerChoice, ARelic relicLost, int gold)
        {
            List<string> tempList2 = new() { relicLost.relicId };
            logMetric(eventName, playerChoice, null, null, null, null, null, null, tempList2, 0, 0, 0, 0, gold, 0);
        }

        public static void logMetricGainGoldAndCard(string eventName, string playerChoice, ACard cardGained, int gold)
        {
            List<string> tempList2 = new() { cardGained.cardID };
            logMetric(eventName, playerChoice, tempList2, null, null, null, null, null, null, 0, 0, 0, 0, gold, 0);
        }

        public static void logMetricObtainRelicAndLoseMaxHP(string eventName, string playerChoice, ARelic relicGained, int hpLoss)
        {
            List<string> tempList2 = new() { relicGained.relicId };
            logMetric(eventName, playerChoice, null, null, null, null, tempList2, null, null, 0, 0, hpLoss, 0, 0, 0);
        }

        public static void logMetricObtainRelicAndDamage(string eventName, string playerChoice, ARelic relicGained, int damage)
        {
            List<string> tempList2 = new() { relicGained.relicId };
            logMetric(eventName, playerChoice, null, null, null, null, tempList2, null, null, damage, 0, 0, 0, 0, 0);
        }

        public static void logMetricObtainRelicAtCost(string eventName, string playerChoice, ARelic relicGained, int cost)
        {
            List<string> tempList2 = new() { relicGained.relicId };
            logMetric(eventName, playerChoice, null, null, null, null, tempList2, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricGainAndLoseGold(string eventName, string playerChoice, int goldGain, int goldLoss)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, 0, 0, 0, 0, goldGain, goldLoss);
        }

        public static void logMetricGainGold(string eventName, string playerChoice, int gold)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, 0, 0, 0, 0, gold, 0);
        }

        public static void logMetricLoseGold(string eventName, string playerChoice, int gold)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, 0, 0, 0, 0, 0, gold);
        }

        public static void logMetricTakeDamage(string eventName, string playerChoice, int damage)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, damage, 0, 0, 0, 0, 0);
        }

        public static void logMetricCardRemovalAtCost(string eventName, string playerChoice, ACard cardRemoved, int cost)
        {
            List<string> tempList = new() { cardRemoved.cardID };
            logMetric(eventName, playerChoice, null, tempList, null, null, null, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricCardRemovalAndDamage(string eventName, string playerChoice, ACard cardRemoved, int damage)
        {
            List<string> tempList = new() { cardRemoved.cardID };
            logMetric(eventName, playerChoice, null, tempList, null, null, null, null, null, damage, 0, 0, 0, 0, 0);
        }

        public static void logMetricCardRemovalHealMaxHPUp(string eventName, string playerChoice, ACard cardRemoved, int heal, int maxUp)
        {
            List<string> tempList = new() { cardRemoved.cardID };
            logMetric(eventName, playerChoice, null, tempList, null, null, null, null, null, 0, heal, 0, maxUp, 0, 0);
        }

        public static void logMetricCardRemovalAndHeal(string eventName, string playerChoice, ACard cardRemoved, int heal)
        {
            logMetricCardRemovalHealMaxHPUp(eventName, playerChoice, cardRemoved, heal, 0);
        }

        public static void logMetricCardRemoval(string eventName, string playerChoice, ACard cardRemoved)
        {
            logMetricCardRemovalAtCost(eventName, playerChoice, cardRemoved, 0);
        }

        public static void logMetricCardUpgradeAndRemovalAtCost(string eventName, string playerChoice, ACard cardUpgraded, ACard cardRemoved, int cost)
        {
            List<string> tempList = new() { cardUpgraded.cardID };
            List<string> tempList2 = new() { cardRemoved.cardID };
            logMetric(eventName, playerChoice, null, tempList2, null, tempList, null, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricCardUpgradeAndRemoval(string eventName, string playerChoice, ACard cardUpgraded, ACard cardRemoved)
        {
            logMetricCardUpgradeAndRemovalAtCost(eventName, playerChoice, cardUpgraded, cardRemoved, 0);
        }

        public static void logMetricCardUpgradeAtCost(string eventName, string playerChoice, ACard cardUpgraded, int cost)
        {
            List<string> tempList = new();
            tempList.Add(cardUpgraded.cardID);
            logMetric(eventName, playerChoice, null, null, null, tempList, null, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricCardUpgrade(string eventName, string playerChoice, ACard cardUpgraded)
        {
            logMetricCardUpgradeAtCost(eventName, playerChoice, cardUpgraded, 0);
        }

        public static void logMetricHealAtCost(string eventName, string playerChoice, int cost, int healAmount)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, 0, healAmount, 0, 0, 0, cost);
        }

        public static void logMetricHealAndLoseMaxHP(string eventName, string playerChoice, int healAmount, int maxHPLoss)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, 0, healAmount, maxHPLoss, 0, 0, 0);
        }

        public static void logMetricHeal(string eventName, string playerChoice, int healAmount)
        {
            logMetricHealAtCost(eventName, playerChoice, 0, healAmount);
        }

        public static void logMetric(string eventName, string playerChoice)
        {
            logMetricHeal(eventName, playerChoice, 0);
        }

        public static void logMetricIgnored(string eventName)
        {
            logMetric(eventName, "Ignored");
        }

        public static void logMetricMaxHPGain(string eventName, string playerChoice, int maxHPAmount)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, 0, 0, 0, maxHPAmount, 0, 0);
        }

        public static void logMetricMaxHPLoss(string eventName, string playerChoice, int hpLoss)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, 0, 0, hpLoss, 0, 0, 0);
        }

        public static void logMetricDamageAndMaxHPGain(string eventName, string playerChoice, int damage, int maxHPAmount)
        {
            logMetric(eventName, playerChoice, null, null, null, null, null, null, null, damage, 0, 0, maxHPAmount, 0, 0);
        }

        public static void logMetricObtainCardAndHeal(string eventName, string playerChoice, ACard cardGained, int heal)
        {
            List<string> tempList = new() { cardGained.cardID };
            logMetric(eventName, playerChoice, tempList, null, null, null, null, null, null, 0, heal, 0, 0, 0, 0);
        }

        public static void logMetricObtainCardAndDamage(string eventName, string playerChoice, ACard cardGained, int damage)
        {
            List<string> tempList = new() { cardGained.cardID };
            logMetric(eventName, playerChoice, tempList, null, null, null, null, null, null, damage, 0, 0, 0, 0, 0);
        }

        public static void logMetricObtainCardAndLoseCard(string eventName, string playerChoice, ACard cardGained, ACard cardLost)
        {
            List<string> tempList = new() { cardGained.cardID };
            List<string> tempList2 = new() { cardLost.cardID };
            logMetric(eventName, playerChoice, tempList, tempList2, null, null, null, null, null, 0, 0, 0, 0, 0, 0);
        }

        public static void logMetricObtainCardAndRelic(string eventName, string playerChoice, ACard cardGained, ARelic relicGained)
        {
            List<string> tempList = new() { cardGained.cardID };
            List<string> tempList2 = new() { relicGained.relicId };
            logMetric(eventName, playerChoice, tempList, null, null, null, tempList2, null, null, 0, 0, 0, 0, 0, 0);
        }

        public static void logMetricRemoveCardAndObtainRelic(string eventName, string playerChoice, ACard cardRemoved, ARelic relicGained)
        {
            List<string> tempList = new() { cardRemoved.cardID };
            List<string> tempList2 = new() { relicGained.relicId };
            logMetric(eventName, playerChoice, null, tempList, null, null, tempList2, null, null, 0, 0, 0, 0, 0, 0);
        }

        public static void logMetricTransformCardAtCost(string eventName, string playerChoice, ACard cardTransformed, ACard cardGained, int cost)
        {
            List<string> tempList = new() { cardTransformed.cardID };
            List<string> tempList2 = new() { cardGained.cardID };
            logMetric(eventName, playerChoice, tempList2, null, tempList, null, null, null, null, 0, 0, 0, 0, 0, cost);
        }

        public static void logMetricTransformCard(string eventName, string playerChoice, ACard cardTransformed, ACard cardGained)
        {
            logMetricTransformCardAtCost(eventName, playerChoice, cardTransformed, cardGained, 0);
        }

        public static void logMetricRelicSwap(string eventName, string playerChoice, ARelic relicGained, ARelic relicLost)
        {
            List<string> tempList = new() { relicGained.relicId };
            List<string> tempList2 = new() { relicLost.relicId };
            logMetric(eventName, playerChoice, null, null, null, null, tempList, null, tempList2, 0, 0, 0, 0, 0, 0);
        }

        public static void logMetricObtainRelic(string eventName, string playerChoice, ARelic relicGained)
        {
            List<string> tempList = new() { relicGained.relicId };
            logMetric(eventName, playerChoice, null, null, null, null, tempList, null, null, 0, 0, 0, 0, 0, 0);
        }

        public static void logMetricObtainCard(string eventName, string playerChoice, ACard cardGained)
        {
            List<string> tempList = new() { cardGained.cardID };
            logMetric(eventName, playerChoice, tempList, null, null, null, null, null, null, 0, 0, 0, 0, 0, 0);
        }

        public Dictionary<string, object> getLocStrings()
        {
            Dictionary<string, object> data = new()
            {
                { "name", NAME },
                { "moves", DESCRIPTIONS },
                { "dialogs", OPTIONS }
            };
            return data;
        }

        public void Dispose()
        {
            if (img)
            {
                log("Disposed event img asset");
                // img.dispose();
                img = null;
            }

            // imageEventText.clear();
            // roomEventText.clear();
        }
    }
}