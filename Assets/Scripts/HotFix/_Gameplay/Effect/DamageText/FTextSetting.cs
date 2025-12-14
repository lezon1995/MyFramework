using MarbleHero;
using UnityEngine;

// namespace MarbleHero;

[CreateAssetMenu(fileName = "FTextSetting", menuName = "MarbleHero/FText Setting/FTextSetting")]
public class FTextSetting : ScriptableObject
{
    public virtual bool CalculateTotalPct => false;
    public Color[] FontColors;
    public Sprite[] Icons;
    public float ContentScale = 1F;
    public float StartSequenceDuration = 0.5f;
    public float StaticDuration = 0.5f;
    public float FloatingDuration = 1;
    public float FinishSequenceDuration = 0.2f;

    public AnimationCurve StartScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve FinishScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve FadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve FadeOverLifeTime = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public AnimationCurve FadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    public virtual void ModifyPosition(ref Vector2 rectPos, in FText.Data data, float pct)
    {
    }

    public virtual void ModifyFloatDirection(ref FText.Data data)
    {
    }
}