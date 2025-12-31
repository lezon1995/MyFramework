using System.Collections.Generic;

namespace MarbleHero;

public class GameActionManager : FrameSystem
{
    public GameActionManager() => actionManager = this;

    public List<AGameAction> nextCombatActions = new();
    public List<AGameAction> actions = new();
    public List<AGameAction> preTurnActions = new();
    public List<CardQueueItem> cardQueue = new();
    public List<MonsterQueueItem> monsterQueue = new();
    public List<ACard> cardsPlayedThisTurn = new();
    public List<ACard> cardsPlayedThisCombat = new();

    public int mantraGained;
    public AGameAction currentAction;
    public AGameAction previousAction;
    public AGameAction turnStartCurrentAction;
    public ACard lastCard;
    public Phase phase = Phase.WAITING_ON_USER;
    public bool hasControl = true;
    public bool turnHasEnded { get; set; }
    public bool usingCard { get; set; }
    public bool monsterAttacksQueued { get; set; } = true;
    public static int totalDiscardedThisTurn { get; set; }
    public static int damageReceivedThisTurn { get; set; }
    public static int damageReceivedThisCombat { get; set; }
    public static int hpLossThisCombat { get; set; }
    public static int playerHpLastTurn { get; set; }
    public static int energyGainedThisCombat { get; set; }
    public static int turn { get; set; }

    public enum Phase
    {
        WAITING_ON_USER,
        EXECUTING_ACTIONS
    }

    public bool containsInCardQueue(ACard card)
    {
        foreach (var i in cardQueue)
        {
            if (i.card == card)
                return true;
        }

        return false;
    }

    public void useNextCombatActions()
    {
        foreach (var a in nextCombatActions)
            addToBot(a);

        nextCombatActions.Clear();
    }

    public void addCardQueueItem(CardQueueItem c, bool inFrontOfQueue)
    {
        if (inFrontOfQueue)
        {
            if (cardQueue.Count == 0)
                cardQueue.Add(c);
            else
                cardQueue.Insert(1, c);
        }
        else
        {
            cardQueue.Add(c);
        }
    }

    public void addCardQueueItem(CardQueueItem c) => addCardQueueItem(c, false);

    public void removeFromQueue(ACard c)
    {
        int index = -1;
        for (int i = 0; i < cardQueue.Count; i++)
        {
            var item = cardQueue[i];
            if (item.card != null && item.card == c)
            {
                index = i;
                break;
            }
        }

        if (index != -1)
            cardQueue.RemoveAt(index);
    }

    public void addMonsterQueueItem(MonsterQueueItem item) => monsterQueue.Add(item);

    public void clearPostCombatActions()
    {
        for (var i = actions.Count - 1; i >= 0; i--)
        {
            var e = actions[i];

            // if (e is HealAction || e is GainBlockAction)
            //     continue;

            // if (e is UseCardAction)
            // continue;

            // if (e.actionType == ActionType.DAMAGE)
            // continue;

            actions.RemoveAt(i);
        }
    }

    public void addToTop(AGameAction action)
    {
        if (room.inCombat())
            actions.Insert(0, action);
    }

    public void addToBot(AGameAction action)
    {
        if (room.inCombat())
            actions.Add(action);
    }

    public void addToTurnStart(AGameAction action)
    {
        if (room.inCombat())
            preTurnActions.Insert(0, action);
    }

    public void addToNextCombat(AGameAction action)
    {
        nextCombatActions.Add(action);
    }

    public override void update(float dt)
    {
        base.update(dt);
        switch (phase)
        {
            case Phase.WAITING_ON_USER:
                getNextAction();
                return;
            case Phase.EXECUTING_ACTIONS:
                if (currentAction is { isDone: false })
                {
                    currentAction.update(dt);
                }
                else
                {
                    previousAction = currentAction;
                    currentAction = null;
                    getNextAction();
                    if (currentAction == null && room.inCombat() && !usingCard)
                    {
                        phase = Phase.WAITING_ON_USER;
                        hasControl = false;
                    }

                    usingCard = false;
                }

                return;
        }
    }

    public override void fixedUpdate(float dt)
    {
        base.fixedUpdate(dt);
        switch (phase)
        {
            case Phase.WAITING_ON_USER:
                return;
            case Phase.EXECUTING_ACTIONS:
                if (currentAction is { isDone: false })
                    currentAction.fixedUpdate(dt);
                return;
        }
    }

    public void endTurn()
    {
        player.resetControllerValues();
        turnHasEnded = true;
        playerHpLastTurn = player.currentHealth;
    }

    void getNextAction()
    {
        if (checkActions())
            return;

        if (checkPreTurnActions())
            return;

        if (checkCardQueue())
            return;

        if (checkMonsterAttacksQueue())
            return;

        if (checkMonsterQueue())
            return;

        if (checkPawnsFighting())
            return;

        if (checkStartNextTurn())
            return;
    }

    bool checkStartNextTurn()
    {
        if (!turnHasEnded)
            return false;

        if (monsters is { areMonstersBasicallyDead: true })
            return false;

        if (!room.skipMonsterTurn)
            room.monsters.applyEndOfTurnPowers();

        room.startTurn();
        return true;
    }

