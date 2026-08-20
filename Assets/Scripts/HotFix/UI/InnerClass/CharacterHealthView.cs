
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public partial class CharacterHealthView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple expBar;
	protected myUGUITextTMP curExp;
	protected myUGUITextTMP maxExp;
	// auto generate member end
	public CharacterHealthView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out expBar, "Fill");
		newObject(out curExp, "Health/TextCurHealth");
		newObject(out maxExp, "Health/TextMaxHealth");
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
