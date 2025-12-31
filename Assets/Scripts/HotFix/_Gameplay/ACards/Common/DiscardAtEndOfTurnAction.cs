using System;
using UnityEngine.Pool;

namespace MarbleHero
{
    public class DiscardAtEndOfTurnAction : AGameAction
    {
        static float DURATION = Settings.ACTION_DUR_XFAST;

        public override void update(float dt)
        {
            var hand = player.hand;

            if (duration < DURATION)
            {
                for (var i = 0; i < hand.Count;)
                {
                    var e = hand.get(i);
                    if (e.retain || e.selfRetain)
                    {
                        player.limbo.addToTop(e);
                        hand.removeCardAt(i);
                    }
                    else
                        i++;
                }

                addToTop(new RestoreRetainedCardsAction(player.limbo));

                if (!player.hasRelic("Runic Pyramid") && !player.hasPower("Equilibrium"))
                {
                    int tempSize = hand.size();
                    for (int i = 0; i < tempSize; i++)
                        addToTop(new DiscardAction(player, null, hand.size(), true, true));
                }

                using var _ = ListPool<ACard>.Get(out var cards);
                cards.AddRange(hand.group);
                cards.Shuffle(new Random());
                foreach (var card in cards)
                    card.triggerOnEndOfPlayerTurn();

                isDone = true;
            }
        }
    }
}