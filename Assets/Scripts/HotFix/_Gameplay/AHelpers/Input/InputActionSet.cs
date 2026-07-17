using UnityEngine;

namespace MoreMountains
{
    public class InputActionSet
    {
        public static Prefs prefs = SaveHelper.getPrefs("InputSettings");
        public static InputAction confirm;
        public static InputAction cancel;
        public static InputAction topPanel;
        public static InputAction proceed;
        public static InputAction settings;
        public static InputAction map;
        public static InputAction masterDeck;
        public static InputAction drawPile;
        public static InputAction discardPile;
        public static InputAction exhaustPile;
        public static InputAction endTurn;
        public static InputAction peek;
        public static InputAction up;
        public static InputAction down;
        public static InputAction left;
        public static InputAction right;
        public static InputAction releaseCard;
        public static InputAction selectCard_1;
        public static InputAction selectCard_2;
        public static InputAction selectCard_3;
        public static InputAction selectCard_4;
        public static InputAction selectCard_5;
        public static InputAction selectCard_6;
        public static InputAction selectCard_7;
        public static InputAction selectCard_8;
        public static InputAction selectCard_9;
        public static InputAction selectCard_10;
        public static InputAction[] selectCardActions;
        
        const string CONFIRM_KEY = "CONFIRM";
        const string CANCEL_KEY = "CANCEL";
        const string MAP_KEY = "MAP";
        const string DECK_KEY = "DECK";
        const string DRAW_PILE_KEY = "DRAW_PILE";
        const string DISCARD_PILE_KEY = "DISCARD_PILE";
        const string EXHAUST_PILE_KEY = "EXHAUST_PILE";
        const string END_TURN_KEY = "END_TURN";
        const string PEEK_KEY = "PEEK";
        const string UP_KEY = "UP";
        const string DOWN_KEY = "DOWN";
        const string LEFT_KEY = "LEFT";
        const string RIGHT_KEY = "RIGHT";
        const string DROP_CARD = "DROP_CARD";
        const string CARD_1_KEY = "CARD_1";
        const string CARD_2_KEY = "CARD_2";
        const string CARD_3_KEY = "CARD_3";
        const string CARD_4_KEY = "CARD_4";
        const string CARD_5_KEY = "CARD_5";
        const string CARD_6_KEY = "CARD_6";
        const string CARD_7_KEY = "CARD_7";
        const string CARD_8_KEY = "CARD_8";
        const string CARD_9_KEY = "CARD_9";
        const string CARD_10_KEY = "CARD_10";

        public static void load()
        {
            confirm = new(prefs.getInteger(CONFIRM_KEY, (int)KeyCode.KeypadEnter));
            cancel = new(prefs.getInteger(CANCEL_KEY, (int)KeyCode.Escape));
            map = new(prefs.getInteger(MAP_KEY, (int)KeyCode.M));
            masterDeck = new(prefs.getInteger(DECK_KEY, (int)KeyCode.D));
            drawPile = new(prefs.getInteger(DRAW_PILE_KEY, (int)KeyCode.A));
            discardPile = new(prefs.getInteger(DISCARD_PILE_KEY, (int)KeyCode.S));
            exhaustPile = new(prefs.getInteger(EXHAUST_PILE_KEY, (int)KeyCode.X));
            endTurn = new(prefs.getInteger(END_TURN_KEY, (int)KeyCode.E));
            peek = new(prefs.getInteger(PEEK_KEY, (int)KeyCode.Space));
            up = new(prefs.getInteger(UP_KEY, (int)KeyCode.UpArrow));
            down = new(prefs.getInteger(DOWN_KEY, (int)KeyCode.DownArrow));
            left = new(prefs.getInteger(LEFT_KEY, (int)KeyCode.LeftArrow));
            right = new(prefs.getInteger(RIGHT_KEY, (int)KeyCode.RightArrow));
            releaseCard = new(prefs.getInteger(DROP_CARD, (int)KeyCode.DownArrow));
            selectCard_1 = new(prefs.getInteger(CARD_1_KEY, (int)KeyCode.Alpha1));
            selectCard_2 = new(prefs.getInteger(CARD_2_KEY, (int)KeyCode.Alpha2));
            selectCard_3 = new(prefs.getInteger(CARD_3_KEY, (int)KeyCode.Alpha3));
            selectCard_4 = new(prefs.getInteger(CARD_4_KEY, (int)KeyCode.Alpha4));
            selectCard_5 = new(prefs.getInteger(CARD_5_KEY, (int)KeyCode.Alpha5));
            selectCard_6 = new(prefs.getInteger(CARD_6_KEY, (int)KeyCode.Alpha6));
            selectCard_7 = new(prefs.getInteger(CARD_7_KEY, (int)KeyCode.Alpha7));
            selectCard_8 = new(prefs.getInteger(CARD_8_KEY, (int)KeyCode.Alpha8));
            selectCard_9 = new(prefs.getInteger(CARD_9_KEY, (int)KeyCode.Alpha9));
            selectCard_10 = new(prefs.getInteger(CARD_10_KEY, (int)KeyCode.Alpha0));

            selectCardActions = new[]
            {
                selectCard_1,
                selectCard_2,
                selectCard_3,
                selectCard_4,
                selectCard_5,
                selectCard_6,
                selectCard_7,
                selectCard_8,
                selectCard_9,
                selectCard_10
            };
        }

