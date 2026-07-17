namespace MoreMountains
{
    public class GainExpAction : AGameAction, IArgs<int>
    {
        int expToGain;
        
        public void onCreate(int exp)
        {
            expToGain = exp;
        }

        public override void update(float dt)
        {
            player.gainExp(expToGain);
            isDone = true;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            expToGain = 0;
        }
    }
}