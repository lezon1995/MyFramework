namespace MoreMountains
{
    public class MonsterStartTurnAction : AGameAction
    {
        static float DURATION = Settings.ACTION_DUR_FAST;

        public override void onCreate()
        {
            duration = DURATION;
        }

        public override void update(float dt)
        {
            if (duration.unstarted)
            {
                monsters.applyPreTurnLogic();
                isDone = true;
            }

            tickDuration(dt);
        }
    }
}