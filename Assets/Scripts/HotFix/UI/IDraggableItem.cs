using UnityEngine.EventSystems;

namespace MoreMountains;

public interface IDraggableItem
{
    void onPotentialDragInitialized(PointerEventData data);
    void onDragStarted(PointerEventData data);
    void onDragging(PointerEventData data);
    void onDragEnded(PointerEventData data);
    void onDragReleasedOverUI(UIDragReleaseEventData data);
}