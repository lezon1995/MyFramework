using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoreMountains
{
    [Serializable]
    public partial class CardGroup
    {
        public enum Type
        {
            DRAW_PILE,
            MASTER_DECK,
            HAND,
            DISCARD_PILE,
            EXHAUST_PILE,
            CARD_POOL,
            UNSPECIFIED
        }

        static StringBuilder sb = new();

        public List<ACard> group = new();

        public ACard this[int index] => group[index];

        public Type type;

        public CardGroup(Type _type)
        {
            type = _type;
        }

        public CardGroup(CardGroup g, Type type) : this(type)
        {
            group.AddRange(g.group.Select(card => card.makeSameInstanceOf()));
        }

        public ACard get(int index)
        {
            if (index < 0 || index >= group.Count)
                return null;

            return group[index];
        }

        public bool get(int index, out ACard card)
        {
            if (index < 0 || index >= group.Count)
            {
                card = null;
                return false;
            }

            card = group[index];
            return true;
        }

        public List<string> getCardNames() => group.Select(card => card.cardID).ToList();
        public List<string> getCardIdsForMetrics() => group.Select(card => card.getMetricID()).ToList();

        public void clear()
        {
            group.Clear();
        }

        public bool contains(ACard c) => group.Contains(c);
        public bool isEmpty() => group.Count == 0;
        public int size() => group.Count;
        public int Count => group.Count;

        public virtual void preBattlePrep()
        {
        }

        #region Remove

        protected virtual void OnRemove(ACard c)
        {
        }

        public virtual void removeCard(ACard c)
        {
            if (group.Remove(c))
            {
                OnRemove(c);
            }
        }

        public virtual bool removeCardAt(int index)
        {
            if (get(index, out var card))
            {
                group.RemoveAt(index);
                OnRemove(card);
                return true;
            }

            return false;
        }

        public virtual bool removeCardAt(int index, out ACard card)
        {
            if (get(index, out card))
            {
                group.RemoveAt(index);
                OnRemove(card);
                return true;
            }

            return false;
        }

        #endregion

        #region Add

        protected virtual void OnAdd(ACard c)
        {
        }

        public virtual void addToTop(ACard c)
        {
            group.Add(c);
            OnAdd(c);
        }

        public virtual void addToBottom(ACard c)
        {
            group.Insert(0, c);
            OnAdd(c);
        }

        public virtual void addToRandomSpot(ACard c)
        {
            if (group.Count == 0)
                group.Add(c);
            else
                group.Insert(ADungeon.cardRandomRng.random(group.Count - 1), c);

            OnAdd(c);
        }

        #endregion

        #region Shuffle

        public void shuffle() => group.shuffle(new Random(ADungeon.shuffleRng.randomInt()));
        public void shuffle(Rand rng) => group.shuffle(new Random(rng.randomInt()));

        #endregion

        protected void resetCardBeforeMoving(ACard c)
        {
            actionManager.removeFromQueue(c);
            if (group.Remove(c))
            {
                OnRemove(c);
            }
        }

        void discardAll(CardGroup discardPile)
        {
            foreach (var c in group)
            {
                discardPile.addToTop(c);
            }

            group.Clear();
        }

        public override string ToString()
        {
            sb.Length = 0;
            foreach (var c in group)
            {
                sb.Append($"[{c.cardID}]");
                sb.Append("\n");
            }

            return sb.ToString();
        }
    }


    public class PoolCards : CardGroup
    {
        public PoolCards() : base(Type.CARD_POOL)
        {
        }

        public PoolCards(CardGroup g) : base(g, Type.CARD_POOL)
        {
        }

        public bool removeCard(string id)
        {
            for (var i = group.Count - 1; i >= 0; i--)
            {
                var c = group[i];
                if (c.cardID == id)
                {
                    group.RemoveAt(i);
                    OnRemove(c);
                    return true;
                }
            }

            return false;
        }
    }

    public class TempCards : CardGroup
    {
        public TempCards() : base(Type.UNSPECIFIED)
        {
        }

        public TempCards(CardGroup g) : base(g, Type.UNSPECIFIED)
        {
        }

        public void removeTopCard()
        {
            if (Count == 0)
                return;

            var c = group[^1];
            group.RemoveAt(group.Count - 1);
            OnRemove(c);
        }
    }
}