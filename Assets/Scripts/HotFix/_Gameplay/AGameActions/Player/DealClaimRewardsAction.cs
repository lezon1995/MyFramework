namespace MoreMountains
{
    public class DealClaimRewardsAction : AGameAction
    {
        public override void onCreate()
        {
        }

        public override void update(float dt)
        {
            if (!isDone)
            {
                for (int i = 0; i < player.toClaimRewardCount; i++)
                {
                    actionManager.addToBot<OpenRewardChoosePanelAction>();
                }

                player.toClaimRewardCount = 0;
            }

            isDone = true;
        }
    }
}