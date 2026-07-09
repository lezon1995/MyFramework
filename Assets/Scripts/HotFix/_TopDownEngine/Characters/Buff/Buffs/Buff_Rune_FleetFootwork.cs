namespace MoreMountains.TopDownEngine
{
    public class Buff_Rune_FleetFootwork : Buff
    {
        public Data[] Buffs;

        protected override void OnIncreaseStackFrom(DoAttackEffect e)
        {
            if (IsMaxStacked == false)
            {
                base.OnIncreaseStackFrom(e);
            }
            else
            {
                DecreaseStack(MaxStack);

                foreach (var data in Buffs)
                {
                    GetActor(data.ApplyTo).ApplyBuff(data.Buff);
                }
            }
        }
    }
}