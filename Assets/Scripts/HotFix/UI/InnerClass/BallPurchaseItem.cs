namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class BallPurchaseItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIObject hovered;
	protected myUGUIImageSimple itemBorder;
	protected myUGUIImageSimple itemBg;
	protected myUGUIObject newTag;
	protected myUGUIImageSimple IconBg;
	protected myUGUIImageSimple itemIcon;
	protected myUGUITextTMP itemDesc;
	protected myUGUITextTMP itemName;
	protected myUGUIObject tagsParent;
	protected myUGUITextTMP itemPrice;
	protected myUGUIObject itemSold;
	protected WindowStructPool<TagItem> TagItemPool;
	// auto generate member end
	public BallPurchaseItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		TagItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out btn, "Btn");
		newObject(out hovered, "Btn/Hovered");
		newObject(out itemBorder, "Btn/Border");
		newObject(out itemBg, "Btn/Bg");
		newObject(out newTag, "Btn/New");
		newObject(out IconBg, "Btn/Icon/IconBg");
		newObject(out itemIcon, "Btn/Icon/Image");
		newObject(out itemDesc, "Btn/Desc/TextDesc");
		newObject(out itemName, "Btn/Name/TextName");
		newObject(out tagsParent, "Btn/Tags");
		newObject(out itemPrice, "Btn/Price/TextPrice");
		newObject(out itemSold, "Btn/Sold");
		TagItemPool.assignTemplate(mRoot, "Btn/Tags/TagItem");
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
