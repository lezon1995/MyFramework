using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class RelicInventoryItem : IDraggableItem
{
    public myUGUIButton Btn => btn;
    public GameObject ItemGO => mRoot.getGameObject();
    public void SetSelected(bool on) => focus?.setActive(on);
    public void SetEnabled(bool on) => disable.setActive(!on);
    public void SetRelicIcon(Sprite s) => icon.setSpriteOnly(s);
    public void SetIconVisible(bool on) => icon.setActive(on);
    public void SetOnClick(UnityAction a)
    {
        if (btn == null)
            return;
        btn.setUGUIButtonClick(a);
    }

    /// <summary>替换此 item 的拖拽松开回调。</summary>
    public void SetOnDragReleased(Action<RelicInventoryItem, UIDragReleaseEventData> callback)
    {
        _onDragReleased = callback;
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
        _onDragReleased?.Invoke(this, data);
    }

    Action<RelicInventoryItem, UIDragReleaseEventData> _onDragReleased;
}
