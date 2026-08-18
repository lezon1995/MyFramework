namespace MoreMountains
{
    public class Buff_IceFreeze : Buff
    {
        protected override void OnAfterAdd()
        {
            base.OnAfterAdd();

            if (Target.Character is Brick b)
            {
                b.brickRenderer.setFreezeEffect(true);
                b.Controller.MovementDisabled = true;
            }
        }

        protected override void OnBeforeRemove()
        {
            base.OnBeforeRemove();

            if (Target.Character is Brick b)
            {
                b.brickRenderer.setFreezeEffect(false);
                b.Controller.MovementDisabled = false;
            }
        }
    }
}