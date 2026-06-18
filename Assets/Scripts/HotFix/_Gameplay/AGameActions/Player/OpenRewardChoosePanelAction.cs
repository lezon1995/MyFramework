namespace MarbleHero
{
    public class OpenRewardChoosePanelAction : AGameAction
    {
        RewardChoosePanel panel;

        public override void update(float dt)
        {
            if (panel == null && !isDone)
            {
                panel = LT.LOAD<RewardChoosePanel>();
                panel.setOnChose(() =>
                {
                    LT.HIDE<RewardChoosePanel>();
                    panel = null;
                    isDone = true;
                });
                var ranIndex = ADungeon.relicRng.random(0, RelicLibrary.commonList.count() - 1);
                var relicId1 = RelicLibrary.commonList[ranIndex].relicId;
                ranIndex = ADungeon.relicRng.random(0, RelicLibrary.commonList.count() - 1);
                var relicId2 = RelicLibrary.commonList[ranIndex].relicId;
                ranIndex = ADungeon.relicRng.random(0, RelicLibrary.commonList.count() - 1);
                var relicId3 = RelicLibrary.commonList[ranIndex].relicId;
                panel.with(relicId1, relicId2, relicId3);
            }
        }
    }
}