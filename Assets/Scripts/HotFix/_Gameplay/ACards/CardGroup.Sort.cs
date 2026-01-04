using System;

namespace MarbleHero
{
    public partial class CardGroup
    {
        static Comparison<ACard> _cardRarity = (c1, c2) => ((int)c1.rarity).CompareTo((int)c2.rarity);
        static Comparison<ACard> _cardType = (c1, c2) => ((int)c1.type).CompareTo((int)c2.type);
        static Comparison<ACard> _cardName = (c1, c2) => string.Compare(c1.name, c2.name, StringComparison.Ordinal);

        static Comparison<ACard> _cardLock = (c1, c2) =>
        {
            int c1Rank = 0;
            if (UnlockTracker.isCardLocked(c1.cardID))
                c1Rank = 2;
            else if (!UnlockTracker.isCardSeen(c1.cardID))
                c1Rank = 1;

            int c2Rank = 0;
            if (UnlockTracker.isCardLocked(c2.cardID))
                c2Rank = 2;
            else if (!UnlockTracker.isCardSeen(c2.cardID))
                c2Rank = 1;

            return c1Rank - c2Rank;
        };

        static Comparison<ACard> _statusCardsLast = (c1, c2) =>
        {
            if (c1.type == CardType.Status && c2.type != CardType.Status)
                return 1;
            if (c1.type != CardType.Status && c2.type == CardType.Status)
                return -1;
            return 0;
        };

        void sortWith(Comparison<ACard> comp, bool ascending)
        {
            group.Sort(comp);
            if (!ascending)
                group.Reverse();
        }

        public void sortByRarity(bool ascending) => sortWith(_cardRarity, ascending);

        public void sortByRarityPlusStatusCardType(bool ascending)
        {
            sortWith(_cardRarity, ascending);
            sortWith(_statusCardsLast, true);
        }

        public void sortByType(bool ascending) => sortWith(_cardType, ascending);

        public void sortByAcquisition()
        {
        }

        public void sortByStatus(bool ascending) => sortWith(_cardLock, ascending);
        public void sortAlphabetically(bool ascending) => sortWith(_cardName, ascending);
    }
}