using UnityEngine;

namespace MarbleHero
{
    public class DevInputActionSet
    {
        public static InputAction toggleDebug = new(KeyCode.RightBracket);
        public static InputAction toggleInfo = new(KeyCode.LeftBracket);
        public static InputAction uploadData = new(KeyCode.BackQuote);
        public static InputAction toggleCursor = new(KeyCode.C);
        public static InputAction toggleVersion = new(KeyCode.V);
        public static InputAction hideTopBar = new(KeyCode.Keypad1);
        public static InputAction hidePopUps = new(KeyCode.Keypad2);
        public static InputAction hideRelics = new(KeyCode.Keypad3);
        public static InputAction hideCombatLowUI = new(KeyCode.Keypad4);
        public static InputAction hideCards = new(KeyCode.Keypad5);
        public static InputAction hideEndTurnButton = new(KeyCode.Keypad6);
        public static InputAction hideCombatInfo = new(KeyCode.Keypad7);
        public static InputAction increaseOrbCount = new(37);
        public static InputAction decreaseOrbCount = new(49);
        public static InputAction gainGold = new(KeyCode.G);
        public static InputAction drawCard = new(KeyCode.Space);
        public static InputAction deleteSteamCloud = new(KeyCode.Space);
    }
}