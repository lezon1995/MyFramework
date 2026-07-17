using MoreMountains;
using UnityEngine;

[CreateAssetMenu(fileName = "FTextSetting", menuName = "MoreMountains/FText Setting/RandomAxis")]
public class FTextSetting_RandomAxis : FTextSetting
{
    public Vector2 XFloatRange = new Vector2(-1, 1);
    public Vector2 YFloatRange = new Vector2(1, 1);

    public override void ModifyPosition(ref Vector2 rectPos, in FText.Data data, float pct)
    {
        rectPos += data.floatDirection;
    }

    public override void ModifyFloatDirection(ref FText.Data data)
    {
        var rangeX = XFloatRange;
        var rangeY = YFloatRange;
        var x = Random.Range(rangeX.x, rangeX.y);
        var y = Random.Range(rangeY.x, rangeY.y);
        data.floatDirection = new Vector2(x, y);
    }
}