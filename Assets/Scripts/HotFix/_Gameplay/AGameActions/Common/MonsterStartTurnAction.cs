namespace MarbleHero
{
    public class MonsterStartTurnAction : AGameAction
    {
        static float DURATION = Settings.ACTION_DUR_FAST;

        public MonsterStartTurnAction()
        {
            duration = DURATION;
        }

        public override void update(float dt)
        {
            if (duration == DURATION)
            {
                monsters.applyPreTurnLogic();
                isDone = true;
            }

            tickDuration(dt);
        }
    }
}