using UnityEngine;

namespace MoreMountains
{
    public class PlayerPower : CreaturePower
    {
        protected new APlayer owner;

        public void onCreate(APlayer ball)
        {
            owner = ball;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            owner = null;
        }

        public virtual int onPlayerGainedBlock(int blockAmount) => blockAmount;

        public virtual void onBeforeApplyDamage(Brick brick, ref Dmg dmg) { }
        public virtual void onKnockbackReceived(Brick brick, Vector2 direction, float force) { }
        public override void onGainPower(ACreature c) => onGainPower(c as APlayer);
        public override void onLosePower(ACreature c) => onLosePower(c as APlayer);

        protected virtual void onGainPower(APlayer p) { }
        protected virtual void onLosePower(APlayer p) { }
    }
}