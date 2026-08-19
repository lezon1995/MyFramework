using System;

namespace MoreMountains;

public class OpenRewardChoosePanelAction : AGameAction
{
    static Action onChoose = OnChoose;
    Action onHideEnd;
    // RewardChoosePanel panel;

    public override void onCtor()
    {
        onHideEnd = OnHideEnd;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        // onChoose = null;
        // onHideEnd = null;
        // if (panel)
        // {
        //     LT.HIDE<RewardChoosePanel>();
        //     panel = null;
        // }
    }

    public override void update(float dt)
    {
        // if (panel == null && !isDone)
        // {
        //     panel = LT.LOAD<RewardChoosePanel>();
        //     panel.setOnChoose(onChoose);
        //     panel.setOnHideEnd(onHideEnd);
        //     using var _ = new ListScope<ARelic>(out var list);
        //     RelicLibrary.commonList.randomTake(3, ref list, (min, max) => ADungeon.relicRng.random(min, max - 1));
        //     var relicId1 = list[0].relicId;
        //     var relicId2 = list[1].relicId;
        //     var relicId3 = list[2].relicId;
        //     panel.with(relicId1, relicId2, relicId3);
        // }
    }

    static void OnChoose()
    {
        // LT.HIDE<RewardChoosePanel>();
    }

    void OnHideEnd()
    {
        // panel = null;
        isDone = true;
    }
}