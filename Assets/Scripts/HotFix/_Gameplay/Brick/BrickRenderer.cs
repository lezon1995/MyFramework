using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoreMountains
{
    [RequireComponent(typeof(Brick))]
    public class BrickRenderer : MonoBehaviour
    {
        static int StrongTintFade = Shader.PropertyToID("_StrongTintFade");
        static int BrickBorn = Animator.StringToHash("BrickBorn");
        static int BrickIdle = Animator.StringToHash("BrickIdle");
        static int BrickHit_1 = Animator.StringToHash("BrickHit_1");
        static int BrickHit_2 = Animator.StringToHash("BrickHit_2");
        static int BrickHit_3 = Animator.StringToHash("BrickHit_3");
        static int BrickDie_1 = Animator.StringToHash("BrickDie_1");
        static int BrickDie_2 = Animator.StringToHash("BrickDie_2");
        static int BrickDie_3 = Animator.StringToHash("BrickDie_3");

        Brick brick;

        Transform root;
        Animator animator;
        SortingGroup sortingGroup;
        SpriteRenderer spriteBlock, spriteUnit, spriteShadow;

        SpriteRenderer spriteShield;
        TextMeshPro shieldAmount;

        Material matBlock, matUnit;
        Timer flashRemainSeconds;

        HealthBar healthBar;
        BrickAnimationReceiver receiver;
        AnimationState curAnimation;
        Action onBornAnimationComplete;

        public void Awake()
        {
            if (brick)
                return;

            TryGetComponent(out brick);
            var obj = brick.gameObject;
            obj.find(out animator);
            if (obj.find(out receiver))
            {
                receiver.setOnAnimationEnd(onAnimationEnd);
            }

            obj.find(out sortingGroup);
            obj.find(out root, "Root");
            obj.find(out spriteShadow, "SpriteShadow");

            if (obj.find(out spriteBlock, "SpriteBlock"))
            {
                matBlock = spriteBlock.material;
            }

            if (obj.find(out spriteUnit, "SpriteUnit"))
            {
                matUnit = spriteUnit.material;
            }

            obj.find(out spriteShield, "Shield");
            obj.find(out shieldAmount, "ShieldAmount");
            spriteShield.gameObject.SetActive(false);
            shieldAmount.gameObject.SetActive(false);
            if (obj.find(out Transform h, "HealthBar"))
            {
                healthBar = new(h);
            }
        }

        void Update()
        {
            update(Time.deltaTime);
        }

        public void update(float elapsedTime)
        {
            healthBar?.update(elapsedTime);

            if (flashRemainSeconds.update(elapsedTime))
            {
                matBlock.SetFloat(StrongTintFade, 0);
                matUnit.SetFloat(StrongTintFade, 0);
                flashRemainSeconds.kill();
            }
        }

        public void setOnBornAnimationComplete(Action a)
        {
            onBornAnimationComplete = a;
        }

        public void setRendererActive(bool active)
        {
            spriteBlock.gameObject.SetActive(active);
            spriteShadow.gameObject.SetActive(active);
        }

        public void setHealthBarActive(bool active)
        {
            healthBar.setActive(active);
        }

        public void refreshHealthByDamage(int v, int max) => healthBar.refreshByDamage(v, max);
        public void refreshHealthByHealing(int v, int max) => healthBar.refreshByHealing(v, max);
        public void refreshHealthByBorn(int v, int max) => healthBar.refreshByBorn(v, max);

        public void refreshBlockAmount(int v)
        {
            shieldAmount.text = v.ToString();
            playFxBlockHit();
        }

        public void setSortingOrder(int v)
        {
            sortingGroup.sortingOrder = v;
        }

        public void playBornAnimation()
        {
            animator.Play(BrickBorn, 0, 0F);
            curAnimation = AnimationState.BORN;
        }

        public void playFxDamage(Vector3 direction)
        {
            flashRemainSeconds = 0.08F;
            matBlock.SetFloat(StrongTintFade, 1);
            matUnit.SetFloat(StrongTintFade, 1);
        }

        public void playFxHit(Vector2 normal)
        {
            if (brick.IsDead())
                return;

            if (isAnimationDiedOrDying())
                return;

            var dir = determineUnderHitDirection(normal);
            switch (dir)
            {
                case UnderHitDirection.None:
                    break;
                case UnderHitDirection.Top:
                case UnderHitDirection.Bot:
                    animator.Play(BrickHit_1, 0, 0F);
                    curAnimation = AnimationState.HITTING;
                    break;
                case UnderHitDirection.Left:
                    animator.Play(BrickHit_2, 0, 0F);
                    curAnimation = AnimationState.HITTING;
                    break;
                case UnderHitDirection.Right:
                    animator.Play(BrickHit_3, 0, 0F);
                    curAnimation = AnimationState.HITTING;
                    break;
                case UnderHitDirection.TopLeft:
                case UnderHitDirection.BotLeft:
                    animator.Play(BrickHit_2, 0, 0F);
                    curAnimation = AnimationState.HITTING;
                    break;
                case UnderHitDirection.TopRight:
                case UnderHitDirection.BotRight:
                    animator.Play(BrickHit_3, 0, 0F);
                    curAnimation = AnimationState.HITTING;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static UnderHitDirection determineUnderHitDirection(Vector2 normal)
        {
            UnderHitDirection dir;
            if (normal.x.isZero())
            {
                if (normal.y > 0F)
                    dir = UnderHitDirection.Top; //上方受击
                else
                    dir = UnderHitDirection.Bot; //下方受击
            }
            else
            {
                if (normal.y.isZero())
                {
                    if (normal.x > 0F)
                        dir = UnderHitDirection.Right; //右方受击
                    else
                        dir = UnderHitDirection.Left; //左方受击
                }
                else
                {
                    dir = (normal.x, normal.y) switch
                    {
                        (> 0, > 0) => UnderHitDirection.TopRight, //右上受击
                        (> 0, < 0) => UnderHitDirection.BotRight, //右下受击
                        (< 0, < 0) => UnderHitDirection.BotLeft, //左下受击
                        (< 0, > 0) => UnderHitDirection.TopLeft, //左上受击
                        _ => UnderHitDirection.None
                    };
                }
            }

            return dir;
        }

        public void playFxHeal()
        {
            flashRemainSeconds = 0.05F;
            matBlock.SetFloat(StrongTintFade, 1);
            matUnit.SetFloat(StrongTintFade, 1);
        }

        public void playFxGainBlock()
        {
            spriteShield.gameObject.SetActive(true);
            shieldAmount.gameObject.SetActive(true);
            spriteShield.transform.localScale = Vector3.one * 2F;
            spriteShield.color = new(1F, 1F, 1F, 0F);
            shieldAmount.alpha = 0F;

            Tween.Scale(spriteShield.transform, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
            Tween.Alpha(spriteShield, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
            Tween.Alpha(shieldAmount, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        }

        public void playFxBlockHit()
        {
            spriteShield.transform.localScale = Vector3.one * 1.15F;
            Sequence
                .Create(Tween.Scale(spriteShield.transform, endValue: Vector3.one * 0.85F, duration: 0.15F, ease: Ease.OutCubic))
                .Chain(Tween.Scale(spriteShield.transform, endValue: Vector3.one * 1F, duration: 0.15F, ease: Ease.OutCubic));
        }

        public void playFxLoseBlock()
        {
            spriteShield.transform.localScale = Vector3.one * 1F;
            spriteShield.color = new(1F, 1F, 1F, 1F);
            shieldAmount.alpha = 1F;

            Tween.Scale(spriteShield.transform, endValue: 2F, duration: 0.2F, ease: Ease.OutCubic);
            Tween.Alpha(spriteShield, endValue: 0F, duration: 0.2F, ease: Ease.OutCubic).OnComplete(spriteShield, s => s.gameObject.SetActive(false));
            Tween.Alpha(shieldAmount, endValue: 0F, duration: 0.2F, ease: Ease.OutCubic).OnComplete(shieldAmount, s => s.gameObject.SetActive(false));
        }

        public void playFxDead()
        {
            if (curAnimation == AnimationState.NONE)
            {
                var f = randomFloat(0F, 1F);
                var die = f < 0.33F ? BrickDie_1 : f < 0.66F ? BrickDie_2 : BrickDie_3;
                animator.Play(die, 0, 0F);
                curAnimation = AnimationState.DIED;
            }
            else
            {
                curAnimation = AnimationState.DYING;
            }
        }

        bool isAnimationDiedOrDying()
        {
            return curAnimation is AnimationState.DIED or AnimationState.DYING;
        }

        public bool isPlayingAnimationBorn()
        {
            return curAnimation == AnimationState.BORN;
        }

        void onAnimationEnd()
        {
            if (curAnimation == AnimationState.BORN)
            {
                curAnimation = AnimationState.NONE;
                onBornAnimationComplete?.Invoke();
            }
            else if (curAnimation == AnimationState.HITTING)
            {
                curAnimation = AnimationState.NONE;
            }
            else if (curAnimation == AnimationState.DYING)
            {
                var f = randomFloat(0F, 1F);
                var die = f < 0.33F ? BrickDie_1 : f < 0.66F ? BrickDie_2 : BrickDie_3;
                animator.Play(die, 0, 0F);
                curAnimation = AnimationState.DIED;
            }
            else if (curAnimation == AnimationState.DIED)
            {
                playBrickDestroyFx();
                setRendererActive(false);
            }
        }

        protected virtual void playBrickDestroyFx()
        {
            fx.play(FxDefine.BRICK_DESTROY, brick.getWorldPosition());
        }


        class HealthBar
        {
            Transform transform;
            DamageChunkHealthBarRenderer barRenderer;
            TextMeshPro health;

            public HealthBar(Transform t)
            {
                transform = t;
                t.find(out barRenderer, "HealthBarRenderer");
                t.find(out health, "Health");
            }

            public void setActive(bool active)
            {
                transform.localScale = active ? Vector3.one : Vector3.zero;
            }

            public void refreshByDamage(int cur, int max)
            {
                health.SetText(cur.IToS());

                var f = Mathf.Clamp01(((float)cur) / max);
                barRenderer.ApplyDamage(f);
            }

            public void refreshByHealing(int cur, int max)
            {
                health.SetText(cur.IToS());

                var f = Mathf.Clamp01(((float)cur) / max);
                barRenderer.SetProgress(f);
                barRenderer.ClearAllChunks();
                barRenderer.ApplyToMaterial();
            }

            public void refreshByBorn(int cur, int max)
            {
                health.SetText(cur.IToS());

                var f = Mathf.Clamp01(((float)cur) / max);
                barRenderer.SetProgress(f);
                barRenderer.ClearAllChunks();
                barRenderer.ApplyToMaterial();
            }

            public void update(float dt)
            {
            }
        }

        enum UnderHitDirection
        {
            None,
            Top,
            Bot,
            Left,
            Right,
            TopLeft,
            TopRight,
            BotLeft,
            BotRight,
        }

        enum AnimationState
        {
            NONE,
            BORN,
            HITTING,
            DYING,
            DIED,
        }

        public void ResetToIdle()
        {
            animator.Play(BrickIdle, 0, 0F);
            animator.Update(0);
        }
    }
}