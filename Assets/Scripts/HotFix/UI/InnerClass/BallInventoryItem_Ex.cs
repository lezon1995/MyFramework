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
        if (stars == null) return;
        for (int i = 0; i < stars.Length; ++i)
            stars[i]?.setActive(i < count);
    }

    // IBallOperationTarget 实现

    public void BeginFollowMouse(RectTransform iconSource)
    {
        _iconRect = icon?.getGameObject()?.GetComponent<RectTransform>();
        if (_iconRect == null) return;
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
        if (!_isFollowingMouse || _iconRect == null) return;

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
        if (!_isFollowingMouse) return;
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

    /// <summary>左键点击在此 BallInventoryItem 上时执行操作。</summary>
    public void ExecuteOperation(IBallOperationTarget hoveredTarget)
    {
        // source 可能是 BallSlotItem(装备/Unequip)或其他 BallInventoryItem(无效操作)
        // 实际执行由 slotBinder 统一处理
        slotBinder.OnInventoryOperationConfirmed(slotIndex);
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
