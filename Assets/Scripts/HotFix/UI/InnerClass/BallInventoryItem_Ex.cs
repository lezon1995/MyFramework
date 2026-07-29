using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class BallInventoryItem : IDraggableItem
{
    public myUGUIButton Btn => btn;
    public void SetSelected(bool on) => focus?.setActive(on);

    public void SetEnabledState(bool on)
    {
        /* 视觉留白：normal/disable 切换，可视迭代阶段再补 */
    }

    public void SetIconVisible(bool on) => icon.setActive(on);

    /// <summary>把 0..stars.Length 颗星点亮, 简单的 0/1 表达：可见=已到达该等级。</summary>
    public void SetStarCount(int count)
    {
        for (int i = 0; i < stars.Length; ++i)
            stars[i]?.setActive(i < count);
    }

    /// <summary>替换此 item 的点击回调（先清空已注册，再设）。</summary>
    public void SetOnClick(UnityEngine.Events.UnityAction callback)
    {
        if (btn != null) btn.setUGUIButtonClick(callback);
    }

    public void onPotentialDragInitialized(PointerEventData data)
    {
    }

    public void onDragStarted(PointerEventData data)
    {
    }

    public void onDragging(PointerEventData data)
    {
    }

    public void onDragEnded(PointerEventData data)
    {
    }

    public void onDragReleasedOverUI(UIDragReleaseEventData data)
    {
    }
}