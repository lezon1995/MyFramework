using System;

namespace MarbleHero;

public class OpenRewardChoosePanelAction : AGameAction
{
    static Action onChoose = OnChoose;
    Action onHideEnd;
    RewardChoosePanel panel;

    public override void onCtor()
    {
        onHideEnd = OnHideEnd;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        // onChoose = null;
        // onHideEnd = null;
        if (panel)
        {
            LT.HIDE<RewardChoosePanel>();
            panel = null;
        }
    }

    public override void update(float dt)
    {
        if (panel == null && !isDone)
        {
            panel = LT.LOAD<RewardChoosePanel>();
            panel.setOnChoose(onChoose);
            panel.setOnHideEnd(onHideEnd);
            var ranIndex = ADungeon.relicRng.random(0, RelicLibrary.commonList.count() - 1);
            var relicId1 = RelicLibrary.commonList[ranIndex].relicId;
            ranIndex = ADungeon.relicRng.random(0, RelicLibrary.commonList.count() - 1);
            var relicId2 = RelicLibrary.commonList[ranIndex].relicId;
            ranIndex = ADungeon.relicRng.random(0, RelicLibrary.commonList.count() - 1);
            var relicId3 = RelicLibrary.commonList[ranIndex].relicId;
            panel.with(relicId1, relicId2, relicId3);
        }
    }

    static void OnChoose()
    {
        LT.HIDE<RewardChoosePanel>();
    }

    void OnHideEnd()
    {
        panel = null;
        isDone = true;
    }
}