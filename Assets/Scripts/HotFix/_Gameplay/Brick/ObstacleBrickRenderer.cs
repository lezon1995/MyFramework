using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Brick))]
    public class ObstacleBrickRenderer : BrickRenderer
    {
        public Sprite[] BlockSprites;
        int blockSpriteCount;
        float healthPctGap;
        MMObservable<float> healthPct;

        public override void Awake()
        {
            base.Awake();

            blockSpriteCount = BlockSprites == null ? 0 : BlockSprites.Length;
            if (blockSpriteCount > 0)
            {
                healthPctGap = 1 / (float)blockSpriteCount;
                healthPct.OnValueChangedFromTo = OnValueChangedFromTo;
            }
        }

        void OnValueChangedFromTo(float pre, float cur)
        {
            for (int i = 1; i <= blockSpriteCount; i++)
            {
                if (cur < (healthPctGap * i) && pre >= (healthPctGap * i))
                {
                    var index = i - 1;
                    spriteBlock.sprite = BlockSprites[index];
                }
                else if (pre < (healthPctGap * i) && cur >= (healthPctGap * i))
                {
                    var index = i - 1;
                    spriteBlock.sprite = BlockSprites[index];
                }
            }
        }

        public override void refreshHealthByDamage(int v, int max)
        {
            base.refreshHealthByDamage(v, max);

            if (blockSpriteCount > 0)
            {
                var pct = v / (float)max;
                healthPct.Value = pct;
            }
        }

        public override void refreshHealthByHealing(int v, int max)
        {
            base.refreshHealthByHealing(v, max);
            if (blockSpriteCount > 0)
            {
                var pct = v / (float)max;
                healthPct.Value = pct;
            }
        }

        public override void refreshHealthByBorn(int v, int max)
        {
            base.refreshHealthByBorn(v, max);
            if (blockSpriteCount > 0)
            {
                var pct = v / (float)max;
                healthPct.Value = pct;
            }
        }

        public override void playBornAnimation()
        {
            curAnimation = AnimationState.NONE;
            fx.play(FxDefine.SMOKE_FLASH, _brick.getWorldPosition());
            onBornAnimationComplete?.Invoke();
        }
    }
}