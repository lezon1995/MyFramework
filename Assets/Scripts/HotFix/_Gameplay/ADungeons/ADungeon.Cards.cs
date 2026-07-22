using System.Collections.Generic;

namespace MoreMountains
{
    public partial class ADungeon
    {
        public static int cardInstanceIdGenerator { get; set; }
        public static PoolCards srcColorlessCardPool = new();
        public static PoolCards srcCurseCardPool = new();
        public static PoolCards srcCommonCardPool = new();
        public static PoolCards srcUncommonCardPool = new();
        public static PoolCards srcRareCardPool = new();
        public static PoolCards colorlessCardPool = new();
        public static PoolCards curseCardPool = new();
        public static PoolCards commonCardPool = new();
        public static PoolCards uncommonCardPool = new();
        public static PoolCards rareCardPool = new();

        protected void initializeCardPools()
        {
            log("INIT CARD POOL");
            long startTime = TimeUtility.getNowTimeStampMS();
            commonCardPool.clear();
            uncommonCardPool.clear();
            rareCardPool.clear();
            colorlessCardPool.clear();
            curseCardPool.clear();
            List<ACard> tmpPool = new();
            if (ModHelper.isModEnabled("Colorless Cards"))
                CardLibrary.addColorlessCards(tmpPool);

            if (ModHelper.isModEnabled("Diverse"))
            {
                CardLibrary.addRedCards(tmpPool);
                CardLibrary.addGreenCards(tmpPool);
                CardLibrary.addBlueCards(tmpPool);
                if (!UnlockTracker.isCharacterLocked("Watcher"))
                    CardLibrary.addPurpleCards(tmpPool);
            }
            else
            {
                player.getCardPool(tmpPool);
            }

            addColorlessCards();
            addCurseCards();
            foreach (var c in tmpPool)
            {
                switch (c.rarity)
                {
                    case CardRarity.Common:
                        commonCardPool.addToTop(c);
                        continue;
                    case CardRarity.Uncommon:
                        uncommonCardPool.addToTop(c);
                        continue;
                    case CardRarity.Rare:
                        rareCardPool.addToTop(c);
                        continue;
                    case CardRarity.Curse:
                        curseCardPool.addToTop(c);
                        continue;
                }

                log("Unspecified rarity: " + c.rarity + " when creating pools");
            }

            srcColorlessCardPool = new();
            srcCurseCardPool = new();
            srcRareCardPool = new();
            srcUncommonCardPool = new();
            srcCommonCardPool = new();
            foreach (var c in colorlessCardPool.group)
                srcColorlessCardPool.addToBottom(c);
            foreach (var c in curseCardPool.group)
                srcCurseCardPool.addToBottom(c);
            foreach (var c in rareCardPool.group)
                srcRareCardPool.addToBottom(c);
            foreach (var c in uncommonCardPool.group)
                srcUncommonCardPool.addToBottom(c);
            foreach (var c in commonCardPool.group)
                srcCommonCardPool.addToBottom(c);
            log("CardPool load time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
        }

        void addColorlessCards()
        {
            foreach (var (_, card) in CardLibrary.cards)
            {
                if (card.color == CardColor.Colorless)
                {
                    if (card.type == CardType.Status)
                        continue;

                    switch (card.rarity)
                    {
                        case CardRarity.Basic:
                        case CardRarity.Special:
                            continue;
                    }

                    colorlessCardPool.addToTop(card);
                }
            }

            log("COLORLESS CARDS: " + colorlessCardPool.size());
        }

        void addCurseCards()
        {
            foreach (var (_, card) in CardLibrary.cards)
            {
                if (card.type == CardType.Curse)
                {
                    switch (card.cardID)
                    {
                        case "Necronomicurse":
                        case "AscendersBane":
                        case "CurseOfTheBell":
                        case "Pride":
                            continue;
                    }

                    curseCardPool.addToTop(card);
                }
            }

            log("CURSE CARDS: " + curseCardPool.size());
        }

        public static ACard returnRandomCard()
        {
            List<ACard> list = new();
            CardRarity rarity = rollRarity();
            switch (rarity)
            {
                case CardRarity.Common:
                    list.AddRange(srcCommonCardPool.group);
                    break;
                case CardRarity.Uncommon:
                    list.AddRange(srcUncommonCardPool.group);
                    break;
                default:
                    list.AddRange(srcRareCardPool.group);
                    break;
            }

            return list[cardRandomRng.random(list.Count - 1)];
        }

        public static CardRarity rollRarity(Rand rng)
        {
            int roll = cardRng.random(99);
            // roll += cardBlizzRandomizer;
            if (currMapNode == null)
                return getCardRarityFallback(roll);
            return room.getCardRarity(roll);
        }

        public static CardRarity rollRareOrUncommon(float rareChance)
        {
            if (cardRng.randomBool(rareChance))
                return CardRarity.Rare;
            return CardRarity.Uncommon;
        }

        static CardRarity getCardRarityFallback(int roll)
        {
            int rareRate = 3;
            if (roll < rareRate)
                return CardRarity.Rare;
            if (roll < 40)
                return CardRarity.Uncommon;

            return CardRarity.Common;
        }

        public static CardRarity rollRarity()
        {
            return rollRarity(cardRng);
        }

        public static CardGroup getEachRare()
        {
            var everyRareCard = new TempCards();
            foreach (var c in rareCardPool.group)
                everyRareCard.addToBottom(c.makeCopy());
            return everyRareCard;
        }

        public static List<ACard> getColorlessRewardCards()
        {
            List<ACard> retVal = new();
            int numCards = 3;
            foreach (ARelic r in player.relics)
                numCards = r.changeNumberOfCardsInReward(numCards);

            if (ModHelper.isModEnabled("Binary"))
                numCards--;

            for (int i = 0; i < numCards; i++)
            {
                CardRarity rarity = rollRareOrUncommon(colorlessRareChance);
                ACard card = null;
                switch (rarity)
                {
                    case CardRarity.Uncommon:
                        card = getColorlessCardFromPool(rarity);
                        // cardBlizzRandomizer = cardBlizzStartOffset;
                        break;
                    case CardRarity.Rare:
                        card = getColorlessCardFromPool(rarity);
                        break;
                    default:
                        log("WTF?");
                        break;
                }

                while (retVal.Contains(card))
                {
                    if (card != null)
                        log("DUPE: " + card.originalName);
                    card = getColorlessCardFromPool(rarity);
                }

                if (card != null)
                    retVal.Add(card);
            }

            List<ACard> retVal2 = new();
            foreach (ACard c in retVal)
                retVal2.Add(c.makeCopy());
            return retVal2;
        }

        public static List<ACard> getRewardCards()
        {
            List<ACard> retVal = new();
            int numCards = 3;
            foreach (ARelic r in player.relics)
                numCards = r.changeNumberOfCardsInReward(numCards);

            if (ModHelper.isModEnabled("Binary"))
                numCards--;

            for (int i = 0; i < numCards; i++)
            {
                CardRarity rarity = rollRarity();
                ACard card = null;
                switch (rarity)
                {
                    case CardRarity.Common:
                        // cardBlizzRandomizer = cardBlizzStartOffset;
                        break;
                    case CardRarity.Uncommon:
                        break;
                    case CardRarity.Rare:
                        // cardBlizzRandomizer -= cardBlizzGrowth;
                        // if (cardBlizzRandomizer <= cardBlizzMaxOffset)
                        // cardBlizzRandomizer = cardBlizzMaxOffset;
                        break;
                    default:
                        log("WTF?");
                        break;
                }

                bool containsDupe = true;
                while (containsDupe)
                {
                    containsDupe = false;
                    if (player.hasRelic("PrismaticShard"))
                    {
                        card = CardLibrary.getAnyColorCard(rarity);
                    }
                    else
                    {
                        card = getCard(rarity);
                    }

                    foreach (ACard c in retVal)
                    {
                        if (c.cardID == card.cardID)
                        {
                            containsDupe = true;
                            break;
                        }
                    }
                }

                if (card != null)
                    retVal.Add(card);
            }

            List<ACard> retVal2 = new();
            foreach (ACard c in retVal)
                retVal2.Add(c.makeCopy());
            foreach (ACard c in retVal2)
            {
                foreach (ARelic r in player.relics)
                    r.onPreviewObtainCard(c);
            }

            return retVal2;
        }

        public static ACard getCard(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Rare:
                    return rareCardPool.getRandomCard(true);
                case CardRarity.Uncommon:
                    return uncommonCardPool.getRandomCard(true);
                case CardRarity.Common:
                    return commonCardPool.getRandomCard(true);
                case CardRarity.Curse:
                    return curseCardPool.getRandomCard(true);
            }

            log("No rarity on getCard in Abstract Dungeon");
            return null;
        }

