using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 球撞击{0}次触发{1}次Effect
    /// </summary>
    public class BallHitChanceTriggerElectricChainPower : BallPower, IArgs<float, bool>
    {
        float chance;
        bool onlyBrick;

        public void onCreate(float c, bool b)
        {
            chance = c;
            onlyBrick = b;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            chance = 0F;
            onlyBrick = false;
        }

        public override void onHitBrick(Brick brick, Vector2 normal)
        {
            if (randomHit(chance))
            {
                effectManager.addLogic<ElectricChainEffect>().with(owner, brick, 2);
            }
        }

        public override void onHitBorder(Border border)
        {
            if (!onlyBrick && randomHit(chance))
            {
                if (brickManager.getRandomActiveBrick(out var brick))
                {
                    effectManager.addLogic<ElectricChainEffect>().with(owner, brick, 2);
                }
            }
        }
    }
}