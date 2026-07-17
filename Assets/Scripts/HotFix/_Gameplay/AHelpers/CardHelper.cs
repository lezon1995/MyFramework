using System.Collections.Generic;

namespace MoreMountains
{
    public class CardHelper
    {
        public static int COMMON_CARD_LIMIT = 3;
        public static int UNCOMMON_CARD_LIMIT = 2;
        public static Dictionary<string, int> obtainedCards = new();
        public static List<CardInfo> removedCards = new();

        public static void obtain(string key, CardRarity rarity, CardColor color)
        {
            if (rarity == CardRarity.Special || rarity == CardRarity.Basic || rarity == CardRarity.Curse)
            {
                log("No need to track rarity type: " + rarity);
                return;
            }

            if (obtainedCards.ContainsKey(key))
            {
                int tmp = obtainedCards[key] + 1;
                obtainedCards.Add(key, tmp);
                log("Obtained " + key + " (" + rarity + "). You have " + tmp + " now");
            }
            else
            {
                obtainedCards.Add(key, 1);
                log("Obtained " + key + " (" + rarity + "). Creating new map entry.");
            }

            UnlockTracker.markCardAsSeen(key);
        }

        public static void clear()
        {
            log("Clearing CardHelper (obtained cards)");
            removedCards.Clear();
            obtainedCards.Clear();
        }

        public class CardInfo
        {
            string id;
            string name;
            CardRarity rarity;
            CardColor color;

            public CardInfo(string id, string name, CardRarity rarity, CardColor color)
            {
                this.id = id;
                this.name = name;
                this.rarity = rarity;
                this.color = color;
            }
        }

        /*public static bool hasCardWithXDamage(int damage)
        {
            foreach (var c in player.masterDeck.group)
            {
                if (c.type == CardType.Enhance && c.baseDamage >= 10)
                {
                    log(c.name + " is 10 Attack!");
                    return true;
                }
            }

            return false;
        }

        public static bool hasCardWithID(string targetID)
        {
            foreach (var c in player.masterDeck.group)
            {
                if (c.cardID == targetID)
                    return true;
            }

            return false;
        }

        public static bool hasCardType(CardType hasType)
        {
            foreach (var c in player.masterDeck.group)
            {
                if (c.type == hasType)
                    return true;
            }

            return false;
        }

        public static bool hasCardWithType(CardType type)
        {
            foreach (var c in (CardGroup.getGroupWithoutBottledCards(player.masterDeck)).group)
            {
                if (c.type == type)
                    return true;
            }

            return false;
        }

        public static ACard returnCardOfType(CardType type, Rand rng)
        {
            List<ACard> cards = new();
            foreach (var c in CardGroup.getGroupWithoutBottledCards(player.masterDeck).group)
            {
                if (c.type == type)
                    cards.Add(c);
            }

            var index = rng.random(cards.Count - 1);
            var card = cards[index];
            cards.RemoveAt(index);
            return card;
        }

        public static bool hasUpgradedCard()
        {
            foreach (var c in CardGroup.getGroupWithoutBottledCards(player.masterDeck).group)
            {
                if (c.upgraded)
                    return true;
            }

            return false;
        }

        public static ACard returnUpgradedCard(Rand rng)
        {
            List<ACard> cards = new();
            foreach (var c in CardGroup.getGroupWithoutBottledCards(player.masterDeck).group)
            {
                if (c.upgraded)
                    cards.Add(c);
            }

            var index = rng.random(cards.Count - 1);
            var card = cards[index];
            cards.RemoveAt(index);
            return card;
        }*/
    }
}