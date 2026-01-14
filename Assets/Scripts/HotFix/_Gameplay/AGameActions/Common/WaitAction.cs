namespace MarbleHero
{
    public class WaitAction : AGameAction, IArgs<float>
    {
        public void onCreate(float time)
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