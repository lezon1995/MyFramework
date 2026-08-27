using UnityEngine;

namespace MoreMountains;

public partial class BallInventoryItem : IBallOperationTarget
{
    BallInventorySlot ballInventorySlot;
    RectTransform _iconRect;
    Transform _originalParent;
    int _originalSiblingIndex;
    Vector2 _originalPos;
    bool _isFollowingMouse;
    bool _highlightVisible;
    bool _highlightHoveredVisible;

    public bool isOccupied => ballInventorySlot.IsOccupied;
    public myUGUIButton Btn => btn;
    public BallInventorySlot Slot => ballInventorySlot;
    public BallItem Item => ballInventorySlot.Item;

    public GameObject ItemGO => mRoot.getGameObject();
    public void SetSelected(bool on) => focus?.setActive(on);

    public void SetEnabledState(bool on)
    {
        /* 视觉留白 */
    }

    public void SetIconVisible(bool on) => icon?.setActive(on);

    public void SetBallItem(BallItem item)
    {
        tooltipTrigger.setBallItem(item);
        tooltipTrigger.setBallTooltipItem(inventoryBinder.View.BallTooltipItem);
    }

    public void SetBallIcon(Sprite s)
    {
        if (s)
        {
            icon.gameObject.SetActive(true);
            icon?.setSpriteOnly(s);
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
    }

    public void SetBallInventorySlot(BallInventorySlot slot) => ballInventorySlot = slot;

    public void SetRarity(ItemRarity rarity)
    {
        var c = gameDesign.getRarityColor(rarity);
        itemBorder.setColor(c.border);
        iconBg.setColor(c.iconBg);
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

    public void RefreshUpgradeVisual(IBallOperationTarget source, bool visible)
    {
        if (visible)
        {
            var canUpgrade = inventoryBinder.Player.BallManagement.Upgrade.CanUpgradeWith(Slot.Item, source.Item);
            upgrade.setActive(canUpgrade);
        }
        else
        {
            upgrade.setActive(false);
        }
    }

    public void ExecuteOperation(IItemOperationTarget hoveredTarget)
    {
        var source = BallOperationStateManager.Instance.CurrentSource;
        if (source == null)
            return;

        if (hoveredTarget is ShopSellZoneView shopSellZoneView)
        {
            shopSellZoneView.shopBinder.OnPlayerSellBall(ballInventorySlot.Item);
            return;
        }

        // source 来自 BallSlotItem → Equip 或 Swap 到此背包格
        if (source is BallSlotItem srcSlot)
        {
            int srcSlotIndex = -1;
            srcSlot.slotBinder?.GetSlotIndexForItem(srcSlot, out srcSlotIndex);
            if (srcSlotIndex < 0)
                return;

            var srcSlots = srcSlot.slotBinder?.Model;
            var srcBall = srcSlots != null && srcSlotIndex < srcSlots.Slots.Count
                ? srcSlots.Slots[srcSlotIndex].Item
                : null;
            if (srcBall == null)
                return;

            int dstIdx = slotIndex;

            if (hoveredTarget is BallInventoryItem targetInv)
            {
                var targetBag = inventoryBinder?.Bag;
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
                inventoryBinder?.Rebuild();
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
                    ? targetSlots.Slots[targetSlotIndex].Item
                    : null;

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

            var srcBag = inventoryBinder?.Bag;
            var srcBall = srcBag != null && srcIdx < srcBag.SlotList.Count
                ? srcBag.SlotList[srcIdx].Item
                : null;
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
                    ? targetSlots.Slots[targetSlotIndex].Item
                    : null;

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

                inventoryBinder?.Rebuild();
                targetSlot.slotBinder?.Rebuild();
                return;
            }

            if (hoveredTarget is BallInventoryItem dstInv)
            {
                int dstIdx = dstInv.slotIndex;
                if (dstIdx < 0 || dstIdx == srcIdx)
                    return;

                if (dstInv.isOccupied)
                {
                    if (inventoryBinder.Player.BallManagement.Upgrade.TryUpgradeWith(srcInv.Slot, dstInv.Slot, out var srcResult))
                    {
                        switch (srcResult)
                        {
                            case BallItemUpgradeResult.Vanished:
                                BallItem.Release(srcInv.Slot.Item);
                                srcInv.Slot.Set(null);
                                break;
                            case BallItemUpgradeResult.Downgraded:
                                inventoryBinder.Bag.OnSlotItemDowngraded?.Invoke(srcInv.Slot);
                                break;
                        }

                        inventoryBinder.Bag.OnSlotItemUpgraded?.Invoke(dstInv.Slot);
                    }
                    else
                    {
                        srcBag.Swap(srcIdx, dstIdx);
                    }
                }
                else
                {
                    srcBag.SlotList[srcIdx].Set(null);
                    inventoryBinder?.Bag?.AddAt(dstIdx, srcBall);
                }

                inventoryBinder?.Rebuild();
            }
        }
    }
}