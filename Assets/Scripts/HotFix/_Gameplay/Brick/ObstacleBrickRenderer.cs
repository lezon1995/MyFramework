using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Brick))]
    public class ObstacleBrickRenderer : BrickRenderer
    {
        public override void playBornAnimation()
        {
            curAnimation = AnimationState.NONE;
            fx.play(FxDefine.SMOKE_FLASH, brick.getWorldPosition());
            onBornAnimationComplete?.Invoke();
        }
    }
}