        public static ACard getCard(CardRarity rarity, Rand rng)
        {
            switch (rarity)
            {
                case CardRarity.Rare:
                    return rareCardPool.getRandomCard(rng);
                case CardRarity.Uncommon:
                    return uncommonCardPool.getRandomCard(rng);
                case CardRarity.Common:
                    return commonCardPool.getRandomCard(rng);
                case CardRarity.Curse:
                    return curseCardPool.getRandomCard(rng);
            }

            log("No rarity on getCard in Abstract Dungeon");
            return null;
        }

        public static ACard getCardWithoutRng(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Rare:
                    return rareCardPool.getRandomCard(false);
                case CardRarity.Uncommon:
                    return uncommonCardPool.getRandomCard(false);
                case CardRarity.Common:
                    return commonCardPool.getRandomCard(false);
                case CardRarity.Curse:
                    return returnRandomCurse();
            }

            log("Check getCardWithoutRng");
            return null;
        }

        public static ACard getCardFromPool(CardRarity rarity, CardType type, bool useRng)
        {
            ACard retVal;
            switch (rarity)
            {
                case CardRarity.Rare:
                    retVal = rareCardPool.getRandomCard(type, useRng);
                    if (retVal != null)
                        return retVal;
                    log("ERROR: Could not find Rare card of type: " + type);
                    break;
                case CardRarity.Uncommon:
                    retVal = uncommonCardPool.getRandomCard(type, useRng);
                    if (retVal != null)
                        return retVal;
                    if (type == CardType.Power)
                        return getCardFromPool(CardRarity.Rare, type, useRng);
                    log("ERROR: Could not find Uncommon card of type: " + type);
                    break;
                case CardRarity.Common:
                    retVal = commonCardPool.getRandomCard(type, useRng);
                    if (retVal != null)
                        return retVal;
                    if (type == CardType.Power)
                        return getCardFromPool(CardRarity.Uncommon, type, useRng);
                    log("ERROR: Could not find Common card of type: " + type);
                    break;
                case CardRarity.Curse:
                    retVal = curseCardPool.getRandomCard(type, useRng);
                    if (retVal != null)
                        return retVal;
                    log("ERROR: Could not find Curse card of type: " + type);
                    break;
            }

            log("ERROR: Default in getCardFromPool");
            return null;
        }

        public static ACard getColorlessCardFromPool(CardRarity rarity)
        {
            ACard retVal;
            switch (rarity)
            {
                case CardRarity.Uncommon:
                    retVal = colorlessCardPool.getRandomCard(true, rarity);
                    if (retVal != null)
                        return retVal;
                    break;
                case CardRarity.Special:
                    retVal = colorlessCardPool.getRandomCard(true, rarity);
                    if (retVal != null)
                        return retVal;
                    break;
            }

            log("ERROR: getColorlessCardFromPool");
            return null;
        }

        public static ACard returnRandomCurse()
        {
            ACard c = CardLibrary.getCurse().makeCopy();
            UnlockTracker.markCardAsSeen(c.cardID);
            return c;
        }
    }
}