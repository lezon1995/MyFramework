namespace MarbleHero
{
    public class EnemyTurnEffect : AGameEffect
    {
        const float maxDuration = 4.0F;

        public override void onCreate()
        {
            duration = maxDuration;
        }

        public override bool update(float dt)
        {
            if (isFloatEqual(duration, maxDuration))
            {
                Toast.Show("Enemy Turn Start");
            }

            return base.update(dt);
        }
    }
}