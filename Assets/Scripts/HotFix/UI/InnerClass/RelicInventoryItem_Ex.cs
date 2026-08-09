using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class RelicInventoryItem : IRelicOperationTarget
{
    RelicInventorySlot relicInventorySlot;
    RectTransform _iconRect;
    Transform _originalParent;
    int _originalSiblingIndex;
    Vector2 _originalPos;
    bool _isFollowingMouse;
    bool _highlightVisible;
    bool _highlightHoveredVisible;

    public bool isOccupied => relicInventorySlot != null && relicInventorySlot.IsOccupied;
    public myUGUIButton Btn => btn;
    public GameObject ItemGO => mRoot.getGameObject();
    public void SetSelected(bool on) => focus?.setActive(on);
    public void SetEnabled(bool on) => disable?.setActive(!on);
    public void SetRelicIcon(Sprite s) => icon?.setSpriteOnly(s);
    public void SetIconVisible(bool on) => icon?.setActive(on);
    public void SetRelicInventorySlot(RelicInventorySlot slot) => relicInventorySlot = slot;

    // IRelicOperationTarget 实现

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

    /// <summary>
    /// Relic 操作状态确认:只能 RelicInventoryItem 之间交换/移动。
    /// </summary>
    public void ExecuteOperation(IItemOperationTarget hoveredTarget)
    {
        var source = RelicOperationStateManager.Instance.CurrentSource;
        if (source == null)
            return;
        
        if (hoveredTarget is ShopSellZoneView shopSellZoneView)
        {
            var relicItem = relicInventorySlot.Item;
            shopSellZoneView.shopBinder.OnPlayerSellRelic(relicItem);
            return;
        }

        // source 来自 RelicInventoryItem → 交换位置
        if (source is RelicInventoryItem srcInv)
        {
            int srcIdx = srcInv.slotIndex;
            if (srcIdx < 0)
                return;

            var srcBag = relicBinder?.Bag;
            if (srcBag == null)
                return;

            var srcRelic = srcIdx < srcBag.SlotList.Count ? srcBag.SlotList[srcIdx].Item : null;
            if (srcRelic == null)
                return;

            if (hoveredTarget is RelicInventoryItem dstInv)
            {
                int dstIdx = dstInv.slotIndex;
                if (dstIdx < 0 || dstIdx == srcIdx)
                    return;

                if (dstInv.isOccupied)
                    srcBag.Swap(srcIdx, dstIdx);
                else
                {
                    srcBag.SlotList[srcIdx].Set(null);
                    srcBag.AddAt(dstIdx, srcRelic);
                }

                relicBinder?.Rebuild();
            }
        }
    }
}