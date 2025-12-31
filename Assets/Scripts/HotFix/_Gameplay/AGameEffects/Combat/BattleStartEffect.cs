namespace MarbleHero
{
    public class BattleStartEffect : AGameEffect
    {
        public BattleStartEffect()
        {
            Duration = 4.0F;
        }

        public override void update(float dt)
        {
            if (Duration == 4.0F)
            {
                Toast.Show("Battle Start");
            }

            base.update(dt);
        }
    }
}