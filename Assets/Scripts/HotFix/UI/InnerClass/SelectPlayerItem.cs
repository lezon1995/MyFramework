using System;
using MoreMountains;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 
public class SelectPlayerItem : WindowRecyclableUGUI
// auto generate classname end
{
    // auto generate member start
    protected myUGUIObject hovered;
    protected myUGUIObject selected;
    protected myUGUIImageSimple icon;
    protected myUGUIButton button;
    // auto generate member end

    private Action<SelectPlayerItem> onClicked;
    private Action<SelectPlayerItem> onHovered;
    private Action<SelectPlayerItem> onHoveredEnd;
    private bool isSelected;

    public PlayerDef Def { get; private set; }
    public bool IsUnlocked { get; private set; }

    public SelectPlayerItem(IWindowObjectOwner parent) : base(parent)
    {
        // auto generate constructor start
        // auto generate constructor end
    }

    protected override void assignWindowInternal()
    {
        // auto generate assignWindowInternal start
        newObject(out hovered, "Hovered");
        newObject(out selected, "Selected");
        newObject(out icon, "Icon/Image");
        newObject(out button, "Button");
        // auto generate assignWindowInternal end
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        button.registeCollider(onbuttonClick);
        // auto generate init end

        button.setUGUIButtonClick(onItemClick);
        button.setUGUIMouseEnter(onItemHoverStart);
        button.setUGUIMouseExit(onItemHoverEnd);
    }

    void onItemHoverStart(PointerEventData p, GameObject o)
    {
        if (!IsUnlocked)
            return;

        hovered.setActive(true);
        Tween.Scale(getRoot().getRectTransform(), endValue: 1.05F, duration: 0.15F, ease: Ease.OutCubic);
        onHovered?.Invoke(this);
    }

    void onItemHoverEnd(PointerEventData p, GameObject o)
    {
        if (!IsUnlocked)
            return;

        hovered.setActive(false);
        onHoveredEnd?.Invoke(this);
    }

    public override void recycle()
    {
        base.recycle();
        Def = null;
        IsUnlocked = false;
        isSelected = false;
        onClicked = null;
        onHovered = null;
        onHoveredEnd = null;

        setActive(true);
    }

    public void refresh(PlayerDef def, bool unlocked, Action<SelectPlayerItem> clicked, Action<SelectPlayerItem> hovered, Action<SelectPlayerItem> hoveredEnd)
    {
        Def = def;
        IsUnlocked = unlocked;
        onClicked = clicked;
        onHovered = hovered;
        onHoveredEnd = hoveredEnd;
        isSelected = false;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (IsUnlocked)
        {
            // lockOverlay.setActive(false);
            // lockIcon.setActive(false);
            button.setInteractable(true);
            SetGrayscale(false);

            icon.setSpriteOnly(Def.Icon);
        }
        else
        {
            // lockOverlay.setActive(true);
            // lockIcon.setActive(true);
            button.setInteractable(false);
            SetGrayscale(true);

            icon.setSpriteOnly(Def.Icon);
        }

        selected.setActive(false);
    }

    void SetGrayscale(bool grayscale)
    {
        if (grayscale)
        {
            icon.setColor(new Color(0.5f, 0.5f, 0.5f));
        }
        else
        {
            icon.setColor(Color.white);
        }
    }

    public void SetSelected(bool a)
    {
        isSelected = a;
        selected.setActive(a);
    }

    void onItemClick()
    {
        onClicked?.Invoke(this);
    }

    public override void onHide()
    {
        base.onHide();
        getRoot().setScale(1f);
    }

    protected void onbuttonClick()
    {
        ;
    }
}