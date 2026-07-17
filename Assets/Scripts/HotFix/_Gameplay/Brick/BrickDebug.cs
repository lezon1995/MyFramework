using Drawing;
using UnityEngine;

namespace MoreMountains;

public class BrickDebug : MonoBehaviour
{
    public Brick brick;

    void Update()
    {
        if (brick == null)
            return;

        // var rect = brick.getRect();
        // var p1 = new Vector2(rect.xMin, rect.yMin);
        // var p2 = new Vector2(rect.xMin, rect.yMax);
        // var p3 = new Vector2(rect.xMax, rect.yMax);
        // var p4 = new Vector2(rect.xMax, rect.yMin);
        // Debug.DrawLine(p1, p2, Color.red);
        // Debug.DrawLine(p2, p3, Color.red);
        // Debug.DrawLine(p3, p4, Color.red);
        // Debug.DrawLine(p4, p1, Color.red);
    }
}