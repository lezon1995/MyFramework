
using UnityEngine.Localization.Components;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class WaveMonsterView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUITextTMP textTitle;
	protected myUGUIObject itemParent;
	protected WindowStructPool<WaveMonsterItem> WaveMonsterItemPool;
	// auto generate member end

	LocalizeStringEvent _stringEvent;
	
	public WaveMonsterView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		WaveMonsterItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out textTitle, "Title/TextTitle");
		newObject(out itemParent, "G");
		WaveMonsterItemPool.assignTemplate(mRoot, "G/WaveMonsterItem");
		// auto generate assignWindowInternal end

		textTitle.tryGetUnityComponent(out _stringEvent);
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
