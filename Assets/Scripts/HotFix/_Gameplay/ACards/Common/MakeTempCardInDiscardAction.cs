namespace MarbleHero
{
    public class MakeTempCardInDiscardAction : AGameAction
    {
        ACard c;
        int numCards;
        bool sameUUID;

        public MakeTempCardInDiscardAction(ACard card, int amount)
        {
            UnlockTracker.markCardAsSeen(card.cardID);
            numCards = amount;
            actionType = ActionType.CARD_MANIPULATION;
            startDuration = Settings.FAST_MODE ? Settings.ACTION_DUR_FAST : 0.5F;
            duration = startDuration;
            c = card;
            sameUUID = false;
        }

        public MakeTempCardInDiscardAction(ACard card, bool sameUUID) : this(card, 1)
        {
            this.sameUUID = sameUUID;
            if (!sameUUID && c.type != CardType.Curse && c.type != CardType.Status && player.hasPower("MasterRealityPower"))
                c.upgrade();
        }

        public override void update(float dt)
        {
            if (duration == startDuration)
            {
                if (numCards < 6)
                {
                    for (int i = 0; i < numCards; i++)
                        ADungeon.effectList.Add(new ShowCardAndAddToDiscardEffect(makeNewCard()));
                }

                duration -= dt;
            }

            tickDuration(dt);
        }

        ACard makeNewCard()
        {
            if (sameUUID)
                return c.makeSameInstanceOf();

            return c.makeStatEquivalentCopy();
        }
    }
}