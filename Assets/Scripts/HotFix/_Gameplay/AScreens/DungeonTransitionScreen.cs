using UnityEngine;

namespace MoreMountains
{
    public class DungeonTransitionScreen
    {
        // static UIStrings uiStrings = CardCrawlGame.languagePack.getUIString("DungeonTransitionScreen");
        // public static string[] TEXT = uiStrings.TEXT;
        public bool isComplete;
        public bool msgCreated;
        public bool isFading;
        public float timer;
        public string name;
        public string levelNum;
        public string levelName;
        string source;
        bool playSFX;
        // ConfirmPopup popup = null;
        Color color = Color.white;
        float oscillateTimer;
        float continueFader;
        float animTimer;
        Color continueColor = Color.gold;

        public DungeonTransitionScreen(string key)
        {
            if (!TipTracker.tips.get("NO_FTUE"))
            {
                // popup = new ConfirmPopup(TEXT[0], TEXT[1], ConfirmPopup.ConfirmType.SKIP_FTUE);
                // popup.show();
            }

            source = "";
            name = "";
            timer = 2.0F;
            continueFader = 0.0F;
            oscillateTimer = 0.0F;
            continueColor.a = 0.0F;
            color.a = 0.0F;
            setAreaName(key);
            isComplete = true;
        }

        void setAreaName(string key)
        {
            switch (key)
            {
                case "Exordium":
                    // levelNum = TEXT[2];
                    // levelName = TEXT[3];
                    break;
                case "TheCity":
                    // levelNum = TEXT[4];
                    // levelName = TEXT[5];
                    break;
                case "TheBeyond":
                    // levelNum = TEXT[6];
                    // levelName = TEXT[7];
                    break;
                case "TheEnding":
                    // levelNum = TEXT[8];
                    // levelName = TEXT[9];
                    break;
                default:
                    // levelNum = TEXT[8];
                    // levelName = TEXT[9];
                    break;
            }

            ADungeon.name = levelName;
            ADungeon.levelNum = levelNum;
        }

        void oscillateColor(float dt)
        {
            oscillateTimer += dt * 5.0F;
            continueColor.a = 0.33F + (MathUtils.cos(oscillateTimer) + 1.0F) / 3.0F;
            if (!isFading)
            {
                if (continueFader != 1.0F)
                {
                    continueFader += dt / 2.0F;
                    if (continueFader > 1.0F)
                        continueFader = 1.0F;
                }
            }
            else if (continueFader != 0.0F)
            {
                continueFader -= dt;
                if (continueFader < 0.0F)
                    continueFader = 0.0F;
            }

            continueColor.a *= continueFader;
        }

        public bool update(float dt)
        {
            // if (popup != null && popup.shown)
            // {
            //     popup.update();
            //     return;
            // }

            if (msgCreated)
                oscillateColor(dt);

            if (Settings.isDebug || InputHelper.justClickedLeft)
            {
                InputHelper.justClickedLeft = false;
                isComplete = true;
            }

            if (isFading)
            {
                timer -= dt;
                if (timer < 0.0F)
                {
                    isComplete = true;
                }
                else
                {
                    color.a = timer;
                    return isComplete;
                }
            }

            if (animTimer > 0.5F && !playSFX)
            {
                playSFX = true;
                sound.play("DUNGEON_TRANSITION");
            }

            if (!msgCreated)
            {
                animTimer += dt;
                if (animTimer > 4.0F)
                {
                    msgCreated = true;
                    animTimer = 4.0F;
                }

                if (animTimer > 2.0F)
                {
                    color.a = 1.0F;
                }
                else
                {
                    color.a = animTimer / 2.0F;
                }
            }

            return isComplete;
        }
    }
}