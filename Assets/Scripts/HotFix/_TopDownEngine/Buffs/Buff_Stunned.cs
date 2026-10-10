namespace MoreMountains
{
    public class Buff_Stunned : Buff
    {
        protected override void OnAfterAdd()
        {
            base.OnAfterAdd();

            if (Target.Character is Brick b)
            {
                b.Stun();
            }
        }

        protected override void OnBeforeRemove()
        {
            base.OnBeforeRemove();

            if (Target.Character is Brick b)
            {
                b.UnStun();
            }
        }
    }
}