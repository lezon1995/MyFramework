
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public class EnemyIntentsView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject intentsParent;
	protected myUGUIObject[] intent = new myUGUIObject[5];
	protected myUGUIObject intentEffecting;
	// auto generate member end
	public EnemyIntentsView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out intentsParent, "V");
		for (int i = 0; i < intent.Length; ++i)
		{
			newObject(out intent[i], "V/Intent" + IToS(i));
		}
		newObject(out intentEffecting, "V/IntentEffecting");
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
