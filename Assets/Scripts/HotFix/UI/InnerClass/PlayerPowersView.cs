
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public class PlayerPowersView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected WindowStructPool<PlayerPowerItem> PlayerPowerItemPool;
	// auto generate member end
	public PlayerPowersView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		PlayerPowerItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		PlayerPowerItemPool.assignTemplate("H/PlayerPowerItem");
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
