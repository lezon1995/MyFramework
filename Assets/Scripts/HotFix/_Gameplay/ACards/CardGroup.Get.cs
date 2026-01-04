using System;
using System.Collections.Generic;

namespace MarbleHero
{
    public partial class CardGroup
    {
        public ACard getTopCard() => group[^1];
        public ACard getNCardFromTop(int num) => group[group.Count - 1 - num];
        public ACard getBottomCard() => group[0];
        public ACard getRandomCard(Rand rng)
        {
            return group[rng.random(group.Count - 1)];
        }

        public ACard getRandomCard(bool useRng)
        {
            if (useRng)
                return group[ADungeon.cardRng.random(group.Count - 1)];

            return group[MathUtils.random(group.Count - 1)];
        }

        public ACard getRandomCard(bool useRng, CardRarity rarity)
        {
            List<ACard> tmp = new();
            foreach (var c in group)
            {
                if (c.rarity == rarity)
                    tmp.Add(c);
            }

            if (tmp.Count == 0)
            {
                log("ERROR: No cards left for type: " + type);
                return null;
            }

            tmp.Sort();

            if (useRng)
                return tmp[ADungeon.cardRng.random(tmp.Count - 1)];

            return tmp[MathUtils.random(tmp.Count - 1)];
        }

        public ACard getRandomCard(Rand rng, CardRarity rarity)
        {
            List<ACard> tmp = new();
            foreach (var c in group)
            {
                if (c.rarity == rarity)
                    tmp.Add(c);
            }

            if (tmp.Count == 0)
            {
                log("ERROR: No cards left for type: " + type);
                return null;
            }

            tmp.Sort();
            return tmp[rng.random(tmp.Count - 1)];
        }

        public ACard getRandomCard(CardType type, bool useRng)
        {
            List<ACard> tmp = new();
            foreach (var c in group)
            {
                if (c.type == type)
                    tmp.Add(c);
            }

            if (tmp.Count == 0)
            {
                log("ERROR: No cards left for type: " + type);
                return null;
            }

            tmp.Sort();
            if (useRng)
                return tmp[ADungeon.cardRng.random(tmp.Count - 1)];

            return tmp[MathUtils.random(tmp.Count - 1)];
        }

        public ACard getSpecificCard(ACard card)
        {
            if (group.Contains(card))
                return card;
            return null;
        }

        public ACard findCardById(string id)
        {
            foreach (var c in group)
            {
                if (c.cardID == id)
                    return c;
            }

            return null;
        }

        public CardGroup getPurgeableCards()
        {
            var retVal = new TempCards();
            foreach (var c in group)
            {
                switch (c.cardID)
                {
                    case "Necronomicurse":
                    case "CurseOfTheBell":
                    case "AscendersBane":
                        continue;
                }

                retVal.group.Add(c);
                break;
            }

            return retVal;
        }

        public CardGroup getCardsOfType(CardType cardType)
        {
            var retVal = new TempCards();
            foreach (var c in group)
            {
                if (c.type == cardType)
                    retVal.addToBottom(c);
            }

            return retVal;
        }

        public CardGroup getGroupedByColor()
        {
            List<CardGroup> colorGroups = new();

            foreach (var color in Enum.GetValues(typeof(CardColor)))
                colorGroups.Add(new TempCards());

            foreach (var card in group)
                colorGroups[(int)card.color].addToTop(card);

            var retVal = new TempCards();
            foreach (var cardGroup in colorGroups)
                retVal.group.AddRange(cardGroup.group);

            return retVal;
        }
    }
}