
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/RewardChoosePanel.prefab
// 
public partial class RewardChooseView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUITextTMP textTitle;
	protected myUGUIObject rewardItems;
	protected myUGUIButton btnReroll;
	protected WindowStructPool<RewardChooseItem> RewardChooseItemPool;
	// auto generate member end
	public RewardChooseView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		RewardChooseItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out textTitle, "Title/TextTitle");
		newObject(out rewardItems, "RewardItems");
		newObject(out btnReroll, "BtnReroll");
		RewardChooseItemPool.assignTemplate(mRoot, "RewardItems/RewardChooseItem");
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
