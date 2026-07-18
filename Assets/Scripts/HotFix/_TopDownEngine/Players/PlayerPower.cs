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
        
        public virtual int onPlayerGainedBlock(float blockAmount)
        {
            return floor(blockAmount);
        }

        public virtual int onPlayerGainedBlock(int blockAmount)
        {
            return blockAmount;
        }
    
        public virtual void onBeforeApplyDamage(Brick brick, ref Dmg dmg)
        {
        }

        public virtual void onKnockbackReceived(Brick brick, Vector2 direction, float force)
        {
        }
    
        public virtual void onGainPower(APlayer player)
        {
        }

        public virtual void onLosePower(APlayer player)
        {
        }

    }
}