using System;

namespace MoreMountains
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