using MoreMountains;
using UnityEngine;

[CreateAssetMenu(fileName = "FTextSetting", menuName = "MoreMountains/FText Setting/CurveAxis")]
public class FTextSetting_CurveAxis : FTextSetting
{
    public override bool CalculateTotalPct => true;
    public AnimationCurve XFloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    public AnimationCurve YFloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public override void ModifyPosition(ref Vector2 rectPos, in FText.Data data, float pct)
    {
        rectPos.x = XFloatCurve.Evaluate(pct);
        rectPos.y = YFloatCurve.Evaluate(pct);
    }
}