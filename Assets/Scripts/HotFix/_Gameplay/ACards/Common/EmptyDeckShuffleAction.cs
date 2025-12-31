namespace MarbleHero
{
    public class EmptyDeckShuffleAction : AGameAction
    {
        // static TutorialStrings tutorialStrings = Game.languagePack.getTutorialString("Shuffle Tip");
        // public static String[] MSG = tutorialStrings.TEXT;
        // public static String[] LABEL = tutorialStrings.LABEL;
        bool shuffled;
        bool vfxDone;
        int count;

        public EmptyDeckShuffleAction()
        {
            setValues(null, null, 0);
            actionType = ActionType.SHUFFLE;
            if (!TipTracker.tips["SHUFFLE_TIP"])
            {
                // ADungeon.ftue = new FtueTip(LABEL[0], MSG[0], Settings.WIDTH / 2.0F, Settings.HEIGHT / 2.0F, FtueTip.TipType.SHUFFLE);
                TipTracker.neverShowAgain("SHUFFLE_TIP");
            }

            foreach (var relic in player.relics)
                relic.onShuffle();
        }

        public override void update(float dt)
        {
            var discardPile = player.discardPile;
            if (!shuffled)
            {
                shuffled = true;
                discardPile.shuffle(ADungeon.shuffleRng);
            }

            if (!vfxDone)
            {
                for (var i = discardPile.Count - 1; i >= 0; i--)
                {
                    count++;
                    discardPile.removeCardAt(i, out var card);
                    room.souls.shuffle(card, count >= 11);
                    return;
                }

                // Iterator<AbstractCard> c = player.discardPile.group.iterator();
                // if (c.hasNext())
                // {
                //     count++;
                //     AbstractCard e = c.next();
                //     c.remove();
                //     (room).souls.shuffle(e, count >= 11);
                //     return;
                // }

                vfxDone = true;
            }

            isDone = true;
        }
    }
}