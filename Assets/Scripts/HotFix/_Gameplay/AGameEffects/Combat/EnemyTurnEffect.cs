namespace MarbleHero
{
    public class EnemyTurnEffect : AGameEffect
    {
        public EnemyTurnEffect()
        {
            Duration = 4.0F;
        }

        public override void update(float dt)
        {
            if (Duration == 4.0F)
            {
                Toast.Show("Enemy Turn Start");
            }

            base.update(dt);
        }
    }
}