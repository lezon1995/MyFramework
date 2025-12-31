using System;
using System.Collections.Generic;

namespace MarbleHero
{
    [Serializable]
    public partial class ACard : IComparable<ACard>
    {
        const int COMMON_CARD_PRICE = 50;
        const int UNCOMMON_CARD_PRICE = 75;
        const int RARE_CARD_PRICE = 150;

        public int instanceId;
        public string cardID;
        public string uuid;
        public string originalName;
        public string name;
        public string rawDescription;
        public CardType type;
        public CardRarity rarity;
        public CardColor color;

        public int price;
        public int misc;

        public string assetUrl;
        public string cantUseMessage;

        public List<string> keywords = new();

        public bool isLocked;
        public bool isUsed;
        public bool isSeen = true;
        public bool isSelected;
        public bool inBottleFlame;
        public bool inBottleLightning;
        public bool inBottleTornado;

        public ACard cardsToPreview;

        public int CompareTo(ACard other) => string.Compare(cardID, other.cardID, StringComparison.Ordinal);

        protected ACard(string id, string imgUrl, CardType _type, CardRarity _rarity)
        {
            Data = CardData.Get(id);
            var strings = languagePack.getCardStrings(id);
            cardStrings = strings;
            
            cardID = id;
            originalName = name = strings.NAME;
            rawDescription = strings.DESCRIPTION;
            assetUrl = imgUrl;
            type = _type;
            rarity = _rarity;
            createCardImage();
            if (name == null || rawDescription == null)
                logger.Info("Card initialized incorrectly");
            initializeTitle();
            initializeDescription();
            updateTransparency(0);
            uuid = Ulid.NewUlid().ToString();
            instanceId = ++ADungeon.cardInstanceIdGenerator;
        }

        protected ACard(CardData data)
        {
            Data = data;
            instanceId = ++ADungeon.cardInstanceIdGenerator;
            var cardId = data.Id;
            var strings = languagePack.getCardStrings(cardId);
            cardStrings = strings;
            cardID = cardId;
            originalName = name = strings.NAME;
            rawDescription = strings.DESCRIPTION;
            type = data.CardType;
            rarity = data.CardRarity;
            uuid = Ulid.NewUlid().ToString();
        }

        public ACard makeSameInstanceOf()
        {
            ACard card = makeStatEquivalentCopy();
            card.uuid = uuid;
            return card;
        }

        public ACard makeStatEquivalentCopy()
        {
            ACard card = makeCopy();
            card.name = name;
            card.isCostModified = isCostModified;
            card.isCostModifiedForTurn = isCostModifiedForTurn;
            card.inBottleLightning = inBottleLightning;
            card.inBottleFlame = inBottleFlame;
            card.inBottleTornado = inBottleTornado;
            card.isSeen = isSeen;
            card.isLocked = isLocked;
            card.misc = misc;
            return card;
        }

        public void onRemoveFromMasterDeck()
        {
        }

        public virtual void tookDamage()
        {
        }

        public virtual void didDiscard()
        {
        }


        public void resetAttributes()
        {
            isCostModifiedForTurn = false;
        }


        public virtual void triggerOnEndOfPlayerTurn()
        {
        }

        public virtual void triggerOnEndOfTurnForPlayingCard()
        {
        }

        public virtual void triggerOnOtherCardPlayed(ACard c)
        {
        }

        public virtual void triggerOnGainEnergy(int e, bool dueToCard)
        {
        }

        public virtual void triggerOnManualDiscard()
        {
        }

        public virtual void triggerAtStartOfTurn()
        {
        }

        public virtual void atTurnStartPreDraw()
        {
        }

        public void onChoseThisOption()
        {
        }

        public void clearPowers()
        {
            resetAttributes();
        }

        protected void addToBot(AGameAction action) => actionManager.addToBot(action);
        protected void addToTop(AGameAction action) => actionManager.addToTop(action);

        public override string ToString() => name;

        public Dictionary<string, object> getLocStrings()
        {
            // initializeDescription();
            Dictionary<string, object> cardData = new()
            {
                { "name", name },
                { "description", rawDescription }
            };
            return cardData;
        }

        public string getMetricID()
        {
            string id = cardID;
            return id;
        }

        public virtual void triggerOnGlowCheck()
        {
        }

        public virtual void use()
        {
        }

        public virtual ACard makeCopy() => null;

        public static implicit operator bool(ACard self) => self != null;
    }
}