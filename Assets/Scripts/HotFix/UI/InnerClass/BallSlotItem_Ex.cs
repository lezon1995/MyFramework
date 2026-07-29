using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class BallSlotItem : IDraggableItem
{
    public myUGUIButton Btn => btn;
    public void SetSelected(bool on) => selected.setActive(on);
    public void SetIconVisible(bool on) => icon.setActive(on);

    public void SetStarCount(int count)
    {
        for (int i = 0; i < stars.Length; ++i)
            stars[i]?.setActive(i < count);
    }

    /// <summary>替换此 item 的点击回调（先清空已注册，再设）。</summary>
    public void SetOnClick(UnityAction callback)
    {
        if (btn != null) btn.setUGUIButtonClick(callback);
    }


    public void onPotentialDragInitialized(PointerEventData data)
    {
        //Debug.Log($"OnPotentialDragInitialized point={data.position}");
    }

    public void onDragStarted(PointerEventData data)
    {
        //Debug.Log($"Drag Start point={data.position}");
    }

    public void onDragging(PointerEventData data)
    {
        //Debug.Log($"Dragging point={data.position}");
    }

    public void onDragEnded(PointerEventData data)
    {
        //Debug.Log($"Drag End point={data.position}");
    }

    public void onDragReleasedOverUI(UIDragReleaseEventData data)
    {
        //Debug.Log($"Drag ReleasedOverUI obj={data.TopmostGameObject?.name}");
    }
}