        public static void save()
        {
            prefs.putInteger(CONFIRM_KEY, confirm.getKey());
            prefs.putInteger(CANCEL_KEY, cancel.getKey());
            prefs.putInteger(MAP_KEY, map.getKey());
            prefs.putInteger(DECK_KEY, masterDeck.getKey());
            prefs.putInteger(DRAW_PILE_KEY, drawPile.getKey());
            prefs.putInteger(DISCARD_PILE_KEY, discardPile.getKey());
            prefs.putInteger(EXHAUST_PILE_KEY, exhaustPile.getKey());
            prefs.putInteger(END_TURN_KEY, endTurn.getKey());
            prefs.putInteger(PEEK_KEY, peek.getKey());
            prefs.putInteger(UP_KEY, up.getKey());
            prefs.putInteger(DOWN_KEY, down.getKey());
            prefs.putInteger(LEFT_KEY, left.getKey());
            prefs.putInteger(RIGHT_KEY, right.getKey());
            prefs.putInteger(DROP_CARD, releaseCard.getKey());
            prefs.putInteger(CARD_1_KEY, selectCard_1.getKey());
            prefs.putInteger(CARD_2_KEY, selectCard_2.getKey());
            prefs.putInteger(CARD_3_KEY, selectCard_3.getKey());
            prefs.putInteger(CARD_4_KEY, selectCard_4.getKey());
            prefs.putInteger(CARD_5_KEY, selectCard_5.getKey());
            prefs.putInteger(CARD_6_KEY, selectCard_6.getKey());
            prefs.putInteger(CARD_7_KEY, selectCard_7.getKey());
            prefs.putInteger(CARD_8_KEY, selectCard_8.getKey());
            prefs.putInteger(CARD_9_KEY, selectCard_9.getKey());
            prefs.putInteger(CARD_10_KEY, selectCard_10.getKey());
            prefs.flush();
        }

        public static void resetToDefaults()
        {
            confirm.remap(KeyCode.KeypadEnter);
            cancel.remap(KeyCode.Escape);
            map.remap(KeyCode.M);
            masterDeck.remap(KeyCode.D);
            drawPile.remap(KeyCode.A);
            discardPile.remap(KeyCode.S);
            exhaustPile.remap(KeyCode.X);
            endTurn.remap(KeyCode.E);
            peek.remap(KeyCode.Space);
            up.remap(KeyCode.UpArrow);
            down.remap(KeyCode.DownArrow);
            left.remap(KeyCode.LeftArrow);
            right.remap(KeyCode.RightArrow);

            releaseCard.remap(KeyCode.DownArrow);
            selectCard_1.remap(KeyCode.Alpha1);
            selectCard_2.remap(KeyCode.Alpha2);
            selectCard_3.remap(KeyCode.Alpha3);
            selectCard_4.remap(KeyCode.Alpha4);
            selectCard_5.remap(KeyCode.Alpha5);
            selectCard_6.remap(KeyCode.Alpha6);
            selectCard_7.remap(KeyCode.Alpha7);
            selectCard_8.remap(KeyCode.Alpha8);
            selectCard_9.remap(KeyCode.Alpha9);
            selectCard_10.remap(KeyCode.Alpha0);
        }
    }
}