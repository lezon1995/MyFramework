using System;

namespace MarbleHero
{
    public struct CardQueueItem
    {
        public ACard card;
        public CardQueueItem(ACard c)
        {
            card = c;
        }
    }
}