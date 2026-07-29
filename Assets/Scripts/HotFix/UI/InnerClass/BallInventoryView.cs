namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class BallInventoryView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUITextTMP textTitle;
	protected myUGUIObject itemParent;
	protected WindowStructPool<BallInventoryItem> BallInventoryItemPool;
	// auto generate member end
	public BallInventoryView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		BallInventoryItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out textTitle, "Title/TextTitle");
		newObject(out itemParent, "G");
		BallInventoryItemPool.assignTemplate(mRoot, "G/BallInventoryItem");
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
