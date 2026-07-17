using UnityEngine;

namespace MoreMountains
{
    public class InputAction
    {
        // static UIStrings uiStrings = CardCrawlGame.languagePack.getUIString("InputKeyNames");
        // public static Dictionary<string, string> TEXT_CONVERSIONS = uiStrings.TEXT_DICT;
        KeyCode keycode;

        public InputAction(KeyCode code)
        {
            keycode = code;
        }

        public InputAction(int code)
        {
            keycode = (KeyCode)code;
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
            return Input.GetKeyDown(keycode);
        }

        public bool isPressed()
        {
            return Input.GetKey(keycode);
        }

        public void remap(KeyCode newKeycode)
        {
            keycode = newKeycode;
        }
    }
}