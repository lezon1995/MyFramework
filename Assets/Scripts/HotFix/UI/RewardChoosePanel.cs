using System;
using Obfuz;
using PrimeTween;
using static StringUtility;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/RewardChoosePanel.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class RewardChoosePanel : LayoutScript
// auto generate classname end
    , IArgs<string, string, string>
{
    // auto generate member start
	protected WindowStructPool<RewardChooseItem> RewardChooseItemPool;
    // auto generate member end

    Action onChoose;

    public RewardChoosePanel()
    {
        // auto generate constructor start
		RewardChooseItemPool = new(this);
        // auto generate constructor end
    }

    public override void assignWindow()
    {
        // auto generate assignWindow start
		RewardChooseItemPool.assignTemplate(mRoot, "Content/Mid/H/RewardChooseItem");
        // auto generate assignWindow end
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end

        mLayout.setScriptControlShow(true);
        mLayout.setScriptControlHide(true);
    }

    public override void onGameState()
    {
        base.onGameState();
        mRoot.setActive(true);
        mRoot.setScale(0);
        mRoot.setAlpha(0);

        Tween.Alpha(mCanvasGroup, endValue: 1F, duration: 0.5F, ease: Ease.OutCubic);
        Tween.Scale(mTransform, endValue: 1F, duration: 0.5F, ease: Ease.OutCubic)
            .OnComplete(this, script =>
            {
                script.setActive(true);
            });
    }

    public override void destroy()
    {
        base.destroy();
        onChoose = null;
        RewardChooseItemPool.unuseAll();
    }

    public override void onHide()
    {
        base.onHide();
        mRoot.setScale(1);
        mRoot.setAlpha(1);
        Tween.Alpha(mCanvasGroup, endValue: 0F, duration: 0.5F, ease: Ease.OutCubic);
        Tween.Scale(mTransform, endValue: 0F, duration: 0.5F, ease: Ease.OutCubic)
            .OnComplete(this, script =>
            {
                script.setActive(false);
            });
    }

    public override void close()
    {
        base.close();
    }

    public void onCreate(string p1, string p2, string p3)
    {
        RewardChooseItemPool.newItem().refresh(p1, onChoose);
        RewardChooseItemPool.newItem().refresh(p2, onChoose);
        RewardChooseItemPool.newItem().refresh(p3, onChoose);
    }

    public void setOnChoose(Action value)
    {
        onChoose = value;
    }
}