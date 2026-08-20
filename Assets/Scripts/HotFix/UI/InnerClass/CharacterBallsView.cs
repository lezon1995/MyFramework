
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/CharacterInfoView.prefab
// 
public partial class CharacterBallsView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject itemParent;
	protected WindowStructPool<BallInventoryItem> BallInventoryItemPool;
	// auto generate member end
	public CharacterBallsView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		BallInventoryItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out itemParent, "H");
		BallInventoryItemPool.assignTemplate(mRoot, "H/BallInventoryItem");
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
