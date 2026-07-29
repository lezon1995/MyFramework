using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class RelicInventoryItem : IDraggableItem
{
    public myUGUIButton Btn => btn;
    public void SetSelected(bool on) => focus?.setActive(on);
    public void SetEnabled(bool on) => disable.setActive(!on);
    public void SetIconVisible(bool on) => icon.setActive(on);
    public void SetOnClick(UnityAction a) => btn.setUGUIButtonClick(a);
    
    
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