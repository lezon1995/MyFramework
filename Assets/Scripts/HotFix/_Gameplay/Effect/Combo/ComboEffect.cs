using PrimeTween;
using UnityEngine;

public class ComboEffect : Transformable
{
    public SpriteRenderer sprite;
    public SpriteRenderer spriteOverlay;

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);

        obj.find(out sprite, "Sprite");
        obj.find(out spriteOverlay, "SpriteOverlay");
    }

    public override void resetProperty()
    {
        base.resetProperty();
        sprite = null;
        spriteOverlay = null;
    }

    public void setSprite(Sprite s)
    {
        sprite.sprite = s;
        spriteOverlay.sprite = s;
    }

    public void setCombo(int count)
    {
        if (count == 10)
        {
            //Lucky bonus when combo is 10
            // CtrUI.instance.LuckyBonus();
        }


        //Play sound according to the combo
        /*if (count <= 1)
        {
            SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_block1_destory);
        }
        if (count == 2 || count == 3)
        {
            SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_block2_destory);
        }
        else if (count == 4 || count == 5)
        {
            SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_block3_destory);
        }
        else if (count == 6 || count == 7)
        {
            SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_block4_destory);
        }
        else if (count == 8 || count == 9)
        {
            SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_block5_destory);
        }
        else
        {
            SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_block6_dsstory);
        }*/

        sprite.transform.localScale = Vector3.one * (count + 0.5f);

        Tween.Scale(sprite.transform, endValue: 1F, duration: 0.15F, ease: Ease.OutBounce);

        sprite.color = Color.white;
        spriteOverlay.color = Color.white;

        Tween
            .Alpha(spriteOverlay, endValue: 0F, duration: 1F, ease: Ease.Linear);

        Tween
            .Alpha(sprite, endValue: 0F, duration: 1F, ease: Ease.Linear, startDelay: 0.15F)
            .OnComplete(this, effect => comboManager.destroyComboEffect(effect));
    }
}