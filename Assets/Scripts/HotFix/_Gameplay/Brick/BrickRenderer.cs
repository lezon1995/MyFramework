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

    public override void init(ComponentOwner owner)
    {
        base.init(owner);
        if (owner is Brick brick)
        {
            var obj = brick.getObject();
            gameObject = obj;
            findComponent(obj, "Renderer", out renderer);
            findComponent(obj, "Health", out health);
            findComponent(obj, "Shadow", out shadow);
            findComponent(obj, "FxHit", out fxHit);
            findComponent(obj, "FxDead", out fxDead);

            if (findComponent(obj, "Sprite", out sprite))
            {
                brickMat = sprite.material;
            }

            findComponent(obj, "Block", out block);
            findComponent(obj, "BlockAmount", out blockAmount);
            block.gameObject.SetActive(false);
            blockAmount.gameObject.SetActive(false);
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
        brickFlashFrames = 0;
    }


    public void setRendererActive(bool active)
    {
        health.gameObject.SetActive(active);
        sprite.gameObject.SetActive(active);
        shadow.gameObject.SetActive(active);
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
    }

    public void setWidth(float width)
    {
        var size = new Vector2(width, sprite.size.y);
        sprite.size = size;
        shadow.size = size;
        block.size = size * 1.08F;
        blockAmount.GetComponent<RectTransform>().sizeDelta = size;
    }

    public void setHeight(float height)
    {
        var size = new Vector2(sprite.size.x, height);
        sprite.size = size;
        shadow.size = size;
        block.size = size * 1.08F;
        blockAmount.GetComponent<RectTransform>().sizeDelta = size;
    }

    public void refreshHealth(int v)
    {
        health.text = v.ToString();
    }
    
    public void refreshBlockAmount(int v)
    {
        blockAmount.text = v.ToString();
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
    
    public void playFxGainBlock()
    {
        block.gameObject.SetActive(true);
        blockAmount.gameObject.SetActive(true);
        block.transform.localScale = Vector3.one * 1.5F;
        block.color = new(1F, 1F, 1F, 0F);
        blockAmount.alpha = 0F;
        health.alpha = 1F;
        
        Tween.Scale(block.transform, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(block, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(blockAmount, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(health, endValue: 0.5F, duration: 0.2F, ease: Ease.OutCubic);
    }

    public void playFxLoseBlock()
    {
        block.transform.localScale = Vector3.one * 1F;
        block.color = new(1F, 1F, 1F, 1F);
        blockAmount.alpha = 1F;
        health.alpha = 0.5F;
        
        Tween.Scale(block.transform, endValue: 1.5F, duration: 0.2F, ease: Ease.OutCubic);
        Tween.Alpha(block, endValue: 0F, duration: 0.2F, ease: Ease.OutCubic).OnComplete(block, s => s.gameObject.SetActive(false));
        Tween.Alpha(blockAmount, endValue: 0F, duration: 0.2F, ease: Ease.OutCubic).OnComplete(blockAmount, s => s.gameObject.SetActive(false));
        Tween.Alpha(health, endValue: 1F, duration: 0.2F, ease: Ease.OutCubic);
    }

    public void playFxDead()
    {
        fxDead.Play();
    }
}