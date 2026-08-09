
using UnityEngine;

namespace MoreMountains;

public partial class ShopSellZoneView : IItemOperationTarget
{
    public ShopBinder shopBinder;
    bool _highlightVisible;
    bool _highlightHoveredVisible;
    internal bool _eventBlocked;
    
    public void BeginFollowMouse(RectTransform iconSource)
    {
    }

    public void UpdateFollowMouse(Vector2 screenMousePos)
    {
    }

    public void EndFollowMouse()
    {
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
}
