using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class BallSlotItem : IDraggableItem
{
    public myUGUIButton Btn => btn;
    public GameObject ItemGO => mRoot.getGameObject();
    public void SetSelected(bool on) => selected.setActive(on);
    public void SetBallIcon(Sprite s) => icon.setSpriteOnly(s);
    public void SetIconVisible(bool on) => icon.setActive(on);

    public void SetStarCount(int count)
    {
        for (int i = 0; i < stars.Length; ++i)
            stars[i]?.setActive(i < count);
    }

    /// <summary>替换此 item 的拖拽松开回调。</summary>
    public void SetOnDragReleased(Action<BallSlotItem, UIDragReleaseEventData> callback)
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

    Action<BallSlotItem, UIDragReleaseEventData> _onDragReleased;
}
