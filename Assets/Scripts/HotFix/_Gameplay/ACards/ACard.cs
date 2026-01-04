using System;
using System.Collections.Generic;

namespace MarbleHero
{
    [Serializable]
    public partial class ACard : IComparable<ACard>
    {
        public int instanceId;
        public string cardID;
        public string uuid;
        public string originalName;
        public string name;
        public string rawDescription;
        public CardType type;
        public CardRarity rarity;
        public CardColor color;

        public int misc;

        public string assetUrl;
        public bool isLocked;
        public bool isSeen = true;

        public int CompareTo(ACard other) => string.Compare(cardID, other.cardID, StringComparison.Ordinal);

        protected ACard(string id, string imgUrl, CardType _type, CardRarity _rarity)
        {
            cardID = id;
            originalName = name = "";
            rawDescription = "";
            assetUrl = imgUrl;
            type = _type;
            rarity = _rarity;
            // uuid = Ulid.NewUlid().ToString();
            instanceId = ++ADungeon.cardInstanceIdGenerator;
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
        
        public void unlock()
        {
            isLocked = false;
            // portrait = cardAtlas.findRegion(assetUrl);
            // if (portrait == null)
            // portrait = oldCardAtlas.findRegion(assetUrl);
        }

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