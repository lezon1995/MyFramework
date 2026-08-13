using System;
using PrimeTween;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/RewardChoosePanel.prefab
// 
public partial class RewardChooseItem : WindowRecyclableUGUI
// auto generate classname end
{
    // auto generate member start
    protected myUGUIButton btn;
    protected myUGUIObject hovered;
    protected myUGUIObject itemBorder;
    protected myUGUIObject itemBg;
    protected myUGUIObject iconBg;
    protected myUGUIImageSimple itemIcon;
    protected myUGUITextTMP itemDesc;
    protected myUGUITextTMP itemName;
    // auto generate member end

    string relicId;
    Action onChoose;

    public override void recycle()
    {
        base.recycle();
        relicId = null;
        onChoose = null;
    }

    public RewardChooseItem(IWindowObjectOwner parent) : base(parent)
    {
        // auto generate constructor start
        // auto generate constructor end
    }

    protected override void assignWindowInternal()
    {
        // auto generate assignWindowInternal start
        newObject(out btn, "Btn");
        newObject(out hovered, "Btn/Hovered");
        newObject(out itemBorder, "Btn/Border");
        newObject(out itemBg, "Btn/Bg");
        newObject(out iconBg, "Btn/Icon/IconBg");
        newObject(out itemIcon, "Btn/Icon/Image");
        newObject(out itemDesc, "Btn/Desc/TextDesc");
        newObject(out itemName, "Btn/Name/TextName");
        // auto generate assignWindowInternal end
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end

        btn.setUGUIButtonClick(onClick);
        btn.setUGUIMouseEnter((pointer, go) => Tween.Scale(go.transform, endValue: 1.2F, duration: 0.1F, ease: Ease.OutCubic, useUnscaledTime: true));
        btn.setUGUIMouseExit((pointer, go) => Tween.Scale(go.transform, endValue: 1F, duration: 0.1F, ease: Ease.OutCubic, useUnscaledTime: true));
    }

    public override void onShow()
    {
        base.onShow();
    }

    public void refresh(string id, Action chooseAction)
    {
        relicId = id;
        onChoose = chooseAction;
        btn.setScale(1);
    }

    void onClick()
    {
        if (btn.getScript().isHide())
            return;

        RelicLibrary.getRelic(relicId).makeCopy().instantObtain(player, player.relics.Count, true);
        onChoose?.Invoke();
    }
}