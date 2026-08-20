
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public partial class CharacterInfoView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected CharacterStatsView characterStatsView;
	protected CharacterBallsView characterBallsView;
	protected CharacterHealthView characterHealthView;
	protected CharacterExpView characterExpView;
	// auto generate member end
	public CharacterInfoView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		characterStatsView = new(this);
		characterBallsView = new(this);
		characterHealthView = new(this);
		characterExpView = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		characterStatsView.assignWindow(mRoot, "H/CharacterStatsView");
		characterBallsView.assignWindow(mRoot, "H/Center/CharacterBallsView");
		characterHealthView.assignWindow(mRoot, "H/Center/CharacterHealthView");
		characterExpView.assignWindow(mRoot, "H/Center/CharacterExpView");
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
