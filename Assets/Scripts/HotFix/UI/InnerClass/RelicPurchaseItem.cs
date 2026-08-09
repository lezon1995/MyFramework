namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class RelicPurchaseItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIObject hovered;
	protected myUGUIObject rarityTop;
	protected myUGUIObject newTag;
	protected myUGUIImageSimple itemIcon;
	protected myUGUITextTMP itemDesc;
	protected myUGUIObject rarityBot;
	protected myUGUITextTMP itemName;
	protected myUGUITextTMP itemPrice;
	protected myUGUIObject itemSold;
	// auto generate member end
	public RelicPurchaseItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out btn, "Btn");
		newObject(out hovered, "Btn/Hovered");
		newObject(out rarityTop, "Btn/RarityTop");
		newObject(out newTag, "Btn/New");
		newObject(out itemIcon, "Btn/Icon/Image");
		newObject(out itemDesc, "Btn/Desc/TextDesc");
		newObject(out rarityBot, "Btn/RarityBot");
		newObject(out itemName, "Btn/Name/TextName");
		newObject(out itemPrice, "Btn/Price/TextPrice");
		newObject(out itemSold, "Btn/Sold");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		newTag.registeCollider(onnewTagClick);
		// auto generate init end
	}
	public override void onShow()
	{
		base.onShow();
	}
	//--------------------------------------------------------------------------------------------------------------------------------------------
	protected void onnewTagClick()
	{
		;
	}
}
