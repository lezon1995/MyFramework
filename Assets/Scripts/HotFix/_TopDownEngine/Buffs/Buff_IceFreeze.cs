namespace MoreMountains
{
    public class Buff_IceFreeze : Buff
    {
        protected override void OnAfterAdd()
        {
            base.OnAfterAdd();

            if (Target.Character is Brick b)
            {
                b.Freeze();
            }
        }

        protected override void OnBeforeRemove()
        {
            base.OnBeforeRemove();

            if (Target.Character is Brick b)
            {
                b.UnFreeze();
            }
        }
    }
}