using MoreMountains.Tools;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoreMountains
{
    [RequireComponent(typeof(APlayer))]
    public class PlayerRenderer : MonoBehaviour
    {
        static int StrongTintFade = Shader.PropertyToID("_StrongTintFade");
        static int VibrateFade = Shader.PropertyToID("_VibrateFade");

        APlayer _player;

        Transform root;
        SortingGroup sortingGroup;
        SpriteRenderer spriteUnit, spriteShadow;
        ParticleSystem fxDodge;

        SpriteRenderer spriteShield;
        TextMeshPro shieldAmount;

        Material matUnit;
        Timer flashRemainSeconds;

        public HealthBar healthBar;
        BrickAnimationReceiver receiver;
        AnimationState curAnimation;

        public void Awake()
        {
            if (_player)
                return;

            TryGetComponent(out _player);
            var obj = _player.gameObject;
            if (obj.find(out receiver))
            {
                receiver.setOnAnimationEnd(onAnimationEnd);
            }

            obj.find(out sortingGroup);
            obj.find(out root, "Root");
            obj.find(out spriteShadow, "SpriteShadow");
            obj.find(out fxDodge, "Fx_Dodge");

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
                healthBar = new(_player. Health, h);
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
                matUnit.SetFloat(StrongTintFade, 0F);
                matUnit.SetFloat(VibrateFade, 0F);
                flashRemainSeconds.kill();
            }
        }

        public void setRendererActive(bool active)
        {
            spriteUnit.gameObject.SetActive(active);
            spriteShadow.gameObject.SetActive(active);
        }

        public void setHealthBarActive(bool active)
        {
            healthBar.setActive(active);
        }

        public void refreshHealthByDamage(int v, int max) => healthBar.refreshHealthByDamage(v, max);
        public void refreshShieldByDamage(float curProgress) => healthBar.refreshShieldByDamage(curProgress);
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
            curAnimation = AnimationState.BORN;
        }

        public void playFxDamage(Vector3 direction)
        {
            flashRemainSeconds = 0.15F;
            matUnit.SetFloat(StrongTintFade, 1F);
            matUnit.SetFloat(VibrateFade, 1F);
        }

        public void playFxSkillHit(Vector2 direction)
        {
            playFxHit(direction);
        }

        public void playFxDodge()
        {
            fxDodge.Play();
            new DodgeChanceTextEvent(true, _player.transform).trigger();
        }

        public void playFxHit(Vector2 normal)
        {
        }

        public void playFxHeal()
        {
            flashRemainSeconds = 0.05F;
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
                curAnimation = AnimationState.DIED;
            }
            else
            {
                curAnimation = AnimationState.DYING;
            }
        }

        void onAnimationEnd()
        {
            if (curAnimation == AnimationState.BORN)
            {
                setHealthBarActive(true);
                curAnimation = AnimationState.NONE;
            }
            else if (curAnimation == AnimationState.HITTING)
            {
                curAnimation = AnimationState.NONE;
            }
            else if (curAnimation == AnimationState.DYING)
            {
                var f = randomFloat(0F, 1F);
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
            fx.play(FxDefine.BRICK_DESTROY, _player.getWorldPosition());
        }


        public class HealthBar
        {
            Transform transform;
            public DamageChunkHealthBarRenderer barRenderer;
            TextMeshPro health;

            public HealthBar(Health h, Transform t)
            {
                transform = t;
                if (t.find(out barRenderer, "HealthBarRenderer"))
                    barRenderer.SetHealth(h);

                t.find(out health, "Health");
            }

            public void setActive(bool active)
            {
                transform.localScale = active ? Vector3.one : Vector3.zero;
            }

            public void refreshHealthByDamage(int cur, int max)
            {
                health.SetText(cur.IToS());

                var f = Mathf.Clamp01((float)cur / max);
                barRenderer.ApplyDamageToHealthBar(f);
            }

            public void refreshShieldByDamage(float curProgress)
            {
                barRenderer.ApplyDamageToShieldBar(curProgress);
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


        enum AnimationState
        {
            NONE,
            BORN,
            HITTING,
            DYING,
            DIED,
        }
    }
}