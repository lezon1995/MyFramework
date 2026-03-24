namespace MarbleHero
{
    public class OpenRewardChoosePanelAction : AGameAction
    {
        RewardChoosePanel panel;

        public override void onCreate()
        {
            duration = 1F;
        }

        public override void update(float dt)
        {
            if (panel == null && !isDone)
            {
                panel = LT.LOAD<RewardChoosePanel>();
                panel.setOnChose(() =>
                {
                    LT.UNLOAD<RewardChoosePanel>();
                    panel = null;
                    isDone = true;
                });
                panel.with(ImpactHammer.ID);
            }
        }
    }
}