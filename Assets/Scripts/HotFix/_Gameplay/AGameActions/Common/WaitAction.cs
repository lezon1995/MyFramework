namespace MarbleHero
{
    public class WaitAction : AGameAction
    {
        public WaitAction(float time)
        {
            if (Settings.FAST_MODE && time > 0.1F)
                duration = 0.1F;
            else
                duration = time;
        }

        public override void update(float dt)
        {
            tickDuration(dt);
        }
    }
}