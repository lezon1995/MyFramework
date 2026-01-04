using UnityEngine;

namespace MarbleHero
{
    public class InputAction
    {
        // static UIStrings uiStrings = CardCrawlGame.languagePack.getUIString("InputKeyNames");
        // public static Dictionary<string, string> TEXT_CONVERSIONS = uiStrings.TEXT_DICT;
        KeyCode keycode;

        public InputAction(KeyCode keycode)
        {
            this.keycode = keycode;
        }

        public InputAction(int keycode)
        {
            this.keycode = (KeyCode)keycode;
        }

        public int getKey()
        {
            return (int)keycode;
        }

        public string getKeyString()
        {
            // string keycodeStr = Input.Keys.toString(keycode);
            // return TEXT_CONVERSIONS.getOrDefault(keycodeStr, keycodeStr);
            return null;
        }

        public bool isJustPressed()
        {
            return UnityEngine.Input.GetKeyDown(keycode);
        }

        public bool isPressed()
        {
            return UnityEngine.Input.GetKey(keycode);
        }

        public void remap(KeyCode newKeycode)
        {
            keycode = newKeycode;
        }
    }
}