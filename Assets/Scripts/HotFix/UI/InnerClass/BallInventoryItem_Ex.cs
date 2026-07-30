using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class BallInventoryItem : IBallOperationTarget
{
    BallInventorySlot ballInventorySlot;
    BallItem ballItem;
    RectTransform _iconRect;
    Transform _originalParent;
    int _originalSiblingIndex;
    Vector2 _originalPos;
    bool _isFollowingMouse;
    bool _highlightVisible;
    bool _highlightHoveredVisible;

    public bool isOccupied => ballInventorySlot.IsOccupied;
    public myUGUIButton Btn => btn;
    public GameObject ItemGO => mRoot.getGameObject();
    public void SetSelected(bool on) => focus?.setActive(on);

    public void SetEnabledState(bool on)
    {
        /* 视觉留白 */
    }

    public void SetIconVisible(bool on) => icon?.setActive(on);
    public void SetBallIcon(Sprite s) => icon?.setSpriteOnly(s);
    public void SetBallItem(BallItem item) => ballItem = item;
    public void SetBallInventorySlot(BallInventorySlot slot) => ballInventorySlot = slot;

    public void SetStarCount(int count)
    {
        if (stars == null)
            return;
        for (int i = 0; i < stars.Length; ++i)
            stars[i]?.setActive(i < count);
    }

    // IBallOperationTarget 实现

    public void BeginFollowMouse(RectTransform iconSource)
    {
        _iconRect = icon?.getGameObject()?.GetComponent<RectTransform>();
        if (_iconRect == null)
            return;
        _originalParent = _iconRect.parent;
        _originalSiblingIndex = _iconRect.GetSiblingIndex();
        _originalPos = _iconRect.anchoredPosition;

        // 移到 Canvas 根下,确保渲染在最上层
        var canvasRoot = _originalParent;
        while (canvasRoot != null && canvasRoot.GetComponent<Canvas>() == null)
            canvasRoot = canvasRoot.parent;

        if (canvasRoot != null)
        {
            _iconRect.SetParent(canvasRoot, false);
            _iconRect.SetAsLastSibling();
        }

        _isFollowingMouse = true;
    }

    public void UpdateFollowMouse(Vector2 screenMousePos)
    {
        if (!_isFollowingMouse || _iconRect == null)
            return;

        var canvas = _iconRect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _iconRect.position = screenMousePos;
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _iconRect.parent as RectTransform, screenMousePos, canvas?.worldCamera, out var localPos))
        {
            _iconRect.anchoredPosition = localPos;
        }
    }

    public void EndFollowMouse()
    {
        if (!_isFollowingMouse)
            return;
        _isFollowingMouse = false;

        if (_iconRect != null)
        {
            _iconRect.SetParent(_originalParent, false);
            _iconRect.SetSiblingIndex(_originalSiblingIndex);
            _iconRect.anchoredPosition = _originalPos;
        }
    }

    public void SetHovered(bool isHovered)
    {
        _highlightHoveredVisible = isHovered;
        RefreshHighlightVisuals();
    }

    public void SetHighlightVisible(bool visible)
    {
        _highlightVisible = visible;
        _highlightHoveredVisible = false;
        RefreshHighlightVisuals();
    }

    public void SetEventBlocking(bool blocking)
    {
        _eventBlocked = blocking;
    }

    void RefreshHighlightVisuals()
    {
        highlight?.setActive(_highlightVisible);
        highlightHovered?.setActive(_highlightVisible && _highlightHoveredVisible);
    }

    public void ExecuteOperation(IBallOperationTarget hoveredTarget)
    {
        var source = BallOperationStateManager.Instance.CurrentSource;
        if (source == null)
            return;

        // source 来自 BallSlotItem → Equip 或 Swap 到此背包格
        if (source is BallSlotItem srcSlot)
        {
            int srcSlotIndex = -1;
            srcSlot.slotBinder?.GetSlotIndexForItem(srcSlot, out srcSlotIndex);
            if (srcSlotIndex < 0)
                return;

            var srcSlots = srcSlot.slotBinder?.Model;
            var srcBall = srcSlots != null && srcSlotIndex < srcSlots.Slots.Count
                ? srcSlots.Slots[srcSlotIndex].Item : null;
            if (srcBall == null)
                return;

            int dstIdx = slotIndex;

            if (hoveredTarget is BallInventoryItem targetInv)
            {
                var targetBag = slotBinder?.Bag;
                if (targetBag == null)
                    return;

                if (targetInv.isOccupied)
                {
                    var targetBall = dstIdx < targetBag.SlotList.Count ? targetBag.SlotList[dstIdx].Item : null;
                    srcSlots.Slots[srcSlotIndex].Set(targetBall);
                    targetBag.SlotList[dstIdx].Set(srcBall);
                }
                else
                {
                    targetBag.AddAt(dstIdx, srcBall);
                    srcSlots.Slots[srcSlotIndex].Set(null);
                }

                srcSlot.slotBinder?.Rebuild();
                slotBinder?.Rebuild();
                return;
            }

            if (hoveredTarget is BallSlotItem targetSlot && targetSlot != srcSlot)
            {
                int targetSlotIndex = -1;
                targetSlot.slotBinder?.GetSlotIndexForItem(targetSlot, out targetSlotIndex);
                if (targetSlotIndex < 0)
                    return;

                var targetSlots = targetSlot.slotBinder?.Model;
                var targetBall = targetSlots != null && targetSlotIndex < targetSlots.Slots.Count
                    ? targetSlots.Slots[targetSlotIndex].Item : null;

                if (targetBall != null)
                {
                    targetSlots.Slots[targetSlotIndex].Set(srcBall);
                    srcSlots.Slots[srcSlotIndex].Set(targetBall);
                }
                else
                {
                    targetSlots.Slots[targetSlotIndex].Set(srcBall);
                    srcSlots.Slots[srcSlotIndex].Set(null);
                }

                srcSlot.slotBinder?.Rebuild();
                targetSlot.slotBinder?.Rebuild();
                return;
            }
        }

        // source 来自 BallInventoryItem
        if (source is BallInventoryItem srcInv)
        {
            int srcIdx = srcInv.slotIndex;
            if (srcIdx < 0)
                return;

            var srcBag = slotBinder?.Bag;
            var srcBall = srcBag != null && srcIdx < srcBag.SlotList.Count
                ? srcBag.SlotList[srcIdx].Item : null;
            if (srcBall == null)
                return;

            if (hoveredTarget is BallSlotItem targetSlot)
            {
                int targetSlotIndex = -1;
                targetSlot.slotBinder?.GetSlotIndexForItem(targetSlot, out targetSlotIndex);
                if (targetSlotIndex < 0)
                    return;

                var targetSlots = targetSlot.slotBinder?.Model;
                if (targetSlots == null)
                    return;
                var targetBall = targetSlotIndex < targetSlots.Slots.Count
                    ? targetSlots.Slots[targetSlotIndex].Item : null;

                if (targetBall != null)
                {
                    srcBag.SlotList[srcIdx].Set(targetBall);
                    targetSlots.Slots[targetSlotIndex].Set(srcBall);
                }
                else
                {
                    targetSlots.Slots[targetSlotIndex].Set(srcBall);
                    srcBag.SlotList[srcIdx].Set(null);
                }

                slotBinder?.Rebuild();
                targetSlot.slotBinder?.Rebuild();
                return;
            }

            if (hoveredTarget is BallInventoryItem dstInv)
            {
                int dstIdx = dstInv.slotIndex;
                if (dstIdx < 0 || dstIdx == srcIdx)
                    return;

                if (dstInv.isOccupied)
                    srcBag.Swap(srcIdx, dstIdx);
                else
                {
                    srcBag.SlotList[srcIdx].Set(null);
                    slotBinder?.Bag?.AddAt(dstIdx, srcBall);
                }

                slotBinder?.Rebuild();
            }
        }
    }
}

// IDraggableItem 保留(拖拽已废弃)
public partial class BallInventoryItem : IDraggableItem
{
    public void SetOnClick(UnityEngine.Events.UnityAction callback) { }
    public void SetOnDragReleased(Action<BallInventoryItem, UIDragReleaseEventData> callback) { }

    public void onPotentialDragInitialized(PointerEventData data) { }
    public void onDragStarted(PointerEventData data) { }
    public void onDragging(PointerEventData data) { }
    public void onDragEnded(PointerEventData data) { }
    public void onDragReleasedOverUI(UIDragReleaseEventData data) { }
}
