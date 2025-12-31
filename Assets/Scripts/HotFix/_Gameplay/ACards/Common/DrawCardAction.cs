using System.Collections.Generic;

namespace MarbleHero
{
    public class DrawCardAction : AGameAction
    {
        bool shuffleCheck;
        static ILogger logger = Log.GetLogger<DrawCardAction>();
        public static List<ACard> drawnCards = new();
        bool clearDrawHistory = true;
        AGameAction followUpAction;

        public DrawCardAction(ACreature source, int amount)
        {
            setValues(player, source, amount);
            actionType = ActionType.DRAW;
            if (Settings.FAST_MODE)
                duration = Settings.ACTION_DUR_XFAST;
            else
                duration = Settings.ACTION_DUR_FASTER;
        }


        public DrawCardAction(int amount, bool clearDrawHistory) : this(amount)
        {
            this.clearDrawHistory = clearDrawHistory;
        }

        public DrawCardAction(int amount) : this(player, amount)
        {
        }

        public DrawCardAction(int amount, AGameAction action) : this(amount, action, true)
        {
        }

        public DrawCardAction(int amount, AGameAction action, bool clearDrawHistory) : this(amount, clearDrawHistory)
        {
            followUpAction = action;
        }

        public override void update(float dt)
        {
            if (clearDrawHistory)
            {
                clearDrawHistory = false;
                drawnCards.Clear();
            }

            if (player.TryGetPower("No Draw", out var power))
            {
                power.flash();
                Done();
                return;
            }

            if (amount <= 0)
            {
                Done();
                return;
            }

            if (SoulGroup.isActive())
                return;

            int deckSize = player.drawPile.size();
            int discardSize = player.discardPile.size();
            if (deckSize + discardSize == 0)
            {
                Done();
                return;
            }

            if (player.hand.size() == 10)
            {
                player.createHandIsFullDialog();
                Done();
                return;
            }

            if (!shuffleCheck)
            {
                if (amount + player.hand.size() > 10)
                {
                    int handSizeAndDraw = 10 - amount + player.hand.size();
                    amount += handSizeAndDraw;
                    player.createHandIsFullDialog();
                }

                if (amount > deckSize)
                {
                    int tmp = amount - deckSize;
                    addToTop(new DrawCardAction(tmp, followUpAction, false));
                    addToTop(new EmptyDeckShuffleAction());
                    if (deckSize != 0)
                        addToTop(new DrawCardAction(deckSize, false));

                    amount = 0;
                    isDone = true;
                    return;
                }

                shuffleCheck = true;
            }

            duration -= dt;
            if (amount != 0 && duration < 0.0F)
            {
                if (Settings.FAST_MODE)
                    duration = Settings.ACTION_DUR_XFAST;
                else
                    duration = Settings.ACTION_DUR_FASTER;

                amount--;
                if (player.drawPile.Count > 0)
                {
                    drawnCards.Add(player.drawPile.getTopCard());
                    player.draw();
                    player.hand.refreshHandLayout();
                }
                else
                {
                    logger.Warn("Player attempted to draw from an empty draw-pile mid-DrawAction?MASTER DECK: " + player.masterDeck.getCardNames());
                    Done();
                }

                if (amount == 0)
                    Done();
            }
        }

        void Done()
        {
            isDone = true;
            if (followUpAction != null)
                addToTop(followUpAction);
        }
    }
}