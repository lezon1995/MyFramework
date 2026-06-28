using PrimeTween;
using TMPro;
using UnityEngine;

namespace MarbleHero;

public class BrickRenderer : GameComponent
{
    static int StrongTintFade = Shader.PropertyToID("_StrongTintFade");

    GameObject gameObject;

    Transform renderer;
    TextMeshPro health;
    SpriteRenderer sprite;
    SpriteRenderer shadow;
    ParticleSystem fxHit;
    ParticleSystem fxDead;

    SpriteRenderer block;
    TextMeshPro blockAmount;

    Material brickMat;
    int brickFlashFrames;

    HealthBar healthBar;

    public override void init(ComponentOwner owner)
    {
        base.init(owner);
        if (owner is Brick brick)
        {
            var obj = brick.gameObject;
            gameObject = obj;
            obj.find(out renderer, "Renderer");
            obj.find(out health, "Health");
            obj.find(out shadow, "Shadow");
            obj.find(out fxHit, "FxHit");
            obj.find(out fxDead, "FxDead");

            if (obj.find(out sprite, "Sprite"))
            {
                brickMat = sprite.material;
            }

            obj.find(out block, "Block");
            obj.find(out blockAmount, "BlockAmount");
            block.gameObject.SetActive(false);
            blockAmount.gameObject.SetActive(false);
            if (obj.find(out Transform h, "HealthBar"))
            {
                healthBar = new(h);
            }
        }
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);

