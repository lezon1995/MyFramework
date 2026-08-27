using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class BallSlotItem : IBallOperationTarget
{
    RectTransform _iconRect;
    Canvas _iconRectCanvas;
    Transform _originalParent;
    int _originalSiblingIndex;
    Vector2 _originalPos;
    bool _isFollowingMouse;
    bool _highlightVisible;
    bool _highlightHoveredVisible;

    // IBallOperationTarget 实现

    public void BeginFollowMouse(RectTransform iconSource)
    {
        _iconRect = icon.getRectTransform();
        _iconRectCanvas = _iconRect.GetComponentInParent<Canvas>();
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

        // Overlay Canvas:屏幕坐标直接减去屏幕中心即为 local position(当 Canvas Scaler Reference Resolution 与屏幕分辨率一致时)
        // 若不一致,需要乘以 canvas 的缩放因子
        var canvas = _iconRectCanvas;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _iconRect.position = screenMousePos;
            return;
        }

        // Camera Space / World Space:用 RectTransformUtility
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
            // 归位渲染层级
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

    public void RefreshUpgradeVisual(IBallOperationTarget source, bool visible)
    {
    }

    public void ExecuteOperation(IItemOperationTarget hoveredTarget)
    {
        var source = BallOperationStateManager.Instance.CurrentSource;
        if (source == null)
            return;

        if (hoveredTarget is ShopSellZoneView shopSellZoneView)
        {
            shopSellZoneView.shopBinder.OnPlayerSellBall(ballSlot.Item);
            return;
        }

        // source 来自 BallInventoryItem → Equip 或 Swap 到此槽
        if (source is BallInventoryItem srcInv)
        {
            int srcSlotIndex = srcInv.slotIndex;
            if (srcSlotIndex < 0)
                return;

            var srcBag = srcInv.inventoryBinder?.Bag;
            var srcBall = srcBag != null && srcSlotIndex < srcBag.SlotList.Count
                ? srcBag.SlotList[srcSlotIndex].Item : null;
            if (srcBall == null)
                return;

            var targetGroup = slotBinder?.Model;
            if (targetGroup == null)
                return;

            if (hoveredTarget is BallInventoryItem targetInv)
            {
                // 背包格之间 Swap
                int targetSlotIndex = targetInv.slotIndex;
                if (targetSlotIndex < 0 || targetSlotIndex == srcSlotIndex)
                    return;

                srcBag?.Swap(srcSlotIndex, targetSlotIndex);
                slotBinder?.Rebuild();
                return;
            }

            if (hoveredTarget is BallSlotItem targetSlotItem && targetSlotItem != this)
            {
                int targetSlotIndex = -1;
                targetSlotItem.slotBinder?.GetSlotIndexForItem(targetSlotItem, out targetSlotIndex);
                if (targetSlotIndex < 0)
                    return;

                var targetBall = targetSlotIndex < targetGroup.Slots.Count
                    ? targetGroup.Slots[targetSlotIndex].Item : null;

                if (targetBall != null)
                {
                    srcBag.SlotList[srcSlotIndex].Set(targetBall);
                    targetGroup.Slots[targetSlotIndex].Set(srcBall);
                }
                else
                {
                    targetGroup.Slots[targetSlotIndex].Set(srcBall);
                    srcBag.SlotList[srcSlotIndex].Set(null);
                }

                slotBinder?.Rebuild();
                targetSlotItem.slotBinder?.Rebuild();
                return;
            }
        }

        // source 来自 BallSlotItem → Swap 或移动到背包
        if (source is BallSlotItem srcSlotItem && srcSlotItem != hoveredTarget)
        {
            int srcSlotIndex = -1;
            slotBinder?.GetSlotIndexForItem(srcSlotItem, out srcSlotIndex);
            if (srcSlotIndex < 0)
                return;

            var srcGroup = slotBinder?.Model;
            var srcBall = srcGroup != null && srcSlotIndex < srcGroup.Slots.Count
                ? srcGroup.Slots[srcSlotIndex].Item : null;
            if (srcBall == null)
                return;

            if (hoveredTarget is BallSlotItem targetSlotItem)
            {
                int targetSlotIndex = -1;
                targetSlotItem.slotBinder?.GetSlotIndexForItem(targetSlotItem, out targetSlotIndex);
                if (targetSlotIndex < 0 || targetSlotIndex == srcSlotIndex)
                    return;

                var targetBall = targetSlotIndex < srcGroup.Slots.Count
                    ? srcGroup.Slots[targetSlotIndex].Item : null;

                if (srcGroup != null)
                {
                    if (targetBall != null)
                    {
                        srcGroup.Slots[srcSlotIndex].Set(targetBall);
                        srcGroup.Slots[targetSlotIndex].Set(srcBall);
                    }
                    else
                    {
                        srcGroup.Slots[srcSlotIndex].Set(null);
                        srcGroup.Slots[targetSlotIndex].Set(srcBall);
                    }
                }

                slotBinder?.Rebuild();
                return;
            }

            if (hoveredTarget is BallInventoryItem targetInv)
            {
                int targetSlotIndex = targetInv.slotIndex;
                if (targetSlotIndex < 0)
                    return;

                var targetBag = targetInv.inventoryBinder?.Bag;
                if (targetBag == null)
                    return;

                if (targetInv.isOccupied)
                {
                    var targetBall = targetSlotIndex < targetBag.SlotList.Count
                        ? targetBag.SlotList[targetSlotIndex].Item : null;
                    srcGroup.Slots[srcSlotIndex].Set(targetBall);
                    targetBag.SlotList[targetSlotIndex].Set(srcBall);
                }
                else
                {
                    srcGroup.Slots[srcSlotIndex].Set(null);
                    targetBag.AddAt(targetSlotIndex, srcBall);
                }

                slotBinder?.Rebuild();
                targetInv.inventoryBinder?.Rebuild();
            }
        }
    }
}

// IDraggableItem 保留(拖拽已废弃但接口保留兼容)
public partial class BallSlotItem : IDraggableItem
{
    BallSlot ballSlot;
    public myUGUIButton Btn => btn;
    public BallSlot Slot => ballSlot;
    public BallItem Item => ballSlot.Item;
    public GameObject ItemGO => mRoot.getGameObject();
    public void SetSelected(bool on) => selected?.setActive(on);
    public void SetBallIcon(Sprite s) => icon?.setSpriteOnly(s);
    public void SetIconVisible(bool on) => icon?.setActive(on);
    public void SetBallSlot(BallSlot slot) => ballSlot = slot;
    public void SetStarCount(int count)
    {
        if (stars == null)
            return;
        for (int i = 0; i < stars.Length; ++i)
            stars[i]?.setActive(i < count);
    }

    public void SetOnDragReleased(Action<BallSlotItem, UIDragReleaseEventData> callback) { }

    public void onPotentialDragInitialized(PointerEventData data) { }
    public void onDragStarted(PointerEventData data) { }
    public void onDragging(PointerEventData data) { }
    public void onDragEnded(PointerEventData data) { }
    public void onDragReleasedOverUI(UIDragReleaseEventData data) { }
}
