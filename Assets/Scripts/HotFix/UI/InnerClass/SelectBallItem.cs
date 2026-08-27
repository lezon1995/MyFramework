using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using static StringUtility;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 
public partial class SelectBallItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject hovered;
	protected myUGUIObject selected;
	protected myUGUIImageSimple icon;
	protected myUGUIObject[] stars = new myUGUIObject[3];
	protected myUGUIButton button;
	// auto generate member end
	
	Action<SelectBallItem> onClicked;
	Action<SelectBallItem> onHovered;
	Action<SelectBallItem> onHoveredEnd;
	bool isSelected;

	public BallItem Item { get; private set; }
	public bool IsUnlocked { get; private set; }
	
	
	public SelectBallItem(IWindowObjectOwner parent) : base(parent)
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
		for (int i = 0; i < stars.Length; ++i)
		{
			newObject(out stars[i], "Icon/Grade_Star_01/Star" + i.IToS());
		}
		newObject(out button, "Button");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
		
		button.setUGUIButtonClick(onItemClick);
		button.setUGUIMouseEnter(onItemHoverStart);
		button.setUGUIMouseExit(onItemHoverEnd);
	}
	public override void onShow()
	{
		base.onShow();
	}
	//--------------------------------------------------------------------------------------------------------------------------------------------
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
        Item = null;
        IsUnlocked = false;
        isSelected = false;
        onClicked = null;
        onHovered = null;
        onHoveredEnd = null;

        setActive(true);
    }

    public void refresh(BallItem item, bool unlocked, Action<SelectBallItem> clicked, Action<SelectBallItem> hovered, Action<SelectBallItem> hoveredEnd)
    {
        Item = item;
        IsUnlocked = unlocked;
        onClicked = clicked;
        onHovered = hovered;
        onHoveredEnd = hoveredEnd;
        isSelected = false;

        UpdateVisuals();
    }
    
    
    public void refresh(BallItem item)
    {
	    Item = item;
	    IsUnlocked = true;
	    // onClicked = clicked;
	    // onHovered = hovered;
	    // onHoveredEnd = hoveredEnd;
	    // isSelected = false;

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

            icon.setSpriteOnly(Item.Def.Icon);
        }
        else
        {
            // lockOverlay.setActive(true);
            // lockIcon.setActive(true);
            button.setInteractable(false);
            SetGrayscale(true);

            icon.setSpriteOnly(Item.Def.Icon);
        }

        selected.setActive(false);
        
        int level = 1;
        for (var i = 0; i < stars.Length; i++)
        {
	        stars[i].setActive(i < level);
        }
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
}
