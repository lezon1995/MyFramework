
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public partial class BossHealthBarView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple healthBarRenderer;
	protected myUGUITextTMP health;
	// auto generate member end
	
	protected DamageChunkHealthBarUI damageChunkHealthBarUI;
	public BossHealthBarView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out healthBarRenderer, "HealthBarRenderer");
		newObject(out health, "Health");
		// auto generate assignWindowInternal end

		healthBarRenderer.tryGetUnityComponent(out damageChunkHealthBarUI);
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
