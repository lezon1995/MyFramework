using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseHotFix;

// 窗口的碰撞检测相关逻辑
public class COMWindowCollider : GameComponent
{
    protected BoxCollider collider; // 碰撞组件

    public override void resetProperty()
    {
        base.resetProperty();
        collider = null;
    }

    public void setBoxCollider(BoxCollider c)
    {
        if (collider == c)
            return;

        collider = c;
        var window = owner as myUGUIObject;
        GameLayout layout = window.getLayout();
        if (layout && layout.isCheckBoxAnchor() && mLayoutManager.isUseAnchor())
        {
            collider.isTrigger = true;
            string layoutName = layout.getName();
            string windowName = window.getName();
            GameObject go = window.getObject();
            // BoxCollider的中心必须为0,因为UIWidget会自动调整BoxCollider的大小和位置,而且调整后位置为0,所以在制作时BoxCollider的位置必须为0
            if (!isFloatZero(collider.center.sqrMagnitude))
            {
                logWarning("BoxCollider's center must be zero! Otherwise can not adapt to the screen sometimes! name : " + windowName + ", layout : " + layoutName);
            }

            if (!go.TryGetComponent<ScaleAnchor>(out _) && !go.TryGetComponent<PaddingAnchor>(out _))
            {
                logWarning("Window with BoxCollider and Widget must has ScaleAnchor! Otherwise can not adapt to the screen sometimes! name : " + windowName + ", layout : " + layoutName);
            }
        }
    }

    public BoxCollider getBoxCollider()
    {
        return collider;
    }

    public bool isHandleInput()
    {
        return collider && collider.enabled;
    }

    public bool raycast(ref Ray ray, out RaycastHit hit, float maxDistance)
    {
        if (!collider)
        {
            hit = new();
            return false;
        }

        return collider.Raycast(ray, out hit, maxDistance);
    }

    public void enableCollider(bool enable)
    {
        if (collider)
            collider.enabled = enable;
    }

    public void setColliderSize(Vector2 size)
    {
        if (collider == null)
            return;

        if (!isFloatEqual(size.x, collider.size.x) || !isFloatEqual(size.y, collider.size.y))
        {
            collider.size = size;
            collider.center = Vector2.zero;
        }
    }

    public void setColliderSize(RectTransform transform)
    {
        if (collider == null || transform == null)
            return;

        Vector2 size = transform.rect.size;
        if (!isFloatEqual(size.x, collider.size.x) || !isFloatEqual(size.y, collider.size.y))
        {
            collider.size = size;
            collider.center = multiVector2(size, new Vector2(0.5f, 0.5f) - transform.pivot);
        }
    }
}