namespace MarbleHero
{
    public class DiscardAction : AGameAction
    {
        APlayer p;
        bool isRandom;
        bool endTurn;
        public static int numDiscarded;
        static float DURATION = Settings.ACTION_DUR_XFAST;

        public DiscardAction(ACreature target, ACreature source, int amount, bool isRandom) : this(target, source, amount, isRandom, false)
        {
        }

        public DiscardAction(ACreature target, ACreature source, int amount, bool isRandom, bool endTurn)
        {
            p = (APlayer)target;
            this.isRandom = isRandom;
            setValues(target, source, amount);
            actionType = ActionType.DISCARD;
            this.endTurn = endTurn;
            duration = DURATION;
        }

        public override void update(float dt)
        {
            if (duration == DURATION)
            {
                if (monsters is { areMonstersBasicallyDead: true })
                {
                    isDone = true;
                    return;
                }

                if (p.hand.size() <= amount)
                {
                    amount = p.hand.size();
                    int tmp = p.hand.size();
                    for (int i = 0; i < tmp; i++)
                    {
                        ACard c = p.hand.getTopCard();
                        p.hand.moveToDiscardPile(c);
                        if (!endTurn)
                            c.triggerOnManualDiscard();

                        GameActionManager.incrementDiscard(endTurn);
                    }

                    player.hand.applyPowers();
                    tickDuration(dt);
                    return;
                }

                if (isRandom)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        ACard c = p.hand.getRandomCard(ADungeon.cardRandomRng);
                        p.hand.moveToDiscardPile(c);
                        c.triggerOnManualDiscard();
                        GameActionManager.incrementDiscard(endTurn);
                    }
                }
                else
                {
                    if (amount < 0)
                    {
                        // ADungeon.handCardSelectScreen.open(TEXT[0], 99, true, true);
                        player.hand.applyPowers();
                        tickDuration(dt);
                        return;
                    }

                    numDiscarded = amount;
                    // if (p.hand.size() > amount)
                    // ADungeon.handCardSelectScreen.open(TEXT[0], amount, false);

                    player.hand.applyPowers();
                    tickDuration(dt);
                    return;
                }
            }

            // if (!ADungeon.handCardSelectScreen.wereCardsRetrieved)
            // {
            //     foreach (ACard c in ADungeon.handCardSelectScreen.selectedCards.group)
            //     {
            //         p.hand.moveToDiscardPile(c);
            //         c.triggerOnManualDiscard();
            //         GameActionManager.incrementDiscard(endTurn);
            //     }
            //
            //     ADungeon.handCardSelectScreen.wereCardsRetrieved = true;
            // }

            tickDuration(dt);
        }
    }
}