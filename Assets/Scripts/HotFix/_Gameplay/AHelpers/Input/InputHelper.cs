using UnityEngine;

namespace MarbleHero
{
    public class InputHelper
    {
        public static float mX;
        public static float mY;
        public static bool isMouseDown;
        public static bool isMouseDown_R;
        public static bool isPrevMouseDown;
        public static bool isPrevMouseDown_R;
        public static bool justClickedLeft;
        public static bool justClickedRight;
        public static bool touchDown;
        public static bool touchUp;
        public static bool justReleasedClickLeft, justReleasedClickRight;
        public static bool scrolledUp, scrolledDown;
        public static bool pressedEscape;

        static bool ignoreOneCycle;

        public static void initialize()
        {
            // processor = new ScrollInputProcessor();
            // Gdx.input.setInputProcessor(processor);
            log("Setting input processor to Scroller");
            InputActionSet.load();
        }

        public static void regainInputFocus()
        {
            // Gdx.input.setInputProcessor(processor);
            ignoreOneCycle = true;
        }

        public static void updateFirst()
        {
            if (ignoreOneCycle)
            {
                ignoreOneCycle = false;
                return;
            }

            var p = getMousePosition();
            if (!Settings.isTouchScreen)
            {
                mX = p.x;

                if (mX > Settings.WIDTH)
                    mX = Settings.WIDTH;
                else if (mX < 0)
                    mX = 0;

                mY = Settings.HEIGHT - p.y;

                if (mY > Settings.HEIGHT)
                    mY = Settings.HEIGHT;
                else if (mY < 1)
                    mY = 1;
            }
            else
            {
                mX = p.x + Settings.VERT_LETTERBOX_AMT;
                mY = Settings.HEIGHT - p.y + Settings.HORIZ_LETTERBOX_AMT;
                if (mY < 1)
                    mY = 1;
            }


            isMouseDown = Input.GetMouseButton(0);
            isMouseDown_R = Input.GetMouseButton(1);

            var delta = getMouseDelta();
            // if (delta.x != 0 && player != null && player.isInKeyboardMode)
            // GameCursor.hidden = false;

            if ((!isPrevMouseDown && isMouseDown) || touchDown)
            {
                // Game.cursor.color.a = 0.7F;
                touchDown = false;
                justClickedLeft = true;
                if (Settings.isControllerMode)
                    leaveControllerMode();

                if (Settings.isDebug)
                    log("Clicked: (" + mX + "," + mY + ")");
            }
            else if ((isPrevMouseDown && !isMouseDown) || touchUp)
            {
                touchUp = false;
                justReleasedClickLeft = true;
            }

            if (!isPrevMouseDown_R && isMouseDown_R)
            {
                justClickedRight = true;
                if (Settings.isControllerMode)
                    leaveControllerMode();
            }
            else if (isPrevMouseDown_R && !isMouseDown_R)
            {
                justReleasedClickRight = true;
            }

            pressedEscape = InputActionSet.cancel.isJustPressed();
            isPrevMouseDown_R = isMouseDown_R;
            isPrevMouseDown = isMouseDown;
        }

        static void leaveControllerMode()
        {
            if (Settings.isConsoleBuild)
            {
                log("ENTERING TOUCH SCREEN MODE");
                Settings.isTouchScreen = true;
            }
            else
            {
                log("LEAVING CONTROLLER MODE");
                Settings.isTouchScreen = Settings.TOUCHSCREEN_ENABLED;
            }

            Settings.isControllerMode = false;
            // GameCursor.hidden = false;
            if (player != null && _dungeon != null)
            {
                player.viewingRelics = false;
                // ADungeon.topPanel.selectPotionMode = false;
                // player.releaseCard();
            }
        }

        public static void updateLast()
        {
            justClickedLeft = false;
            justClickedRight = false;
            justReleasedClickLeft = false;
            justReleasedClickRight = false;
            scrolledUp = false;
            scrolledDown = false;
        }

        public static ACard getCardSelectedByHotkey(CardGroup cards)
        {
            var actions = InputActionSet.selectCardActions;
            for (int i = 0; i < actions.Length && i < cards.size(); i++)
            {
                if (actions[i].isJustPressed())
                    return cards.group[i];
            }

            return null;
        }

        public static KeyCode[] SHORTCUT_MODIFIER_KEYS =
        {
            KeyCode.LeftControl,
            KeyCode.RightControl /*, KeyCode.SysReq*/
        };

        public static bool isShortcutModifierKeyPressed()
        {
            foreach (var keycode in SHORTCUT_MODIFIER_KEYS)
            {
                if (Input.GetKey(keycode))
                    return true;
            }

            return false;
        }

        public static bool isPasteJustPressed()
        {
            return (isShortcutModifierKeyPressed() && Input.GetKeyDown(KeyCode.V));
        }

        public static bool didMoveMouse()
        {
            var delta = getMouseDelta();
            return delta.x != 0 || delta.y != 0;
        }

        public static void moveCursorToNeutralPosition()
        {
            if (Settings.isTouchScreen && !Settings.isControllerMode)
            {
                // Gdx.input.setCursorPosition(10, Settings.HEIGHT / 2);
                // Game.cursor.color.a = 0.0F;
            }
        }

        public static Vector2 getMousePosition()
        {
            if (Application.isMobilePlatform)
            {
                Vector3 touchPos = Vector2.zero;

                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    touchPos = touch.position;
                }

                return touchPos;
            }

            return Input.mousePosition;
        }

        public static Vector2 getMouseDelta()
        {
            if (Application.isMobilePlatform)
            {
                Vector3 delta = Vector2.zero;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    delta = touch.deltaPosition;
                }

                return delta;
            }

            return Input.mouseScrollDelta;
        }
    }
}