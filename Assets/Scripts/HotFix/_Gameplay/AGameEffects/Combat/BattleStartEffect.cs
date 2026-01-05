namespace MarbleHero
{
    public class BattleStartEffect : AGameEffect
    {
        const float maxDuration = 4.0F;

        public override void onCreate()
        {
            base.onCreate();
            duration = maxDuration;
        }

        public override bool update(float dt)
        {
            if (isFloatEqual(duration, maxDuration))
            {
                Toast.Show("Battle Start");
            }

            return base.update(dt);
        }
    }
}