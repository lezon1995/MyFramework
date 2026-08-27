
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class TagItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple colorBg;
	protected myUGUITextTMP tagName;
	// auto generate member end
	public TagItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out colorBg, "Bg");
		newObject(out tagName, "TextName");
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
}
