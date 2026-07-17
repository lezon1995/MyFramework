namespace MoreMountains
{
    public class ClearCardQueueAction : AGameAction
    {
        public override void update(float dt)
        {
            actionManager.cardQueue.Clear();
            isDone = true;
        }
    }
}