    bool checkPawnsFighting()
    {
        if (room is not MonsterRoom)
            return false;

        if (!room.isTurnEnd)
            return false;

        if (room.isFightEnded)
            return false;

        if (!room.isFightStarted)
            room.startFight();
        else
            room.checkFightingResult();

        return true;
    }

    bool checkMonsterQueue()
    {
        if (monsterQueue.Count == 0)
            return false;

        AMonster m = monsterQueue[0].monster;
        if (!m.isDeadOrEscaped() || m.halfDead)
        {
            if (m.intent != Intent.NONE)
            {
                // addToBot(new ShowMoveNameAction(m));
                // addToBot(new IntentFlashAction(m));
            }

            if (!TipTracker.tips["INTENT_TIP"] && player.currentBlock == 0 &&
                m.intent is Intent.ATTACK or Intent.ATTACK_DEBUFF
                    or Intent.ATTACK_BUFF or Intent.ATTACK_DEFEND)
            {
                if (ADungeon.floorNum <= 5)
                    TipTracker.blockCounter++;
                else
                    TipTracker.neverShowAgain("INTENT_TIP");
            }

            m.takeTurn();
            m.applyTurnPowers();
        }

        monsterQueue.RemoveAt(0);
        if (monsterQueue.Count == 0)
            addToBot(new WaitAction(1.5F));

        return true;
    }

    bool checkMonsterAttacksQueue()
    {
        if (monsterAttacksQueued)
            return false;

        monsterAttacksQueued = true;

        if (room.skipMonsterTurn)
            return true;

        room.monsters.queueMonsters();
        return true;
    }

    bool checkCardQueue()
    {
        if (cardQueue.Count == 0)
            return false;

        usingCard = true;
        var item = cardQueue[0];
        var toPlay = item.card;
        if (toPlay == null)
        {
            callEndOfTurnActions();
        }
        else if (toPlay == lastCard)
        {
            logBase("Last card! " + toPlay.name);
            lastCard = null;
        }

        bool canPlay = false;
        var monster = item.monster;
        if (toPlay != null)
        {
            canPlay = true;
            foreach (var power in player.powers)
                power.onPlayCard(toPlay, monsters.main);

            foreach (var power in monsters.main.powers)
                power.onPlayCard(toPlay, monster);

            foreach (var relic in player.relics)
                relic.onPlayCard(toPlay, monster);

            player.cardsPlayedThisTurn++;
            cardsPlayedThisTurn.Add(toPlay);
            cardsPlayedThisCombat.Add(toPlay);

            if (cardsPlayedThisTurn.Count == 25)
                UnlockTracker.unlockAchievement("INFINITY");

            if (cardsPlayedThisTurn.Count >= 20 && !Game.combo)
                Game.combo = true;

            player.useCard(toPlay);
        }

        cardQueue.RemoveAt(0);
        return true;
    }

    bool checkPreTurnActions()
    {
        if (preTurnActions.Count == 0)
            return false;

        currentAction = preTurnActions[0];
        preTurnActions.RemoveAt(0);
        phase = Phase.EXECUTING_ACTIONS;
        hasControl = true;
        return true;
    }

    bool checkActions()
    {
        if (actions.Count == 0)
            return false;

        currentAction = actions[0];
        actions.RemoveAt(0);
        phase = Phase.EXECUTING_ACTIONS;
        hasControl = true;
        return true;
    }

    void callEndOfTurnActions()
    {
        room.applyEndOfTurnRelics();
        room.applyEndOfTurnPreCardPowers();
        // addToBot(new TriggerEndOfTurnOrbsAction());
    }

    public void callEndTurnEarlySequence()
    {
        cardQueue.Clear();
        // ADungeon.overlayMenu.endTurnButton.disable(true);
    }

    public void cleanCardQueue()
    {
        cardQueue.Clear();
    }

    public bool isEmpty() => actions.Count == 0;
    public void clearNextRoomCombatActions() => nextCombatActions.Clear();

    public void clear()
    {
        actions.Clear();
        preTurnActions.Clear();
        currentAction = null;
        previousAction = null;
        turnStartCurrentAction = null;
        cardsPlayedThisCombat.Clear();
        cardsPlayedThisTurn.Clear();
        cardQueue.Clear();
        energyGainedThisCombat = 0;
        mantraGained = 0;
        damageReceivedThisCombat = 0;
        damageReceivedThisTurn = 0;
        hpLossThisCombat = 0;
        turnHasEnded = false;
        turn = 0;
        phase = Phase.WAITING_ON_USER;
        totalDiscardedThisTurn = 0;
    }

    public static void incrementDiscard(bool endOfTurn)
    {
        totalDiscardedThisTurn++;
        if (!actionManager.turnHasEnded && !endOfTurn)
        {
            foreach (var r in player.relics)
                r.onManualDiscard();
        }
    }

    public void updateEnergyGain(int energyGain) => energyGainedThisCombat += energyGain;

    public static void queueExtraCard(ACard card, AMonster m)
    {
        ACard tmp = card.makeSameInstanceOf();
        int extraCount = 0;
        foreach (var item in actionManager.cardQueue)
        {
            if (item.card.uuid == card.uuid)
                extraCount++;
        }

        actionManager.addCardQueueItem(new(tmp), true);
    }
}