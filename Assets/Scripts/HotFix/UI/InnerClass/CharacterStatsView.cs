
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public partial class CharacterStatsView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject G;
	protected WindowStructPool<PlayerStatItem> PlayerStatItemPool;
	// auto generate member end
	public CharacterStatsView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		PlayerStatItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out G, "G");
		PlayerStatItemPool.assignTemplate(mRoot, "G/PlayerStatItem");
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
