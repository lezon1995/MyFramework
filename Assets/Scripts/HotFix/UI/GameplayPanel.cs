using System.Collections.Generic;
using MoreMountains;
using MoreMountains.Tools;
using Obfuz;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public class GameplayPanel : LayoutScript
{
    protected myUGUIObject mPlayerInfo;

    public PlayerInfo playerInfo;

    // auto generate member end
    public GameplayPanel()
    {
        // auto generate constructor start
        // auto generate constructor end
    }

    public override void assignWindow()
    {
        // auto generate assignWindow start
        // newObject(out myUGUIObject content, "Content", false);
        // newObject(out myUGUIObject top, content, "Top", false);
        // newObject(out mPlayerInfo, top, "PlayerInfo");
        // auto generate assignWindow end
    }

    public override void init()
    {
        base.init();

        // playerInfo = new(mPlayerInfo);
    }

    public override void onGameState()
    {
        base.onGameState();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        playerInfo?.update(elapsedTime);
    }

    public override void destroy()
    {
        base.destroy();

        playerInfo?.Dispose();
        playerInfo = null;
    }

    public class PlayerInfo : UIObject
        , IEvent<OnAddXp>
        , IEvent<OnXpChange>
        , IEvent<OnLevelUp>
        , IEvent<OnXpRequiredChange>
        , IEvent<OnTurnChanged>
    {
        Text phase, turn, level, curXp, maxXp;
        Image expBar;
        Queue<OnLevelUp> levelUpQueue = new();
        OnAddXp? addXp;
        OnXpChange? xpChange;
        int lastXpValue, toXpValue;
        bool addXpTweenFinished;

        float tweenExpTimer;
        const float tweenExpTime = 0.5F;

        public PlayerInfo(myUGUIObject t) : base(t)
        {
            find("TextPhase", out phase);
            find("TextTurn", out turn);
            find("TextLevel", out level);
            find("ImgExpBar", out expBar);
            find("TextCurExp", out curXp);
            find("TextMaxExp", out maxXp);

            this.addListener<OnAddXp>();
            this.addListener<OnXpChange>();
            this.addListener<OnLevelUp>();
            this.addListener<OnXpRequiredChange>();
            this.addListener<OnTurnChanged>();
        }

        public override void Dispose()
        {
            base.Dispose();

            this.removeListener<OnAddXp>();
            this.removeListener<OnXpChange>();
            this.removeListener<OnLevelUp>();
            this.removeListener<OnXpRequiredChange>();
            this.removeListener<OnTurnChanged>();
        }

        public override void update(float elapsedTime)
        {
            base.update(elapsedTime);

            if (addXp != null)
            {
                var e = addXp.Value;
                refreshCurXp(e.Xp);
                refreshExpBar(e.Ratio);

                addXp = null;
                xpChange = null;
            }
            else if (xpChange != null)
            {
                var e = xpChange.Value;
                refreshCurXp((int)e.Xp);
                refreshExpBar(e.Ratio);

                xpChange = null;
            }

            if (addXpTweenFinished)
            {
                addXpTweenFinished = false;
                if (levelUpQueue.TryDequeue(out var e))
                {
                    expBar.fillAmount = 0;
                    refreshLevel(e.Level);
                    refreshCurXp(e.Xp);
                    refreshExpBar(e.Ratio);
                    lastXpValue = 0;
                }
            }

            if (tweenExpTimer > 0)
            {
                tweenExpTimer = (tweenExpTimer - elapsedTime).clampMin();
                var t = (tweenExpTime - tweenExpTimer) / tweenExpTime;
                var curve = mKeyFrameManager.getKeyFrame(KEY_CURVE.CUBIC_OUT);
                var f = curve.evaluate(t);
                var curXpValue = (int)lerpSimple(lastXpValue, toXpValue, f);
                curXp.text = curXpValue.ToString();

                if (tweenExpTimer <= 0)
                {
                    curXp.transform.localScale = Vector3.one * 2F;
                    Tween.Scale(curXp.transform, endValue: 1, duration: 0.5F, ease: Ease.OutCubic);
                    curXp.text = toXpValue.ToString();
                }
            }
        }

        public void refreshTurn(int v)
        {
            turn.transform.localScale = Vector3.one * 2F;
            Tween.Scale(turn.transform, endValue: 1, duration: 0.5F, ease: Ease.OutCubic);
            turn.text = v.ToString();
        }

        public void refreshLevel(int v)
        {
            level.transform.localScale = Vector3.one * 2F;
            Tween.Scale(level.transform, endValue: 1, duration: 0.5F, ease: Ease.OutCubic);
            level.text = v.ToString();
        }

        public void refreshExpBar(float v)
        {
            Tween
                .UIFillAmount(expBar, endValue: v, duration: 0.5F, ease: Ease.OutCubic)
                .OnComplete(this, info =>
                {
                    info.addXpTweenFinished = true;
                });
        }

        void refreshCurXp(int v)
        {
            tweenExpTimer = tweenExpTime;
            lastXpValue = toXpValue;
            toXpValue = v;
            // Tween.Custom(startValue: lastXpValue, endValue: v, duration: 0.5F, ease: Ease.OutCubic, onValueChange: f =>
            // {
            //     curXp.text = f.ToString("F0");
            // });
            // lastXpValue = v;
        }

        public void onEvent(OnAddXp e)
        {
            addXp = e;
        }

        public void onEvent(OnXpChange e)
        {
            xpChange = e;
        }

        public void onEvent(OnLevelUp e)
        {
            levelUpQueue.Enqueue(e);
        }

        public void onEvent(OnTurnChanged e)
        {
            refreshTurn(e.turn);
        }

        public void onEvent(OnXpRequiredChange e)
        {
            maxXp.text = e.Xp.ToString();
        }
    }
}