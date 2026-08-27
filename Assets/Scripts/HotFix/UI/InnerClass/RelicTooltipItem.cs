
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/RelicTooltipItem.prefab
// 
public partial class RelicTooltipItem : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIObject hovered;
	protected myUGUIImageSimple itemBorder;
	protected myUGUIImageSimple itemBg;
	protected myUGUIImageSimple IconBg;
	protected myUGUIImageSimple itemIcon;
	protected myUGUITextTMP itemDesc;
	protected myUGUITextTMP itemName;
	protected myUGUIObject tagsParent;
	protected myUGUITextTMP itemPrice;
	protected WindowStructPool<TagItem> TagItemPool;
	// auto generate member end
	public RelicTooltipItem(IWindowObjectOwner parent) : base(parent)
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
		newObject(out IconBg, "Btn/Icon/IconBg");
		newObject(out itemIcon, "Btn/Icon/Image");
		newObject(out itemDesc, "Btn/Desc/TextDesc");
		newObject(out itemName, "Btn/Name/TextName");
		newObject(out tagsParent, "Btn/Tags");
		newObject(out itemPrice, "Btn/Price/TextPrice");
		TagItemPool.assignTemplate(mRoot, "Btn/Tags/TagItem");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
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
