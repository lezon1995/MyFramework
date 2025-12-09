using UnityEngine;

namespace MarbleHero;

[CreateAssetMenu(fileName = "FTextSetting", menuName = "MarbleHero/FText Setting/FixedDuration")]
public class FTextSetting_FixedDuration : FTextSetting
{
    public Vector2 FloatDirection = new Vector2(1, 0);

    public override void ModifyPosition(ref Vector2 rectPos, in FText.Data data, float pct)
    {
        rectPos += FloatDirection;
    }
}