        if (brickFlashFrames > 0)
        {
            brickFlashFrames = clampMin(brickFlashFrames - 1);
            if (brickFlashFrames <= 0)
            {
                brickMat.SetFloat(StrongTintFade, 0);
            }
        }
    }

    public override void destroy()
    {
        base.destroy();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        gameObject = null;
        renderer = null;
        health = null;
        sprite = null;
        shadow = null;
        fxHit = null;
        fxDead = null;
        brickMat = null;
        block = null;
        blockAmount = null;
        healthBar = null;
        brickFlashFrames = 0;
    }


    public void setRendererActive(bool active)
    {
        health.gameObject.SetActive(active);
        sprite.gameObject.SetActive(active);
        shadow.gameObject.SetActive(active);
        healthBar.setActive(active);
    }

    public void setBrickSprite(Sprite s)
    {
        sprite.sprite = s;
    }

    public void setSize(float width, float height)
    {
        var size = new Vector2(width, height);
        sprite.size = size;
        shadow.size = size;
        block.size = size * 1.08F;
        blockAmount.GetComponent<RectTransform>().sizeDelta = size;
        healthBar.refreshPositionAndSizeBy(size);
    }

    public void setWidth(float width)
    {
        var size = new Vector2(width, sprite.size.y);
        sprite.size = size;
        shadow.size = size;
        block.size = size * 1.08F;
        blockAmount.GetComponent<RectTransform>().sizeDelta = size;
        healthBar.refreshPositionAndSizeBy(size);
    }

    public void setHeight(float height)
    {
        var size = new Vector2(sprite.size.x, height);
        sprite.size = size;
        shadow.size = size;
        block.size = size * 1.08F;
        blockAmount.GetComponent<RectTransform>().sizeDelta = size;
        healthBar.refreshPositionAndSizeBy(size);
    }

    public void refreshHealth(int v, int max)
    {
        health.text = v.ToString();
        healthBar.refresh(v, max);
    }

    public void refreshBlockAmount(int v)
    {
        blockAmount.text = v.ToString();
        playFxBlockHit();
    }

    public void playFadeIn()
    {
        shadow.color = new(1, 1, 1, 0);
        sprite.color = new(1, 1, 1, 0);

        sprite.transform.localScale = Vector3.one;
        renderer.localPosition = new(0, 0.3F, 0);

        Tween.Alpha(shadow, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(sprite, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.LocalPositionY(renderer, endValue: 0F, duration: 0.2F, ease: Ease.OutCubic);
    }

    public void playFxHit()
    {
        health.transform.localScale = Vector3.one * 1F;
        Sequence
            .Create(Tween.Scale(health.transform, endValue: Vector3.one * 1.25F, duration: 0.05F, ease: Ease.OutCubic))
            .Chain(Tween.Scale(health.transform, endValue: Vector3.one * 1F, duration: 0.05F, ease: Ease.OutCubic));

        fxHit.Play();

        Sequence
            .Create(Tween.Scale(sprite.transform, endValue: Vector3.one * 0.95F, duration: 0.1F, ease: Ease.OutCubic))
            .Chain(Tween.Scale(sprite.transform, endValue: Vector3.one * 1F, duration: 0.1F, ease: Ease.OutCubic));

        brickFlashFrames = 2;
        brickMat.SetFloat(StrongTintFade, 1);
    }

    public void playFxHeal()
    {
        health.transform.localScale = Vector3.one * 1F;
        Sequence
            .Create(Tween.Scale(health.transform, endValue: Vector3.one * 1.25F, duration: 0.05F, ease: Ease.OutCubic))
            .Chain(Tween.Scale(health.transform, endValue: Vector3.one * 1F, duration: 0.05F, ease: Ease.OutCubic));

        fxHit.Play();

        Sequence
            .Create(Tween.Scale(sprite.transform, endValue: Vector3.one * 0.95F, duration: 0.1F, ease: Ease.OutCubic))
            .Chain(Tween.Scale(sprite.transform, endValue: Vector3.one * 1F, duration: 0.1F, ease: Ease.OutCubic));

        brickFlashFrames = 2;
        brickMat.SetFloat(StrongTintFade, 1);
    }

    public void playFxGainBlock()
    {
        block.gameObject.SetActive(true);
        blockAmount.gameObject.SetActive(true);
        block.transform.localScale = Vector3.one * 2F;
        block.color = new(1F, 1F, 1F, 0F);
        blockAmount.alpha = 0F;
        health.alpha = 1F;

        Tween.Scale(block.transform, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(block, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(blockAmount, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(health, endValue: 0.5F, duration: 0.2F, ease: Ease.OutCubic);
    }

    public void playFxBlockHit()
    {
        block.transform.localScale = Vector3.one * 1.15F;
        Sequence
            .Create(Tween.Scale(block.transform, endValue: Vector3.one * 0.85F, duration: 0.15F, ease: Ease.OutCubic))
            .Chain(Tween.Scale(block.transform, endValue: Vector3.one * 1F, duration: 0.15F, ease: Ease.OutCubic));
    }

    public void playFxLoseBlock()
    {
        block.transform.localScale = Vector3.one * 1F;
        block.color = new(1F, 1F, 1F, 1F);
        blockAmount.alpha = 1F;
        health.alpha = 0.5F;

        Tween.Scale(block.transform, endValue: 2F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(block, endValue: 0F, duration: 0.2F, ease: Ease.OutCubic).OnComplete(block, s => s.gameObject.SetActive(false));
        Tween.Alpha(blockAmount, endValue: 0F, duration: 0.2F, ease: Ease.OutCubic).OnComplete(blockAmount, s => s.gameObject.SetActive(false));
        Tween.Alpha(health, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
    }

    public void playFxDead()
    {
        fxDead.Play();
    }

    class HealthBar
    {
        Transform transform;
        SpriteRenderer barBack, barFront;
        TextMeshPro health;

        float barFontOriginalWidth;

        public HealthBar(Transform t)
        {
            transform = t;
            t.find(out barBack, "Back");
            t.find(out barFront, "Front");
            t.find(out health, "Health");
        }

        public void refreshPositionAndSizeBy(Vector2 size)
        {
            var localPosition = transform.localPosition;
            localPosition.y = -size.y * 0.5F;
            transform.localPosition = localPosition;

            barBack.size = new(size.x + 0.04F, 0.12F);
            barFront.size = new(size.x, 0.08F);
            barFontOriginalWidth = barFront.size.x;
        }

        public void setActive(bool active)
        {
            transform.gameObject.SetActive(active);
        }

        public void refresh(int cur, int max)
        {
            health.text = IToS(cur);

            var f = Mathf.Clamp01(((float)cur) / max);
            var barFrontWidth = barFontOriginalWidth * f;
            var delta = barFontOriginalWidth - barFrontWidth;
            var localPosition = barFront.transform.localPosition;
            localPosition.x = -delta * 0.5F;
            barFront.transform.localPosition = localPosition;

            var size = barFront.size;
            size.x = barFrontWidth;
            barFront.size = size;
        }
    }
}