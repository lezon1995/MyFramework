using System.Collections.Generic;
using System.Text;

namespace MarbleHero
{
    public class CharSelectInfo
    {
        public string name;
        public string flavorText;
        public string hp;
        public int gold;
        public int currentHp;
        public int maxHp;
        public int cardDraw;
        public int floorNum;
        public string levelName;
        public long saveDate;
        public APlayer player;
        public string deckString;
        public List<string> relics;
        public List<string> deck;
        public bool resumeGame;
        public bool isHardMode;

        public CharSelectInfo(string _name, string _flavorText, int _currentHp, int _maxHp, int _gold, int _cardDraw, APlayer _player, List<string> _relics, List<string> _deck, bool _resumeGame)
        {
            name = _name;
            flavorText = _flavorText;
            currentHp = _currentHp;
            maxHp = _maxHp;
            hp = _currentHp + "/" + _maxHp;
            gold = _gold;
            cardDraw = _cardDraw;
            relics = _relics;
            deck = _deck;
            player = _player;
            resumeGame = _resumeGame;
            if (!_resumeGame)
                setDeck();
        }

        public CharSelectInfo(string _name, string fText, int _currentHp, int _maxHp, int _gold, int _cardDraw, APlayer _player, List<string> _relics, List<string> _deck, long _saveDate, int _floorNum, string _levelName, bool _isHardMode)
            : this(_name, fText, _currentHp, _maxHp, _gold, _cardDraw, _player, _relics, _deck, true)
        {
            isHardMode = _isHardMode;
            saveDate = _saveDate;
            floorNum = _floorNum;
            levelName = _levelName;
        }

        void setDeck()
        {
            var startingDeck = player.getStartingDeck();
            deckString = createDeckInfoString(startingDeck);
        }

        public static string createDeckInfoString(List<string> deck)
        {
            using var __ = new DicScope<string, int>(out var cards);
            foreach (string s in deck)
            {
                if (!cards.ContainsKey(s))
                {
                    cards.Add(s, 1);
                    continue;
                }

                cards[s] += 1;
            }

            using var _ = new MyStringBuilderScope(out var sb);
            foreach (var c in cards)
            {
                sb.add("#b").add(c.Value).add(" ").add(c.Key);
                if (c.Value > 1)
                    sb.add("s");
                sb.add(", ");
            }

            string retVal = sb.ToString();
            if (retVal.isEmpty())
                return string.Empty;
            
            if (retVal.Length > 80)
                return "Click the deck icon to view starting cards.";

            return retVal.Substring(0, retVal.Length - 2);
        }
    }
}